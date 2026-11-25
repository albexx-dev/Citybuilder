using System;
using UnityEngine;

namespace Citybuilder
{
    // Зум мини-карты: уровень 1..4 и центр окна в координатах текстуры.
    // Состояние живёт в памяти сессии: запись в профиль означает расширить GameSettings и
    // GameSettingsStore, а issue #119 требует на это ADR; в этой задаче сейв не трогаем.
    // Геометрия — чистые функции, чтобы тестировать зум без IMGUI и сцены.
    public class MinimapZoom
    {
        public const int MinLevel = 1;
        public const int MaxLevel = 4;
        private const float ButtonSize = 16f;

        private int level = MinLevel;
        private float centerX;
        private float centerY;

        public MinimapZoom(int textureSize) { Reset(textureSize); }

        public int Level { get { return level; } }
        public float CenterX { get { return centerX; } }
        public float CenterY { get { return centerY; } }

        public void Reset(int textureSize)
        {
            level = MinLevel;
            float half = textureSize * 0.5f;
            ClampCenter(half, half, textureSize, ViewSize(textureSize), out centerX, out centerY);
        }

        // Кнопки и колесо: вернуть true, если уровень реально изменился.
        public bool Step(int delta, int textureSize)
        {
            int next = level + delta;
            if (next < MinLevel) next = MinLevel;
            if (next > MaxLevel) next = MaxLevel;
            if (next == level) return false;
            level = next;
            float x, y;
            ClampCenter(centerX, centerY, textureSize, ViewSize(textureSize), out x, out y);
            centerX = x;
            centerY = y;
            return true;
        }

        // Клик/колесо ведут центр окна к точке под курсором, не вылезая за текстуру.
        public void Focus(float pixelX, float pixelY, int textureSize)
        {
            ClampCenter(pixelX, pixelY, textureSize, ViewSize(textureSize), out centerX, out centerY);
        }

        // Сторона видимого окна в пикселях текстуры: при зуме 1 видна вся схема.
        public int ViewSize(int textureSize)
        {
            int texture = SafeSize(textureSize);
            int view = (int)Math.Ceiling((float)texture / level);
            if (view < 1) view = 1;
            return view > texture ? texture : view;
        }

        // UV-окно для GUI.DrawTextureWithTexCoords: v=0 — нижний ряд текстуры.
        public Rect TexCoords(int textureSize)
        {
            int texture = SafeSize(textureSize);
            int view = ViewSize(textureSize);
            float half = view * 0.5f;
            float x, y;
            ClampCenter(centerX, centerY, texture, view, out x, out y);
            return new Rect((x - half) / texture, (y - half) / texture,
                (float)view / texture, (float)view / texture);
        }

        // Точка rect'а → пиксель текстуры с зумом; на уровне 1 совпадает с RectToPixel.
        public void RectToPixel(Rect rect, float guiX, float guiY, int textureSize,
            out float pixelX, out float pixelY)
        {
            int texture = SafeSize(textureSize);
            int view = ViewSize(textureSize);
            float half = view * 0.5f;
            float x, y;
            ClampCenter(centerX, centerY, texture, view, out x, out y);
            float fx = Fraction(guiX - rect.x, rect.width);
            // GUI-y растёт вниз, а v=0 текстуры — нижний ряд (конвенция RectToPixel).
            float fy = 1f - Fraction(guiY - rect.y, rect.height);
            pixelX = ClampAxis(x + (fx - 0.5f) * view, 0f, texture);
            pixelY = ClampAxis(y + (fy - 0.5f) * view, 0f, texture);
        }

        // Кнопки внутри rect'а: гард мини-карты уже покрывает их.
        public static void ButtonRects(Rect rect, out Rect plus, out Rect minus)
        {
            float size = Mathf.Min(ButtonSize, Mathf.Min(rect.width * 0.2f, rect.height * 0.2f));
            plus = new Rect(rect.x + 2f, rect.y + 2f, size, size);
            minus = new Rect(rect.x + 2f, rect.y + 4f + size, size, size);
        }

        private static void ClampCenter(float x, float y, int textureSize, int viewSize,
            out float resultX, out float resultY)
        {
            int texture = SafeSize(textureSize);
            int view = viewSize < 1 ? 1 : (viewSize > texture ? texture : viewSize);
            float half = view * 0.5f;
            resultX = ClampAxis(x, half, texture - half);
            resultY = ClampAxis(y, half, texture - half);
        }

        private static float ClampAxis(float value, float min, float max)
        {
            if (float.IsNaN(value)) return min;
            if (value < min) return min;
            return value > max ? max : value;
        }

        private static float Fraction(float value, float size)
        {
            if (size <= 0f || float.IsNaN(value)) return 0f;
            return Mathf.Clamp01(value / size);
        }

        private static int SafeSize(int value) { return value > 0 ? value : 1; }
    }
}
