using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Виды зданий: детерминированные варианты кубов (EPIC-17), стройфаза всех типов (EPIC-61).
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
            foreach (KeyValuePair<EntityId, GameObject> entry in views) DestroySafe(entry.Value);
            views.Clear();
        }

        private GameObject SpawnView(BuildingState state, BuildingDefinition def, float cellSize, bool skipConstructionStage)
        {
            Vector3 at = FootprintCenter(state.Origin, def, cellSize, 0f);
            bool constructing = !skipConstructionStage && def.ConstructionTicks > 0;
            if (def.Prefab != null)
            {
                if (constructing)
                {
                    return ConstructionSiteView.Spawn(transform, at, def, cellSize,
                        ConstructionSiteView.TargetScale(def, cellSize), null,
                        delegate { AddBuilding(state, def, cellSize, true); });
                }
                GameObject prefab = Instantiate(def.Prefab, at, Quaternion.identity, transform);
                float scale = LevelScale(state.Level, false);
                prefab.transform.localScale = Vector3.Scale(def.Prefab.transform.localScale, new Vector3(scale, scale, scale));
                return prefab;
            }
            return BuildCube(state, def, cellSize, constructing);
        }

        private GameObject BuildCube(BuildingState state, BuildingDefinition def, float cellSize, bool constructing)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform, false);
            int variant = BuildingDefinition.SelectVariant(state.Id, def.VariantCount);
            float w = def.Footprint.x * cellSize * 0.9f;
            float d = def.Footprint.y * cellSize * 0.9f;
            float h = (1f + variant * 0.5f) * LevelScale(state.Level, true);
            Vector3 fullScale = new Vector3(w, h, d);
            Vector3 groundCenter = FootprintCenter(state.Origin, def, cellSize, 0f);
            cube.transform.localScale = fullScale;
            cube.transform.localPosition = new Vector3(groundCenter.x, h * 0.5f, groundCenter.z);
            DestroySafe(cube.GetComponent<Collider>());
            Color wallColor = WallColor(def, variant);
            MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = BuildMaterial(wallColor);
            GameObject roof = BuildRoof(cube, h, variant, state.Level);
            if (constructing)
            {
                renderer.sharedMaterial.color = new Color(0.5f, 0.5f, 0.5f);
                roof.SetActive(false);
                cube.AddComponent<ConstructionSiteView>().Begin(cube.transform, fullScale, groundCenter,
                    def.Footprint, cellSize, def.ConstructionTicks,
                    delegate { roof.SetActive(true); renderer.sharedMaterial.color = wallColor; });
            }
            return cube;
        }

        private GameObject BuildRoof(GameObject parent, float h, int variant, int level)
        {
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(parent.transform, false);
            roof.transform.localScale = new Vector3(0.7f, 0.3f / h, 0.7f);
            roof.transform.localPosition = new Vector3(0f, 0.5f + 0.15f / h, 0f);
            DestroySafe(roof.GetComponent<Collider>());
            roof.GetComponent<MeshRenderer>().sharedMaterial = BuildMaterial(RoofColor(level, variant));
            return roof;
        }

        // Множитель вида по уровню дома (EPIC-59): высота куба ×1.5/×2, масштаб префаба ×1.25/×1.5.
        private static float LevelScale(int level, bool height)
        {
            if (level >= 3) return height ? 2f : 1.5f;
            if (level >= 2) return height ? 1.5f : 1.25f;
            return 1f;
        }

        // Цвет крыши по уровню: 1 — коричневый базовый с оттенком варианта, 2 — терракота, 3 — синяя черепица.
        private static Color RoofColor(int level, int variant)
        {
            float tint = variant * 0.08f;
            if (level >= 3) return new Color(0.2f, 0.35f, 0.6f);
            if (level >= 2) return new Color(0.75f, 0.3f + tint * 0.5f, 0.2f);
            return new Color(0.45f + tint, 0.25f + tint * 0.5f, 0.1f);
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
            return new Material(Shader.Find("Unlit/Color")) { color = color };
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
