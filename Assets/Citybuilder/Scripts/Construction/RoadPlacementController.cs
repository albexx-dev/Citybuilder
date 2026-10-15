using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Дороги drag-отрезками: ЛКМ тянет, релиз строит, ПКМ сносит. Только старый Input.
    public class RoadPlacementController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязателен: даёт клетку под курсором.")]
        public GridVisualizer GridVisualizer;
        [Tooltip("Хранилище дорог. Обязательно: иначе ищется на этом же объекте.")]
        public RoadNetworkStore Store;
        [Tooltip("Казна. Пусто = строить бесплатно.")]
        public TreasuryView TreasuryView;
        [Tooltip("Вид дорог. Пусто = создастся на этом же объекте.")]
        public RoadView View;

        [Header("Placement")]
        [Tooltip("Режим стройки дорог. Выключите, когда строите дома.")]
        public bool placementActive = true;

        private bool dragging;
        private GridPosition dragStart;
        private GridPosition lastHover;
        private List<GridPosition> previewCells = new List<GridPosition>();

        private void Start()
        {
            EnsureReady();
        }

        // Сброс drag при выключении: иначе висящее превью/коммит.
        // View может быть не создан (ленивый), previewCells — гард на null.
        private void OnDisable()
        {
            dragging = false;
            if (previewCells != null) previewCells.Clear();
            if (View != null) View.HidePreview();
        }

        // Проверка ссылок и ленивый добор View/Store. Систему готовит стор.
        private bool EnsureReady()
        {
            if (GridVisualizer == null)
            {
                Debug.LogError("RoadPlacementController: задайте GridVisualizer в Inspector.");
                enabled = false;
                return false;
            }
            if (View == null) View = GetComponent<RoadView>();
            if (View == null) View = gameObject.AddComponent<RoadView>();
            if (Store == null) Store = GetComponent<RoadNetworkStore>();
            if (Store == null) Store = gameObject.AddComponent<RoadNetworkStore>();
            return Store != null;
        }

        private void Update()
        {
            if (!EnsureReady()) return;
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
            if (Input.GetMouseButtonDown(1) && hovered && !UiClickGuard.IsOverMenu(Input.mousePosition)) TryDemolish(cell);
        }

        private void HandleDrag(GridPosition cell, bool hovered)
        {
            if (Input.GetMouseButtonDown(0) && hovered && !UiClickGuard.IsOverMenu(Input.mousePosition))
            {
                dragging = true;
                dragStart = cell;
                UpdatePreview(cell);
            }
            else if (dragging && Input.GetMouseButton(0) && hovered && cell != lastHover) UpdatePreview(cell);
            if (dragging && Input.GetMouseButtonUp(0)) CommitDrag();
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

        // Коммит drag: проверка отрезка, оплата новых клеток, раскладка — отдельно.
        private void CommitDrag()
        {
            dragging = false;
            View.HidePreview();
            if (!CheckCells()) return;
            int total = NewCellsCount() * Store.CellCost();
            if (total > 0 && TreasuryView != null && TreasuryView.Treasury != null)
            {
                Treasury.TransactionResult spent = TreasuryView.Treasury.TrySpend(total, "Road");
                if (!spent.Success) return;
            }
            PlacePreviewed();
        }

        // Раскладка превью в стор: свои клетки — пропуск, вид и занятость обновляются.
        private void PlacePreviewed()
        {
            for (int i = 0; i < previewCells.Count; i++)
            {
                if (!Store.IsOwnRoadCell(previewCells[i])) Store.Place(previewCells[i]);
            }
            Store.RefreshView();
            GridVisualizer.RefreshOccupied();
        }

        // Снос ПКМ: поиск в сторе, снос и обновление вида.
        private void TryDemolish(GridPosition cell)
        {
            RoadState state = Store.FindStateAt(cell);
            if (state == null) return;
            if (Store.DemolishState(state)) Store.RefreshView();
        }

        private bool CheckCells()
        {
            for (int i = 0; i < previewCells.Count; i++)
            {
                if (!Store.CanPlace(previewCells[i]) && !Store.IsOwnRoadCell(previewCells[i])) return false;
            }
            if (TreasuryView != null && TreasuryView.Treasury != null)
            {
                int total = NewCellsCount() * Store.CellCost();
                return TreasuryView.Treasury.CanAfford(total);
            }
            return true;
        }

        // Число клеток превью, которых ещё нет в сторе.
        private int NewCellsCount()
        {
            int count = 0;
            for (int i = 0; i < previewCells.Count; i++)
            {
                if (!Store.IsOwnRoadCell(previewCells[i])) count++;
            }
            return count;
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
