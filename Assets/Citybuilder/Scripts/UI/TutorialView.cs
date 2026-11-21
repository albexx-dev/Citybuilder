using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Туториал v3 (EPIC-70): баннер с текстом, кнопка «Понятно» и стрелка над целевой кнопкой.
    // Логика шагов — TutorialPolicy (чистый C#), рамка и стрелка — TutorialArrowView.
    public class TutorialView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Кисть зон. Пусто = ищется на этом же объекте.")] public ZonePaintingController Zones;
        [Tooltip("Автовырастание домов. Пусто = ищется на этом же объекте.")] public SettlementGrowthController Growth;
        [Tooltip("Меню режимов. Пусто = ищется на этом же объекте.")] public BuildModeMenu Menu;
        [Tooltip("Ручная стройка зданий. Пусто = ищется на этом же объекте.")] public BuildingPlacementController Placement;
        [Tooltip("Палитра типов зданий. Пусто = ищется на этом же объекте.")] public BuildPaletteView Palette;

        [Header("События v1.2")]
        [Tooltip("Экономика: снимок пожаров. Пусто = ищется в сцене.")] public EconomyTicker Economy;
        [Tooltip("Квесты: награды. Пусто = ищется в сцене.")] public QuestLogView Quests;
        [Tooltip("Лента города: первый вывоз мусора. Пусто = ищется в сцене.")] public EventLogView Log;
        [Tooltip("Часы: наступление ночи. Пусто = ищется в сцене.")] public SimulationRunner Runner;

        private readonly TutorialEventView events = new TutorialEventView();
        private bool dismissed;
        public TutorialPolicy.Step CurrentStep { get { return PickStep(); } }
        public string CurrentText { get { return TutorialPolicy.FormatText(CurrentStep, SelectedId()); } }
        public bool IsDismissed { get { return dismissed; } }
        // Загрузка сейта вызывает Dismiss: подсказки событий тоже гаснут, не включаются сами.
        public void Dismiss() { ResetState(true); }
        public void ResetTutorial() { ResetState(false); }

        private void ResetState(bool value)
        {
            ResolveRefs();
            dismissed = value;
            events.Reset(Quests, Runner);
            if (value) UiClickGuard.Set("tutorial_dismiss", new Rect());
        }
        public void OnDisable() { UiClickGuard.Set("tutorial_dismiss", new Rect()); }
        private void OnGUI()
        {
            ResolveRefs();
            events.Sample(Economy, Quests, Log, Runner, HasPlaced("fire_station"), Time.unscaledTime);
            TutorialEventPolicy.Kind eventKind = events.Current;
            if (eventKind != TutorialEventPolicy.Kind.None)
            {
                DrawBanner(TutorialEventPolicy.FormatText(eventKind), true);
                TutorialArrowView.Draw(EventArrowTarget(eventKind), Time.unscaledTime);
                return;
            }
            TutorialPolicy.Step step = CurrentStep;
            string text = TutorialPolicy.FormatText(step, SelectedId());
            if (string.IsNullOrEmpty(text)) { UiClickGuard.Set("tutorial_dismiss", new Rect()); return; }
            DrawBanner(text, false);
            TutorialArrowView.Draw(ArrowTarget(step), Time.unscaledTime);
        }
        // Для событий v1.2 стрелка целится в карточку каталога (пожарная станция).
        private Rect EventArrowTarget(TutorialEventPolicy.Kind kind)
        {
            string id = TutorialEventPolicy.TargetBuildingId(kind);
            if (id == null || Placement == null || Placement.Definitions == null) return new Rect();
            int defIndex = IndexOfId(Placement.Definitions, id);
            if (defIndex < 0) return new Rect();
            List<BuildingDefinition> cards = new List<BuildingDefinition>();
            BuildCatalogView.FilterByCategory(Placement.Definitions, Placement.Definitions[defIndex].Category, cards);
            int index = IndexOfId(cards, id);
            return index < 0 ? new Rect()
                : TutorialArrowView.CatalogCardRect(Screen.width, Screen.height, cards.Count, index);
        }

        private void DrawBanner(string text, bool isEvent)
        {
            if (SettingsLayout.IsNarrow(Screen.width) || !HudLayout.IsHudRowVisible(430f, 1, Screen.height))
            {
                DrawCompactBanner(text, isEvent);
                return;
            }
            float y = HudLayout.FixedOrBottom(430f, 1);
            HudText.Draw(10f, y, text);
            Rect btn = new Rect(Screen.width - 100f, y - 2f, 90f, 24f);
            UiClickGuard.Set("tutorial_dismiss", btn);
            if (GUI.Button(btn, Localization.T("tutorial_dismiss"))) { if (isEvent) events.Acknowledge(); else Dismiss(); }
        }

        // Компактный баннер: в узком/низком окне подсказка не исчезает, текст обрезается группой.
        private void DrawCompactBanner(string text, bool isEvent)
        {
            Rect rect = SettingsLayout.CompactBannerRect(Screen.width, Screen.height);
            GUI.BeginGroup(rect);
            HudText.Draw(0f, 0f, text);
            GUI.EndGroup();
            Rect btn = SettingsLayout.CompactDismissRect(Screen.width, Screen.height);
            UiClickGuard.Set("tutorial_dismiss", btn);
            if (GUI.Button(btn, Localization.T("tutorial_dismiss"))) { if (isEvent) events.Acknowledge(); else Dismiss(); }
        }

        // Цель стрелки: «x4» — кнопка скорости; шаги-стройки — кнопка здания в палитре
        // над панелью, а пока палитра закрыта — кнопка «Здания [1]»; остальное — кнопка режима.
        private Rect ArrowTarget(TutorialPolicy.Step step)
        {
            TutorialPolicy.TargetButton fallback = TutorialPolicy.HighlightTarget(step);
            if (fallback == TutorialPolicy.TargetButton.Speed4)
                return TutorialArrowView.SpeedButtonRect(Screen.width, Screen.height, TutorialArrowView.X4SpeedIndex);
            string targetId = TutorialPolicy.TargetBuildingId(step);
            if (targetId == null) return TutorialArrowView.ModeButtonRect(Screen.width, Screen.height, fallback);
            if (TutorialPolicy.IsTypeSelected(step, SelectedId())) return new Rect();
            int index = Palette != null && Palette.Definitions != null ? IndexOfId(Palette.Definitions, targetId) : -1;
            if (index >= 0 && Menu != null && Menu.CurrentMode == BuildModeMenu.Mode.Houses)
            {
                return TutorialArrowView.PaletteButtonRect(Screen.width, Screen.height, index, Palette.Definitions.Count);
            }
            return TutorialArrowView.ModeButtonRect(Screen.width, Screen.height, fallback);
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
                Dismissed = dismissed,
                SelectedBuildingId = SelectedId()
            };
            return TutorialPolicy.Pick(s);
        }

        private bool HasPlaced(string id)
        {
            if (Placement == null || Placement.Placed == null) return false;
            foreach (BuildingState b in Placement.Placed)
            {
                if (b != null && b.Definition != null && b.Definition.Id == id) return true;
            }
            return false;
        }

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
