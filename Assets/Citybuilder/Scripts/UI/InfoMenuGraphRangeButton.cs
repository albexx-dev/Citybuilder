using UnityEngine;

namespace Citybuilder
{
    // Кнопка масштаба графиков 8/30 дней (issue #113): рисуется из InfoMenuView.OnGUI.
    public static class InfoMenuGraphRangeButton
    {
        public const float RowHeight = 26f;
        private const float ButtonHeight = 24f;

        public static void Draw(ref float y, float width)
        {
            string text = Localization.T("metrics_range", MetricsGraphView.RangeDays);
            Rect rect = new Rect(4f, y, Mathf.Min(260f, width - 8f), ButtonHeight);
            if (GUI.Button(rect, text))
            {
                AudioController.PlayClickGlobal();
                MetricsGraphView.ToggleRange();
            }
            y += RowHeight;
        }
    }
}
