using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса ZonePaintingController (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class ZonePaintingController : MonoBehaviour
    {
        private void RefreshView()
        {
            View.Rebuild(BuildGroups(), GridVisualizer.Grid.CellSize);
        }

        private Dictionary<ZoneType, List<GridPosition>> BuildGroups()
        {
            Dictionary<ZoneType, List<GridPosition>> groups = new Dictionary<ZoneType, List<GridPosition>>();
            groups[ZoneType.Residential] = new List<GridPosition>();
            groups[ZoneType.Commercial] = new List<GridPosition>();
            groups[ZoneType.Industrial] = new List<GridPosition>();
            List<KeyValuePair<GridPosition, ZoneType>> painted = system.GetZonedCells();
            for (int i = 0; i < painted.Count; i++)
            {
                List<GridPosition> list;
                if (groups.TryGetValue(painted[i].Value, out list))
                {
                    list.Add(painted[i].Key);
                }
            }
            return groups;
        }

        // Перерисовать вид по живым зонам (после загрузки слота).
        public void RepaintAll()
        {
            if (!EnsureReady() || system == null)
            {
                return;
            }
            RefreshView();
        }

        // Снять все зоны и перерисовать (первый шаг загрузки слота).
        public void ClearZones()
        {
            if (!EnsureReady() || system == null)
            {
                return;
            }
            List<KeyValuePair<GridPosition, ZoneType>> cells = system.GetZonedCells();
            for (int i = 0; i < cells.Count; i++)
            {
                system.SetZone(cells[i].Key, ZoneType.None);
            }
            RefreshView();
        }
    }
}
