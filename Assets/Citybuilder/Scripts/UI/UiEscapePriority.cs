using UnityEngine;

namespace Citybuilder
{
    // Единый арбитр Esc (issue #123): один обработчик на кадр и жёсткий приоритет панелей.
    public static class UiEscapePriority
    {
        public enum Target
        {
            None = 0, InfoPanel = 1, Settings = 2, SaveSlot = 3, LoadSlot = 4,
            Panel = 5, Pause = 6, BuildMode = 7, OpenPause = 8
        }

        public static Target Evaluate(bool mainOpen, bool pauseOpen, BuildModeMenu.Mode mode,
            bool infoPanelOpen, bool settingsOpen, bool saveOpen, bool loadOpen, bool panelOpen)
        {
            if (mainOpen) return Target.None;
            if (infoPanelOpen) return Target.InfoPanel;
            if (settingsOpen) return Target.Settings;
            if (saveOpen) return Target.SaveSlot;
            if (loadOpen) return Target.LoadSlot;
            if (panelOpen) return Target.Panel;
            if (pauseOpen) return Target.Pause;
            if (mode != BuildModeMenu.Mode.Off) return Target.BuildMode;
            return Target.OpenPause;
        }

        public static bool TryBegin(Target target, int frame)
        {
            return target != Target.None && UiEscapeGuard.TryConsume(frame);
        }

        // Единая точка входа: ввод, приоритет и гард. None — Esc в этом кадре не сработал.
        public static Target Resolve(int keyboardControl, int frame, bool mainOpen, bool pauseOpen,
            BuildModeMenu.Mode mode, bool infoPanelOpen, bool settingsOpen, bool saveOpen,
            bool loadOpen, bool panelOpen)
        {
            if (!BuildModeHotkeys.CanProcessHotkeys(false, keyboardControl)) return Target.None;
            Target target = Evaluate(mainOpen, pauseOpen, mode, infoPanelOpen, settingsOpen, saveOpen, loadOpen, panelOpen);
            return TryBegin(target, frame) ? target : Target.None;
        }

        // Обёртка совместимости со старым API EvaluateEsc (EPIC-16/issue #98).
        public static BuildModeHotkeys.EscAction ToLegacyAction(Target target)
        {
            switch (target)
            {
                case Target.Pause: return BuildModeHotkeys.EscAction.ClosePause;
                case Target.BuildMode: return BuildModeHotkeys.EscAction.TurnOffMode;
                case Target.OpenPause: return BuildModeHotkeys.EscAction.OpenPause;
                default: return BuildModeHotkeys.EscAction.None;
            }
        }

        // Выполняет действие верхнего слоя: гард не пустит остальные обработчики в этом кадре.
        public static void Apply(Target target, BuildModeMenu menu, PauseMenuView pause, InfoPanelView info,
            SettingsView settings, SaveMenuView save, LoadMenuView load, InfoMenuView infoMenu,
            EventLogView log, QuestLogView quests)
        {
            if (ApplySlot(target, settings, save, load)) return;
            switch (target)
            {
                case Target.InfoPanel:
                    if (info != null) info.Close();
                    return;
                case Target.Panel:
                    CloseTopPanel(infoMenu, log, quests);
                    return;
                case Target.Pause:
                    if (pause != null) pause.Close();
                    return;
                case Target.BuildMode:
                    if (menu != null) menu.SetMode(BuildModeMenu.Mode.Off);
                    return;
                case Target.OpenPause:
                    if (pause != null) pause.Open();
                    return;
            }
        }

        private static bool ApplySlot(Target target, SettingsView settings, SaveMenuView save, LoadMenuView load)
        {
            if (target == Target.Settings && settings != null) settings.ReturnToPause();
            else if (target == Target.SaveSlot && save != null) save.ReturnToPause();
            else if (target == Target.LoadSlot && load != null) load.ReturnToPause();
            else return false;
            return true;
        }

        private static void CloseTopPanel(InfoMenuView infoMenu, EventLogView log, QuestLogView quests)
        {
            if (quests != null && quests.IsOpen) quests.SetOpen(false);
            else if (log != null && log.IsOpen) log.SetOpen(false);
            else if (infoMenu != null && infoMenu.IsOpen) infoMenu.SetOpen(false);
        }

        // Подсказки хоткеев в Настройках: один источник строк для IMGUI.
        private static readonly string[] HintKeys =
        {
            "hint_hotkeys_panels", "hint_hotkeys_keys", "hint_hotkeys_camera"
        };

        public static void DrawHints(float x, float y)
        {
            for (int i = 0; i < HintKeys.Length; i++)
            {
                HudText.Draw(x, y + i * 22f, Localization.T(HintKeys[i]));
            }
        }
    }
}
