using UnityEngine;

namespace Citybuilder
{
    // Окно сохранения в один из 3 слотов с редактируемым именем (EPIC-11 шаг 3).
    public class SaveMenuView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер сохранения. Пусто = ищется в сцене.")]
        public SaveController Save;
        [Tooltip("Драйвер часов.")] public SimulationRunner Runner;
        [Tooltip("Меню паузы.")] public PauseMenuView Pause;

        private readonly string[] slotNames = new string[SaveSlotIndex.ManualSlotCount];
        private int warningSlot = -1;
        private bool isOpen;
        private SimulationSpeed speedBeforeOpen = SimulationSpeed.Playing;

        public bool IsOpen { get { return isOpen; } }

        public void Open()
        {
            ResolveRefs();
            string baseDir = Application.persistentDataPath;
            for (int i = 0; i < SaveSlotIndex.ManualSlotCount; i++)
            {
                SaveSlotIndex.SlotMeta meta = SaveSlotIndex.GetMeta(baseDir, i);
                slotNames[i] = meta != null && !string.IsNullOrEmpty(meta.name)
                    ? meta.name
                    : SaveSlotIndex.DefaultName(i);
            }
            warningSlot = -1;
            isOpen = true;
            speedBeforeOpen = Runner != null ? Runner.Speed : SimulationSpeed.Playing;
            if (Runner != null) Runner.SetSpeed(SimulationSpeed.Paused);
        }

        public void Close()
        {
            isOpen = false;
            UiClickGuard.Set("save_menu", new Rect());
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
            Rect panel = SettingsLayout.CenteredRect(460f, 262f, Screen.width, Screen.height);
            UiClickGuard.Set("save_menu", panel);
            BottomBarStyle.DrawBarBackground(panel);
            float x = panel.x + 14f;
            float y = panel.y + 12f;
            HudText.Draw(x, y, Localization.T("save_title"));
            y += 30f;
            string baseDir = Application.persistentDataPath;
            for (int i = 0; i < SaveSlotIndex.ManualSlotCount; i++)
            {
                DrawSlotRow(x, y, i, baseDir);
                y += 48f;
            }
            if (GUI.Button(new Rect(x, y + 6f, 120f, 28f), Localization.T("save_close"))) ReturnToPause();
        }

        private void DrawSlotRow(float x, float y, int slot, string baseDir)
        {
            slotNames[slot] = GUI.TextField(new Rect(x, y, 160f, 26f),
                slotNames[slot] ?? string.Empty, SaveSlotIndex.MaxNameLength);
            SaveSlotIndex.SlotMeta meta = SaveSlotIndex.GetMeta(baseDir, slot);
            string info = meta == null
                ? Localization.T("save_slot_empty")
                : Localization.T("save_slot_info", meta.savedAtTick / 24, meta.savedAtReal);
            HudText.Draw(x + 170f, y + 2f, info);
            if (GUI.Button(new Rect(x, y + 28f, 160f, 18f), Localization.T("save_rename")))
            {
                bool renamed = SaveSlotIndex.Rename(baseDir, slot, slotNames[slot]);
                warningSlot = renamed ? -1 : slot;
                if (renamed)
                {
                    SaveSlotIndex.SlotMeta updated = SaveSlotIndex.GetMeta(baseDir, slot);
                    if (updated != null) slotNames[slot] = updated.name;
                }
            }
            string hint = warningSlot == slot
                ? Localization.T("save_name_invalid", SaveSlotIndex.MaxNameLength)
                : Localization.T("save_name_hint", SaveSlotIndex.MaxNameLength);
            HudText.Draw(x + 170f, y + 28f, hint);
            if (GUI.Button(new Rect(x + 330f, y, 100f, 26f), Localization.T("save_save")) && Save != null)
            {
                Save.SaveNow(slot, slotNames[slot]);
                Close();
            }
        }

        private void ResolveRefs()
        {
            if (Save == null) Save = FindFirstObjectByType<SaveController>();
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Pause == null) Pause = GetComponent<PauseMenuView>();
            if (Pause == null) Pause = FindFirstObjectByType<PauseMenuView>();
        }
    }
}
