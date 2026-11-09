using UnityEngine;

namespace Citybuilder
{
    // Главное меню поверх живого города (EPIC-16 шаг 2): Продолжить, Новая игра, Загрузить, Выход.
    // Вешать на Grid, ссылки на контроллеры Persistence и меню слотов добираются автоматически.
    public class MainMenu : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Запись слота.")] public SaveController Save;
        [Tooltip("Загрузка слота и новая игра.")] public LoadController Load;
        [Tooltip("Меню выбора слота загрузки (EPIC-11).")] public LoadMenuView LoadMenu;
        [Tooltip("Драйвер часов.")] public SimulationRunner Runner;

        private bool isOpen;

        public bool IsOpen { get { return isOpen; } }

        public void Open()
        {
            ResolveRefs();
            isOpen = true;
            if (Runner != null) Runner.SetSpeed(SimulationSpeed.Paused);
        }

        public void Close()
        {
            isOpen = false;
            UiClickGuard.Set("mainmenu", new Rect());
        }

        // Последний сохранённый слот по времени (0..2 или автосейв). Если пусто — 0.
        public static int FindLatestSlot(string baseDir)
        {
            long bestTick = -1;
            int bestSlot = 0;
            int[] candidates = { 0, 1, 2, SaveSlotIndex.AutosaveSlot };
            for (int i = 0; i < candidates.Length; i++)
            {
                SaveSlotIndex.SlotMeta meta = SaveSlotIndex.GetMeta(baseDir, candidates[i]);
                if (meta != null && meta.savedAtTick >= bestTick)
                {
                    bestTick = meta.savedAtTick;
                    bestSlot = candidates[i];
                }
            }
            return bestSlot;
        }

        private void OnGUI()
        {
            if (!isOpen) return;
            ResolveRefs();
            Rect full = new Rect(0f, 0f, Screen.width, Screen.height);
            UiClickGuard.Set("mainmenu", full);
            BottomBarStyle.DrawPopupBackground(full);
            Rect panel = new Rect((Screen.width - 320f) * 0.5f, (Screen.height - 230f) * 0.5f, 320f, 230f);
            BottomBarStyle.DrawBarBackground(panel);
            float x = panel.x + 20f;
            float y = panel.y + 14f;
            HudText.Draw(x, y, "=== Citybuilder ===");
            y += 34f;
            if (GUI.Button(new Rect(x, y, 280f, 32f), "Продолжить"))
            {
                if (Load != null) Load.LoadNow(FindLatestSlot(Application.persistentDataPath));
                if (Runner != null) Runner.SetSpeed(SimulationSpeed.Playing);
                Close();
            }
            y += 40f;
            if (GUI.Button(new Rect(x, y, 280f, 32f), "Новая игра"))
            {
                if (Load != null) Load.NewGame();
                if (Runner != null) Runner.SetSpeed(SimulationSpeed.Playing);
                Close();
            }
            y += 40f;
            if (GUI.Button(new Rect(x, y, 280f, 32f), "Загрузить") && LoadMenu != null)
            {
                Close();
                LoadMenu.Open();
            }
            y += 40f;
            if (GUI.Button(new Rect(x, y, 280f, 32f), "Выход")) Application.Quit();
        }

        private void ResolveRefs()
        {
            if (Save == null) Save = FindFirstObjectByType<SaveController>();
            if (Load == null) Load = FindFirstObjectByType<LoadController>();
            if (LoadMenu == null) LoadMenu = GetComponent<LoadMenuView>();
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
        }
    }
}
