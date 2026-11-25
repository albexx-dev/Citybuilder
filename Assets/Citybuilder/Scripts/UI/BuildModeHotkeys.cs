using UnityEngine;

namespace Citybuilder
{
    // Хоткеи режимов, панелей и паузы (EPIC-16, issue #123): Esc идёт через единый арбитр.
    public class BuildModeHotkeys : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Меню режимов. Пусто = ищется на этом же объекте.")]
        public BuildModeMenu Menu;
        [Tooltip("Меню паузы (EPIC-16). Пусто = ищется на этом же объекте.")]
        public PauseMenuView Pause;
        [Tooltip("Главное меню. Пусто = ищется на этом же объекте.")]
        public MainMenu Main;
        [Tooltip("Контроллер сохранения (для флага IsDirty). Пусто = ищется в сцене.")]
        public SaveController Save;
        [Tooltip("Меню сохранения.")] public SaveMenuView SaveMenu;
        [Tooltip("Меню загрузки.")] public LoadMenuView LoadMenu;
        [Tooltip("Окно настроек (EPIC-69).")] public SettingsView Settings;
        [Tooltip("Инфопанель выбранного здания: её Esc имеет приоритет над паузой (issue #98).")] public InfoPanelView InfoPanel;
        [Tooltip("Меню статистики (Tab).")] public InfoMenuView InfoMenu;
        [Tooltip("Лента событий (L).")] public EventLogView EventLog;
        [Tooltip("Журнал задач (J).")] public QuestLogView Quests;

        public enum EscAction
        {
            None = 0,
            ClosePause = 1,
            TurnOffMode = 2,
            OpenPause = 3
        }

        public static EscAction EvaluateEsc(bool mainOpen, bool pauseOpen, BuildModeMenu.Mode mode)
        {
            return UiEscapePriority.ToLegacyAction(UiEscapePriority.Evaluate(
                mainOpen, pauseOpen, mode, false, false, false, false, false));
        }

        public static EscAction EvaluateEsc(bool mainOpen, bool pauseOpen, BuildModeMenu.Mode mode, bool infoPanelOpen)
        {
            return UiEscapePriority.ToLegacyAction(UiEscapePriority.Evaluate(
                mainOpen, pauseOpen, mode, infoPanelOpen, false, false, false, false));
        }

        public static bool CanProcessHotkeys(bool isModalOpen, int keyboardControl)
        {
            return !isModalOpen && keyboardControl == 0;
        }
        public static bool CanProcessPanelHotkeys()
        {
            BuildModeHotkeys hotkeys = FindFirstObjectByType<BuildModeHotkeys>();
            if (hotkeys != null && hotkeys.IsModalOpen()) return false;
            return CanProcessHotkeys(false, GUIUtility.keyboardControl);
        }

        public bool IsModalOpen()
        {
            ResolveModalRefs();
            return (Pause != null && Pause.IsOpen)
                || (SaveMenu != null && SaveMenu.IsOpen)
                || (LoadMenu != null && LoadMenu.IsOpen)
                || (Main != null && Main.IsOpen)
                || (Settings != null && Settings.IsOpen);
        }

        private void Update()
        {
            BuildModeMenu menu = ResolveMenu();
            if (menu == null) return;
            ResolveModalRefs();
            bool mainOpen = Main != null && Main.IsOpen;
            bool pauseOpen = Pause != null && Pause.IsOpen;
            bool slotModalOpen = (SaveMenu != null && SaveMenu.IsOpen)
                || (LoadMenu != null && LoadMenu.IsOpen)
                || (Settings != null && Settings.IsOpen);
            bool infoPanelOpen = InfoPanel != null && InfoPanel.IsOpen;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UiEscapePriority.Target target = UiEscapePriority.Resolve(GUIUtility.keyboardControl, Time.frameCount,
                    mainOpen, pauseOpen, menu.CurrentMode, infoPanelOpen,
                    Settings != null && Settings.IsOpen, SaveMenu != null && SaveMenu.IsOpen,
                    LoadMenu != null && LoadMenu.IsOpen, AnyPanelOpen());
                if (target != UiEscapePriority.Target.None)
                    UiEscapePriority.Apply(target, menu, Pause, InfoPanel, Settings, SaveMenu, LoadMenu, InfoMenu, EventLog, Quests);
                return;
            }
            // O закрывает настройки, хотя IsModalOpen блокирует остальные хоткеи панелей.
            if (Input.GetKeyDown(KeyCode.O) && Settings != null && GUIUtility.keyboardControl == 0
                && (Settings.IsOpen || CanProcessPanelHotkeys())) Settings.Toggle();
            if (!CanProcessHotkeys(mainOpen || pauseOpen || slotModalOpen, GUIUtility.keyboardControl)) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectMode(menu, BuildModeMenu.Mode.Houses);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectMode(menu, BuildModeMenu.Mode.Roads);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectMode(menu, BuildModeMenu.Mode.Zones);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectMode(menu, BuildModeMenu.Mode.Growth);
            else if (Input.GetKeyDown(KeyCode.Alpha5)) SelectMode(menu, BuildModeMenu.Mode.Decor);
            if (Input.GetKeyDown(KeyCode.J) && Quests != null && CanProcessPanelHotkeys()) OpenQuests();
        }

        private void OpenQuests()
        {
            if (InfoMenu != null) InfoMenu.SetOpen(false);
            if (EventLog != null) EventLog.SetOpen(false);
            if (Settings != null && Settings.IsOpen) Settings.Close();
            Quests.Toggle();
        }

        private bool AnyPanelOpen()
        {
            return (InfoMenu != null && InfoMenu.IsOpen)
                || (EventLog != null && EventLog.IsOpen)
                || (Quests != null && Quests.IsOpen);
        }

        public static bool TryBeginEscape(EscAction act, int frame)
        {
            return act != EscAction.None && UiEscapeGuard.TryConsume(frame);
        }

        public void SelectMode(BuildModeMenu menu, BuildModeMenu.Mode mode)
        {
            menu.SetMode(mode);
        }

        private void ResolveModalRefs()
        {
            if (Pause == null) Pause = GetComponent<PauseMenuView>() ?? FindFirstObjectByType<PauseMenuView>();
            if (Main == null) Main = GetComponent<MainMenu>() ?? FindFirstObjectByType<MainMenu>();
            if (SaveMenu == null) SaveMenu = GetComponent<SaveMenuView>() ?? FindFirstObjectByType<SaveMenuView>();
            if (LoadMenu == null) LoadMenu = GetComponent<LoadMenuView>() ?? FindFirstObjectByType<LoadMenuView>();
            if (Settings == null) Settings = GetComponent<SettingsView>() ?? FindFirstObjectByType<SettingsView>();
            if (InfoPanel == null) InfoPanel = FindFirstObjectByType<InfoPanelView>();
            if (InfoMenu == null) InfoMenu = FindFirstObjectByType<InfoMenuView>();
            if (EventLog == null) EventLog = FindFirstObjectByType<EventLogView>();
            if (Quests == null) Quests = FindFirstObjectByType<QuestLogView>();
        }

        private BuildModeMenu ResolveMenu()
        {
            if (Menu != null) return Menu;
            BuildModeMenu found = GetComponent<BuildModeMenu>();
            if (found == null)
            {
                Debug.LogError("BuildModeHotkeys: задайте Menu в Inspector.");
                enabled = false;
            }
            Menu = found;
            return found;
        }
    }
}
