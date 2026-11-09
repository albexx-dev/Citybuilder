using UnityEngine;

namespace Citybuilder
{
    // Геометрия HUD и нижней панели действий (EPIC-13).
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
        public const float MenuButtonWidth = 92f;
        public const float PaletteButtonWidth = 128f;
        public const float SubRowOffset = 34f;

        private const float BottomMargin = 72f;
        private const float LineStep = 24f;
        private const float MinHudRowY = 404f;

        // Адаптивные координаты нижних строк HUD: не наезжают на нижнюю панель.
        public static float FixedOrBottom(float preferredY, int linesFromBottom)
        {
            return FixedOrBottom(preferredY, linesFromBottom, Screen.height);
        }

        // Чистая математика для тестов без зависимости от Screen.height.
        public static float FixedOrBottom(float preferredY, int linesFromBottom, float screenHeight)
        {
            float maxY = Mathf.Max(10f, screenHeight - BottomMargin);
            if (preferredY <= maxY)
            {
                return preferredY;
            }
            return Mathf.Max(10f, maxY - linesFromBottom * LineStep);
        }

        // Видна ли нижняя справочная строка HUD при данной высоте окна (панель видна всегда).
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
            float width = 5f * ModeButtonWidth + 4f * ButtonGap;
            return new Rect(x, speed.y, width, ButtonHeight);
        }

        public static Rect MenuButtonsRect(float screenWidth, float screenHeight)
        {
            Rect speed = SpeedGroupRect(screenWidth, screenHeight);
            float width = 2f * MenuButtonWidth + ButtonGap;
            float x = Mathf.Max(ModeGroupRect(screenWidth, screenHeight).xMax + GroupGap, screenWidth - width - BarPadding);
            return new Rect(x, speed.y, width, ButtonHeight);
        }

        // Попап над панелью: палитра зданий (режим Дома) или кисти зон (режим Зоны).
        public static Rect PaletteRect(float screenWidth, float screenHeight, int itemCount)
        {
            if (itemCount <= 0) return new Rect();
            Rect mode = ModeGroupRect(screenWidth, screenHeight);
            float width = itemCount * PaletteButtonWidth + (itemCount - 1) * ButtonGap;
            float y = Mathf.Max(0f, BottomBarRect(screenWidth, screenHeight).y - SubRowOffset);
            return new Rect(mode.x, y, width, ButtonHeight);
        }
    }
}
