using UnityEngine;

namespace Citybuilder
{
    // Окно загрузки из 3 слотов или автосейва (EPIC-11 шаг 3).
    public class LoadMenuView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Контроллер загрузки. Пусто = ищется в сцене.")]
        public LoadController Load;

        private bool isOpen;

        public bool IsOpen { get { return isOpen; } }

        public void Open()
        {
            ResolveRefs();
            isOpen = true;
        }

        public void Close()
        {
            isOpen = false;
            UiClickGuard.Set("load_menu", new Rect());
        }

        private void OnGUI()
        {
            if (!isOpen) return;
            ResolveRefs();
            Rect panel = CenterRect(460f, 270f);
            UiClickGuard.Set("load_menu", panel);
            BottomBarStyle.DrawBarBackground(panel);
            float x = panel.x + 14f;
            float y = panel.y + 12f;
            HudText.Draw(x, y, "=== Загрузить город ===");
            y += 30f;
            string baseDir = Application.persistentDataPath;
            for (int i = 0; i < SaveSlotIndex.ManualSlotCount; i++)
            {
                DrawRow(x, y, i, baseDir);
                y += 38f;
            }
            DrawRow(x, y, SaveSlotIndex.AutosaveSlot, baseDir);
            y += 44f;
            if (GUI.Button(new Rect(x, y, 120f, 28f), "Закрыть")) Close();
        }

        private void DrawRow(float x, float y, int slot, string baseDir)
        {
            SaveSlotIndex.SlotMeta meta = SaveSlotIndex.GetMeta(baseDir, slot);
            string title = meta != null && !string.IsNullOrEmpty(meta.name)
                ? meta.name
                : SaveSlotIndex.DefaultName(slot);
            string label = meta == null
                ? title + " — пусто"
                : title + " — День " + (meta.savedAtTick / 24) + " (" + meta.savedAtReal + ")";
            HudText.Draw(x, y + 2f, label);
            Rect btn = new Rect(x + 330f, y, 100f, 26f);
            if (meta == null)
            {
                BottomBarStyle.DrawDisabledButton(btn, "Загрузить");
            }
            else if (GUI.Button(btn, "Загрузить") && Load != null)
            {
                Load.LoadNow(slot);
                Close();
            }
        }

        private void ResolveRefs()
        {
            if (Load == null) Load = FindFirstObjectByType<LoadController>();
        }

        private static Rect CenterRect(float width, float height)
        {
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }
    }
}
