using UnityEngine;

namespace Citybuilder
{
    // Секция «История» в инфоменю (EPIC-68 шаг 2): четыре графика за 8 игровых дней.
    // Вызывается из InfoMenuView.DrawCachedSections внутри его scroll-view, поэтому координаты
    // локальные (x = 12, как у остальных строк меню). Линии — готовые текстуры MetricsGraphTexture.
    // Историю пишет MetricsRecorder (#63); после загрузки она копится заново — это ожидаемо.
    public static class MetricsGraphView
    {
        public const float GraphHeight = 32f;
        public const float RowHeight = 54f;
        public const float HeaderHeight = 24f;
        public const float EmptyHeight = 48f;

        private static MetricsRecorder recorder;

        // Высота секции: пустая история — только заголовок и подсказка.
        public static float SectionHeight(MetricsHistory history)
        {
            if (history == null || history.Count == 0) return EmptyHeight;
            return HeaderHeight + MetricsGraphTexture.SeriesCount * RowHeight;
        }

        // Игровой путь: рекордер ищется в сцене один раз (прецедент TaxPolicyView).
        public static void Draw(ref float y, float width)
        {
            if (recorder == null) recorder = Object.FindFirstObjectByType<MetricsRecorder>();
            Draw(ref y, width, recorder != null ? recorder.History : null);
        }

        // Тестируемый шов: история приходит параметром, поиск в сцене не участвует.
        public static void Draw(ref float y, float width, MetricsHistory history)
        {
            HudText.Draw(12f, y, Localization.T("metrics_title"));
            y += HeaderHeight;
            if (history == null || history.Count == 0)
            {
                HudText.Draw(12f, y, Localization.T("metrics_empty"));
                y += EmptyHeight - HeaderHeight;
                return;
            }
            MetricsGraphTexture.RefreshIfDue(history);
            float graphWidth = GraphWidth(width);
            for (int i = 0; i < MetricsGraphTexture.SeriesCount; i++)
            {
                DrawRow(ref y, graphWidth, history, i);
            }
        }

        // Ширина графика под шириной меню: не шире 260 и не уже 64.
        public static float GraphWidth(float width)
        {
            return Mathf.Min(260f, Mathf.Max(64f, width - 24f));
        }

        // Тестируемый шов без IMGUI (GUI доступен только в OnGUI): то же продвижение y, что в Draw.
        public static float Advance(MetricsHistory history)
        {
            if (history == null || history.Count == 0) return EmptyHeight;
            MetricsGraphTexture.RefreshIfDue(history);
            return HeaderHeight + MetricsGraphTexture.SeriesCount * RowHeight;
        }

        private static void DrawRow(ref float y, float graphWidth, MetricsHistory history, int index)
        {
            HudText.Draw(12f, y, MetricsGraphTexture.LabelAt(index, history));
            y += 18f;
            Texture2D graph = MetricsGraphTexture.At(index);
            if (graph != null) GUI.DrawTexture(new Rect(12f, y, graphWidth, GraphHeight), graph);
            y += GraphHeight + 4f;
        }
    }
}