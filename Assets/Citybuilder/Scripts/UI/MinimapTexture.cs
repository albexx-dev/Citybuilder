using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Схематичная текстура города для мини-карты (EPIC-67 шаг 2): фон, зоны, дороги, здания.
    // Только чтение данных города и пиксельная математика; IMGUI-рисование — в MinimapView.
    // Цвета зон совпадают с 3D-видом (Presentation/ZoneView.cs:50-52), чтобы схема не расходилась с картой.
    // Texture2D и Color[] создаются один раз и переиспользуются, RenderTexture не используется (запрет плана).
    public static class MinimapTexture
    {
        // Сторона схемы в пикселях: 1:1 с окном мини-карты (140).
        public const int Size = (int)MinimapMapping.SidePixels;

        private const float RefreshIntervalSec = 0.5f;

        public static readonly Color BackgroundColor = new Color(0.13f, 0.15f, 0.17f);
        public static readonly Color ResidentialColor = new Color(0.1f, 0.45f, 0.1f);
        public static readonly Color CommercialColor = new Color(0.1f, 0.2f, 0.6f);
        public static readonly Color IndustrialColor = new Color(0.6f, 0.3f, 0.05f);
        public static readonly Color RoadColor = new Color(0.45f, 0.45f, 0.45f);
        // Здания — одна светлая точка в клетке Origin; дом и станция одним цветом (объём шага).
        public static readonly Color BuildingColor = new Color(0.92f, 0.93f, 0.95f);

        private static Texture2D texture;
        private static Color[] buffer;
        private static float nextRefreshTime;

        // Схема для рисования; null, пока её нечем заполнить.
        public static Texture2D Current { get { return texture; } }

        // Забыть таймер: следующее обновление произойдёт сразу (тесты, смена города).
        public static void Reset() { nextRefreshTime = 0f; }

        // Перезаполнить не чаще раза в 0.5 с реального времени (Time.unscaledTime — прецедент InfoMenuView).
        // Возвращает true, если перекрасили; без сетки молча пропускает.
        public static bool RefreshIfDue(GridSystem grid, ZoneSystem zones, List<RoadState> roads,
            List<BuildingState> homes, List<BuildingState> placed)
        {
            if (grid == null || Time.unscaledTime < nextRefreshTime) return false;
            nextRefreshTime = Time.unscaledTime + RefreshIntervalSec;
            Repaint(grid, zones, roads, homes, placed);
            return true;
        }

        // Полное перезаполнение: фон, затем зоны, дороги, здания (поверх — порядок слоёв схемы).
        private static void Repaint(GridSystem grid, ZoneSystem zones, List<RoadState> roads,
            List<BuildingState> homes, List<BuildingState> placed)
        {
            EnsureBuffers();
            for (int i = 0; i < buffer.Length; i++) buffer[i] = BackgroundColor;
            PaintZones(grid, zones);
            PaintRoads(grid, roads);
            PaintBuildings(grid, homes);
            PaintBuildings(grid, placed);
            texture.SetPixels(buffer);
            texture.Apply();
        }

        private static void PaintZones(GridSystem grid, ZoneSystem zones)
        {
            if (zones == null) return;
            List<KeyValuePair<GridPosition, ZoneType>> cells = zones.GetZonedCells();
            for (int i = 0; i < cells.Count; i++)
            {
                PaintCell(grid, cells[i].Key, ColorForZone(cells[i].Value));
            }
        }

        // Дороги: серый по клетке RoadState.Cell. Список может быть null (система не найдена).
        private static void PaintRoads(GridSystem grid, List<RoadState> roads)
        {
            if (roads == null) return;
            for (int i = 0; i < roads.Count; i++)
            {
                if (roads[i] != null) PaintCell(grid, roads[i].Cell, RoadColor);
            }
        }

        // Здание — точка в клетке Origin: габарит на 140 пикселях всё равно спорит с зонами.
        private static void PaintBuildings(GridSystem grid, List<BuildingState> states)
        {
            if (states == null) return;
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i] != null) PaintCell(grid, states[i].Origin, BuildingColor);
            }
        }

        private static Color ColorForZone(ZoneType zone)
        {
            switch (zone)
            {
                case ZoneType.Residential: return ResidentialColor;
                case ZoneType.Commercial: return CommercialColor;
                case ZoneType.Industrial: return IndustrialColor;
                default: return BackgroundColor;
            }
        }

        // Пиксельный блок клетки: [PixelStart(cell), PixelStart(cell + 1)) по обеим осям.
        // Строка 0 буфера — нижняя (так же считает Texture2D.SetPixels), поэтому v=0 = клетка Y = 0.
        private static void PaintCell(GridSystem grid, GridPosition cell, Color color)
        {
            if (!grid.IsInside(cell)) return;
            int x0 = PixelStart(cell.X, grid.Size.Width);
            int y0 = PixelStart(cell.Y, grid.Size.Height);
            int x1 = PixelStart(cell.X + 1, grid.Size.Width);
            int y1 = PixelStart(cell.Y + 1, grid.Size.Height);
            for (int y = y0; y < y1; y++)
            {
                for (int x = x0; x < x1; x++) buffer[y * Size + x] = color;
            }
        }

        private static int PixelStart(int cell, int gridCells)
        {
            if (gridCells <= 0 || cell < 0) return 0;
            return Mathf.Min((int)(cell * ((float)Size / gridCells)), Size);
        }

        private static void EnsureBuffers()
        {
            if (texture != null && buffer != null) return;
            if (texture == null)
            {
                texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
            }
            if (buffer == null) buffer = new Color[Size * Size];
        }
    }
}
