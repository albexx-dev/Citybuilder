using UnityEngine;

namespace Citybuilder
{
    // Окно сохранения в один из 3 слотов с редактируемым именем (EPIC-11 шаг 3).
    public class SaveMenuView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер сохранения. Пусто = ищется в сцене.")]
        public SaveController Save;

        private readonly string[] slotNames = new string[SaveSlotIndex.ManualSlotCount];
        private bool isOpen;

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
            isOpen = true;
        }

        public void Close()
        {
            isOpen = false;
            UiClickGuard.Set("save_menu", new Rect());
        }

        private void OnGUI()
        {
            if (!isOpen) return;
            ResolveRefs();
            Rect panel = CenterRect(460f, 230f);
            UiClickGuard.Set("save_menu", panel);
            BottomBarStyle.DrawBarBackground(panel);
            float x = panel.x + 14f;
            float y = panel.y + 12f;
            HudText.Draw(x, y, "=== Сохранить город ===");
            y += 30f;
            string baseDir = Application.persistentDataPath;
            for (int i = 0; i < SaveSlotIndex.ManualSlotCount; i++)
            {
                DrawSlotRow(x, y, i, baseDir);
                y += 40f;
            }
            if (GUI.Button(new Rect(x, y + 6f, 120f, 28f), "Закрыть")) Close();
        }

        private void DrawSlotRow(float x, float y, int slot, string baseDir)
        {
            slotNames[slot] = GUI.TextField(new Rect(x, y, 160f, 26f), slotNames[slot] ?? string.Empty, 32);
            SaveSlotIndex.SlotMeta meta = SaveSlotIndex.GetMeta(baseDir, slot);
            string info = meta == null
                ? "пусто"
                : "День " + (meta.savedAtTick / 24) + " • " + meta.savedAtReal;
            HudText.Draw(x + 170f, y + 2f, info);
            if (GUI.Button(new Rect(x + 330f, y, 100f, 26f), "Сохранить") && Save != null)
            {
                Save.SaveNow(slot, slotNames[slot]);
                Close();
            }
        }

        private void ResolveRefs()
        {
            if (Save == null) Save = FindFirstObjectByType<SaveController>();
        }

        private static Rect CenterRect(float width, float height)
        {
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }
    }
}
