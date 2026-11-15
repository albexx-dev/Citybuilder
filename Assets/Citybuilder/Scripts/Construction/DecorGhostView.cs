using UnityEngine;

namespace Citybuilder
{
    // Призрак декора (по образцу PlacementGhostView): куб клетки под курсором, зелёный/красный.
    public class DecorGhostView
    {
        private GameObject ghost;
        private Material validMaterial;
        private Material invalidMaterial;
        private GridVisualizer grid;

        // Первый вызов создаёт материалы и призрак; дальше только обновляет ссылки.
        public void Ensure(GridVisualizer visualizer, Transform owner)
        {
            grid = visualizer;
            if (validMaterial == null || invalidMaterial == null)
            {
                validMaterial = BuildingViewFactory.CreateGhostMaterial(new Color(0f, 1f, 0f, 0.4f));
                invalidMaterial = BuildingViewFactory.CreateGhostMaterial(new Color(1f, 0f, 0f, 0.4f));
            }
            if (ghost == null) Rebuild(owner);
        }

        // Прячем призрак при выключении: иначе висящий куб.
        public void Hide()
        {
            if (ghost != null) ghost.SetActive(false);
        }

        // Позиция в центре клетки и цвет по доступности клетки.
        public void Show(GridPosition cell, bool canPlace)
        {
            if (ghost == null || grid == null) return;
            float worldX, worldZ;
            grid.Grid.GridToWorld(cell, out worldX, out worldZ);
            ghost.transform.position = new Vector3(worldX, 0.5f, worldZ);
            MeshRenderer renderer = ghost.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = canPlace ? validMaterial : invalidMaterial;
            ghost.SetActive(true);
        }

        private void Rebuild(Transform owner)
        {
            float cellSize = grid.Grid.CellSize;
            ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ghost.name = "DecorGhost";
            ghost.transform.localScale = new Vector3(cellSize, 1f, cellSize);
            ghost.transform.SetParent(owner, false);
            Collider collider = ghost.GetComponent<Collider>();
            if (collider != null) BuildingViewFactory.DestroySafe(collider);
            ghost.SetActive(false);
        }
    }
}
