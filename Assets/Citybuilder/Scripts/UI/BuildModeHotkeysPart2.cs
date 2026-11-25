using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хелпер Escape (issue #127).
    public partial class BuildModeHotkeys
    {
        // Обработка Escape (issue #127): приоритет и применение вынесены в хелпер.
        private void HandleEscape(BuildModeMenu menu, bool mainOpen, bool pauseOpen, bool infoPanelOpen)
        {
            UiEscapePriority.Target target = UiEscapePriority.Resolve(GUIUtility.keyboardControl, Time.frameCount,
                mainOpen, pauseOpen, menu.CurrentMode, infoPanelOpen,
                Settings != null && Settings.IsOpen, SaveMenu != null && SaveMenu.IsOpen,
                LoadMenu != null && LoadMenu.IsOpen, AnyPanelOpen());
            if (target != UiEscapePriority.Target.None)
                UiEscapePriority.Apply(target, menu, Pause, InfoPanel, Settings, SaveMenu, LoadMenu, InfoMenu, EventLog, Quests);
        }
    }
}
