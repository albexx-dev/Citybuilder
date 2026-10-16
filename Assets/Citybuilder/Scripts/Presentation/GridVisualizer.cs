using UnityEngine;

namespace Citybuilder
{
    // Логическая сетка и ховер клетки. Меши строит GridMeshBuilder на том же объекте.
    // Настройка сцены: пустой GameObject + этот компонент, камера сверху.
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Grid")]
        [Tooltip("Клеток по X.")]
        public int Width = 50;
        [Tooltip("Клеток по Y.")]
        public int Height = 50;
        [Tooltip("Размер клетки в метрах.")]
        public float CellSize = 1f;

        [Header("Colors")]
        public Color LineColor = Color.white;
        public Color HoverColor = new Color(1f, 1f, 0f, 0.4f);
        public Color OccupiedColor = new Color(1f, 0f, 0f, 0.4f);

        [Header("References")]
        [Tooltip("Строитель мешей. Пусто = создастся на этом же объекте.")]
        public GridMeshBuilder Builder;

        private GridSystem grid;

        // Ленивый доступ: после перезагрузки домена Unity поле пустеет,
        // сетка пересоздаётся из настроек Inspector. Занятость при этом сбрасывается (V1).
        public GridSystem Grid
        {
            get
            {
                if (grid == null)
                {
                    grid = new GridSystem(new GridSize(Width, Height), CellSize);
                }
                return grid;
            }
        }
        public GridPosition HoveredCell { get; private set; }
        public bool HasHoveredCell { get; private set; }

        public bool TryGetHoveredCell(out GridPosition cell)
        {
            cell = HoveredCell;
            return HasHoveredCell;
        }

        // Перестроить красный оверлей после Occupy/Release.
        public void RefreshOccupied()
        {
            EnsureBuilder();
            if (Builder == null) return;
            Builder.RebuildOccupied();
        }

        private void Awake()
        {
            EnsureBuilder();
            if (Builder == null) return;
            Builder.EnsureVisuals();
            RefreshOccupied();
        }

        private void Update()
        {
            EnsureBuilder();
            if (Builder == null) return;
            if (!Builder.HasVisuals) Builder.EnsureVisuals();
            UpdateHover();
        }

        // Добирает билдер: Inspector, свой объект, фолбэк-создание.
        private void EnsureBuilder()
        {
            if (Builder != null)
            {
                Builder.Visualizer = this;
                return;
            }
            Builder = GetComponent<GridMeshBuilder>();
            if (Builder == null) Builder = gameObject.AddComponent<GridMeshBuilder>();
            if (Builder != null) Builder.Visualizer = this;
        }

        private void UpdateHover()
        {
            GridPosition cell;
            if (!TryPickCell(out cell))
            {
                Builder.HideHover();
                return;
            }
            HasHoveredCell = true;
            HoveredCell = cell;
            float wx, wz;
            Grid.GridToWorld(cell, out wx, out wz);
            Builder.SetHover(wx, wz);
        }

        // Луч из камеры в плоскость сетки → клетка. Нет камеры/луча/клетки — false.
        private bool TryPickCell(out GridPosition cell)
        {
            cell = new GridPosition();
            HasHoveredCell = false;
            Camera camera = Camera.main;
            if (camera == null) return false;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            if (!plane.Raycast(ray, out distance)) return false;
            Vector3 point = ray.GetPoint(distance);
            cell = Grid.WorldToGrid(point.x, point.z);
            return Grid.IsInside(cell);
        }
    }
}
