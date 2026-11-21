using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Кражи (EPIC-47): дом с покрытием полиции ниже SafeCoverage может быть обворован через
    // детерминированный хеш (tick, EntityId) — паттерн ADR-009, как пожары. Соль хеша отделяет
    // кражи от возгораний: одно и то же (tick, id) не даёт и пожар, и кражу одновременно.
    // NaN или покрытие ≥ порога — дом в безопасности. Эффекты кражи: штраф казне через делегат
    // steal (списывает CrimeTicker) и временный −досуг дома на 12 тиков: состояние модификатора
    // живёт в системе, в домен населения и сейв не попадает.
    public class CrimeSystem
    {
        public const float TheftChancePerTick = 0.0005f; // смягчено по жалобе владельца «экономика быстро падает»: 20 домов ≈ 1 кража за 4–5 дней
        public const float SafeCoverage = 0.5f;          // порог покрытия полиции: ниже — дом могут обокрасть
        public const int FinePerTheft = 20;              // штраф казне за одну кражу
        public const float LeisurePenalty = 0.2f;        // −досуг дома после кражи
        public const int LeisurePenaltyTicks = 12;       // срок временного модификатора досуга
        private const byte CrimeSalt = 0x63;             // соль FNV: кражи и пожары идут разными следами

        private readonly Dictionary<EntityId, int> robbedAt = new Dictionary<EntityId, int>();
        private int totalThefts;

        // Событие для ленты города: вид, сообщение с номером дома и днём, тик.
        public event Action<EventKind, string, int> Fired;

        // Уровень преступности города 0..1 = 1 − Coverage01(Police); обновляется каждый тик.
        public float CrimeLevel01 { get; private set; }

        // Всего краж за игру (тесты и статистика).
        public int TotalThefts
        {
            get { return totalThefts; }
        }

        // Сброс несериализуемых краж и модификаторов перед «Новая игра»/загрузкой.
        public void Reset()
        {
            robbedAt.Clear();
            totalThefts = 0;
            CrimeLevel01 = 0f;
        }

        // Один тик часов: обновить уровень преступности и проверить дома без полиции.
        public void Tick(int tick, List<BuildingState> homes, Func<BuildingState, float> coverage,
            float crimeLevel01, Action<BuildingState> steal)
        {
            CrimeLevel01 = Clamp01(crimeLevel01);
            if (homes == null) return;
            for (int i = 0; i < homes.Count; i++)
            {
                BuildingState home = homes[i];
                if (home == null || IsSafe(coverage, home) || HasActivePenalty(home.Id, tick)) continue;
                TrySteal(tick, home, steal);
            }
        }

        // Активен ли модификатор досуга: кража была в пределах LeisurePenaltyTicks тиков.
        public bool HasActivePenalty(EntityId id, int tick)
        {
            int robbed;
            return robbedAt.TryGetValue(id, out robbed) && tick - robbed < LeisurePenaltyTicks;
        }

        // Штраф досуга дома на текущий тик: −LeisurePenalty, пока модификатор жив, иначе 0.
        public float LeisurePenalty01(EntityId id, int tick)
        {
            return HasActivePenalty(id, tick) ? LeisurePenalty : 0f;
        }

        // FNV-1a с соль-байтом краж: тот же паттерн, что FireSystem.Hash, но другой след.
        public static uint Hash(int tick, EntityId id)
        {
            uint hash = 2166136261u;
            hash = MixByte(hash, CrimeSalt);
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

        // Порог кражи: младшие 16 бит хеша против шанса за тик (0.0008 ≈ 1 кража на 21 минуту без покрытия).
        public static bool ShouldSteal(int tick, EntityId id)
        {
            return (Hash(tick, id) & 0xFFFF) / 65536.0 < TheftChancePerTick;
        }

        // Хук E2E и тестов: принудительная кража; дом с активным модификатором не крадут повторно.
        public void ForceSteal(BuildingState home, int tick, Action<BuildingState> steal)
        {
            if (home == null || HasActivePenalty(home.Id, tick)) return;
            Rob(tick, home, steal);
        }

        private void TrySteal(int tick, BuildingState home, Action<BuildingState> steal)
        {
            if (!ShouldSteal(tick, home.Id)) return;
            Rob(tick, home, steal);
        }

        // Сама кража: отметка времени, эффект через делегат, событие в ленту.
        private void Rob(int tick, BuildingState home, Action<BuildingState> steal)
        {
            robbedAt[home.Id] = tick;
            totalThefts++;
            if (steal != null) steal(home);
            Emit(EventKind.Crime,
                "Кража в доме #" + home.Id.Value + " (День " + tick / SimulationTime.TicksPerDay + ")", tick);
        }

        // NaN в покрытии — безопасная сторона: дом считается накрытым (план тестов эпика).
        private static bool IsSafe(Func<BuildingState, float> coverage, BuildingState home)
        {
            if (coverage == null) return true;
            float value = coverage(home);
            return float.IsNaN(value) || value >= SafeCoverage;
        }

        private void Emit(EventKind kind, string message, int tick)
        {
            if (Fired != null) Fired(kind, message, tick);
        }

        private static uint MixByte(uint hash, byte value)
        {
            return (hash ^ value) * 16777619u;
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || value < 0f) return 0f;
            return value > 1f ? 1f : value;
        }
    }
}
