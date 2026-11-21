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
        // 340 -> 372: слайдер «Эмбиент» (#71); 372 -> 448: кнопка повтора и справка (#97).
        public const float PanelHeight = 448f;
        public const int AgentStep = 8;
        public const int AutosaveStep = 5;
        // Ряд кнопок меню нижней панели: «Статистика/Лента/Задачи/Настройки» (4-я — EPIC-69).
        public const int MenuButtonCount = 4;
        // Узкое окно: 4 кнопки не помещаются справа от режимов, используются два ряда слева.
        public const float NarrowWidth = 900f;
        public const int MenuRowCount = 2;
        public const int NarrowColumns = 2;

        public static bool IsNarrow(float screenWidth) { return screenWidth < NarrowWidth; }

        // Центр экрана, как у меню сохранения (SaveMenuView.CenterRect), с клампом по окну.
        public static Rect PanelRect(float screenWidth, float screenHeight)
        {
            return CenteredRect(PanelWidth, PanelHeight, screenWidth, screenHeight);
        }

        // Центрированная панель целиком внутри экрана, даже когда окно меньше панели.
        public static Rect CenteredRect(float width, float height, float screenWidth, float screenHeight)
        {
            float w = Mathf.Min(width, Mathf.Max(0f, screenWidth));
            float h = Mathf.Min(height, Mathf.Max(0f, screenHeight));
            float x = Mathf.Clamp((screenWidth - w) * 0.5f, 0f, Mathf.Max(0f, screenWidth - w));
            float y = Mathf.Clamp((screenHeight - h) * 0.5f, 0f, Mathf.Max(0f, screenHeight - h));
            return new Rect(x, y, w, h);
        }

        // Ряд 4 кнопок меню: продолжение HudLayout.MenuButtonsRect. При узком окне ряд
        // сжимается, чтобы не наезжать на группу режимов (семантика BottomBarLayoutTests).
        public static Rect MenuButtonsRect(float screenWidth, float screenHeight)
        {
            if (IsNarrow(screenWidth)) return MenuRowRect(screenWidth, screenHeight, MenuRowCount - 1);
            Rect mode = HudLayout.ModeGroupRect(screenWidth, screenHeight);
            float natural = MenuButtonCount * HudLayout.MenuButtonWidth + (MenuButtonCount - 1) * HudLayout.ButtonGap;
            float available = Mathf.Max(0f, screenWidth - HudLayout.BarPadding - mode.xMax - HudLayout.GroupGap);
            float width = Mathf.Min(natural, available);
            float x = Mathf.Max(mode.xMax + HudLayout.GroupGap, screenWidth - width - HudLayout.BarPadding);
            return new Rect(x, HudLayout.SpeedGroupRect(screenWidth, screenHeight).y, width, HudLayout.ButtonHeight);
        }

        // Узкий fallback: два ряда по две кнопки слева над каталогом (включая вкладки).
        public static Rect MenuRowRect(float screenWidth, float screenHeight, int row)
        {
            float subTop = HudLayout.CatalogCardsRect(screenWidth, screenHeight, 1).y
                - HudLayout.CatalogRowGap - HudLayout.ButtonHeight;
            float y = subTop - HudLayout.ButtonGap - HudLayout.ButtonHeight;
            y -= (MenuRowCount - 1 - row) * (HudLayout.ButtonHeight + HudLayout.ButtonGap);
            float natural = NarrowColumns * HudLayout.MenuButtonWidth + (NarrowColumns - 1) * HudLayout.ButtonGap;
            float width = Mathf.Min(natural, Mathf.Max(0f, screenWidth - 2f * HudLayout.BarPadding));
            return new Rect(HudLayout.BarPadding, Mathf.Max(0f, y), width, HudLayout.ButtonHeight);
        }

        // Прямоугольник кнопки в ряду: ширина ряда делится поровну между кнопками.
        public static Rect MenuButtonRect(Rect row, int index)
        {
            float buttonWidth = (row.width - (MenuButtonCount - 1) * HudLayout.ButtonGap) / MenuButtonCount;
            return new Rect(row.x + index * (buttonWidth + HudLayout.ButtonGap), row.y, buttonWidth, row.height);
        }

        // Кнопка меню по индексу: широкий ряд или две колонки узкого fallback-а.
        public static Rect MenuButtonRect(float screenWidth, float screenHeight, int index)
        {
            if (!IsNarrow(screenWidth)) return MenuButtonRect(MenuButtonsRect(screenWidth, screenHeight), index);
            Rect row = MenuRowRect(screenWidth, screenHeight, index / NarrowColumns);
            float width = (row.width - (NarrowColumns - 1) * HudLayout.ButtonGap) / NarrowColumns;
            return new Rect(row.x + (index % NarrowColumns) * (width + HudLayout.ButtonGap), row.y, width, row.height);
        }

        // Компактный баннер туториала: строка слева, кнопка «Понятно» справа от текста.
        // Ряд идёт под мини-картой: при 1024×600 старый y=10 пересекал её квадрат 8..148.
        public static Rect CompactBannerRect(float screenWidth, float screenHeight)
        {
            float dismissX = CompactDismissRect(screenWidth, screenHeight).x;
            float width = Mathf.Max(0f, dismissX - 2f * HudLayout.BarPadding - HudLayout.ButtonGap);
            return new Rect(HudLayout.BarPadding, CompactTop(screenWidth, screenHeight), width, HudLayout.ButtonHeight);
        }

        public static Rect CompactDismissRect(float screenWidth, float screenHeight)
        {
            float x = Mathf.Max(HudLayout.BarPadding + 90f, screenWidth - 100f);
            return new Rect(x, CompactTop(screenWidth, screenHeight) - 2f, 90f, HudLayout.ButtonHeight);
        }

        // Верх компактного ряда: низ мини-карты + зазор, с клампом внутри окна.
        private static float CompactTop(float screenWidth, float screenHeight)
        {
            float belowMap = HudLayout.MinimapRect(screenWidth, screenHeight).yMax + HudLayout.ButtonGap;
            return Mathf.Min(belowMap, screenHeight - HudLayout.ButtonHeight - 2f);
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
            return Localization.T("pause_volume", Mathf.RoundToInt(volume * 100f));
        }

        // Подпись слайдера эмбиента: тот же формат 0..100%, что у общей громкости (хвост #71).
        public static string AmbientLabel(float ambientVolume)
        {
            return Localization.T("settings_ambient", Mathf.RoundToInt(ambientVolume * 100f));
        }

        public static string AgentLimitLabel(int limit)
        {
            return Localization.T("settings_agent_limit", limit);
        }

        public static string AutosaveLabel(int hours)
        {
            return Localization.T("settings_autosave", hours);
        }
    }
}
