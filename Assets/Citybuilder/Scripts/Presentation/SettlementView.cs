using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Виды выросших домов: префаб или бежевый куб. Только показ, без логики.
    // Настройка сцены: компонент рядом с SettlementGrowthController.
    public class SettlementView : MonoBehaviour
    {
        [Header("Fallback")]
        [Tooltip("Цвет куба, когда у BuildingDefinition нет префаба.")]
        public Color FallbackColor = new Color(1f, 0.9f, 0.6f);

        private readonly Dictionary<EntityId, GameObject> views = new Dictionary<EntityId, GameObject>();

        // Ставит вид и запоминает по Id. Без префаба — куб Footprint x 1 x Footprint.
        public GameObject AddBuilding(BuildingState state, BuildingDefinition def, float cellSize)
        {
            if (state == null || def == null)
            {
                return null;
            }
            RemoveBuilding(state);
            GameObject go = SpawnView(state, def, cellSize);
            if (go == null)
            {
                return null;
            }
            views[state.Id] = go;
            return go;
        }

        // Убирает вид по Id. Нет вида — ничего не делает.
        public void RemoveBuilding(BuildingState state)
        {
            if (state == null)
            {
                return;
            }
            GameObject go;
            if (!views.TryGetValue(state.Id, out go))
            {
                return;
            }
            views.Remove(state.Id);
            if (go != null)
            {
                Destroy(go);
            }
        }

        // Сносит все виды.
        public void Clear()
        {
            foreach (KeyValuePair<EntityId, GameObject> entry in views)
            {
                if (entry.Value != null)
                {
                    Destroy(entry.Value);
                }
            }
            views.Clear();
        }

        private GameObject SpawnView(BuildingState state, BuildingDefinition def, float cellSize)
        {
            Vector3 at = FootprintCenter(state.Origin, def, cellSize, 0f);
            if (def.Prefab != null)
            {
                return Instantiate(def.Prefab, at, Quaternion.identity, transform);
            }
            return BuildCube(state.Origin, def, cellSize);
        }

        private GameObject BuildCube(GridPosition origin, BuildingDefinition def, float cellSize)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform, false);
            float width = def.Footprint.x * cellSize;
            float depth = def.Footprint.y * cellSize;
            cube.transform.localScale = new Vector3(width, 1f, depth);
            cube.transform.localPosition = FootprintCenter(origin, def, cellSize, 0.5f);
            Destroy(cube.GetComponent<Collider>());
            cube.GetComponent<MeshRenderer>().material = BuildMaterial();
            return cube;
        }

        private Vector3 FootprintCenter(GridPosition origin, BuildingDefinition def, float cellSize, float height)
        {
            float baseX = (origin.X + 0.5f) * cellSize;
            float baseZ = (origin.Y + 0.5f) * cellSize;
            float centerX = baseX + (def.Footprint.x - 1) * cellSize * 0.5f;
            float centerZ = baseZ + (def.Footprint.y - 1) * cellSize * 0.5f;
            return new Vector3(centerX, height, centerZ);
        }

        private Material BuildMaterial()
        {
            Material material = new Material(Shader.Find("Unlit/Color"));
            material.color = FallbackColor;
            return material;
        }
    }
}
