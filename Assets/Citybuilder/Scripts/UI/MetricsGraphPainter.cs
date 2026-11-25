using UnityEngine;

namespace Citybuilder
{
    // Отрисовка ступенчатой линии графика в пиксельный буфер (вынесено из MetricsGraphTexture).
    // Чистый C#: буфер и размеры приходят от вызывающего, аллокаций нет.
    public static class MetricsGraphPainter
    {
        // Горизонтальный пролёт от прошлой точки и вертикальный подъём к новой:
        // так серия из 8 точек не выглядит редкими столбиками, а линия непрерывна.
        public static void PlotLine(float[] values, int count, Color color, float min, float max,
            Color[] buffer, int width, int height)
        {
            int previousX = -1;
            int previousY = 0;
            for (int i = 0; i < count && i < width; i++)
            {
                int x = count == 1 ? 0 : i * (width - 1) / (count - 1);
                float fraction = GraphMath.Normalize(values[i], min, max);
                int y = Mathf.Clamp((int)(fraction * (height - 1)), 0, height - 1);
                if (previousX >= 0) PaintSpan(buffer, width, previousX, x, previousY, color);
                int from = Mathf.Min(previousX >= 0 ? previousY : y, y);
                int to = Mathf.Max(previousX >= 0 ? previousY : y, y);
                PaintColumn(buffer, width, x, from, to, color);
                previousX = x;
                previousY = y;
            }
        }

        private static void PaintSpan(Color[] buffer, int width, int fromX, int toX, int row, Color color)
        {
            for (int x = fromX; x <= toX && x < width; x++) buffer[row * width + x] = color;
        }

        private static void PaintColumn(Color[] buffer, int width, int x, int fromRow, int toRow, Color color)
        {
            for (int row = fromRow; row <= toRow; row++) buffer[row * width + x] = color;
        }
    }
}
