using System;
using UnityEngine;

namespace Citybuilder
{
    // Чистая математика мини-карты (EPIC-67 шаг 1): клетка сетки ↔ пиксель текстуры ↔ мир, плюс геометрия окна.
    // Вся сетка gridSize×gridSize растягивается в текстуру textureSize×textureSize, масштаб s = textureSize / gridSize.
    // Прямой переход берёт центр клетки (cell + 0.5) в пиксельных координатах, обратный — точку внутри пикселя
    // (обычно его центр, pixel + 0.5). Поэтому round-trip клетка→пиксель→клетка точен при s >= 1.
    public static class MinimapMapping
    {
        // Сторона квадрата мини-карты в пикселях (план EPIC-67: 140×140).
        public const float SidePixels = 140f;

        // Пиксель текстуры, в который попадает центр клетки. Результат зажат границами текстуры.
        public static void CellToPixel(int cellX, int cellY, int gridSize, int textureSize,
            out int pixelX, out int pixelY)
        {
            pixelX = CellToAxis(cellX, gridSize, textureSize);
            pixelY = CellToAxis(cellY, gridSize, textureSize);
        }

        // Клетка, которой принадлежит точка в координатах текстуры (клик по мини-карте).
        // Отрицательные значения и NaN → 0, запредельные → gridSize - 1.
        public static void PixelToCell(float pixelX, float pixelY, int textureSize, int gridSize,
            out int cellX, out int cellY)
        {
            cellX = PixelToAxis(pixelX, textureSize, gridSize);
            cellY = PixelToAxis(pixelY, textureSize, gridSize);
        }

        // Центр клетки в мировых метрах: то же правило, что GridSystem.GridToWorld ((x + 0.5) * cellSize).
        public static void CellToWorld(int cellX, int cellY, float cellSize, out float worldX, out float worldZ)
        {
            worldX = (cellX + 0.5f) * cellSize;
            worldZ = (cellY + 0.5f) * cellSize;
        }

        // Квадрат мини-карты SidePixels×SidePixels в правом верхнем углу с отступом padding (GUI: y вниз).
        // В низком/узлом окне сторона ужимается, чтобы rect целиком остался на экране.
        public static Rect MinimapRect(float screenWidth, float screenHeight, float padding)
        {
            float byWidth = Mathf.Max(0f, screenWidth - 2f * padding);
            float byHeight = Mathf.Max(0f, screenHeight - 2f * padding);
            float size = Mathf.Min(SidePixels, Mathf.Min(byWidth, byHeight));
            float x = Mathf.Max(padding, screenWidth - size - padding);
            return new Rect(x, padding, size, size);
        }

        // Правый верх занят инфоменю и лентой событий (EPIC-14/EPIC-09), поэтому мини-карта
        // показывается, только когда оба окна закрыты. Чистая функция — спрашивать каждый кадр.
        public static bool IsMinimapVisible(bool infoMenuOpen, bool eventLogOpen)
        {
            return !infoMenuOpen && !eventLogOpen;
        }

        private static int CellToAxis(int cell, int gridSize, int textureSize)
        {
            int grid = SafeSize(gridSize);
            int texture = SafeSize(textureSize);
            float scale = (float)texture / grid;
            return ClampIndex((int)Math.Floor((cell + 0.5f) * scale), texture);
        }

        private static int PixelToAxis(float pixel, int textureSize, int gridSize)
        {
            int grid = SafeSize(gridSize);
            int texture = SafeSize(textureSize);
            if (float.IsNaN(pixel) || pixel < 0f)
            {
                return 0;
            }
            if (pixel >= texture)
            {
                return grid - 1;
            }
            return ClampIndex((int)Math.Floor(pixel * ((float)grid / texture)), grid);
        }

        private static int ClampIndex(int index, int size)
        {
            if (index < 0)
            {
                return 0;
            }
            if (index > size - 1)
            {
                return size - 1;
            }
            return index;
        }

        // Нулевой или отрицательный размер не должен уронить расчёт на чужом вводе.
        private static int SafeSize(int value)
        {
            return value > 0 ? value : 1;
        }
    }
}
