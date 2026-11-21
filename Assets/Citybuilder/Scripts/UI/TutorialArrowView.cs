using UnityEngine;

namespace Citybuilder
{
    // Стрелка туториала v3 (EPIC-70): пульсирующая жёлтая рамка и «прыгающий» треугольник
    // над целевой кнопкой. Чистая математика (пульс, rect-кнопок) тестируется отдельно,
    // Draw — единственное место с IMGUI. Вызывается из TutorialView.
    public static class TutorialArrowView
    {
        private const float FramePadMin = 2f;
        private const float FramePadExtra = 2f;
        private const float AlphaMin = 0.55f;
        private const float AlphaRange = 0.4f;
        private const float ArrowGap = 4f;
        private const float ArrowWidth = 16f;
        private const float ArrowHeight = 10f;
        private const float BounceAmplitude = 4f;
        public const float PulseSpeed = 4f;
        public const int X4SpeedIndex = 2;

        private static Texture2D arrowTexture;

        // Синус-пульс 0..1: фаза толщины рамки, альфы и отскока стрелки.
        public static float Pulse(float time)
        {
            return 0.5f + 0.5f * Mathf.Sin(time * PulseSpeed);
        }

        // Вертикальный отскок стрелки в пикселях (0..BounceAmplitude).
        public static float ArrowBounce(float time)
        {
            return BounceAmplitude * (0.5f + 0.5f * Mathf.Sin(time * PulseSpeed));
        }

        // Рамка вокруг кнопки: отступ (толщина) растёт с фазой пульса.
        public static Rect FrameRect(Rect button, float pulse)
        {
            float pad = FramePadMin + FramePadExtra * pulse;
            return new Rect(button.x - pad, button.y - pad, button.width + 2f * pad, button.height + 2f * pad);
        }

        // Кнопка режима на нижней панели: индекс как в BottomBarView.DrawModeGroup.
        public static Rect ModeButtonRect(float screenWidth, float screenHeight, TutorialPolicy.TargetButton target)
        {
            int index = ModeIndex(target);
            if (index < 0) return new Rect();
            Rect group = HudLayout.ModeGroupRect(screenWidth, screenHeight);
            float x = group.x + index * (HudLayout.ModeButtonWidth + HudLayout.ButtonGap);
            return new Rect(x, group.y, HudLayout.ModeButtonWidth, group.height);
        }

        // Кнопка скорости: индекс как в BottomBarView.DrawSpeedGroup (0 «Пауза», 1 «Игра», 2 «x4»).
        public static Rect SpeedButtonRect(float screenWidth, float screenHeight, int index)
        {
            Rect group = HudLayout.SpeedGroupRect(screenWidth, screenHeight);
            float x = group.x + index * (HudLayout.SpeedButtonWidth + HudLayout.ButtonGap);
            return new Rect(x, group.y, HudLayout.SpeedButtonWidth, group.height);
        }

        // Кнопка здания в попапе палитры: та же математика, что в BottomBarView.DrawPaletteRow.
        public static Rect PaletteButtonRect(float screenWidth, float screenHeight, int index, int itemCount)
        {
            Rect popup = HudLayout.PaletteRect(screenWidth, screenHeight, itemCount);
            float x = popup.x + index * (HudLayout.PaletteButtonWidth + HudLayout.ButtonGap);
            return new Rect(x, popup.y, HudLayout.PaletteButtonWidth, popup.height);
        }

        // Карточка каталога: та же математика, что в CatalogCards.Draw (эпик 97).
        public static Rect CatalogCardRect(float screenWidth, float screenHeight, int itemCount, int index)
        {
            if (itemCount <= 0 || index < 0) return new Rect();
            Rect strip = HudLayout.CatalogCardsRect(screenWidth, screenHeight, itemCount);
            float x = strip.x + index * (HudLayout.CatalogCardWidth + HudLayout.ButtonGap);
            return new Rect(x, strip.y, HudLayout.CatalogCardWidth, strip.height);
        }

        // Индекс кнопки режима: «Здания [1]», «Зоны [3]», «Рост [4]» — порядок из BottomBarView.
        public static int ModeIndex(TutorialPolicy.TargetButton target)
        {
            switch (target)
            {
                case TutorialPolicy.TargetButton.Houses: return 1;
                case TutorialPolicy.TargetButton.Zones: return 3;
                case TutorialPolicy.TargetButton.Growth: return 4;
                default: return -1;
            }
        }

        // Рисует рамку и стрелку. Пустой rect (цели нет) молча пропускается.
        public static void Draw(Rect target, float time)
        {
            if (target.width <= 0f || target.height <= 0f) return;
            float pulse = Pulse(time);
            Color previous = GUI.color;
            GUI.color = new Color(1f, 0.85f, 0.1f, AlphaMin + AlphaRange * pulse);
            GUI.Box(FrameRect(target, pulse), GUIContent.none);
            GUI.DrawTexture(ArrowRect(target, time), ArrowTexture());
            GUI.color = previous;
        }

        // Прямоугольник стрелки над рамкой: верх цели минус высота стрелки и отскок.
        private static Rect ArrowRect(Rect target, float time)
        {
            float top = target.y - ArrowHeight - ArrowGap - ArrowBounce(time);
            return new Rect(target.x + target.width * 0.5f - ArrowWidth * 0.5f, top, ArrowWidth, ArrowHeight);
        }

        // Треугольник вершиной вниз, собранный один раз в текстуру.
        private static Texture2D ArrowTexture()
        {
            if (arrowTexture != null) return arrowTexture;
            arrowTexture = new Texture2D(16, 10, TextureFormat.RGBA32, false);
            FillTriangle(arrowTexture);
            arrowTexture.wrapMode = TextureWrapMode.Clamp;
            arrowTexture.filterMode = FilterMode.Point;
            return arrowTexture;
        }

        private static void FillTriangle(Texture2D texture)
        {
            int half = texture.width / 2;
            for (int y = 0; y < texture.height; y++)
            {
                float span = (float)(texture.height - 1 - y) / (texture.height - 1) * half;
                for (int x = 0; x < texture.width; x++)
                {
                    bool inside = Mathf.Abs(x - half + 0.5f) <= span + 0.5f;
                    texture.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            }
            texture.Apply();
        }
    }
}
