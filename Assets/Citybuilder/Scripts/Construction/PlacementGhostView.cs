using UnityEngine;

namespace Citybuilder
{
    // Призрак постройки: куб-превью под курсором, зелёный/красный материал, скрытие.
    public class PlacementGhostView
    {
        private GameObject ghost;
        private Material validMaterial;
        private Material invalidMaterial;
        private BuildingDefinition definition;
        private GridVisualizer grid;
        private Transform parent;

        // Первый вызов создаёт материалы и призрак; дальше только обновляет ссылки.
        public void Ensure(BuildingDefinition def, GridVisualizer visualizer, Transform owner)
        {
            definition = def;
            grid = visualizer;
            parent = owner;
            if (validMaterial == null || invalidMaterial == null)
            {
                validMaterial = BuildingViewFactory.CreateGhostMaterial(new Color(0f, 1f, 0f, 0.4f));
                invalidMaterial = BuildingViewFactory.CreateGhostMaterial(new Color(1f, 0f, 0f, 0.4f));
            }
            if (ghost == null) Rebuild();
        }

        // Сменить тип: старый габарит не переносится.
        public void SetDefinition(BuildingDefinition def)
        {
            if (def == null || ghost == null) return;
            definition = def;
            Rebuild();
        }

        public void Hide()
        {
            if (ghost != null) ghost.SetActive(false);
        }

        // Позиция по клетке и цвет по доступности.
        public void Show(GridPosition cell, bool canPlace)
        {
            if (ghost == null || definition == null || grid == null) return;
            ghost.transform.position = BuildingViewFactory.FootprintCenter(definition, cell, 0.5f, grid);
            MeshRenderer renderer = ghost.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = canPlace ? validMaterial : invalidMaterial;
            ghost.SetActive(true);
        }

        private void Rebuild()
        {
            if (ghost != null)
            {
                ghost.SetActive(false);
                Object.Destroy(ghost);
            }
            ghost = BuildingViewFactory.CreateGhost(definition, grid, parent);
            if (ghost != null) ghost.SetActive(false);
        }
    }
}
