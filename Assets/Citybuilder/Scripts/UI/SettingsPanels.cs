using UnityEngine;

namespace Citybuilder
{
    // Строки контента окна настроек (EPIC-69): тумблеры, слайдер и степперы IMGUI.
    // Меняют GameSettings напрямую: событие Changed внутри SettingsView делает persist
    // и применение к системам, поэтому класс остаётся без состояния.
    public static class SettingsPanels
    {
        public static void DrawSound(GameSettings s, float x, ref float y)
        {
            HudText.Draw(x, y, Localization.T("settings_section_sound"));
            y += 24f;
            HudText.Draw(x, y, SettingsLayout.VolumeLabel(s.Volume));
            float next = GUI.HorizontalSlider(new Rect(x + 150f, y + 6f, 240f, 20f), s.Volume, 0f, 1f);
            if (Mathf.Abs(next - s.Volume) > 0.001f) s.Volume = next;
            y += 28f;
            HudText.Draw(x, y, SettingsLayout.AmbientLabel(s.AmbientVolume));
            float ambient = GUI.HorizontalSlider(new Rect(x + 150f, y + 6f, 240f, 20f), s.AmbientVolume, 0f, 1f);
            if (Mathf.Abs(ambient - s.AmbientVolume) > 0.001f) s.AmbientVolume = ambient;
            y += 28f;
            bool muted = GUI.Toggle(new Rect(x, y, 220f, 22f), s.Muted, " " + Localization.T("settings_mute"));
            if (muted != s.Muted) s.Muted = muted;
            y += 28f;
        }

        public static void DrawGameplay(GameSettings s, float x, ref float y)
        {
            HudText.Draw(x, y, Localization.T("settings_section_quality"));
            y += 24f;
            DrawStepper(x, ref y, SettingsLayout.AgentLimitLabel(s.AgentLimit), s.AgentLimit,
                GameSettings.MinAgentLimit, GameSettings.MaxAgentLimit, SettingsLayout.AgentStep, v => s.AgentLimit = v);
            bool decor = GUI.Toggle(new Rect(x, y, 320f, 22f), s.DecorEnabled, " " + Localization.T("settings_decor"));
            if (decor != s.DecorEnabled) s.DecorEnabled = decor;
            y += 26f;
            bool dayNight = GUI.Toggle(new Rect(x, y, 220f, 22f), s.DayNightEnabled, Localization.T("settings_day_night"));
            if (dayNight != s.DayNightEnabled) s.DayNightEnabled = dayNight;
            // Доступность (#122): контраст в том же ряду, чтобы панель не росла.
            bool contrast = GUI.Toggle(new Rect(x + 228f, y, 196f, 22f), s.HighContrast, " " + Localization.T("settings_high_contrast"));
            if (contrast != s.HighContrast) s.HighContrast = contrast;
            y += 28f;
            HudText.Draw(x, y, Localization.T("settings_section_game"));
            y += 24f;
            DrawScaleStepper(x + 228f, y, s); // тот же ряд, что и автосейв: высота панели #123 не меняется
            // Короткая подпись: полная «каждые N ч» при 150% перекрыла бы степпер шрифта (#122 review).
            DrawStepper(x, ref y, Localization.T("settings_autosave_short", s.AutosaveHours), s.AutosaveHours,
                GameSettings.MinAutosaveHours, GameSettings.MaxAutosaveHours, SettingsLayout.AutosaveStep, v => s.AutosaveHours = v);
            DrawOverlayToggles(s, x, y);
            y += 52f;
        }

        // Переключатели оверлеев (issue #127): порядок и координаты не меняются.
        private static void DrawOverlayToggles(GameSettings s, float x, float y)
        {
            bool coverage = GUI.Toggle(new Rect(x, y, 140f, 22f), s.ServiceCoverageOverlayEnabled, " " + Localization.T("settings_overlay_coverage"));
            if (coverage != s.ServiceCoverageOverlayEnabled) s.ServiceCoverageOverlayEnabled = coverage;
            bool congestion = GUI.Toggle(new Rect(x + 144f, y, 140f, 22f), s.CongestionOverlayEnabled, " " + Localization.T("settings_overlay_congestion"));
            if (congestion != s.CongestionOverlayEnabled) s.CongestionOverlayEnabled = congestion;
            bool grid = GUI.Toggle(new Rect(x + 288f, y, 140f, 22f), s.GridOverlayEnabled, " " + Localization.T("settings_overlay_grid"));
            if (grid != s.GridOverlayEnabled) s.GridOverlayEnabled = grid;
            bool trucks = GUI.Toggle(new Rect(x, y + 26f, 200f, 22f), s.ShowGarbageTrucks, " " + Localization.T("settings_garbage_trucks"));
            if (trucks != s.ShowGarbageTrucks) s.ShowGarbageTrucks = trucks;
        }

        // Степпер масштаба шрифта HUD (#122): шаг 10%, кламп 1..1.5 в GameSettings.
        private static void DrawScaleStepper(float x, float y, GameSettings s)
        {
            const float step = 0.1f;
            if (BottomBarStyle.DrawButton(new Rect(x, y, 26f, 26f), "-", false))
                s.HudFontScale = GameSettings.ClampHudFontScale(s.HudFontScale - step);
            HudText.Draw(x + 32f, y + 3f, Localization.T("settings_hud_scale", Mathf.RoundToInt(s.HudFontScale * 100f)));
            if (BottomBarStyle.DrawButton(new Rect(x + 164f, y, 26f, 26f), "+", false))
                s.HudFontScale = GameSettings.ClampHudFontScale(s.HudFontScale + step);
        }

        // Степпер «− значение +»: клик меняет настройку, кламп по диапазону GameSettings.
        private static void DrawStepper(float x, ref float y, string label, int value, int min, int max, int step, System.Action<int> apply)
        {
            if (BottomBarStyle.DrawButton(new Rect(x, y, 30f, 26f), "-", false))
                apply(SettingsLayout.StepClamped(value, -step, min, max));
            HudText.Draw(x + 38f, y + 3f, label);
            if (BottomBarStyle.DrawButton(new Rect(x + 190f, y, 30f, 26f), "+", false))
                apply(SettingsLayout.StepClamped(value, step, min, max));
            y += 32f;
        }

        public static void DrawFooter(GameSettings s, SettingsView view, TutorialView tutorial, float x, float y)
        {
            if (BottomBarStyle.DrawButton(new Rect(x, y, 200f, 30f), Localization.T("settings_defaults"), false)) s.ResetToDefaults();
            if (BottomBarStyle.DrawButton(new Rect(x + 216f, y, 200f, 30f), Localization.T("settings_back"), false)) view.ReturnToPause();
            if (BottomBarStyle.DrawButton(new Rect(x, y + 38f, 240f, 30f), Localization.T("settings_replay_tutorial"), false))
            {
                if (tutorial != null) tutorial.ResetTutorial();
                view.Close();
            }
            UiEscapePriority.DrawHints(x, y + 76f);
        }
    }
}
