using UnityEngine;

namespace Citybuilder
{
    // Хоткеи меню режимов и паузы (EPIC-16): Esc сначала гасит режим, при Выкл — открывает/закрывает паузу;
    // открытые меню блокируют 1-4; переключение 1-4 отмечает город как несохранённый (IsDirty).
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

        private void Update()
        {
            BuildModeMenu menu = ResolveMenu();
            if (menu == null) return;
            if (Pause == null) Pause = GetComponent<PauseMenuView>();
            if (Main == null) Main = GetComponent<MainMenu>();
            if (Save == null) Save = FindFirstObjectByType<SaveController>();
            bool mainOpen = Main != null && Main.IsOpen;
            bool pauseOpen = Pause != null && Pause.IsOpen;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EscAction act = EvaluateEsc(mainOpen, pauseOpen, menu.CurrentMode);
                if (act == EscAction.ClosePause && Pause != null) Pause.Close();
                else if (act == EscAction.TurnOffMode) menu.SetMode(BuildModeMenu.Mode.Off);
                else if (act == EscAction.OpenPause && Pause != null) Pause.Open();
                return;
            }
            if (mainOpen || pauseOpen) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectMode(menu, BuildModeMenu.Mode.Houses);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectMode(menu, BuildModeMenu.Mode.Roads);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectMode(menu, BuildModeMenu.Mode.Zones);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectMode(menu, BuildModeMenu.Mode.Growth);
        }

        private void SelectMode(BuildModeMenu menu, BuildModeMenu.Mode mode)
        {
            menu.SetMode(mode);
            if (Save != null) Save.MarkDirty();
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
