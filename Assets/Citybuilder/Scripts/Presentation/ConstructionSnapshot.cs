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

        // Тесты и «Новая игра»: реестр не должен хранить площадки старого города.
        public static void Clear()
        {
            sites.Clear();
        }
    }
}
