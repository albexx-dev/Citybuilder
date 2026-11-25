using UnityEngine;

namespace Citybuilder
{
    // Окно загрузки из 3 слотов или автосейва (EPIC-11 шаг 3).
    public class LoadMenuView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер загрузки. Пусто = ищется в сцене.")]
        public LoadController Load;
        [Tooltip("Драйвер часов.")] public SimulationRunner Runner;
        [Tooltip("Меню паузы.")] public PauseMenuView Pause;

        private bool isOpen;
        private SimulationSpeed speedBeforeOpen = SimulationSpeed.Playing;
        private string transferPath;
        private string transferStatus = string.Empty;

        public bool IsOpen { get { return isOpen; } }

        public void Open()
        {
            ResolveRefs();
            transferPath = SlotTransfer.DefaultPath(Application.persistentDataPath);
            transferStatus = Localization.T("slot_transfer_hint");
            isOpen = true;
            speedBeforeOpen = Runner != null ? Runner.Speed : SimulationSpeed.Playing;
            if (Runner != null) Runner.SetSpeed(SimulationSpeed.Paused);
        }

        public void Close()
        {
            isOpen = false;
            UiClickGuard.Set("load_menu", new Rect());
            if (Runner != null) Runner.SetSpeed(speedBeforeOpen);
        }

        public void ReturnToPause()
        {
            Close();
            if (Pause != null) Pause.Open();
        }

        private void Update()
        {
            if (isOpen && Input.GetKeyDown(KeyCode.Escape) && UiEscapeGuard.TryConsume(Time.frameCount)) ReturnToPause();
        }

        private void OnGUI()
        {
            if (!isOpen) return;
            ResolveRefs();
            Rect panel = SettingsLayout.CenteredRect(460f, 270f, Screen.width, Screen.height);
            UiClickGuard.Set("load_menu", panel);
            BottomBarStyle.DrawBarBackground(panel);
            float x = panel.x + 14f;
            float y = panel.y + 12f;
            HudText.Draw(x, y, Localization.T("load_title"));
            y += 30f;
            string baseDir = Application.persistentDataPath;
            for (int i = 0; i < SaveSlotIndex.ManualSlotCount; i++)
            {
                DrawRow(x, y, i, baseDir);
                y += 38f;
            }
            DrawRow(x, y, SaveSlotIndex.AutosaveSlot, baseDir);
            y += 44f;
            if (GUI.Button(new Rect(x, y, 120f, 28f), Localization.T("load_close"))) ReturnToPause();
            transferPath = GUI.TextField(new Rect(x, y + 36f, 300f, 22f), transferPath ?? string.Empty);
            HudText.Draw(x + 310f, y + 36f, transferStatus);
        }

        private void DrawRow(float x, float y, int slot, string baseDir)
        {
            SaveSlotIndex.SlotMeta meta = SaveSlotIndex.GetMeta(baseDir, slot);
            string title = meta != null && !string.IsNullOrEmpty(meta.name)
                ? meta.name
                : SaveSlotIndex.DefaultName(slot);
            string label = meta == null
                ? Localization.T("load_slot_empty", title)
                : Localization.T("load_slot_info", title, meta.savedAtTick / 24, meta.savedAtReal);
            HudText.Draw(x, y + 2f, label);
            Rect btn = new Rect(x + 330f, y, 100f, 26f);
            if (meta == null)
            {
                BottomBarStyle.DrawDisabledButton(btn, Localization.T("load_load"));
            }
            else if (GUI.Button(btn, Localization.T("load_load")) && Load != null)
            {
                Load.LoadNow(slot);
                Close();
            }
            if (slot != SaveSlotIndex.AutosaveSlot)
            {
                Rect transfer = new Rect(x + 330f, y + 28f, 100f, 18f);
                if (GUI.Button(transfer, Localization.T("slot_import"))) ImportInto(slot, baseDir);
            }
        }

        // Импорт в существующий слот: имя слота сохраняется, валидация — в SlotTransfer.
        private void ImportInto(int slot, string baseDir)
        {
            CitySnapshot snapshot = SlotTransfer.Import(transferPath);
            if (snapshot == null)
            {
                transferStatus = Localization.T("slot_transfer_invalid");
                return;
            }
            SaveSlotIndex.SlotMeta meta = SaveSlotIndex.GetMeta(baseDir, slot);
            bool saved = SaveSlotStorage.Save(baseDir, slot, snapshot, meta == null ? null : meta.name);
            transferStatus = saved ? Localization.T("slot_transfer_ok") : Localization.T("slot_transfer_invalid");
        }

        private void ResolveRefs()
        {
            if (Load == null) Load = FindFirstObjectByType<LoadController>();
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Pause == null) Pause = GetComponent<PauseMenuView>();
            if (Pause == null) Pause = FindFirstObjectByType<PauseMenuView>();
        }
    }
}
