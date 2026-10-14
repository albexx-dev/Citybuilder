using UnityEngine;

namespace Citybuilder
{
    // Минимальное меню: новая / продолжить / сохранить / выйти. Только OnGUI.
    // Вешать на Grid, ссылки на контроллеры объекта Persistence вязать вручную.
    public class MainMenu : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Запись слота. Опционален: иначе ищется на этом же объекте.")]
        public SaveController Save;
        [Tooltip("Загрузка слота и новая игра. Опционален: иначе ищется на этом же объекте.")]
        public LoadController Load;

        private void OnGUI()
        {
            SaveController save = Save;
            if (save == null) save = GetComponent<SaveController>();
            LoadController load = Load;
            if (load == null) load = GetComponent<LoadController>();
            if (save == null && load == null) return;
            float width = 90f;
            float height = 26f;
            float startX = 10f;
            float startY = 182f;
            float gap = 4f;
            UiClickGuard.Set("mainmenu", new Rect(startX, startY, 4 * width + 3 * gap, height));
            string[] labels = { "Новая", "Далее", "Сохранить", "Выйти" };
            for (int i = 0; i < labels.Length; i++)
            {
                Rect rect = new Rect(startX + i * (width + gap), startY, width, height);
                if (DrawButton(rect, labels[i])) Press(i, save, load);
            }
        }

        // Одна кнопка меню по индексу. Нет своей ссылки — тихий игнор.
        private static void Press(int index, SaveController save, LoadController load)
        {
            if (index == 0 && load != null) load.NewGame();
            else if (index == 1 && load != null) load.LoadNow();
            else if (index == 2 && save != null) save.SaveNow();
            else if (index == 3) Application.Quit();
        }

        // Одна кнопка. Возвращает клик.
        private static bool DrawButton(Rect rect, string label)
        {
            return GUI.Button(rect, label);
        }
    }
}
