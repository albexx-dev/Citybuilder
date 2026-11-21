using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Оверлей переполнения (issue #94): зелёный бак над домом с мусором ≥ порога.
    // Читает снимок через GarbageTruckView; после вывоза, сноса и загрузки исчезает
    // сам (опрос каждый кадр). Один куб = 12 треугольников (лимит ADR-010 — 300).
    public class GarbageOverlayView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Вид мусоровозов: снимок GarbageSystem и наличие полигона. Пусто = ищется в сцене.")]
        public GarbageTruckView Trucks;
        [Tooltip("Рост поселений: дома и их клетки. Пусто = ищется в сцене.")]
        public SettlementGrowthController Growth;

        [Header("View")]
        [Tooltip("Высота бака над землёй, метры.")] public float HeightAboveGround = 2.8f;
        [Tooltip("Сторона бака, метры.")] public float BinSize = 0.45f;

        private readonly Dictionary<EntityId, GameObject> bins = new Dictionary<EntityId, GameObject>();
        private readonly List<EntityId> needed = new List<EntityId>();
        private Material binMaterial;

        // Самоподключение без правки сцены: вид мусоровозов вызывает один раз.
        public static void Ensure(GarbageTruckView trucks)
        {
            if (trucks == null || FindFirstObjectByType<GarbageOverlayView>() != null) return;
            GarbageOverlayView overlay = trucks.gameObject.AddComponent<GarbageOverlayView>();
            overlay.Trucks = trucks;
            overlay.Growth = trucks.Economy != null ? trucks.Economy.Growth : null;
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<EntityId, GameObject> pair in bins) DestroySafe(pair.Value);
            bins.Clear();
            DestroySafe(binMaterial);
        }

        private void Update()
        {
            if (!EnsureReady()) return;
            RefreshNow();
        }

        // Пересобрать оверлей по текущему снимку: снос и загрузка убирают дома/мусор.
        public void RefreshNow()
        {
            needed.Clear();
            List<BuildingState> homes = Growth.Homes;
            if (homes == null) { HideGone(); return; }
            bool depot = Trucks.HasDepot;
            GarbageSystem garbage = Trucks.Garbage;
            for (int i = 0; i < homes.Count; i++)
            {
                BuildingState home = homes[i];
                if (home == null || home.Definition == null) continue;
                int acc = garbage != null ? garbage.AccOf(home.Id) : 0;
                if (!GarbageOverlayPolicy.NeedsOverlay(acc, depot)) continue;
                needed.Add(home.Id);
                ShowBin(home, acc);
            }
            HideGone();
        }

        private bool EnsureReady()
        {
            if (Trucks == null) Trucks = FindFirstObjectByType<GarbageTruckView>();
            if (Growth == null) Growth = FindFirstObjectByType<SettlementGrowthController>();
            if (Trucks == null || Growth == null || Growth.GridVisualizer == null) return false;
            if (binMaterial == null)
                binMaterial = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.3f, 0.7f, 0.3f) };
            return true;
        }

        private void ShowBin(BuildingState home, int acc)
        {
            GameObject bin;
            if (!bins.TryGetValue(home.Id, out bin))
            {
                bin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bin.name = "GarbageBin";
                DestroySafe(bin.GetComponent<Collider>());
                bin.GetComponent<MeshRenderer>().sharedMaterial = binMaterial;
                bin.transform.SetParent(transform, false);
                bins.Add(home.Id, bin);
            }
            float fill = GarbageOverlayPolicy.Load01(acc);
            bin.transform.localScale = new Vector3(BinSize, BinSize * (0.6f + 0.8f * fill), BinSize);
            bin.transform.position = BinAt(home);
        }

        private Vector3 BinAt(BuildingState home)
        {
            float cell = Growth.GridVisualizer.Grid.CellSize;
            float x = (home.Origin.X + home.Definition.Footprint.x * 0.5f) * cell;
            float z = (home.Origin.Y + home.Definition.Footprint.y * 0.5f) * cell;
            return new Vector3(x, HeightAboveGround, z);
        }

        private void HideGone()
        {
            List<EntityId> gone = null;
            foreach (KeyValuePair<EntityId, GameObject> pair in bins)
            {
                if (needed.Contains(pair.Key)) continue;
                if (gone == null) gone = new List<EntityId>();
                gone.Add(pair.Key);
            }
            if (gone == null) return;
            for (int i = 0; i < gone.Count; i++)
            {
                DestroySafe(bins[gone[i]]);
                bins.Remove(gone[i]);
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
