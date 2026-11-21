using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Пожары (EPIC-43, ADR-009): дом вне покрытия пожарных может загореться через
    // детерминированный хеш (tick, EntityId) — без System.Random, решение одинаково
    // при одинаковых входах. Станция в радиусе тушит за 1 тик; без станции после
    // BurnTicks дом разрушается через делегат demolish (клетки, выселение — Unity-сторона).
    // События уходят в ленту через делегат Fired. Состояние горения НЕ сериализуется:
    // после загрузки горящих нет (ADR-009).
    public class FireSystem
    {
        public const float IgniteChancePerTick = 0.002f;
        public const int BurnTicks = 6;

        private readonly Dictionary<EntityId, int> burning = new Dictionary<EntityId, int>();

        // Событие для ленты города: вид, уникальное по дому сообщение, тик.
        public event Action<EventKind, string, int> Fired;

        // Сколько домов горит прямо сейчас (снимок для оверлея).
        public int BurningCount
        {
            get { return burning.Count; }
        }

        // Сброс несериализуемого горения перед «Новая игра»/загрузкой (ADR-009).
        public void Reset() { burning.Clear(); }

        // Горит ли конкретный дом.
        public bool IsBurning(EntityId id)
        {
            return burning.ContainsKey(id);
        }

        // Снимок горящих домов для презентации (FireOverlayView).
        public List<EntityId> SnapshotBurning()
        {
            return new List<EntityId>(burning.Keys);
        }

        // Один тик часов: покрытые дома тушатся, непокрытые горят и разрушаются, новые возгораются.
        public void Tick(int tick, List<BuildingState> homes, Func<BuildingState, float> coverage,
            Action<BuildingState> demolish)
        {
            if (homes == null) return;
            for (int i = 0; i < homes.Count; i++)
            {
                BuildingState home = homes[i];
                if (home == null) continue;
                if (IsProtected(coverage, home))
                {
                    Extinguish(home, tick);
                    continue;
                }
                if (burning.ContainsKey(home.Id))
                {
                    AdvanceBurn(tick, home, demolish);
                    continue;
                }
                TryIgnite(tick, home);
            }
        }

        // Хук E2E и тестов: принудительный поджог дома вне зависимости от покрытия и хеша.
        public void ForceIgnite(BuildingState home, int tick)
        {
            if (home == null || burning.ContainsKey(home.Id)) return;
            burning[home.Id] = 1;
            Emit(EventKind.Fire, "Пожар в доме #" + home.Id.Value + "!", tick);
        }

        // FNV-1a по байтам тика и id: мультипликативно, детерминированно (паттерн ADR-009).
        public static uint Hash(int tick, EntityId id)
        {
            uint hash = 2166136261u;
            hash = MixByte(hash, (byte)tick);
            hash = MixByte(hash, (byte)(tick >> 8));
            hash = MixByte(hash, (byte)(tick >> 16));
            hash = MixByte(hash, (byte)(tick >> 24));
            int value = id.Value;
            hash = MixByte(hash, (byte)value);
            hash = MixByte(hash, (byte)(value >> 8));
            hash = MixByte(hash, (byte)(value >> 16));
            hash = MixByte(hash, (byte)(value >> 24));
            return hash;
        }

        // Порог возгорания: младшие 16 бит хеша против шанса за тик (0.002 ≈ 1 пожар на 8 минут без покрытия).
        public static bool ShouldIgnite(int tick, EntityId id)
        {
            return (Hash(tick, id) & 0xFFFF) / 65536.0 < IgniteChancePerTick;
        }

        private static uint MixByte(uint hash, byte value)
        {
            return (hash ^ value) * 16777619u;
        }

        // NaN в покрытии — безопасная сторона: дом считается накрытым (план тестов EPIC-43).
        private static bool IsProtected(Func<BuildingState, float> coverage, BuildingState home)
        {
            if (coverage == null) return true;
            float value = coverage(home);
            return float.IsNaN(value) || value > 0f;
        }

        private void Extinguish(BuildingState home, int tick)
        {
            if (!burning.Remove(home.Id)) return;
            Emit(EventKind.Fire, "Пожар в доме #" + home.Id.Value + " потушен", tick);
        }

        private void AdvanceBurn(int tick, BuildingState home, Action<BuildingState> demolish)
        {
            int burned = burning[home.Id] + 1;
            if (burned < BurnTicks)
            {
                burning[home.Id] = burned;
                return;
            }
            burning.Remove(home.Id);
            if (demolish != null) demolish(home);
            Emit(EventKind.Fire, "Дом #" + home.Id.Value + " сгорел дотла", tick);
        }

        private void TryIgnite(int tick, BuildingState home)
        {
            if (!ShouldIgnite(tick, home.Id)) return;
            burning[home.Id] = 1;
            Emit(EventKind.Fire, "Пожар в доме #" + home.Id.Value + "!", tick);
        }

        private void Emit(EventKind kind, string message, int tick)
        {
            if (Fired != null) Fired(kind, message, tick);
        }
    }
}
