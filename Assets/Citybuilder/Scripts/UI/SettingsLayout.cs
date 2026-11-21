using UnityEngine;

namespace Citybuilder
{
    // Геометрия и подписи окна настроек (EPIC-69): чистая математика без MonoBehaviour,
    // проверяется через dotnet по образцу QuestLogLayoutTests. Окно открывается из меню
    // паузы (план эпика: «Пауза → Настройки») и кнопкой в нижней панели. HudLayout не
    // трогается: там лимит строк и контракт тестов EPIC-48, поэтому расширенный ряд
    // из 4 кнопок меню нижней панели считается здесь.
    public static class SettingsLayout
    {
        public const float PanelWidth = 460f;
        // 340 -> 372: строка слайдера «Эмбиент» в блоке «Звук» (хвост #71).
        public const float PanelHeight = 372f;
        public const int AgentStep = 8;
        public const int AutosaveStep = 5;
        // Ряд кнопок меню нижней панели: «Статистика/Лента/Задачи/Настройки» (4-я — EPIC-69).
        public const int MenuButtonCount = 4;

        // Центр экрана, как у меню сохранения (SaveMenuView.CenterRect).
        public static Rect PanelRect(float screenWidth, float screenHeight)
        {
            return new Rect((screenWidth - PanelWidth) * 0.5f, (screenHeight - PanelHeight) * 0.5f, PanelWidth, PanelHeight);
        }

        // Ряд 4 кнопок меню: продолжение HudLayout.MenuButtonsRect. При узком окне ряд
        // сжимается, чтобы не наезжать на группу режимов (семантика BottomBarLayoutTests).
        public static Rect MenuButtonsRect(float screenWidth, float screenHeight)
        {
            Rect mode = HudLayout.ModeGroupRect(screenWidth, screenHeight);
            float natural = MenuButtonCount * HudLayout.MenuButtonWidth + (MenuButtonCount - 1) * HudLayout.ButtonGap;
            float available = Mathf.Max(0f, screenWidth - HudLayout.BarPadding - mode.xMax - HudLayout.GroupGap);
            float width = Mathf.Min(natural, available);
            float x = Mathf.Max(mode.xMax + HudLayout.GroupGap, screenWidth - width - HudLayout.BarPadding);
            return new Rect(x, HudLayout.SpeedGroupRect(screenWidth, screenHeight).y, width, HudLayout.ButtonHeight);
        }

        // Прямоугольник кнопки в ряду: ширина ряда делится поровну между кнопками.
        public static Rect MenuButtonRect(Rect row, int index)
        {
            float buttonWidth = (row.width - (MenuButtonCount - 1) * HudLayout.ButtonGap) / MenuButtonCount;
            return new Rect(row.x + index * (buttonWidth + HudLayout.ButtonGap), row.y, buttonWidth, row.height);
        }

        // Шаг степпера с клампом: за границей диапазона остаёмся на границе.
        public static int StepClamped(int current, int delta, int min, int max)
        {
            int next = current + delta;
            if (next < min) return min;
            return next > max ? max : next;
        }

        public static string VolumeLabel(float volume)
        {
            return "Громкость: " + Mathf.RoundToInt(volume * 100f) + "%";
        }

        // Подпись слайдера эмбиента: тот же формат 0..100%, что у общей громкости (хвост #71).
        public static string AmbientLabel(float ambientVolume)
        {
            return "Эмбиент: " + Mathf.RoundToInt(ambientVolume * 100f) + "%";
        }

        public static string AgentLimitLabel(int limit)
        {
            return "Машины: лимит " + limit;
        }

        public static string AutosaveLabel(int hours)
        {
            return "Автосейв: каждые " + hours + " ч";
        }
    }
}
