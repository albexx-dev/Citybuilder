using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Машины-агенты (ADR-011, EPIC-44): презентационная симуляция поездок по дорогам.
    // Домен про агентов не знает; единственная связь — нагрузка клеток CongestionSystem.
    public partial class AgentManager
    {
        public const int MaxAgents = 40;
        public const int DefaultActiveLimit = 12;
        public const float CellsPerSecond = 2f;

        // Поставщик маршрутов: в сцене — RouteSystem.FindPath, в тестах — fake.
        public Func<GridPosition, GridPosition, List<GridPosition>> PathFinder;

        // Поставщик множителя скорости клетки (EPIC-45): 1 — полная скорость, 0.3 — затор; null = полная.
        public Func<GridPosition, float> CellSpeedFactor;

        // Агент: маршрут по дорогам, пройденные клетки и индекс текущей клетки (фид пробок).
        public struct VehicleAgent
        {
            public bool Active;
            public List<GridPosition> Path;
            public float Progress;
            public int CellIndex;
        }

        private readonly VehicleAgent[] agents = new VehicleAgent[MaxAgents];
        private readonly CongestionSystem congestion;
        private readonly List<GridPosition> tripCell = new List<GridPosition>(1);
        private long roadCursor;
        private int activeLimit = DefaultActiveLimit;

        public AgentManager(CongestionSystem congestion) { this.congestion = congestion; }

        public int ActiveCount { get { int count = 0; for (int i = 0; i < MaxAgents; i++) if (agents[i].Active) count++; return count; } }

        // Агент по индексу слота 0..MaxAgents-1 (для видов). Вне диапазона — неактивный.
        public VehicleAgent AgentAt(int index)
        {
            return index >= 0 && index < MaxAgents ? agents[index] : default(VehicleAgent);
        }

        // Спавн по событию «построена дорога» (решение владельца: машины появляются, когда
        // строятся дороги, а не по расписанию часов). Пара дом→работа берётся по вращающемуся
        // курсору; TrySpawn сам якорит концы к ближайшим дорогам и проверяет соединённость
        // маршрутом. false — ни одна пара не соединена (машина пока не нужна).
        public bool SpawnForNewRoad(GridPosition newRoad, IReadOnlyList<GridPosition> homes,
            IReadOnlyList<GridPosition> works, IReadOnlyList<GridPosition> roads)
        {
            if (ActiveCount >= activeLimit) return false;
            if (IsEmpty(homes) || IsEmpty(works) || IsEmpty(roads)) return false;
            int pairs = homes.Count * works.Count;
            for (int i = 0; i < pairs; i++)
            {
                long pairIndex = roadCursor + i;
                GridPosition from = homes[(int)(pairIndex % homes.Count)];
                GridPosition to = works[(int)(pairIndex % works.Count)];
                int before = ActiveCount;
                TrySpawn(from, to, roads);
                if (ActiveCount > before)
                {
                    roadCursor += i + 1;
                    return true;
                }
            }
            return false;
        }

        // Лимит одновременных машин: настройки игрока (Настройки → Лимит машин).
        public void SetActiveLimit(int value)
        {
            if (value < 1) value = 1;
            if (value > MaxAgents) value = MaxAgents;
            activeLimit = value;
        }

        public int ActiveLimit { get { return activeLimit; } }

        // Полная очистка пула («Новая игра»/загрузка): старые поездки не едут по новому городу.
        public void Clear()
        {
            for (int i = 0; i < MaxAgents; i++) agents[i] = default(VehicleAgent);
            roadCursor = 0;
        }

        private static bool IsEmpty(IReadOnlyList<GridPosition> list)
        {
            return list == null || list.Count == 0;
        }

        // Спавн одного агента: слот, привязка к дороге, путь. Любая неудача — тихий пропуск.
        private void TrySpawn(GridPosition fromOrigin, GridPosition toOrigin, IReadOnlyList<GridPosition> roads)
        {
            int slot = FreeSlot();
            if (slot < 0 || PathFinder == null) return;
            GridPosition from;
            GridPosition to;
            if (!TryAnchor(fromOrigin, roads, out from) || !TryAnchor(toOrigin, roads, out to)) return;
            List<GridPosition> path = PathFinder(from, to);
            if (path == null || path.Count < 2) return;
            VehicleAgent agent = default(VehicleAgent);
            agent.Active = true;
            agent.Path = path;
            agents[slot] = agent;
            FeedCongestion(path[0]);
        }

        private int FreeSlot()
        {
            for (int i = 0; i < MaxAgents; i++) if (!agents[i].Active) return i;
            return -1;
        }

        // Ближайшая дорожная клетка к зданию (манхэттен). Нет дорог — false.
        private static bool TryAnchor(GridPosition origin, IReadOnlyList<GridPosition> roads, out GridPosition anchor)
        {
            anchor = origin;
            int best = int.MaxValue;
            for (int i = 0; i < roads.Count; i++)
            {
                int distance = Math.Abs(origin.X - roads[i].X) + Math.Abs(origin.Y - roads[i].Y);
                if (distance < best) { best = distance; anchor = roads[i]; }
            }
            return best != int.MaxValue;
        }

    }
}
