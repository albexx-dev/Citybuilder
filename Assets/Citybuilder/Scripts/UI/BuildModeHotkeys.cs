using UnityEngine;

namespace Citybuilder
{
    // Хоткеи меню режимов и паузы (EPIC-16): Esc сначала гасит режим, при Выкл — открывает/закрывает паузу;
    // открытые меню блокируют 1-5; переключение 1-5 переключает режим без изменения состояния города.
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

        public enum EscAction
        {
            None = 0,
            ClosePause = 1,
            TurnOffMode = 2,
            OpenPause = 3
        }

        // Чистая логика приоритета Esc (EPIC-16).
        public static EscAction EvaluateEsc(bool mainOpen, bool pauseOpen, BuildModeMenu.Mode mode)
        {
            if (mainOpen) return EscAction.None;
            if (pauseOpen) return EscAction.ClosePause;
            if (mode != BuildModeMenu.Mode.Off) return EscAction.TurnOffMode;
            return EscAction.OpenPause;
        }

        // Приоритет Esc (issue #98): открытая инфопанель гасит себя и не открывает паузу в этом кадре.
        public static EscAction EvaluateEsc(bool mainOpen, bool pauseOpen, BuildModeMenu.Mode mode, bool infoPanelOpen)
        {
            if (infoPanelOpen) return EscAction.None;
            return EvaluateEsc(mainOpen, pauseOpen, mode);
        }

        public static bool CanProcessHotkeys(bool isModalOpen, int keyboardControl)
        {
            return !isModalOpen && keyboardControl == 0;
        }

        // Единый гард для Tab (статистика) и L (лента): формула не дублируется, модалки спрашиваем здесь.
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
                // Esc работает при открытых паузе/главном меню, но глотается слотами и вводом текста.
                if (!CanProcessHotkeys(slotModalOpen, GUIUtility.keyboardControl)) return;
                EscAction act = EvaluateEsc(mainOpen, pauseOpen, menu.CurrentMode, infoPanelOpen);
                if (act == EscAction.ClosePause && Pause != null) Pause.Close();
                else if (act == EscAction.TurnOffMode) menu.SetMode(BuildModeMenu.Mode.Off);
                else if (act == EscAction.OpenPause && Pause != null) Pause.Open();
                return;
            }
            // Единый гард хоткеев 1-5 (issue #42): формула только в CanProcessHotkeys.
            if (!CanProcessHotkeys(mainOpen || pauseOpen || slotModalOpen, GUIUtility.keyboardControl)) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectMode(menu, BuildModeMenu.Mode.Houses);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectMode(menu, BuildModeMenu.Mode.Roads);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectMode(menu, BuildModeMenu.Mode.Zones);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectMode(menu, BuildModeMenu.Mode.Growth);
            else if (Input.GetKeyDown(KeyCode.Alpha5)) SelectMode(menu, BuildModeMenu.Mode.Decor);
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
