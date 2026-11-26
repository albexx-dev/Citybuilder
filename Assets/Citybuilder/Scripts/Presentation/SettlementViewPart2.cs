using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // ADR-021 / issue #170: тёплые окна зданий ночью. Часть SettlementView: копии материалов
    // лежат в кэше по EntityId и убиваются владельцем вида, общие ассеты не трогаются.
    public partial class SettlementView
    {
        private readonly Dictionary<EntityId, Dictionary<Material, Material>> emissive =
            new Dictionary<EntityId, Dictionary<Material, Material>>();
        private readonly HashSet<EntityId> darkViews = new HashSet<EntityId>();
        private bool night;

        private void OnEnable()
        {
            night = BlackoutViewStyle.IsNightNow;
            if (night) ApplyNightToAll();
            BlackoutViewStyle.SampleChanged += OnSampleChanged;
        }

        private void OnDisable()
        {
            BlackoutViewStyle.SampleChanged -= OnSampleChanged;
            NightClear();
            night = false;
        }

        private void OnDestroy()
        {
            BlackoutViewStyle.SampleChanged -= OnSampleChanged;
            NightClear();
        }

        // Хук из AddBuilding: стройку и обесточенные виды не подсвечиваем.
        private void NightHook(GameObject view, EntityId id, bool powered, bool constructing)
        {
            if (!powered) { darkViews.Add(id); return; }
            if (night && !constructing) ApplyNight(view, id);
        }

        private void NightRemove(EntityId id)
        {
            darkViews.Remove(id);
            ClearEmissive(id);
        }

        // Очистка всех кэшей и старых Id: повтор вызова ничего не убивает и не трогает источники.
        private void NightClear()
        {
            foreach (EntityId id in new List<EntityId>(emissive.Keys)) ClearEmissive(id);
            darkViews.Clear();
        }

        private void OnSampleChanged(DayNightSample sample)
        {
            bool fresh = BlackoutViewStyle.IsNight(sample);
            if (night == fresh) return;
            night = fresh;
            if (night) ApplyNightToAll();
            else NightClear();
        }

        private void ApplyNightToAll()
        {
            foreach (KeyValuePair<EntityId, GameObject> entry in views)
            {
                if (!darkViews.Contains(entry.Key)) ApplyNight(entry.Value, entry.Key);
            }
        }

        private void ApplyNight(GameObject view, EntityId id)
        {
            if (view == null) return;
            Dictionary<Material, Material> cache;
            if (!emissive.TryGetValue(id, out cache)) emissive[id] = cache = new Dictionary<Material, Material>();
            BlackoutViewStyle.ApplyEmissive(view, BlackoutViewStyle.WarmWindows, cache);
        }

        private void ClearEmissive(EntityId id)
        {
            Dictionary<Material, Material> cache;
            if (!emissive.TryGetValue(id, out cache)) return;
            emissive.Remove(id);
            GameObject view;
            if (views.TryGetValue(id, out view)) RestoreShared(view, cache);
            BlackoutViewStyle.ClearEmissive(cache);
        }

        // Возврат рендерерам исходных материалов до убийства копий: вид живёт дальше днём.
        private static void RestoreShared(GameObject view, Dictionary<Material, Material> cache)
        {
            if (view == null || cache.Count == 0) return;
            MeshRenderer[] renderers = view.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Material shared = renderers[i].sharedMaterial;
                foreach (KeyValuePair<Material, Material> pair in cache)
                {
                    if (pair.Value == shared) { renderers[i].sharedMaterial = pair.Key; break; }
                }
            }
        }
    }
}
