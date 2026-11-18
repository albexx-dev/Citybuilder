using UnityEngine;

namespace Citybuilder
{
    // Нижняя панель действий (EPIC-13): скорости, режимы, кисти зон и кнопки меню.
    // Вешать на Grid. Все прямоугольники берутся из HudLayout и регистрируются в UiClickGuard.
    // Каталог зданий (задача 173) вынесен в BuildCatalogView.
    public class BottomBarView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Драйвер игровых часов. Пусто = ищется в сцене.")] public SimulationRunner Runner;
        [Tooltip("Меню режимов стройки. Пусто = ищется на этом же объекте.")] public BuildModeMenu Menu;
        [Tooltip("Палитра типов зданий. Пусто = ищется на этом же объекте.")] public BuildPaletteView Palette;
        [Tooltip("Информационное меню статистики (EPIC-14). Пусто = ищется на этом же объекте.")] public InfoMenuView InfoMenu;
        [Tooltip("Событийная лента города (EPIC-09). Пусто = ищется на этом же объекте.")] public EventLogView EventLog;
        [Tooltip("Панель задач-вех (EPIC-48). Пусто = ищется на этом же объекте.")] public QuestLogView QuestLog;

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
            DrawMenuGroup(HudLayout.MenuButtonsRect(sw, sh));
            BuildCatalogView.ResetGuards();
            DrawSubRow(sw, sh);
        }

        private void ResolveRefs()
        {
            if (Menu == null) Menu = GetComponent<BuildModeMenu>();
            if (Palette == null) Palette = GetComponent<BuildPaletteView>();
            if (InfoMenu == null) InfoMenu = GetComponent<InfoMenuView>();
            if (EventLog == null) EventLog = GetComponent<EventLogView>();
            if (QuestLog == null) QuestLog = GetComponent<QuestLogView>();
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
        }

        private void DrawSpeedGroup(Rect group)
        {
            SimulationSpeed[] speeds = { SimulationSpeed.Paused, SimulationSpeed.Playing, SimulationSpeed.FastForward };
            string[] labels = { "Пауза", "Игра", "x4" };
            for (int i = 0; i < speeds.Length; i++)
            {
                float x = group.x + i * (HudLayout.SpeedButtonWidth + HudLayout.ButtonGap);
                Rect rect = new Rect(x, group.y, HudLayout.SpeedButtonWidth, group.height);
                bool active = Runner != null && Runner.Speed == speeds[i];
                if (BottomBarStyle.DrawButton(rect, labels[i], active) && Runner != null) Runner.SetSpeed(speeds[i]);
            }
        }

        private void DrawModeGroup(Rect group)
        {
            BuildModeMenu.Mode[] modes =
            {
                BuildModeMenu.Mode.Off, BuildModeMenu.Mode.Houses,
                BuildModeMenu.Mode.Roads, BuildModeMenu.Mode.Zones, BuildModeMenu.Mode.Growth,
                BuildModeMenu.Mode.Decor
            };
            string[] labels = { "Выкл [Esc]", "Дома [1]", "Дороги [2]", "Зоны [3]", "Рост [4]", "Декор [5]" };
            for (int i = 0; i < modes.Length; i++)
            {
                float x = group.x + i * (HudLayout.ModeButtonWidth + HudLayout.ButtonGap);
                Rect rect = new Rect(x, group.y, HudLayout.ModeButtonWidth, group.height);
                bool active = Menu != null && Menu.CurrentMode == modes[i];
                if (BottomBarStyle.DrawButton(rect, labels[i], active) && Menu != null) Menu.SetMode(modes[i]);
            }
        }

        private void DrawMenuGroup(Rect group)
        {
            Rect stats = new Rect(group.x, group.y, HudLayout.MenuButtonWidth, group.height);
            Rect log = new Rect(stats.xMax + HudLayout.ButtonGap, group.y, HudLayout.MenuButtonWidth, group.height);
            Rect quests = new Rect(log.xMax + HudLayout.ButtonGap, group.y, HudLayout.MenuButtonWidth, group.height);
            if (InfoMenu == null && !StatsEnabled) BottomBarStyle.DrawDisabledButton(stats, "Статистика");
            else if (BottomBarStyle.DrawButton(stats, "Статистика", InfoMenu != null && InfoMenu.IsOpen) && InfoMenu != null)
            {
                ClosePanels(false, true, true);
                InfoMenu.Toggle();
            }
            if (EventLog == null && !EventLogEnabled) BottomBarStyle.DrawDisabledButton(log, "Лента");
            else if (BottomBarStyle.DrawButton(log, "Лента", EventLog != null && EventLog.IsOpen) && EventLog != null)
            {
                ClosePanels(true, false, true);
                EventLog.Toggle();
            }
            if (QuestLog == null) BottomBarStyle.DrawDisabledButton(quests, "Задачи");
            else if (BottomBarStyle.DrawButton(quests, "Задачи", QuestLog.IsOpen))
            {
                ClosePanels(true, true, false);
                QuestLog.Toggle();
            }
        }

        // Панели меню взаимоисключающие (EPIC-14/09/48): открытие одной закрывает остальные.
        private void ClosePanels(bool stats, bool log, bool quests)
        {
            if (stats && InfoMenu != null) InfoMenu.SetOpen(false);
            if (log && EventLog != null) EventLog.SetOpen(false);
            if (quests && QuestLog != null) QuestLog.SetOpen(false);
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
            string[] labels = { "Жилая", "Коммерция", "Промзона" };
            Rect popup = HudLayout.PaletteRect(sw, sh, brushes.Length);
            UiClickGuard.Set("bottom_subrow", popup);
            BottomBarStyle.DrawPopupBackground(popup);
            for (int i = 0; i < brushes.Length; i++)
            {
                float x = popup.x + i * (HudLayout.PaletteButtonWidth + HudLayout.ButtonGap);
                Rect rect = new Rect(x, popup.y, HudLayout.PaletteButtonWidth, popup.height);
                bool active = Menu.Zones != null && Menu.Zones.Brush == brushes[i];
                if (BottomBarStyle.DrawButton(rect, labels[i], active)) Menu.SetBrush(brushes[i]);
            }
        }

    }
}
