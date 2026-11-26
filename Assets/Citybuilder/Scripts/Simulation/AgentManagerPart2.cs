using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Хвост класса AgentManager (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class AgentManager
    {
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

        // Цепочка дом→работа→магазин (issue #166): старый 4-аргументный вызов в AgentManager.cs
        // сохраняет прежнее поведение, новый добавляет магазин как конечную точку маршрута.
        public bool SpawnForNewRoad(GridPosition newRoad, IReadOnlyList<GridPosition> homes,
            IReadOnlyList<GridPosition> works, IReadOnlyList<GridPosition> shops,
            IReadOnlyList<GridPosition> roads)
        {
            if (ActiveCount >= activeLimit || IsEmpty(homes) || IsEmpty(roads)) return false;
            int stops = Count(works) + Count(shops);
            if (stops == 0) return false;
            int pairs = homes.Count * stops;
            for (int i = 0; i < pairs; i++)
            {
                long index = roadCursor + i;
                GridPosition from = homes[(int)(index % homes.Count)];
                GridPosition work, shop;
                bool hasWork = TryPick(works, index, out work);
                bool hasShop = TryPick(shops, index, out shop);
                int before = ActiveCount;
                TrySpawnChain(from, hasWork ? work : shop, hasWork && hasShop, shop, roads);
                if (ActiveCount > before) { roadCursor += i + 1; return true; }
            }
            return false;
        }
        private static int Count(IReadOnlyList<GridPosition> list) { return IsEmpty(list) ? 0 : list.Count; }
        private static bool TryPick(IReadOnlyList<GridPosition> list, long index, out GridPosition cell)
        {
            cell = default(GridPosition);
            if (IsEmpty(list)) return false;
            cell = list[(int)(index % list.Count)];
            return true;
        }
        // Спавн цепочки: PathFinder вызывается по каждому плечу; аллокации — только при спавне.
        private void TrySpawnChain(GridPosition fromOrigin, GridPosition toOrigin, bool hasShop,
            GridPosition shopOrigin, IReadOnlyList<GridPosition> roads)
        {
            int slot = FreeSlot();
            if (slot < 0 || PathFinder == null) return;
            GridPosition from, to, shopAnchor = default(GridPosition);
            if (!TryAnchor(fromOrigin, roads, out from) || !TryAnchor(toOrigin, roads, out to)) return;
            if (hasShop && !TryAnchor(shopOrigin, roads, out shopAnchor)) return;
            List<GridPosition> path = new List<GridPosition>();
            if (!AppendLeg(path, PathFinder(from, to))) return;
            if (hasShop && !AppendLeg(path, PathFinder(to, shopAnchor))) return;
            if (path.Count < 2) return;
            agents[slot] = new VehicleAgent { Active = true, Path = path };
            FeedCongestion(path[0]);
        }
        // Склейка плеч без дублирования стыковой клетки.
        private static bool AppendLeg(List<GridPosition> path, List<GridPosition> leg)
        {
            if (leg == null || leg.Count == 0) return false;
            int start = path.Count > 0 && path[path.Count - 1] == leg[0] ? 1 : 0;
            for (int i = start; i < leg.Count; i++) path.Add(leg[i]);
            return path.Count > 1;
        }
    }
}
