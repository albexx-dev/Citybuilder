using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Нижняя панель действий (EPIC-13): скорости, режимы, палитра/кисти и кнопки меню.
    // Вешать на Grid. Все прямоугольники берутся из HudLayout и регистрируются в UiClickGuard.
    public class BottomBarView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Драйвер игровых часов. Пусто = ищется в сцене.")]
        public SimulationRunner Runner;
        [Tooltip("Меню режимов стройки. Пусто = ищется на этом же объекте.")]
        public BuildModeMenu Menu;
        [Tooltip("Палитра типов зданий. Пусто = ищется на этом же объекте.")]
        public BuildPaletteView Palette;

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
            DrawSubRow(sw, sh);
        }

        private void ResolveRefs()
        {
            if (Menu == null) Menu = GetComponent<BuildModeMenu>();
            if (Palette == null) Palette = GetComponent<BuildPaletteView>();
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
                if (BottomBarStyle.DrawButton(rect, labels[i], active) && Runner != null)
                {
                    Runner.SetSpeed(speeds[i]);
                }
            }
        }

        private void DrawModeGroup(Rect group)
        {
            BuildModeMenu.Mode[] modes =
            {
                BuildModeMenu.Mode.Off, BuildModeMenu.Mode.Houses,
                BuildModeMenu.Mode.Roads, BuildModeMenu.Mode.Zones, BuildModeMenu.Mode.Growth
            };
            string[] labels = { "Выкл [Esc]", "Дома [1]", "Дороги [2]", "Зоны [3]", "Рост [4]" };
            for (int i = 0; i < modes.Length; i++)
            {
                float x = group.x + i * (HudLayout.ModeButtonWidth + HudLayout.ButtonGap);
                Rect rect = new Rect(x, group.y, HudLayout.ModeButtonWidth, group.height);
                bool active = Menu != null && Menu.CurrentMode == modes[i];
                if (BottomBarStyle.DrawButton(rect, labels[i], active) && Menu != null)
                {
                    Menu.SetMode(modes[i]);
                }
            }
        }

        private void DrawMenuGroup(Rect group)
        {
            Rect stats = new Rect(group.x, group.y, HudLayout.MenuButtonWidth, group.height);
            Rect log = new Rect(stats.xMax + HudLayout.ButtonGap, group.y, HudLayout.MenuButtonWidth, group.height);
            if (!StatsEnabled) BottomBarStyle.DrawDisabledButton(stats, "Статистика");
            else BottomBarStyle.DrawButton(stats, "Статистика", false);
            if (!EventLogEnabled) BottomBarStyle.DrawDisabledButton(log, "Лента");
            else BottomBarStyle.DrawButton(log, "Лента", false);
        }

        private void DrawSubRow(float sw, float sh)
        {
            if (Menu != null && Menu.CurrentMode == BuildModeMenu.Mode.Zones)
            {
                DrawZoneBrushes(sw, sh);
                return;
            }
            if (Menu != null && Menu.CurrentMode == BuildModeMenu.Mode.Houses && Palette != null && Palette.Definitions != null && Palette.Definitions.Count > 0)
            {
                DrawPaletteRow(sw, sh, Palette.Definitions);
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

        private void DrawPaletteRow(float sw, float sh, List<BuildingDefinition> defs)
        {
            Rect popup = HudLayout.PaletteRect(sw, sh, defs.Count);
            UiClickGuard.Set("bottom_subrow", popup);
            BottomBarStyle.DrawPopupBackground(popup);
            int grown = Palette.Growth != null ? Palette.Growth.GrownCount : int.MaxValue;
            for (int i = 0; i < defs.Count; i++)
            {
                BuildingDefinition def = defs[i];
                if (def == null) continue;
                float x = popup.x + i * (HudLayout.PaletteButtonWidth + HudLayout.ButtonGap);
                Rect rect = new Rect(x, popup.y, HudLayout.PaletteButtonWidth, popup.height);
                if (!UnlockPolicy.IsUnlocked(def, grown))
                {
                    BottomBarStyle.DrawDisabledButton(rect, def.DisplayName + " — " + UnlockPolicy.Remaining(def, grown) + " зд.");
                    continue;
                }
                bool active = Palette.Placement != null && Palette.Placement.Definition == def;
                if (BottomBarStyle.DrawButton(rect, def.DisplayName + " $" + def.ConstructionCost, active) && Palette.Placement != null)
                {
                    Palette.Placement.SetDefinition(def);
                }
            }
        }
    }
}
