using UnityEngine;

namespace Citybuilder
{
    // Дороги drag-отрезками: ЛКМ тянет, релиз строит, ПКМ сносит. Только старый Input.
    // Состояние drag и превью вынесены в RoadDragState, геометрия отрезка — в RoadPlacementInput (задача 183).
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

        private readonly RoadDragState drag = new RoadDragState();

        private void Start()
        {
            EnsureReady();
        }

        // Сброс drag при выключении: иначе висящее превью/коммит.
        private void OnDisable()
        {
            drag.Reset();
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
            if (!placementActive || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelDrag();
                return;
            }
            GridPosition cell;
            bool hovered = GridVisualizer.TryGetHoveredCell(out cell);
            bool overMenu = UiClickGuard.IsOverMenu(Input.mousePosition);
            if (drag.TryBegin(Input.GetMouseButtonDown(0), hovered, overMenu, cell)) ShowPreview();
            else if (drag.TryPreview(Input.GetMouseButton(0), hovered, cell)) ShowPreview();
            if (drag.ShouldCommit(Input.GetMouseButtonUp(0))) CommitDrag();
            if (Input.GetMouseButtonDown(1) && hovered && !overMenu) TryDemolish(cell);
        }

        private void ShowPreview()
        {
            View.ShowPreview(drag.PreviewCells, GridVisualizer.Grid.CellSize, CheckCells());
        }

        private void CancelDrag()
        {
            drag.Reset();
            View.HidePreview();
        }

        // Коммит drag: drag гасим, превью оставляем до раскладки; оплата новых клеток — до PlacePreviewed.
        private void CommitDrag()
        {
            drag.Stop();
            View.HidePreview();
            if (!CheckCells()) return;
            int total = RoadPlacementInput.CountNewCells(drag.PreviewCells, Store.IsOwnRoadCell) * Store.CellCost();
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
            for (int i = 0; i < drag.PreviewCells.Count; i++)
            {
                if (!Store.IsOwnRoadCell(drag.PreviewCells[i])) { Store.Place(drag.PreviewCells[i]); placed++; }
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
            drag.BeginLine(from, to);
            ShowPreview();
            if (!CheckCells()) { CancelDrag(); return false; }
            CommitDrag();
            return true;
        }

        private bool CheckCells()
        {
            Treasury treasury = TreasuryView != null ? TreasuryView.Treasury : null;
            return RoadPlacementInput.CanCommit(drag.PreviewCells, Store.CanPlace, Store.IsOwnRoadCell, Store.CellCost(), treasury);
        }
    }
}
