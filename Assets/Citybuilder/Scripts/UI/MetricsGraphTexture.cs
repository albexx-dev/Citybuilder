using UnityEngine;

namespace Citybuilder
{
    // Текстуры графиков инфоменю (EPIC-68 шаг 2): население, казна, нужды, энергия,
    // пожары и кражи (#112).
    // Линия рисуется в пиксельный буфер (IMGUI не умеет DrawLine), поэтому OnGUI только
    // выводит готовую текстуру: пересчёт не чаще раза в 0.5 с и только при открытом меню.
    // Прореживание и оси — GraphMath; история — MetricsHistory (#63), в сейв не пишется.
    public static class MetricsGraphTexture
    {
        public const int Width = 64;
        public const int Height = 32;
        public const int SeriesCount = 6;

        private const float RefreshIntervalSec = 0.5f;

        private static readonly Color Background = new Color(0.13f, 0.15f, 0.17f);
        private static readonly Color[] LineColors =
        {
            new Color(0.45f, 0.85f, 0.45f), // население
            new Color(0.95f, 0.8f, 0.35f),  // казна
            new Color(0.5f, 0.8f, 0.95f),   // нужды
            new Color(0.95f, 0.6f, 0.3f),   // энергия
            new Color(0.95f, 0.45f, 0.45f), // пожары (#112)
            new Color(0.75f, 0.55f, 0.95f)  // кражи (#112)
        };
        private static readonly string[] NameKeys =
        {
            "metric_name_population", "metric_name_treasury", "metric_name_needs", "metric_name_power",
            "metric_name_fires", "metric_name_thefts"
        };

        private static readonly Texture2D[] textures = new Texture2D[SeriesCount];
        private static readonly float[] source = new float[MetricsHistory.Capacity];
        private static readonly float[] points = new float[GraphMath.DefaultMaxPoints];
        private static readonly Color[] buffer = new Color[Width * Height];
        private static float nextRefreshTime;

        // Пересчитать, если пора. Возвращает true при перерисовке; пустая история — false.
        // clockOverride >= 0 подменяет часы (шов для EditMode-тестов: Time не подставить напрямую).
        public static bool RefreshIfDue(MetricsHistory history, float clockOverride = -1f)
        {
            if (history == null || history.Count == 0) return false;
            float now = clockOverride >= 0f ? clockOverride : Time.unscaledTime;
            if (now < nextRefreshTime) return false;
            nextRefreshTime = now + RefreshIntervalSec;
            MetricsSnapshot[] snapshots = history.Snapshot();
            for (int i = 0; i < SeriesCount; i++) Repaint(snapshots, i);
            return true;
        }

        // График серии; null, пока история пуста.
        public static Texture2D At(int index)
        {
            if (index < 0 || index >= SeriesCount) return null;
            return textures[index];
        }

        // Подпись строки: имя и последнее значение («Казна: 320»).
        public static string LabelAt(int index, MetricsHistory history)
        {
            if (index < 0 || index >= SeriesCount) return string.Empty;
            MetricsSnapshot latest = history != null ? history.Latest() : default(MetricsSnapshot);
            return NameAt(index) + ": " + ValueAt(index, latest).ToString("0");
        }

        // Имя серии всегда берётся из реестра: RU/EN-варианты заданы в LocalizationTable.
        private static string NameAt(int index)
        {
            return Localization.T(NameKeys[index]);
        }

        // Забыть таймер (тесты, «Новая игра»): следующий вызов пересчитает сразу.
        public static void Reset() { nextRefreshTime = 0f; }

        private static void Repaint(MetricsSnapshot[] snapshots, int index)
        {
            int count = snapshots.Length > source.Length ? source.Length : snapshots.Length;
            for (int i = 0; i < count; i++) source[i] = ValueAt(index, snapshots[i]);
            int written = GraphMath.Decimate(source, count, points, Width);
            float min, max;
            GraphMath.Axes(points, written, out min, out max);
            for (int i = 0; i < buffer.Length; i++) buffer[i] = Background;
            PlotLine(points, written, LineColors[index], min, max);
            EnsureTexture(index);
            textures[index].SetPixels(buffer);
            textures[index].Apply();
        }

        // Ступенчатая линия: горизонтальный пролёт от прошлой точки и вертикальный подъём
        // к новой. Так серия из 8 точек не выглядит редкими столбиками, а линия непрерывна.
        private static void PlotLine(float[] values, int count, Color color, float min, float max)
        {
            int previousX = -1;
            int previousY = 0;
            for (int i = 0; i < count && i < Width; i++)
            {
                int x = count == 1 ? 0 : i * (Width - 1) / (count - 1);
                float fraction = GraphMath.Normalize(values[i], min, max);
                int y = Mathf.Clamp((int)(fraction * (Height - 1)), 0, Height - 1);
                if (previousX >= 0) PaintSpan(previousX, x, previousY, color);
                PaintColumn(x, Mathf.Min(previousX >= 0 ? previousY : y, y), Mathf.Max(previousX >= 0 ? previousY : y, y), color);
                previousX = x;
                previousY = y;
            }
        }

        private static void PaintSpan(int fromX, int toX, int row, Color color)
        {
            for (int x = fromX; x <= toX && x < Width; x++) buffer[row * Width + x] = color;
        }

        private static void PaintColumn(int x, int fromRow, int toRow, Color color)
        {
            for (int row = fromRow; row <= toRow; row++) buffer[row * Width + x] = color;
        }

        private static float ValueAt(int index, MetricsSnapshot snapshot)
        {
            if (index == 0) return snapshot.Population;
            if (index == 1) return snapshot.Balance;
            if (index == 2) return snapshot.NeedsAvg;
            if (index == 3) return snapshot.PowerPercent;
            if (index == 4) return snapshot.Fires;
            return snapshot.Thefts;
        }

        private static void EnsureTexture(int index)
        {
            if (textures[index] != null) return;
            textures[index] = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            textures[index].filterMode = FilterMode.Point;
            textures[index].wrapMode = TextureWrapMode.Clamp;
        }
    }
}