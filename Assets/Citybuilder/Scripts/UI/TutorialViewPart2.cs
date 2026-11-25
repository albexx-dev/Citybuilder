using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса TutorialView (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class TutorialView : MonoBehaviour
    {
        private bool IsParkAvailable(int grown)
        {
            if (Placement == null || Placement.Definitions == null) return false;
            foreach (BuildingDefinition d in Placement.Definitions)
            {
                if (d != null && d.Id == "park") return UnlockPolicy.IsUnlocked(d, grown);
            }
            return false;
        }

        private void ResolveRefs()
        {
            if (Zones == null) Zones = GetComponent<ZonePaintingController>();
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Menu == null) Menu = GetComponent<BuildModeMenu>();
            if (Placement == null) Placement = GetComponent<BuildingPlacementController>();
            if (Palette == null) Palette = GetComponent<BuildPaletteView>();
            if (Economy == null) Economy = FindFirstObjectByType<EconomyTicker>();
            if (Quests == null) Quests = FindFirstObjectByType<QuestLogView>();
            if (Log == null) Log = FindFirstObjectByType<EventLogView>();
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
        }

        private int CountResidential()
        {
            ZoneSystem zones = Zones != null ? Zones.Zones : null;
            if (zones == null) return 0;
            List<KeyValuePair<GridPosition, ZoneType>> cells = zones.GetZonedCells();
            int count = 0;
            foreach (KeyValuePair<GridPosition, ZoneType> cell in cells)
            {
                if (cell.Value == ZoneType.Residential) count++;
            }
            return count;
        }

        // Ид выбранного в палитре типа (Placement.Definition), null если ничего не выбрано.
        private string SelectedId()
        {
            ResolveRefs();
            return Placement != null && Placement.Definition != null ? Placement.Definition.Id : null;
        }

        private static int IndexOfId(List<BuildingDefinition> definitions, string id)
        {
            for (int i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] != null && definitions[i].Id == id) return i;
            }
            return -1;
        }
    }
}
