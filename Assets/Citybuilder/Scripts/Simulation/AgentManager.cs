using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Машины-агенты (ADR-011, EPIC-44): презентационная симуляция поездок по дорогам.
    // Домен про агентов не знает; единственная связь — нагрузка клеток CongestionSystem.
    public class AgentManager
    {
        public const int MaxAgents = 40;
        public const int SpawnsPerHour = 8;
        public const int DefaultActiveLimit = 12;
        public const float CellsPerSecond = 2f;
        public const int MorningStart = 7;
        public const int MorningEnd = 9;
        public const int EveningStart = 17;
        public const int EveningEnd = 19;

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
        private long spawnCursor;
        private int activeLimit = DefaultActiveLimit;

        public AgentManager(CongestionSystem congestion) { this.congestion = congestion; }

        public int ActiveCount { get { int count = 0; for (int i = 0; i < MaxAgents; i++) if (agents[i].Active) count++; return count; } }

        // Агент по индексу слота 0..MaxAgents-1 (для видов). Вне диапазона — неактивный.
        public VehicleAgent AgentAt(int index)
        {
            return index >= 0 && index < MaxAgents ? agents[index] : default(VehicleAgent);
        }

        // Часовой тик: утро 7-9 спавн дом→работа, вечер 17-19 работа→дом, иначе тихо.
        public void OnGameHour(int hour, IReadOnlyList<GridPosition> homes, IReadOnlyList<GridPosition> works, IReadOnlyList<GridPosition> roads)
        {
            if (hour >= MorningStart && hour < MorningEnd) SpawnBatch(homes, works, roads);
            else if (IsSpawnHour(hour)) SpawnBatch(works, homes, roads);
        }

        // Единая проверка окна спавна (утро 7-9 / вечер 17-19): читают OnGameHour и хост.
        public static bool IsSpawnHour(int hour) { return (hour >= MorningStart && hour < MorningEnd) || (hour >= EveningStart && hour < EveningEnd); }

        // Партия поездок за час окна: пары целей по вращающемуся курсору.
        // Лимит одновременных машин: настройки игрока (Настройки → Лимит машин) и жалоба
        // владельца на «слишком много» — спавн останавливается на пределе.
        public void SetActiveLimit(int value)
        {
            if (value < 1) value = 1;
            if (value > MaxAgents) value = MaxAgents;
            activeLimit = value;
        }

        public int ActiveLimit { get { return activeLimit; } }

        private void SpawnBatch(IReadOnlyList<GridPosition> fromOrigins, IReadOnlyList<GridPosition> toOrigins, IReadOnlyList<GridPosition> roads)
        {
            int budget = activeLimit - ActiveCount;
            if (budget <= 0) return;
            if (IsEmpty(fromOrigins) || IsEmpty(toOrigins) || IsEmpty(roads)) return;
            int count = SpawnsPerHour < budget ? SpawnsPerHour : budget;
            for (int i = 0; i < count; i++)
            {
                long pairIndex = spawnCursor + i;
                GridPosition from = fromOrigins[(int)(pairIndex % fromOrigins.Count)];
                GridPosition to = toOrigins[(int)(pairIndex % toOrigins.Count)];
                TrySpawn(from, to, roads);
            }
            spawnCursor += SpawnsPerHour;
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

        // Движение по реальным секундам; на паузе хост просто не вызывает Advance.
        public void Advance(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            for (int i = 0; i < MaxAgents; i++)
            {
                if (!agents[i].Active) continue;
                VehicleAgent agent = agents[i];
                float factor = CellSpeedFactor != null ? CellSpeedFactor(agent.Path[agent.CellIndex]) : 1f;
                agent.Progress += CellsPerSecond * factor * deltaTime;
                int count = agent.Path.Count;
                int cellIndex = (int)agent.Progress;
                if (cellIndex > count - 1) cellIndex = count - 1;
                if (cellIndex > agent.CellIndex)
                {
                    agent.CellIndex = cellIndex;
                    FeedCongestion(agent.Path[cellIndex]);
                }
                if (agent.Progress >= count - 1)
                {
                    agents[i] = default(VehicleAgent);
                    continue;
                }
                agents[i] = agent;
            }
        }

        // Фид пробок: нагрузка текущей клетки агента как одноклеточная «поездка».
        private void FeedCongestion(GridPosition cell)
        {
            if (congestion == null) return;
            tripCell.Clear();
            tripCell.Add(cell);
            congestion.TrackTrip(tripCell);
        }
    }
}
