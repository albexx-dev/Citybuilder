using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Виды выросших домов: детерминированные варианты (EPIC-17 шаг 2) и фаза стройки (EPIC-17 шаг 1).
    public class SettlementView : MonoBehaviour
    {
        [Header("Fallback")]
        [Tooltip("Цвет куба, когда у BuildingDefinition нет префаба.")]
        public Color FallbackColor = new Color(1f, 0.9f, 0.6f);

        private readonly Dictionary<EntityId, GameObject> views = new Dictionary<EntityId, GameObject>();

        // Ставит вид и запоминает по Id. При skipConstructionStage=true (сейв/лоад) здание сразу готовое.
        public GameObject AddBuilding(BuildingState state, BuildingDefinition def, float cellSize, bool skipConstructionStage = false)
        {
            if (state == null || def == null) return null;
            RemoveBuilding(state);
            GameObject go = SpawnView(state, def, cellSize, skipConstructionStage);
            if (go == null) return null;
            views[state.Id] = go;
            return go;
        }

        public void RemoveBuilding(BuildingState state)
        {
            if (state == null) return;
            GameObject go;
            if (!views.TryGetValue(state.Id, out go)) return;
            views.Remove(state.Id);
            DestroySafe(go);
        }

        public void Clear()
        {
            foreach (KeyValuePair<EntityId, GameObject> entry in views)
            {
                DestroySafe(entry.Value);
            }
            views.Clear();
        }

        private GameObject SpawnView(BuildingState state, BuildingDefinition def, float cellSize, bool skipConstructionStage)
        {
            Vector3 at = FootprintCenter(state.Origin, def, cellSize, 0f);
            if (def.Prefab != null)
            {
                return Instantiate(def.Prefab, at, Quaternion.identity, transform);
            }
            return BuildCube(state, def, cellSize, skipConstructionStage);
        }

        private GameObject BuildCube(BuildingState state, BuildingDefinition def, float cellSize, bool skipConstructionStage)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform, false);
            int variant = BuildingDefinition.SelectVariant(state.Id, def.VariantCount);
            float w = def.Footprint.x * cellSize * 0.9f;
            float d = def.Footprint.y * cellSize * 0.9f;
            float h = 1f + variant * 0.5f;
            Vector3 fullScale = new Vector3(w, h, d);
            Vector3 groundCenter = FootprintCenter(state.Origin, def, cellSize, 0f);
            cube.transform.localScale = fullScale;
            cube.transform.localPosition = new Vector3(groundCenter.x, h * 0.5f, groundCenter.z);
            DestroySafe(cube.GetComponent<Collider>());
            Color wallColor = WallColor(def, variant);
            MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = BuildMaterial(wallColor);
            GameObject roof = BuildRoof(cube, h, variant);
            if (!skipConstructionStage)
            {
                GrowthStageView stage = cube.AddComponent<GrowthStageView>();
                stage.BeginConstruction(renderer, roof, fullScale, groundCenter, wallColor);
            }
            return cube;
        }

        private GameObject BuildRoof(GameObject parent, float h, int variant)
        {
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(parent.transform, false);
            roof.transform.localScale = new Vector3(0.7f, 0.3f / h, 0.7f);
            roof.transform.localPosition = new Vector3(0f, 0.5f + 0.15f / h, 0f);
            DestroySafe(roof.GetComponent<Collider>());
            float tint = variant * 0.08f;
            roof.GetComponent<MeshRenderer>().sharedMaterial = BuildMaterial(new Color(0.45f + tint, 0.25f + tint * 0.5f, 0.1f));
            return roof;
        }

        private Vector3 FootprintCenter(GridPosition origin, BuildingDefinition def, float cellSize, float height)
        {
            float baseX = (origin.X + 0.5f) * cellSize;
            float baseZ = (origin.Y + 0.5f) * cellSize;
            float centerX = baseX + (def.Footprint.x - 1) * cellSize * 0.5f;
            float centerZ = baseZ + (def.Footprint.y - 1) * cellSize * 0.5f;
            return new Vector3(centerX, height, centerZ);
        }

        private static Material BuildMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Unlit/Color"));
            material.color = color;
            return material;
        }

        private static void DestroySafe(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        private Color WallColor(BuildingDefinition def, int variant)
        {
            float shift = (variant - 1) * 0.04f;
            if (def == null) return FallbackColor;
            if (def.AllowedZone == ZoneType.Commercial) return new Color(0.6f + shift, 0.75f + shift, 0.9f);
            if (def.AllowedZone == ZoneType.Industrial) return new Color(0.55f + shift, 0.5f + shift, 0.45f);
            return new Color(Mathf.Clamp01(FallbackColor.r + shift), Mathf.Clamp01(FallbackColor.g + shift), FallbackColor.b);
        }
    }
}
