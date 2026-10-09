using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Кисть зон: ЛКМ красит drag-полосой, ПКМ стирает. Только старый Input.
    // Настройка сцены: компонент на объекте с GridVisualizer, по умолчанию выключен.
    public class ZonePaintingController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязателен: даёт Grid и клетку под курсором.")]
        public GridVisualizer GridVisualizer;
        [Tooltip("Вид зон. Пусто = создастся на этом же объекте.")]
        public ZoneView View;

        [Header("Brush")]
        [Tooltip("Тип зоны для кисти.")]
        public ZoneType Brush = ZoneType.Residential;
        [Tooltip("Режим покраски зон. Выключите, когда строите дома или дороги.")]
        public bool placementActive = true;

        private ZoneSystem system;
        private GridPosition lastCell;
        private bool hasLastCell;

        private void Start()
        {
            EnsureReady();
            RefreshView();
        }

        // Восстанавливает систему после перезагрузки домена.
        // Нарисованное при перезагрузке теряется из данных (чинятся сохранениями, не V1).
        private bool EnsureReady()
        {
            if (GridVisualizer == null)
            {
                Debug.LogError("ZonePaintingController: задайте GridVisualizer в Inspector.");
                enabled = false;
                return false;
            }
            if (View == null)
            {
                View = GetComponent<ZoneView>();
                if (View == null)
                {
                    View = gameObject.AddComponent<ZoneView>();
                }
            }
            if (system == null)
            {
                system = new ZoneSystem(GridVisualizer.Grid);
            }
            return true;
        }

        private void Update()
        {
            if (!EnsureReady())
            {
                return;
            }
            if (!placementActive || system == null)
            {
                return;
            }
            if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                hasLastCell = false;
                return;
            }
            GridPosition cell;
            if (!GridVisualizer.TryGetHoveredCell(out cell))
            {
                return;
            }
            if (hasLastCell && cell == lastCell)
            {
                return;
            }
            lastCell = cell;
            hasLastCell = true;
            ApplyBrush(cell);
            RefreshView();
        }

        private void ApplyBrush(GridPosition cell)
        {
            if (Input.GetMouseButton(0))
            {
                system.SetZone(cell, Brush);
            }
            else
            {
                system.SetZone(cell, ZoneType.None);
            }
        }

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
    }
}
