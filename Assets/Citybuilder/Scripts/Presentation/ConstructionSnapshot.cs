using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Реестр активных стройплощадок и снимок v6 (ADR-014). Пожары и агенты не входят в слот.
    public static class ConstructionSnapshot
    {
        private static readonly List<ConstructionSiteView> sites = new List<ConstructionSiteView>();

        public static void Register(ConstructionSiteView site)
        {
            if (site != null && !sites.Contains(site)) sites.Add(site);
        }

        public static void Unregister(ConstructionSiteView site)
        {
            if (site != null) sites.Remove(site);
        }

        // Только незавершённые площадки с известным типом: готовые здания в слот не пишутся.
        public static List<CitySnapshot.ConstructionProgressEntry> Collect()
        {
            List<CitySnapshot.ConstructionProgressEntry> entries = new List<CitySnapshot.ConstructionProgressEntry>();
            for (int i = 0; i < sites.Count; i++)
            {
                ConstructionSiteView site = sites[i];
                if (site == null || !site.IsConstructing || string.IsNullOrEmpty(site.DefId)) continue;
                entries.Add(new CitySnapshot.ConstructionProgressEntry
                {
                    X = site.Origin.X, Y = site.Origin.Y, DefId = site.DefId, TicksElapsed = site.TicksElapsed
                });
            }
            return entries;
        }

        // Площадка с накопленными тиками ставится на место скрытого готового вида.
        public static ConstructionSiteView Restore(Transform parent, CitySnapshot.ConstructionProgressEntry entry,
            BuildingDefinition def, float cellSize, Action onCompleted)
        {
            if (parent == null || def == null || entry == null) return null;
            float cell = cellSize > 0f ? cellSize : 1f;
            Vector3 center = new Vector3(
                (entry.X + 0.5f + (def.Footprint.x - 1) * 0.5f) * cell, 0f,
                (entry.Y + 0.5f + (def.Footprint.y - 1) * 0.5f) * cell);
            ConstructionSiteView site = ConstructionSiteView.Spawn(parent, center, def, cell,
                ConstructionSiteView.TargetScale(def, cell), null, onCompleted);
            site.SetElapsed(entry.TicksElapsed);
            return site;
        }

        // Загрузка v6: вернуть незавершённые площадки, спрятав готовый вид до завершения.
        public static void RestoreAll(CitySnapshot snapshot, SettlementGrowthController growth,
            GridVisualizer grid, BuildingPlacementController placement)
        {
            if (grid == null) return;
            List<CitySnapshot.ConstructionProgressEntry> entries = SaveV6Snapshot.ReadSites(snapshot);
            for (int i = 0; i < entries.Count; i++)
            {
                CitySnapshot.ConstructionProgressEntry entry = entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.DefId)) continue;
                BuildingDefinition def = FindDefinition(entry.DefId, growth, placement);
                if (def == null || entry.TicksElapsed < 0 || entry.TicksElapsed >= def.ConstructionTicks) continue;
                RestoreOne(entry, def, growth, grid);
            }
        }

        private static void RestoreOne(CitySnapshot.ConstructionProgressEntry entry, BuildingDefinition def,
            SettlementGrowthController growth, GridVisualizer grid)
        {
            GridPosition cell = new GridPosition(entry.X, entry.Y);
            bool grown = growth != null && growth.FindStateAt(cell) != null;
            Transform parent = grown && growth.View != null ? growth.View.transform : grid.transform;
            float cellSize = grid.Grid != null ? grid.Grid.CellSize : 1f;
            GameObject ready = FindViewAt(parent, entry, def, cellSize);
            if (ready == null) return;
            ready.SetActive(false);
            Restore(parent, entry, def, cellSize, delegate { ready.SetActive(true); });
        }

        // Вид ручной стройки корневой: ищем и детей родителя, и корневые объекты по мировой позиции.
        private static GameObject FindViewAt(Transform parent, CitySnapshot.ConstructionProgressEntry entry,
            BuildingDefinition def, float cellSize)
        {
            float x = (entry.X + 0.5f + (def.Footprint.x - 1) * 0.5f) * cellSize;
            float z = (entry.Y + 0.5f + (def.Footprint.y - 1) * 0.5f) * cellSize;
            Vector3 center = new Vector3(x, 0f, z);
            GameObject found = FindChildAt(parent, center);
            return found != null ? found : FindRootAt(center);
        }

        private static GameObject FindChildAt(Transform parent, Vector3 center)
        {
            if (parent == null) return null;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (Matches(child.position, center)) return child.gameObject;
            }
            return null;
        }

        private static GameObject FindRootAt(Vector3 center)
        {
            GameObject[] roots = UnityEngine.Object.FindObjectsByType<GameObject>(UnityEngine.FindObjectsSortMode.None);
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject candidate = roots[i];
                if (candidate.transform.parent == null && Matches(candidate.transform.position, center)
                    && candidate.GetComponent<ConstructionSiteView>() == null) return candidate;
            }
            return null;
        }

        private static bool Matches(Vector3 position, Vector3 center)
        {
            return Mathf.Abs(position.x - center.x) < 0.01f && Mathf.Abs(position.z - center.z) < 0.01f;
        }

        private static BuildingDefinition FindDefinition(string defId, SettlementGrowthController growth,
            BuildingPlacementController placement)
        {
            if (growth != null && growth.Definitions != null)
                for (int i = 0; i < growth.Definitions.Count; i++)
                    if (growth.Definitions[i] != null && growth.Definitions[i].Id == defId) return growth.Definitions[i];
            if (placement == null) return null;
            if (placement.Definition != null && placement.Definition.Id == defId) return placement.Definition;
            if (placement.Definitions == null) return null;
            for (int i = 0; i < placement.Definitions.Count; i++)
                if (placement.Definitions[i] != null && placement.Definitions[i].Id == defId) return placement.Definitions[i];
            return null;
        }

        // Тесты и «Новая игра»: реестр не должен хранить площадки старого города.
        public static void Clear()
        {
            sites.Clear();
        }
    }
}
