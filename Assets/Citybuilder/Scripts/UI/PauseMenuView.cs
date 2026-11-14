using UnityEngine;

namespace Citybuilder
{
    // Меню паузы по Esc с подтверждением выхода и управлением звуком (EPIC-16, EPIC-19).
    public class PauseMenuView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Драйвер часов.")] public SimulationRunner Runner;
        [Tooltip("Контроллер сохранения.")] public SaveController Save;
        [Tooltip("Меню сохранения в слоты.")] public SaveMenuView SaveMenu;
        [Tooltip("Меню загрузки слотов.")] public LoadMenuView LoadMenu;
        [Tooltip("Главное меню.")] public MainMenu MainMenu;
        [Tooltip("Контроллер звука (EPIC-19).")] public AudioController Audio;

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
            if (!isOpen) AudioController.PlaySwooshGlobal();
            isOpen = true;
            confirmingExit = false;
            if (Runner != null) Runner.SetSpeed(SimulationSpeed.Paused);
        }

        public void Close()
        {
            if (isOpen) AudioController.PlaySwooshGlobal();
            isOpen = false;
            confirmingExit = false;
            UiClickGuard.Set("pause_menu", new Rect());
            if (Runner != null) Runner.SetSpeed(SimulationSpeed.Playing);
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
            Rect panel = new Rect((Screen.width - 340f) * 0.5f, (Screen.height - 300f) * 0.5f, 340f, 300f);
            BottomBarStyle.DrawBarBackground(panel);
            if (confirmingExit) DrawConfirm(panel.x + 20f, panel.y + 16f);
            else DrawButtons(panel.x + 20f, panel.y + 14f);
        }

        private void DrawButtons(float x, float y)
        {
            HudText.Draw(x, y, "=== Пауза ===");
            y += 32f;
            if (GUI.Button(new Rect(x, y, 300f, 30f), "Продолжить [Esc]"))
            { AudioController.PlayClickGlobal(); Close(); }
            y += 36f;
            if (GUI.Button(new Rect(x, y, 300f, 30f), "Сохранить") && SaveMenu != null)
            { AudioController.PlayClickGlobal(); Close(); SaveMenu.Open(); }
            y += 36f;
            if (GUI.Button(new Rect(x, y, 300f, 30f), "Загрузить") && LoadMenu != null)
            { AudioController.PlayClickGlobal(); Close(); LoadMenu.Open(); }
            y += 36f;
            DrawAudioControls(x, ref y);
            if (GUI.Button(new Rect(x, y, 300f, 30f), "В главное меню"))
            {
                AudioController.PlayClickGlobal();
                bool dirty = Save != null && Save.IsDirty;
                if (NeedsExitConfirmation(dirty)) confirmingExit = true;
                else GoToMainMenu();
            }
        }

        private void DrawAudioControls(float x, ref float y)
        {
            float vol = Audio != null ? Audio.Volume : AudioController.DefaultVolume;
            HudText.Draw(x, y, "Громкость: " + Mathf.RoundToInt(vol * 100f) + "%");
            float nextVol = GUI.HorizontalSlider(new Rect(x + 135f, y + 6f, 165f, 20f), vol, 0f, 1f);
            if (Audio != null && Mathf.Abs(nextVol - vol) > 0.001f) Audio.SetVolume(nextVol);
            y += 26f;
            bool muted = Audio != null && Audio.IsMuted;
            bool nextMuted = GUI.Toggle(new Rect(x, y, 200f, 24f), muted, " Без звука");
            if (Audio != null && nextMuted != muted) Audio.SetMuted(nextMuted);
            y += 30f;
        }

        private void DrawConfirm(float x, float y)
        {
            HudText.Draw(x, y, "Сохранить перед выходом?");
            y += 42f;
            if (GUI.Button(new Rect(x, y, 300f, 32f), "Да (сохранить и выйти)"))
            {
                AudioController.PlayClickGlobal();
                if (Save != null) Save.SaveNow(0);
                GoToMainMenu();
            }
            y += 40f;
            if (GUI.Button(new Rect(x, y, 300f, 32f), "Нет (выйти без сохранения)"))
            {
                AudioController.PlayClickGlobal();
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
            if (Audio == null) Audio = FindFirstObjectByType<AudioController>();
        }
    }
}
