using UnityEngine;

namespace Citybuilder
{
    // Меню паузы по Esc с подтверждением выхода при несохранённых изменениях (EPIC-16 шаги 1 и 3).
    public class PauseMenuView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Драйвер часов.")] public SimulationRunner Runner;
        [Tooltip("Контроллер сохранения.")] public SaveController Save;
        [Tooltip("Меню сохранения в слоты.")] public SaveMenuView SaveMenu;
        [Tooltip("Меню загрузки слотов.")] public LoadMenuView LoadMenu;
        [Tooltip("Главное меню.")] public MainMenu MainMenu;

        private bool isOpen;
        private bool confirmingExit;

        public bool IsOpen { get { return isOpen; } }
        public bool IsConfirmingExit { get { return confirmingExit; } }

        public void Toggle()
        {
            if (isOpen) Close();
            else Open();
        }

        public void Open()
        {
            ResolveRefs();
            isOpen = true;
            confirmingExit = false;
            if (Runner != null) Runner.SetSpeed(SimulationSpeed.Paused);
        }

        public void Close()
        {
            isOpen = false;
            confirmingExit = false;
            UiClickGuard.Set("pause_menu", new Rect());
        }

        // Чистая логика решения при нажатии «В главное меню» (EPIC-16 шаг 3).
        public static bool NeedsExitConfirmation(bool isDirty)
        {
            return isDirty;
        }

        private void OnGUI()
        {
            if (!isOpen) return;
            ResolveRefs();
            Rect full = new Rect(0f, 0f, Screen.width, Screen.height);
            UiClickGuard.Set("pause_menu", full);
            BottomBarStyle.DrawPopupBackground(full);
            Rect panel = new Rect((Screen.width - 340f) * 0.5f, (Screen.height - 240f) * 0.5f, 340f, 240f);
            BottomBarStyle.DrawBarBackground(panel);
            if (confirmingExit) DrawConfirm(panel.x + 20f, panel.y + 16f);
            else DrawButtons(panel.x + 20f, panel.y + 14f);
        }

        private void DrawButtons(float x, float y)
        {
            HudText.Draw(x, y, "=== Пауза ===");
            y += 34f;
            if (GUI.Button(new Rect(x, y, 300f, 32f), "Продолжить [Esc]"))
            {
                if (Runner != null) Runner.SetSpeed(SimulationSpeed.Playing);
                Close();
            }
            y += 40f;
            if (GUI.Button(new Rect(x, y, 300f, 32f), "Сохранить") && SaveMenu != null)
            {
                Close();
                SaveMenu.Open();
            }
            y += 40f;
            if (GUI.Button(new Rect(x, y, 300f, 32f), "Загрузить") && LoadMenu != null)
            {
                Close();
                LoadMenu.Open();
            }
            y += 40f;
            if (GUI.Button(new Rect(x, y, 300f, 32f), "В главное меню"))
            {
                bool dirty = Save != null && Save.IsDirty;
                if (NeedsExitConfirmation(dirty)) confirmingExit = true;
                else GoToMainMenu();
            }
        }

        private void DrawConfirm(float x, float y)
        {
            HudText.Draw(x, y, "Сохранить перед выходом?");
            y += 42f;
            if (GUI.Button(new Rect(x, y, 300f, 32f), "Да (сохранить и выйти)"))
            {
                if (Save != null) Save.SaveNow(0);
                GoToMainMenu();
            }
            y += 40f;
            if (GUI.Button(new Rect(x, y, 300f, 32f), "Нет (выйти без сохранения)"))
            {
                GoToMainMenu();
            }
            y += 40f;
            if (GUI.Button(new Rect(x, y, 300f, 32f), "Отмена")) confirmingExit = false;
        }

        private void GoToMainMenu()
        {
            Close();
            if (MainMenu != null) MainMenu.Open();
        }

        private void ResolveRefs()
        {
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Save == null) Save = FindFirstObjectByType<SaveController>();
            if (SaveMenu == null) SaveMenu = GetComponent<SaveMenuView>();
            if (LoadMenu == null) LoadMenu = GetComponent<LoadMenuView>();
            if (MainMenu == null) MainMenu = GetComponent<MainMenu>();
        }
    }
}
