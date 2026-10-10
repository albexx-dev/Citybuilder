using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Чистая логика заселения поверх GridSystem, ZoneSystem и BuildingSystem. Без Unity.
    public class SettlementSystem
    {
        private readonly ZoneSystem zones;
        private readonly BuildingSystem buildings;

        public int SettledCount { get; private set; }
        public int TotalCapacity { get; private set; }
        public int TotalJobs { get; private set; }

        public SettlementSystem(GridSystem grid, ZoneSystem zones, BuildingSystem buildings)
        {
            if (grid == null)
            {
                throw new ArgumentNullException("grid");
            }
            if (zones == null)
            {
                throw new ArgumentNullException("zones");
            }
            if (buildings == null)
            {
                throw new ArgumentNullException("buildings");
            }
            this.zones = zones;
            this.buildings = buildings;
        }

        // Все клетки подходят по зоне и стройка свободна.
        public bool CanSettle(BuildingDefinition definition, GridPosition origin)
        {
            if (definition == null)
            {
                return false;
            }
            if (definition.AllowedZone != ZoneType.None)
            {
                List<GridPosition> cells = definition.CoversCells(origin);
                for (int i = 0; i < cells.Count; i++)
                {
                    if (zones.GetZone(cells[i]) != definition.AllowedZone)
                    {
                        return false;
                    }
                }
            }
            return buildings.CanPlace(definition, origin);
        }

        // Ставит здание и растит счётчики. Не подошло — null.
        public BuildingState Settle(BuildingDefinition definition, GridPosition origin)
        {
            if (!CanSettle(definition, origin))
            {
                return null;
            }
            BuildingState state = buildings.Place(definition, origin);
            if (state == null)
            {
                return null;
            }
            SettledCount++;
            TotalCapacity += ClampNonNegative(definition.ResidentCapacity);
            TotalJobs += ClampNonNegative(definition.JobsProvided);
            return state;
        }

        // Сносит здание и уменьшает счётчики. Повторный снос — false.
        public bool Demolish(BuildingState state)
        {
            if (state == null || state.Cells == null || state.Cells.Count == 0)
            {
                return false;
            }
            if (!buildings.Demolish(state))
            {
                return false;
            }
            int capacity = 0;
            int jobs = 0;
            if (state.Definition != null)
            {
                capacity = ClampNonNegative(state.Definition.ResidentCapacity);
                jobs = ClampNonNegative(state.Definition.JobsProvided);
            }
            TotalCapacity = Math.Max(0, TotalCapacity - capacity);
            TotalJobs = Math.Max(0, TotalJobs - jobs);
            SettledCount = Math.Max(0, SettledCount - 1);
            return true;
        }

        private static int ClampNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }
    }
}
