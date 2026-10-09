using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Назначение зон клеткам карты. Чистая логика, без Unity.
    // Хранит только назначенные зоны, None не хранится.
    public class ZoneSystem
    {
        private readonly GridSystem grid;
        private readonly Dictionary<GridPosition, ZoneType> zones =
            new Dictionary<GridPosition, ZoneType>();

        public ZoneSystem(GridSystem grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException("grid");
            }
            this.grid = grid;
        }

        public bool SetZone(GridPosition cell, ZoneType zone)
        {
            if (!grid.IsInside(cell))
            {
                return false;
            }
            if (zone == ZoneType.None)
            {
                zones.Remove(cell);
                return true;
            }
            zones[cell] = zone;
            return true;
        }

        public ZoneType GetZone(GridPosition cell)
        {
            if (!grid.IsInside(cell))
            {
                return ZoneType.None;
            }
            ZoneType zone;
            if (zones.TryGetValue(cell, out zone))
            {
                return zone;
            }
            return ZoneType.None;
        }

        public int CountZone(ZoneType zone)
        {
            int count = 0;
            foreach (KeyValuePair<GridPosition, ZoneType> entry in zones)
            {
                if (entry.Value == zone)
                {
                    count++;
                }
            }
            return count;
        }

        // Копия всех назначенных клеток, только чтение. None не хранится.
        public List<KeyValuePair<GridPosition, ZoneType>> GetZonedCells()
        {
            return new List<KeyValuePair<GridPosition, ZoneType>>(zones);
        }
    }
}
