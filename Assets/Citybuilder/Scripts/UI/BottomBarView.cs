using UnityEngine;

namespace Citybuilder
{
    // Нижняя панель действий (EPIC-13): скорости, режимы, кисти зон и кнопки меню.
    // Вешать на Grid; прямоугольники из HudLayout, иконки/тултипы из BottomBarIcons (EPIC-66).
    public class BottomBarView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Драйвер игровых часов. Пусто = ищется в сцене.")] public SimulationRunner Runner;
        [Tooltip("Меню режимов стройки. Пусто = ищется на этом же объекте.")] public BuildModeMenu Menu;
        [Tooltip("Палитра типов зданий. Пусто = ищется на этом же объекте.")] public BuildPaletteView Palette;
        [Tooltip("Информационное меню статистики (EPIC-14). Пусто = ищется на этом же объекте.")] public InfoMenuView InfoMenu;
        [Tooltip("Событийная лента города (EPIC-09). Пусто = ищется на этом же объекте.")] public EventLogView EventLog;
        [Tooltip("Панель задач-вех (EPIC-48). Пусто = ищется на этом же объекте.")] public QuestLogView QuestLog;
        [Tooltip("Окно настроек (EPIC-69). Пусто = ищется в сцене.")] public SettingsView Settings;

        // Доступность зарезервированных кнопок меню (включатся в EPIC-14 и EPIC-09).
        public bool StatsEnabled;
        public bool EventLogEnabled;

        private void OnGUI()
        {
            ResolveRefs();
            float sw = Screen.width;
            float sh = Screen.height;
            Rect bar = HudLayout.BottomBarRect(sw, sh);
            UiClickGuard.Set("bottom_bar", bar);
            BottomBarStyle.DrawBarBackground(bar);
            DrawSpeedGroup(HudLayout.SpeedGroupRect(sw, sh));
            DrawModeGroup(HudLayout.ModeGroupRect(sw, sh));
            DrawMenuGroup(sw, sh);
            BuildCatalogView.ResetGuards();
            DrawSubRow(sw, sh);
            MinimapView.Draw(InfoMenu != null && InfoMenu.IsOpen, EventLog != null && EventLog.IsOpen, QuestLog != null && QuestLog.IsOpen, Settings != null && Settings.IsOpen);
        }

        private void ResolveRefs()
        {
            if (Menu == null) Menu = GetComponent<BuildModeMenu>();
            if (Palette == null) Palette = GetComponent<BuildPaletteView>();
            if (InfoMenu == null) InfoMenu = GetComponent<InfoMenuView>();
            if (EventLog == null) EventLog = GetComponent<EventLogView>();
            if (QuestLog == null) QuestLog = GetComponent<QuestLogView>();
            if (Settings == null) Settings = FindFirstObjectByType<SettingsView>();
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
        }

        private void DrawSpeedGroup(Rect group)
        {
            SimulationSpeed[] speeds = { SimulationSpeed.Paused, SimulationSpeed.Playing, SimulationSpeed.FastForward };
            string[] labels = Localized(BottomBarIcons.SpeedLabelKeys);
            for (int i = 0; i < speeds.Length; i++)
            {
                float x = group.x + i * (HudLayout.SpeedButtonWidth + HudLayout.ButtonGap);
                Rect rect = new Rect(x, group.y, HudLayout.SpeedButtonWidth, group.height);
                bool active = Runner != null && Runner.Speed == speeds[i];
                if (BottomBarStyle.DrawButton(rect, IconBook.Get(BottomBarIcons.SpeedKeys[i]), labels[i], Localization.T(BottomBarIcons.SpeedTipKeys[i]), active) && Runner != null) Runner.SetSpeed(speeds[i]);
            }
        }

        private void DrawModeGroup(Rect group)
        {
            BuildModeMenu.Mode[] modes =
                { BuildModeMenu.Mode.Off, BuildModeMenu.Mode.Houses, BuildModeMenu.Mode.Roads,
                  BuildModeMenu.Mode.Zones, BuildModeMenu.Mode.Growth, BuildModeMenu.Mode.Decor };
            string[] labels = Localized(BottomBarIcons.ModeLabelKeys);
            for (int i = 0; i < modes.Length; i++)
            {
                float x = group.x + i * (HudLayout.ModeButtonWidth + HudLayout.ButtonGap);
                Rect rect = new Rect(x, group.y, HudLayout.ModeButtonWidth, group.height);
                // Выключенный декор (EPIC-69 шаг 3): кнопка «Декор [5]» неактивна, образец DrawMenuButton.
                if (Menu != null && !Menu.DecorBuildEnabled && modes[i] == BuildModeMenu.Mode.Decor)
                {
                    BottomBarStyle.DrawDisabledButton(rect, labels[i]);
                    continue;
                }
                bool active = Menu != null && Menu.CurrentMode == modes[i];
                if (BottomBarStyle.DrawButton(rect, IconBook.Get(BottomBarIcons.ModeKeys[i]), labels[i], Localization.T(BottomBarIcons.ModeTipKeys[i]), active) && Menu != null) Menu.SetMode(modes[i]);
            }
        }

        private void DrawMenuGroup(float sw, float sh)
        {
            string[] labels = Localized(BottomBarIcons.MenuLabelKeys);
            DrawMenuButton(0, sw, sh, labels[0], InfoMenu == null && !StatsEnabled, InfoMenu != null && InfoMenu.IsOpen,
                () => { if (InfoMenu != null) { ClosePanels(false, true, true); InfoMenu.Toggle(); } });
            DrawMenuButton(1, sw, sh, labels[1], EventLog == null && !EventLogEnabled, EventLog != null && EventLog.IsOpen,
                () => { if (EventLog != null) { ClosePanels(true, false, true); EventLog.Toggle(); } });
            DrawMenuButton(2, sw, sh, labels[2], QuestLog == null, QuestLog != null && QuestLog.IsOpen,
                () => { ClosePanels(true, true, false); QuestLog.Toggle(); });
            DrawMenuButton(3, sw, sh, labels[3], Settings == null, Settings != null && Settings.IsOpen,
                () => { if (!Settings.IsOpen) ClosePanels(true, true, true); Settings.Toggle(); });
        }

        // Кнопка меню нижней панели: без компонента неактивна, открытие одной гасит остальные (EPIC-48/69),
        // иконка + тултип из IconBook (EPIC-66).
        private static void DrawMenuButton(int index, float sw, float sh, string label, bool disabled, bool open, System.Action action)
        {
            Rect rect = SettingsLayout.MenuButtonRect(sw, sh, index);
            if (disabled) { BottomBarStyle.DrawDisabledButton(rect, label); return; }
            if (BottomBarStyle.DrawButton(rect, IconBook.Get(BottomBarIcons.MenuKeys[index]), label,
                Localization.T(BottomBarIcons.MenuTipKeys[index]), open) && action != null) action();
        }

        // Панели меню взаимоисключающие (EPIC-14/09/48/69): открытие одной закрывает остальные и настройки.
        private void ClosePanels(bool stats, bool log, bool quests)
        {
            if (stats && InfoMenu != null) InfoMenu.SetOpen(false);
            if (log && EventLog != null) EventLog.SetOpen(false);
            if (quests && QuestLog != null) QuestLog.SetOpen(false);
            if (Settings != null && Settings.IsOpen) Settings.Close();
        }

        private void DrawSubRow(float sw, float sh)
        {
            if (Menu != null && Menu.CurrentMode == BuildModeMenu.Mode.Zones)
            {
                DrawZoneBrushes(sw, sh);
                return;
            }
            if (Menu != null && Menu.CurrentMode == BuildModeMenu.Mode.Decor)
            {
                DecorPaletteRow.Draw(sw, sh, Menu.Decor);
                return;
            }
            if (Menu != null && Menu.CurrentMode == BuildModeMenu.Mode.Houses && Palette != null && Palette.Definitions != null && Palette.Definitions.Count > 0)
            {
                // Строка палитры заменена каталогом с вкладками и скроллом (задача 173).
                UiClickGuard.Set("bottom_subrow", new Rect());
                BuildCatalogView.Draw(sw, sh, Palette.Definitions, Palette.Placement, Palette.Growth);
                return;
            }
            UiClickGuard.Set("bottom_subrow", new Rect());
        }

        private void DrawZoneBrushes(float sw, float sh)
        {
            ZoneType[] brushes = { ZoneType.Residential, ZoneType.Commercial, ZoneType.Industrial };
            string[] labels = BottomBarIcons.ZoneBrushLabels();
            Rect popup = HudLayout.PaletteRect(sw, sh, brushes.Length);
            UiClickGuard.Set("bottom_subrow", popup);
            BottomBarStyle.DrawPopupBackground(popup);
            for (int i = 0; i < brushes.Length; i++)
            {
                float x = popup.x + i * (HudLayout.PaletteButtonWidth + HudLayout.ButtonGap);
                Rect rect = new Rect(x, popup.y, HudLayout.PaletteButtonWidth, popup.height);
                bool active = Menu.Zones != null && Menu.Zones.Brush == brushes[i];
                if (BottomBarStyle.DrawButton(rect, IconBook.Get(BottomBarIcons.BrushKeys[i]), labels[i], Localization.T(BottomBarIcons.BrushTipKeys[i]), active)) Menu.SetBrush(brushes[i]);
            }
        }

        // Подписи считаются каждый кадр: язык мог смениться между отрисовками IMGUI.
        private static string[] Localized(string[] keys)
        {
            string[] labels = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++) labels[i] = Localization.T(keys[i]);
            return labels;
        }
    }
}
