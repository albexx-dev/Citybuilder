using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Дороги drag-отрезками: ЛКМ тянет, релиз строит, ПКМ сносит. Только старый Input.
    public class RoadPlacementController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязателен: даёт Grid и клетку под курсором.")]
        public GridVisualizer GridVisualizer;
        [Tooltip("Тип дороги. Цена клетки берётся отсюда.")]
        public RoadDefinition Definition;
        [Tooltip("Казна. Пусто = строить бесплатно.")]
        public TreasuryView TreasuryView;
        [Tooltip("Вид дорог. Пусто = создастся на этом же объекте.")]
        public RoadView View;

        [Header("Placement")]
        [Tooltip("Режим стройки дорог. Выключите, когда строите дома.")]
        public bool placementActive = true;

        private RoadSystem system;
        private readonly List<RoadState> states = new List<RoadState>();
        private bool dragging;
        private GridPosition dragStart;
        private GridPosition lastHover;
        private List<GridPosition> previewCells = new List<GridPosition>();

        private void Start()
        {
            EnsureReady();
        }

        // Восстанавливает систему после перезагрузки домена.
        // Список построенного при перезагрузке теряется (чинятся сохранениями, не V1).
        private bool EnsureReady()
        {
            if (GridVisualizer == null)
            {
                Debug.LogError("RoadPlacementController: задайте GridVisualizer в Inspector.");
                enabled = false;
                return false;
            }
            if (View == null)
            {
                View = GetComponent<RoadView>();
                if (View == null)
                {
                    View = gameObject.AddComponent<RoadView>();
                }
            }
            if (system == null)
            {
                system = new RoadSystem(GridVisualizer.Grid);
            }
            return true;
        }

        private void Update()
        {
            if (!EnsureReady())
            {
                return;
            }
            if (!placementActive)
            {
                CancelDrag();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelDrag();
                return;
            }
            GridPosition cell;
            bool hovered = GridVisualizer.TryGetHoveredCell(out cell);
            HandleDrag(cell, hovered);
            if (Input.GetMouseButtonDown(1) && hovered)
            {
                TryDemolish(cell);
            }
        }

        private void HandleDrag(GridPosition cell, bool hovered)
        {
            if (Input.GetMouseButtonDown(0) && hovered)
            {
                dragging = true;
                dragStart = cell;
                UpdatePreview(cell);
            }
            else if (dragging && Input.GetMouseButton(0) && hovered && cell != lastHover)
            {
                UpdatePreview(cell);
            }
            if (dragging && Input.GetMouseButtonUp(0))
            {
                CommitDrag();
            }
        }

        private void UpdatePreview(GridPosition hover)
        {
            lastHover = hover;
            previewCells = GetLineCells(dragStart, hover);
            View.ShowPreview(previewCells, GridVisualizer.Grid.CellSize, CheckCells());
        }

        private void CancelDrag()
        {
            dragging = false;
            previewCells.Clear();
            View.HidePreview();
        }

        private void CommitDrag()
        {
            dragging = false;
            View.HidePreview();
            if (!CheckCells())
            {
                return;
            }
            int total = previewCells.Count * system.CellCost(Definition);
            if (total > 0 && TreasuryView != null && TreasuryView.Treasury != null)
            {
                Treasury.TransactionResult spent = TreasuryView.Treasury.TrySpend(total, "Road");
                if (!spent.Success)
                {
                    return;
                }
            }
            for (int i = 0; i < previewCells.Count; i++)
            {
                RoadState state = system.Place(previewCells[i]);
                if (state != null)
                {
                    states.Add(state);
                }
            }
            RefreshView();
            GridVisualizer.RefreshOccupied();
        }

        private void TryDemolish(GridPosition cell)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].Cell == cell)
                {
                    system.Demolish(states[i]);
                    states.RemoveAt(i);
                    RefreshView();
                    GridVisualizer.RefreshOccupied();
                    return;
                }
            }
        }

        private bool CheckCells()
        {
            for (int i = 0; i < previewCells.Count; i++)
            {
                if (!system.CanPlace(previewCells[i]))
                {
                    return false;
                }
            }
            if (TreasuryView != null && TreasuryView.Treasury != null)
            {
                int total = previewCells.Count * system.CellCost(Definition);
                return TreasuryView.Treasury.CanAfford(total);
            }
            return true;
        }

        private void RefreshView()
        {
            List<GridPosition> cells = new List<GridPosition>(states.Count);
            for (int i = 0; i < states.Count; i++)
            {
                cells.Add(states[i].Cell);
            }
            View.Rebuild(cells, GridVisualizer.Grid.CellSize);
        }

        private List<GridPosition> GetLineCells(GridPosition from, GridPosition to)
        {
            List<GridPosition> cells = new List<GridPosition>();
            int dx = to.X - from.X;
            int dy = to.Y - from.Y;
            bool horizontal = Mathf.Abs(dx) >= Mathf.Abs(dy);
            int step = (horizontal ? dx : dy) >= 0 ? 1 : -1;
            int length = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
            for (int i = 0; i <= length; i++)
            {
                cells.Add(horizontal
                    ? new GridPosition(from.X + i * step, from.Y)
                    : new GridPosition(from.X, from.Y + i * step));
            }
            return cells;
        }
    }
}
