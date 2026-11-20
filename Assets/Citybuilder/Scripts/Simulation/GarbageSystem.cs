using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Вывоз мусора (EPIC-52): дома копят мусор, готовые встают в очередь, флот
    // мусоровозов (GarbageTrucks) объезжает её от полигона. Прирост за тик
    // детерминирован FNV-1a по (tick, EntityId) со своей солью — без System.Random
    // (паттерн ADR-009). Домен не знает про Unity. Состояние НЕ сериализуется:
    // Reset при отмотке времени назад (как пожары и квесты).
    public class GarbageSystem
    {
        public const int ReadyThreshold = 20;
        private const byte Salt = 0x5A;

        // Дом готов к вывозу: id и его клетка на момент постановки в очередь.
        public struct Pickup
        {
            public EntityId Id;
            public GridPosition At;
        }

        // Событие для ленты города: вид, уникальное по дому сообщение, тик.
        public event Action<EventKind, string, int> Collected;

        private readonly GarbageTrucks fleet = new GarbageTrucks();
        private readonly Dictionary<EntityId, int> accumulated = new Dictionary<EntityId, int>();
        private readonly List<Pickup> ready = new List<Pickup>();
        private readonly HashSet<EntityId> queued = new HashSet<EntityId>();
        private int lastTick;

        // Домов ждёт вывоза (HUD-строка и тесты).
        public int WaitingCount { get { return ready.Count; } }

        // Мусоровозов в пути прямо сейчас.
        public int ActiveTrucks { get { return fleet.ActiveCount; } }

        // Накопление дома (после вывоза — 0). Для оверлея переполнения и тестов.
        public int AccOf(EntityId id)
        {
            int value;
            return accumulated.TryGetValue(id, out value) ? value : 0;
        }

        // Мусоровоз по индексу слота (для видов). Вне диапазона — неактивный.
        public GarbageTrucks.Truck TruckAt(int index)
        {
            return fleet.At(index);
        }

        // Один тик: накопление по домам, очередь готовых, выезд от полигона.
        public void Tick(int tick, List<BuildingState> homes, GridPosition? depot,
            Func<GridPosition, GridPosition, List<GridPosition>> pathFinder)
        {
            lastTick = tick;
            if (homes == null) return;
            for (int i = 0; i < homes.Count; i++) Accumulate(tick, homes[i]);
            fleet.Dispatch(depot, ready, pathFinder);
        }

        // Движение по реальным секундам; на паузе хост не вызывает Advance (как AgentManager).
        public void Advance(float deltaTime)
        {
            fleet.Advance(deltaTime, PickUp);
        }

        // Сброс (отмотка времени, новая игра): накопление, очередь и мусоровозы обнуляются.
        public void Reset()
        {
            accumulated.Clear();
            ready.Clear();
            queued.Clear();
            fleet.Reset();
        }

        // Накопление дома: WasteOutput здания плюс детерминированный бит хеша (0..1).
        private void Accumulate(int tick, BuildingState home)
        {
            if (home == null) return;
            int output = home.Definition != null ? home.Definition.WasteOutput : 0;
            int value = AccOf(home.Id) + GainPerTick(tick, home.Id, output);
            accumulated[home.Id] = value;
            if (value < ReadyThreshold || queued.Contains(home.Id)) return;
            queued.Add(home.Id);
            ready.Add(new Pickup { Id = home.Id, At = home.Origin });
        }

        // Вывоз сбрасывает накопление дома; сообщение уникально по дому — дедуп ленты не задет.
        private void PickUp(EntityId id)
        {
            accumulated.Remove(id);
            queued.Remove(id);
            if (Collected != null)
                Collected(EventKind.Garbage, "Мусоровоз вывез мусор из дома #" + id.Value, lastTick);
        }

        // FNV-1a с солью мусора: поток решений другой, чем у пожаров (паттерн ADR-009).
        public static uint Hash(int tick, EntityId id)
        {
            uint hash = 2166136261u;
            hash = MixByte(hash, Salt);
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

        // Прирост за тик: WasteOutput здания плюс младший бит хеша — детерминированный джиттер.
        public static int GainPerTick(int tick, EntityId id, int wasteOutput)
        {
            int baseOutput = wasteOutput < 0 ? 0 : wasteOutput;
            return baseOutput + (int)(Hash(tick, id) & 1u);
        }

        private static uint MixByte(uint hash, byte value)
        {
            return (hash ^ value) * 16777619u;
        }
    }
}
