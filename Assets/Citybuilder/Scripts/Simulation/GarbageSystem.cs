using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Вывоз мусора (EPIC-52): дома копят, флот (GarbageTrucks) объезжает очередь от полигона.
    // Прирост детерминирован FNV-1a
    // (паттерн ADR-009). В v6 (ADR-014) в слот пишутся накопление и адреса очереди, не флот.
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
        private readonly Dictionary<EntityId, GridPosition> origins = new Dictionary<EntityId, GridPosition>();
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
            origins.Clear();
            ready.Clear();
            queued.Clear();
            fleet.Reset();
        }

        // Снимок v6: координаты дома и накопленный мусор; готовые очереди идут первыми.
        public void CollectSnapshot(List<CitySnapshot.GarbageQueueEntry> entries)
        {
            if (entries == null) return;
            for (int i = 0; i < ready.Count; i++) AddEntry(entries, ready[i].Id, ready[i].At);
            foreach (KeyValuePair<EntityId, int> pair in accumulated)
            {
                GridPosition at;
                if (origins.TryGetValue(pair.Key, out at)) AddEntry(entries, pair.Key, at);
            }
        }

        private void AddEntry(List<CitySnapshot.GarbageQueueEntry> entries, EntityId id, GridPosition at)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].X != at.X || entries[i].Y != at.Y) continue;
                entries[i].Accumulated = AccOf(id);
                return;
            }
            entries.Add(new CitySnapshot.GarbageQueueEntry { X = at.X, Y = at.Y, Accumulated = AccOf(id) });
        }

        // Накопление дома: WasteOutput здания плюс детерминированный бит хеша (0..1).
        private void Accumulate(int tick, BuildingState home)
        {
            if (home == null) return;
            int output = home.Definition != null ? home.Definition.WasteOutput : 0;
            int value = AccOf(home.Id) + GainPerTick(tick, home.Id, output);
            accumulated[home.Id] = value;
            origins[home.Id] = home.Origin;
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
            hash = MixByte(hash, Salt); // замыкающая соль: полная декорреляция от пожарного потока
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
