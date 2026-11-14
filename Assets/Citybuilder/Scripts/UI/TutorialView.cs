using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Многошаговый туториал v2 с подсветкой кнопок нижней панели и кнопкой «Понятно» (EPIC-18 шаг 2).
    // Заменяет StartHintView на объекте Grid.
    public class TutorialView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Кисть зон. Пусто = ищется на этом же объекте.")] public ZonePaintingController Zones;
        [Tooltip("Автовырастание домов. Пусто = ищется на этом же объекте.")] public SettlementGrowthController Growth;
        [Tooltip("Меню режимов. Пусто = ищется на этом же объекте.")] public BuildModeMenu Menu;
        [Tooltip("Ручная стройка зданий. Пусто = ищется на этом же объекте.")] public BuildingPlacementController Placement;

        private bool dismissed;

        public TutorialPolicy.Step CurrentStep { get { return PickStep(); } }
        public string CurrentText { get { return TutorialPolicy.FormatText(CurrentStep); } }
        public bool IsDismissed { get { return dismissed; } }

        public void Dismiss()
        {
            dismissed = true;
            UiClickGuard.Set("tutorial_dismiss", new Rect());
        }

        public void ResetTutorial()
        {
            dismissed = false;
        }

        public void OnDisable()
        {
            UiClickGuard.Set("tutorial_dismiss", new Rect());
        }

        private void OnGUI()
        {
            TutorialPolicy.Step step = CurrentStep;
            string text = TutorialPolicy.FormatText(step);
            if (string.IsNullOrEmpty(text))
            {
                UiClickGuard.Set("tutorial_dismiss", new Rect());
                return;
            }
            if (HudLayout.IsHudRowVisible(430f, 1, Screen.height))
            {
                float y = HudLayout.FixedOrBottom(430f, 1);
                HudText.Draw(10f, y, text);
                Rect btn = new Rect(460f, y - 2f, 90f, 24f);
                UiClickGuard.Set("tutorial_dismiss", btn);
                if (GUI.Button(btn, "Понятно")) Dismiss();
            }
            else
            {
                UiClickGuard.Set("tutorial_dismiss", new Rect());
            }
            DrawHighlight(TutorialPolicy.HighlightTarget(step));
        }

        private void DrawHighlight(TutorialPolicy.TargetButton target)
        {
            if (target == TutorialPolicy.TargetButton.None) return;
            Rect modeGroup = HudLayout.ModeGroupRect(Screen.width, Screen.height);
            int index = target == TutorialPolicy.TargetButton.Houses ? 1
                : target == TutorialPolicy.TargetButton.Zones ? 3
                : target == TutorialPolicy.TargetButton.Growth ? 4 : -1;
            if (index < 0) return;
            float x = modeGroup.x + index * (HudLayout.ModeButtonWidth + HudLayout.ButtonGap);
            Rect rect = new Rect(x - 2f, modeGroup.y - 2f, HudLayout.ModeButtonWidth + 4f, modeGroup.height + 4f);
            Color prev = GUI.color;
            GUI.color = new Color(1f, 0.85f, 0.1f, 0.9f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = prev;
        }

        private TutorialPolicy.Step PickStep()
        {
            ResolveRefs();
            int grown = Growth != null ? Growth.GrownCount : 0;
            TutorialPolicy.State s = new TutorialPolicy.State
            {
                ResidentialCells = CountResidential(),
                GrowthMode = Menu != null && Menu.CurrentMode == BuildModeMenu.Mode.Growth,
                GrownCount = grown,
                HasPowerStation = HasPlaced("power_station"),
                HasWaterTower = HasPlaced("water_tower"),
                HasWasteDepot = HasPlaced("waste_depot"),
                HasPark = HasPlaced("park"),
                ParkAvailable = IsParkAvailable(grown),
                Dismissed = dismissed
            };
            return TutorialPolicy.Pick(s);
        }

        private bool HasPlaced(string id)
        {
            if (Placement == null || Placement.Placed == null) return false;
            List<BuildingState> list = Placement.Placed;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].Definition != null && list[i].Definition.Id == id) return true;
            }
            return false;
        }

        private bool IsParkAvailable(int grown)
        {
            if (Placement == null || Placement.Definitions == null) return false;
            for (int i = 0; i < Placement.Definitions.Count; i++)
            {
                BuildingDefinition d = Placement.Definitions[i];
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
        }

        private int CountResidential()
        {
            ZoneSystem zones = Zones != null ? Zones.Zones : null;
            if (zones == null) return 0;
            List<KeyValuePair<GridPosition, ZoneType>> cells = zones.GetZonedCells();
            int count = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].Value == ZoneType.Residential) count++;
            }
            return count;
        }
    }
}
