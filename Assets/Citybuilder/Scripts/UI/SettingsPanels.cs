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
            y += 28f;
            HudText.Draw(x, y, Localization.T("settings_section_game"));
            y += 24f;
            DrawStepper(x, ref y, SettingsLayout.AutosaveLabel(s.AutosaveHours), s.AutosaveHours,
                GameSettings.MinAutosaveHours, GameSettings.MaxAutosaveHours, SettingsLayout.AutosaveStep, v => s.AutosaveHours = v);
            bool coverage = GUI.Toggle(new Rect(x, y, 140f, 22f), s.ServiceCoverageOverlayEnabled, " " + Localization.T("settings_overlay_coverage"));
            if (coverage != s.ServiceCoverageOverlayEnabled) s.ServiceCoverageOverlayEnabled = coverage;
            bool congestion = GUI.Toggle(new Rect(x + 144f, y, 140f, 22f), s.CongestionOverlayEnabled, " " + Localization.T("settings_overlay_congestion"));
            if (congestion != s.CongestionOverlayEnabled) s.CongestionOverlayEnabled = congestion;
            bool grid = GUI.Toggle(new Rect(x + 288f, y, 140f, 22f), s.GridOverlayEnabled, " " + Localization.T("settings_overlay_grid"));
            if (grid != s.GridOverlayEnabled) s.GridOverlayEnabled = grid;
            y += 26f;
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
            HudText.Draw(x, y + 76f, Localization.T("settings_controls"));
        }
    }
}
