using UnityEngine;

namespace Citybuilder
{
    // Презентация зданий: вид, призрак и материал. Все габариты — из BuildingDefinition.
    public static class BuildingViewFactory
    {
        // Построенный вид: префаб из типа или куб по габариту.
        public static GameObject CreateView(BuildingState state, GridVisualizer grid)
        {
            if (state == null || state.Definition == null || grid == null)
            {
                return null;
            }
            BuildingDefinition def = state.Definition;
            Vector3 position = FootprintCenter(def, state.Origin, 0f, grid);
            if (def.Prefab != null)
            {
                return Object.Instantiate(def.Prefab, position, Quaternion.identity);
            }
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = FootprintScale(def, grid);
            cube.transform.position = FootprintCenter(def, state.Origin, 0.5f, grid);
            return cube;
        }

        // Новый призрак под текущий тип: старый габарит не переносится.
        public static GameObject CreateGhost(BuildingDefinition def, GridVisualizer grid, Transform parent)
        {
            if (def == null || grid == null)
            {
                return null;
            }
            GameObject ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ghost.name = "Ghost";
            ghost.transform.SetParent(parent, false);
            ghost.transform.localScale = FootprintScale(def, grid);
            Collider collider = ghost.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }
            return ghost;
        }

        // Полупрозрачный материал призрака на Unlit/Color.
        public static Material CreateGhostMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Unlit/Color"));
            material.color = color;
            return material;
        }

        // Центр прямоугольника здания с учётом размера клетки.
        public static Vector3 FootprintCenter(BuildingDefinition def, GridPosition origin, float height, GridVisualizer grid)
        {
            float worldX, worldZ;
            float cellSize = grid.Grid.CellSize;
            grid.Grid.GridToWorld(origin, out worldX, out worldZ);
            float centerX = worldX + (def.Footprint.x - 1) * cellSize * 0.5f;
            float centerZ = worldZ + (def.Footprint.y - 1) * cellSize * 0.5f;
            return new Vector3(centerX, height, centerZ);
        }

        private static Vector3 FootprintScale(BuildingDefinition def, GridVisualizer grid)
        {
            float cellSize = grid.Grid.CellSize;
            return new Vector3(def.Footprint.x * cellSize, 1f, def.Footprint.y * cellSize);
        }
    }
}
