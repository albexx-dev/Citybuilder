using UnityEngine;

namespace Citybuilder
{
    // Геометрия HUD, нижней панели действий (EPIC-13), инфоменю (EPIC-14), ленты событий (EPIC-09) и мини-карты (EPIC-67).
    // Единственный источник прямоугольников: отсюда рисуются группы и регистрируется UiClickGuard.
    public static class HudLayout
    {
        public const float BarHeight = 38f;
        public const float ButtonHeight = 28f;
        public const float ButtonGap = 4f;
        public const float GroupGap = 14f;
        public const float BarPadding = 8f;
        public const float SpeedButtonWidth = 56f;
        public const float ModeButtonWidth = 84f;
        public const float MenuButtonWidth = 84f;
        public const float PaletteButtonWidth = 128f;
        public const float SubRowOffset = 34f;
        public const float InfoMenuTop = 10f;
        public const float InfoMenuBottomGap = 8f;
        // Каталог постройки (задача 173): карточки фиксированной ширины, переполнение скроллит CatalogScroll.
        public const float CatalogCardWidth = 112f;
        public const float CatalogCardHeight = 96f;
        public const float CatalogBottomGap = 4f;
        public const float CatalogRowGap = 6f;

        private const float BottomMargin = 72f;
        private const float LineStep = 24f;
        private const float MinHudRowY = 90f;

        // Адаптивные координаты нижних строк HUD: не наезжают на нижнюю панель.
        public static float FixedOrBottom(float preferredY, int linesFromBottom)
        {
            return FixedOrBottom(preferredY, linesFromBottom, Screen.height);
        }

        // Чистая математика для тестов без зависимости от Screen.height.
        public static float FixedOrBottom(float preferredY, int linesFromBottom, float screenHeight)
        {
            float maxY = Mathf.Max(10f, screenHeight - BottomMargin);
            if (preferredY <= maxY) return preferredY;
            return Mathf.Max(10f, maxY - linesFromBottom * LineStep);
        }

        // Видна ли нижняя строка HUD при данной высоте окна: при нехватке места строка прячется, панель — никогда.
        public static bool IsHudRowVisible(float preferredY, int linesFromBottom, float screenHeight)
        {
            float y = FixedOrBottom(preferredY, linesFromBottom, screenHeight);
            float barTop = BottomBarRect(1024f, screenHeight).y - SubRowOffset;
            return y >= MinHudRowY && y + LineStep <= barTop;
        }

        public static Rect BottomBarRect(float screenWidth, float screenHeight)
        {
            float y = Mathf.Max(0f, screenHeight - BarHeight);
            return new Rect(0f, y, Mathf.Max(0f, screenWidth), BarHeight);
        }

        public static Rect SpeedGroupRect(float screenWidth, float screenHeight)
        {
            float y = BottomBarRect(screenWidth, screenHeight).y + (BarHeight - ButtonHeight) * 0.5f;
            float width = 3f * SpeedButtonWidth + 2f * ButtonGap;
            return new Rect(BarPadding, y, width, ButtonHeight);
        }

        public static Rect ModeGroupRect(float screenWidth, float screenHeight)
        {
            Rect speed = SpeedGroupRect(screenWidth, screenHeight);
            float x = speed.xMax + GroupGap;
            float width = 6f * ModeButtonWidth + 5f * ButtonGap;
            return new Rect(x, speed.y, width, ButtonHeight);
        }

        public static Rect MenuButtonsRect(float screenWidth, float screenHeight)
        {
            Rect speed = SpeedGroupRect(screenWidth, screenHeight);
            float width = 3f * MenuButtonWidth + 2f * ButtonGap;
            float x = Mathf.Max(ModeGroupRect(screenWidth, screenHeight).xMax + GroupGap, screenWidth - width - BarPadding);
            return new Rect(x, speed.y, width, ButtonHeight);
        }

        public static int MaxPaletteItems(float screenWidth)
        {
            float modeX = ModeGroupRect(screenWidth, 0f).x;
            float maxWidth = Mathf.Max(0f, screenWidth - modeX - BarPadding);
            return Mathf.Max(1, Mathf.FloorToInt((maxWidth + ButtonGap) / (PaletteButtonWidth + ButtonGap)));
        }

        // Попап над панелью: палитра зданий (режим Дома) или кисти зон (режим Зоны).
        public static Rect PaletteRect(float screenWidth, float screenHeight, int itemCount)
        {
            if (itemCount <= 0) return new Rect();
            Rect mode = ModeGroupRect(screenWidth, screenHeight);
            float maxWidth = Mathf.Max(0f, screenWidth - mode.x - BarPadding);
            int count = Mathf.Min(itemCount, MaxPaletteItems(screenWidth));
            float width = Mathf.Min(count * PaletteButtonWidth + (count - 1) * ButtonGap, maxWidth);
            float y = Mathf.Max(0f, BottomBarRect(screenWidth, screenHeight).y - SubRowOffset);
            return new Rect(mode.x, y, Mathf.Max(0f, width), ButtonHeight);
        }

        // Лента карточек каталога: ширина по содержимому (PaletteRect-образец), переполнение скроллится.
        public static Rect CatalogCardsRect(float screenWidth, float screenHeight, int itemCount)
        {
            if (itemCount <= 0) return new Rect();
            float content = itemCount * CatalogCardWidth + (itemCount - 1) * ButtonGap;
            float width = Mathf.Min(content, Mathf.Max(0f, screenWidth - 2f * BarPadding));
            float y = BottomBarRect(screenWidth, screenHeight).y - CatalogBottomGap - CatalogCardHeight;
            return new Rect(BarPadding, y, width, CatalogCardHeight);
        }

        // Ряд вкладок категорий над лентой: полная ширина экрана; ярлыки, не поместившиеся, скроллятся.
        public static Rect CatalogTabsRect(float screenWidth, float screenHeight)
        {
            float y = CatalogCardsRect(screenWidth, screenHeight, 1).y - CatalogRowGap - ButtonHeight;
            return new Rect(BarPadding, y, Mathf.Max(0f, screenWidth - 2f * BarPadding), ButtonHeight);
        }

        // Информационное меню справа (~половина экрана), всегда выше нижней панели и её попапа.
        public static Rect InfoMenuRect(float screenWidth, float screenHeight)
        {
            float width = Mathf.Clamp(screenWidth * 0.48f, 360f, 640f);
            float x = Mathf.Max(BarPadding, screenWidth - width - BarPadding);
            float maxBottom = BottomBarRect(screenWidth, screenHeight).y - SubRowOffset - InfoMenuBottomGap;
            float height = Mathf.Max(120f, maxBottom - InfoMenuTop);
            return new Rect(x, InfoMenuTop, width, height);
        }

        // Окно ленты событий (EPIC-09 шаг 2).
        public static Rect EventLogRect(float screenWidth, float screenHeight)
        {
            return InfoMenuRect(screenWidth, screenHeight);
        }

        // Окно задач-вех (EPIC-48): та же зона, панели взаимоисключающие.
        public static Rect QuestLogRect(float screenWidth, float screenHeight) => InfoMenuRect(screenWidth, screenHeight);

        public static Rect MinimapRect(float screenWidth, float screenHeight) => MinimapMapping.MinimapRect(screenWidth, screenHeight, BarPadding); // EPIC-67: геометрию и видимость считает MinimapMapping
        public static bool IsMinimapVisible(bool infoMenuOpen, bool eventLogOpen) => MinimapMapping.IsMinimapVisible(infoMenuOpen, eventLogOpen);

        // Инфопанель выбранного здания в правом нижнем углу над нижней панелью (EPIC-12 шаг 2).
        public static Rect InfoPanelRect(float screenWidth, float screenHeight)
        {
            const float width = 320f;
            const float defaultHeight = 166f;
            float x = Mathf.Max(BarPadding, screenWidth - width - BarPadding);
            float maxBottom = BottomBarRect(screenWidth, screenHeight).y - SubRowOffset - InfoMenuBottomGap;
            float availableHeight = Mathf.Max(0f, maxBottom - InfoMenuTop);
            float height = Mathf.Min(defaultHeight, availableHeight);
            float y = Mathf.Max(InfoMenuTop, maxBottom - height);
            return new Rect(x, y, width, height);
        }
    }
}
