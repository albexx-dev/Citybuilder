using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Логическая сетка, ховер клетки и подсветка радиуса службы (EPIC-12 шаг 3).
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Grid")]
        [Tooltip("Клеток по X.")] public int Width = 50;
        [Tooltip("Клеток по Y.")] public int Height = 50;
        [Tooltip("Размер клетки в метрах.")] public float CellSize = 1f;

        [Header("Colors")]
        public Color LineColor = Color.white;
        public Color HoverColor = new Color(1f, 1f, 0f, 0.4f);
        public Color OccupiedColor = new Color(1f, 0f, 0f, 0.4f);

        [Header("References")]
        [Tooltip("Строитель мешей. Пусто = создастся на этом же объекте.")]
        public GridMeshBuilder Builder;

        private GridSystem grid;

        public GridSystem Grid
        {
            get
            {
                if (grid == null) grid = new GridSystem(new GridSize(Width, Height), CellSize);
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

        public void RefreshOccupied()
        {
            EnsureBuilder();
            if (Builder == null) return;
            Builder.RebuildOccupied();
        }

        public void ShowRadiusOverlay(GridPosition center, int radius, ServiceKind kind)
        {
            EnsureBuilder();
            if (Builder == null) return;
            List<GridPosition> cells = BuildingInfoQuery.GetRadiusCells(center, radius, Grid.Size);
            Builder.RebuildRadius(cells, ColorForService(kind));
        }

        public void ClearRadiusOverlay()
        {
            EnsureBuilder();
            if (Builder == null) return;
            Builder.RebuildRadius(null, Color.clear);
        }

        public static Color ColorForService(ServiceKind kind)
        {
            switch (kind)
            {
                case ServiceKind.Fire: return new Color(1f, 0.45f, 0.1f, 0.3f);
                case ServiceKind.Police: return new Color(0.2f, 0.55f, 1f, 0.3f);
                case ServiceKind.Health: return new Color(0.2f, 0.9f, 0.7f, 0.3f);
                case ServiceKind.Leisure: return new Color(0.3f, 0.9f, 0.3f, 0.3f);
                default: return new Color(0.8f, 0.8f, 0.8f, 0.25f);
            }
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
