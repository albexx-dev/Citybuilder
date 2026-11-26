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
        // Счётчики по типам: CountZone не перебирает все назначенные клетки.
        private readonly Dictionary<ZoneType, int> counts = new Dictionary<ZoneType, int>();

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
            ZoneType previous;
            bool had = zones.TryGetValue(cell, out previous);
            if (zone == ZoneType.None)
            {
                if (had) counts[previous] = counts[previous] - 1;
                zones.Remove(cell);
                return true;
            }
            zones[cell] = zone;
            if (had && previous == zone) return true;
            if (had) counts[previous] = counts[previous] - 1;
            int count;
            counts.TryGetValue(zone, out count);
            counts[zone] = count + 1;
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
            int count;
            return counts.TryGetValue(zone, out count) ? count : 0;
        }

        // Копия всех назначенных клеток, только чтение. None не хранится.
        public List<KeyValuePair<GridPosition, ZoneType>> GetZonedCells()
        {
            return new List<KeyValuePair<GridPosition, ZoneType>>(zones);
        }
    }
}
