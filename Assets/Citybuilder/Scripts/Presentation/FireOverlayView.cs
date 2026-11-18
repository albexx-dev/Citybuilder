using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Оверлей огня (EPIC-43, ADR-009): простая оранжевая точка-пламя над горящим домом.
    // Только презентация по снимку FireSystem; пожары не сериализуются — после загрузки пусто.
    public class FireOverlayView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Экономика: даёт снимок пожаров Fires. Пусто = ищется в сцене.")] public EconomyTicker Economy;
        [Tooltip("Рост поселений: даёт дома для позиций пламени. Пусто = ищется в сцене.")] public SettlementGrowthController Growth;

        [Header("View")]
        [Tooltip("Пауза между обновлениями пламени в секундах. Минимум 0.25.")] public float RefreshIntervalSec = 0.5f;
        [Tooltip("Высота пламени над землёй в метрах.")] public float HeightAboveGround = 2.4f;

        private readonly Dictionary<EntityId, GameObject> flames = new Dictionary<EntityId, GameObject>();
        private Material flameMaterial;
        private float timer;

        private void Update()
        {
            if (!EnsureReady()) return;
            timer += Time.deltaTime;
            if (timer < RefreshIntervalSec) return;
            timer = 0f;
            RefreshNow();
        }

        // Пересобрать пламя по снимку: горящие — точка над домом, погасшие и сгоревшие — убрать.
        public void RefreshNow()
        {
            List<EntityId> burning = Economy.Fires.SnapshotBurning();
            for (int i = 0; i < burning.Count; i++) ShowFlame(burning[i]);
            HideGone(burning);
        }

        private bool EnsureReady()
        {
            if (Economy == null) Economy = FindFirstObjectByType<EconomyTicker>();
            if (Growth == null) Growth = FindFirstObjectByType<SettlementGrowthController>();
            if (Economy == null || Growth == null || Growth.GridVisualizer == null) return false;
            if (flameMaterial == null) flameMaterial = new Material(Shader.Find("Unlit/Color")) { color = new Color(1f, 0.45f, 0.08f) };
            return true;
        }

        private void ShowFlame(EntityId id)
        {
            if (flames.ContainsKey(id)) return;
            BuildingState home = FindHome(id);
            if (home == null || home.Definition == null) return;
            GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DestroySafe(flame.GetComponent<Collider>());
            flame.GetComponent<MeshRenderer>().sharedMaterial = flameMaterial;
            flame.transform.SetParent(transform, false);
            flame.transform.position = FlameAt(home);
            flames[id] = flame;
        }

        private Vector3 FlameAt(BuildingState home)
        {
            float cellSize = Growth.GridVisualizer.Grid.CellSize;
            float x = (home.Origin.X + home.Definition.Footprint.x * 0.5f) * cellSize;
            float z = (home.Origin.Y + home.Definition.Footprint.y * 0.5f) * cellSize;
            return new Vector3(x, HeightAboveGround, z);
        }

        private BuildingState FindHome(EntityId id)
        {
            List<BuildingState> homes = Growth.Homes;
            for (int i = 0; i < homes.Count; i++)
            {
                if (homes[i] != null && homes[i].Id.Equals(id)) return homes[i];
            }
            return null;
        }

        private void HideGone(List<EntityId> burning)
        {
            List<EntityId> gone = new List<EntityId>();
            foreach (KeyValuePair<EntityId, GameObject> pair in flames)
            {
                if (!burning.Contains(pair.Key)) gone.Add(pair.Key);
            }
            for (int i = 0; i < gone.Count; i++)
            {
                DestroySafe(flames[gone[i]]);
                flames.Remove(gone[i]);
            }
        }

        private static void DestroySafe(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }
    }
}
