using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Чистый запрос сведений о здании под клеткой сетки (EPIC-12 шаг 1).
    // Не зависит от MonoBehaviour: собирает снимок из списков выросших/ручных зданий и чистых систем.
    public static class BuildingInfoQuery
    {
        public static BuildingInfo Query(
            GridPosition cell,
            GridSize gridSize,
            IReadOnlyList<BuildingState> grownHomes,
            IReadOnlyList<BuildingState> manualBuildings,
            PopulationSystem population,
            ServiceBuildingSystem services,
            float powerRate01,
            float waterRate01,
            float wasteRate01,
            Func<BuildingState, bool> isRoadConnected)
        {
            BuildingInfo info = new BuildingInfo();
            if (!gridSize.Contains(cell)) return info;
            bool isGrown;
            BuildingState state = FindAt(cell, grownHomes, manualBuildings, out isGrown);
            if (state == null || state.Definition == null) return info;

            BuildingDefinition def = state.Definition;
            info.Found = true;
            info.IsGrown = isGrown;
            info.Id = def.Id ?? string.Empty;
            info.DisplayName = string.IsNullOrEmpty(def.DisplayName) ? info.Id : def.DisplayName;
            info.DisplayNameKey = def.DisplayNameKey ?? string.Empty;
            info.Origin = state.Origin;
            info.Cost = def.ConstructionCost;
            info.UpkeepPerHour = BuildingLevelPolicy.ScaledUpkeep(
                def.UpkeepPerHour < 0 ? 0 : def.UpkeepPerHour, state.Level);
            info.Capacity = def.ResidentCapacity;
            info.Level = state.Level;
            info.JobsProvided = def.JobsProvided;
            info.Residents = isGrown && population != null ? population.ResidentsIn(state) : 0;
            info.ServiceKind = def.ServiceKind;
            info.ServiceRadius = BuildingLevelPolicy.ServiceRadius(def.ServiceRadius, state.Level);
            info.PowerRate01 = Sanitize01(powerRate01);
            info.WaterRate01 = Sanitize01(waterRate01);
            info.WasteRate01 = Sanitize01(wasteRate01);
            info.RoadConnected = isRoadConnected != null && isRoadConnected(state);
            info.UpgradePrice = UpgradeController.PriceFor(def, state.Level);
            info.Upgradeable = info.UpgradePrice > 0;
            FillServiceFields(ref info, state, grownHomes, services);
            return info;
        }

        // Состояние здания по клетке для команд над выбранной панелью (апгрейд, EPIC-51). Нет — null.
        public static BuildingState FindStateAt(
            GridPosition cell,
            IReadOnlyList<BuildingState> grownHomes,
            IReadOnlyList<BuildingState> manualBuildings)
        {
            bool isGrown;
            return FindAt(cell, grownHomes, manualBuildings, out isGrown);
        }

        // Список клеток внутри сетки, покрытых кругом станции (единая математика через ServiceCoverage).
        public static List<GridPosition> GetRadiusCells(GridPosition center, int radius, GridSize gridSize)
        {
            List<GridPosition> result = new List<GridPosition>();
            if (radius <= 0) return result;
            ServiceCoverage cov = new ServiceCoverage();
            cov.SetSource(EntityId.New(), center, radius);
            int minX = Math.Max(0, center.X - radius);
            int maxX = Math.Min(gridSize.Width - 1, center.X + radius);
            int minY = Math.Max(0, center.Y - radius);
            int maxY = Math.Min(gridSize.Height - 1, center.Y + radius);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    GridPosition pos = new GridPosition(x, y);
                    if (cov.IsCovered(pos)) result.Add(pos);
                }
            }
            return result;
        }

        private static void FillServiceFields(
            ref BuildingInfo info,
            BuildingState state,
            IReadOnlyList<BuildingState> grownHomes,
            ServiceBuildingSystem services)
        {
            if (services != null)
            {
                info.FireCoverage01 = services.Coverage01(ServiceKind.Fire, state.Origin);
                info.PoliceCoverage01 = services.Coverage01(ServiceKind.Police, state.Origin);
                info.HealthCoverage01 = services.Coverage01(ServiceKind.Health, state.Origin);
                info.LeisureCoverage01 = services.Coverage01(ServiceKind.Leisure, state.Origin);
            }
            if (info.ServiceKind == ServiceKind.None || info.ServiceRadius <= 0) return;
            ServiceCoverage single = new ServiceCoverage();
            single.SetSource(EntityId.New(), state.Origin, info.ServiceRadius);
            if (grownHomes != null)
            {
                for (int i = 0; i < grownHomes.Count; i++)
                {
                    if (grownHomes[i] != null) single.SetPoint(grownHomes[i].Id, grownHomes[i].Origin);
                }
            }
            info.CoveredHomes = single.CoveredPoints;
            info.TotalHomes = single.PointCount;
        }

        private static BuildingState FindAt(
            GridPosition cell,
            IReadOnlyList<BuildingState> grown,
            IReadOnlyList<BuildingState> manual,
            out bool isGrown)
        {
            BuildingState state = FindInList(cell, grown);
            if (state != null)
            {
                isGrown = true;
                return state;
            }
            isGrown = false;
            return FindInList(cell, manual);
        }

        private static BuildingState FindInList(GridPosition cell, IReadOnlyList<BuildingState> list)
        {
            if (list == null) return null;
            for (int i = 0; i < list.Count; i++)
            {
                BuildingState s = list[i];
                if (s == null || s.Cells == null) continue;
                for (int c = 0; c < s.Cells.Count; c++)
                {
                    if (s.Cells[c] == cell) return s;
                }
            }
            return null;
        }

        private static float Sanitize01(float value)
        {
            if (float.IsNaN(value)) return 1f;
            if (value < 0f) return 0f;
            return value > 1f ? 1f : value;
        }
    }
}
