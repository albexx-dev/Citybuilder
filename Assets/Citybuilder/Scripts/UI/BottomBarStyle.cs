using UnityEngine;

namespace Citybuilder
{
    // Единый стиль нижней панели действий (EPIC-13): тёмная подложка и кнопки без расползания стилей.
    public static class BottomBarStyle
    {
        private static readonly Color BarBackground = new Color(0f, 0f, 0f, 0.72f);
        private static readonly Color SubBarBackground = new Color(0f, 0f, 0f, 0.58f);

        // Тёмная полоса панели на всю ширину.
        public static void DrawBarBackground(Rect rect)
        {
            if (rect.width <= 0f || rect.height <= 0f) return;
            Color saved = GUI.color;
            GUI.color = BarBackground;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = saved;
        }

        // Тёмная подложка над панелью для палитры или кистей зон.
        public static void DrawPopupBackground(Rect rect)
        {
            if (rect.width <= 0f || rect.height <= 0f) return;
            Color saved = GUI.color;
            GUI.color = SubBarBackground;
            GUI.DrawTexture(new Rect(rect.x - 4f, rect.y - 3f, rect.width + 8f, rect.height + 6f), Texture2D.whiteTexture);
            GUI.color = saved;
        }

        // Кнопка с подсветкой активного состояния и звуком клика (EPIC-19).
        public static bool DrawButton(Rect rect, string label, bool highlighted)
        {
            Color saved = GUI.backgroundColor;
            if (highlighted)
            {
                GUI.backgroundColor = Color.yellow;
            }
            bool clicked = GUI.Button(rect, label);
            GUI.backgroundColor = saved;
            if (clicked) AudioController.PlayClickGlobal();
            return clicked;
        }

        // Неактивная кнопка (заблокированное здание или зарезервированный эпик).
        public static void DrawDisabledButton(Rect rect, string label)
        {
            bool saved = GUI.enabled;
            GUI.enabled = false;
            GUI.Button(rect, label);
            GUI.enabled = saved;
        }
    }
}
