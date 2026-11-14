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
        [Tooltip("Контроллер сохранения.")]
        public SaveController Save;

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
            if (Save == null) Save = FindFirstObjectByType<SaveController>();
            if (View == null) View = GetComponent<RoadView>();
            if (View == null) View = gameObject.AddComponent<RoadView>();
            if (Store == null) Store = GetComponent<RoadNetworkStore>();
            if (Store == null) Store = gameObject.AddComponent<RoadNetworkStore>();
            if (Store != null && Store.GridVisualizer == null) Store.GridVisualizer = GridVisualizer;
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
            previewCells = RoadPlacementInput.GetLineCells(dragStart, hover);
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
            int total = RoadPlacementInput.CountNewCells(previewCells, Store.IsOwnRoadCell) * Store.CellCost();
            if (total > 0 && TreasuryView != null && TreasuryView.Treasury != null)
            {
                Treasury.TransactionResult spent = TreasuryView.Treasury.TrySpend(total, "Road");
                if (!spent.Success) return;
            }
            PlacePreviewed();
        }

        // Раскладка превью в стор: свои клетки — пропуск, вид и занятость обновляются.
        public void PlacePreviewed()
        {
            int placed = 0;
            for (int i = 0; i < previewCells.Count; i++)
            {
                if (!Store.IsOwnRoadCell(previewCells[i])) { Store.Place(previewCells[i]); placed++; }
            }
            Store.RefreshView();
            GridVisualizer.RefreshOccupied();
            if (placed > 0 && Save != null) Save.MarkDirty();
        }

        // Снос ПКМ: поиск в сторе, снос и обновление вида.
        public bool TryDemolish(GridPosition cell)
        {
            RoadState state = Store.FindStateAt(cell);
            if (state == null || !Store.DemolishState(state)) return false;
            Store.RefreshView();
            if (Save != null) Save.MarkDirty();
            return true;
        }

        // Построить линию клеток: для тестов и программного ввода.
        public bool PlaceLine(GridPosition from, GridPosition to)
        {
            if (!EnsureReady()) return false;
            dragStart = from;
            UpdatePreview(to);
            if (!CheckCells()) { CancelDrag(); return false; }
            CommitDrag();
            return true;
        }

        private bool CheckCells()
        {
            Treasury treasury = TreasuryView != null ? TreasuryView.Treasury : null;
            return RoadPlacementInput.CanCommit(previewCells, Store.CanPlace, Store.IsOwnRoadCell, Store.CellCost(), treasury);
        }
    }
}
