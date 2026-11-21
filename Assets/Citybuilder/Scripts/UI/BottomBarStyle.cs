using UnityEngine;

namespace Citybuilder
{
    // Единый стиль нижней панели действий (EPIC-13): тёмная подложка и кнопки без расползания стилей.
    // EPIC-66: кнопки с иконкой слева и тултипы при наведении.
    public static class BottomBarStyle
    {
        private static readonly Color BarBackground = new Color(0f, 0f, 0f, 0.72f);
        private static readonly Color SubBarBackground = new Color(0f, 0f, 0f, 0.58f);
        private static readonly Color IconTint = new Color(0.12f, 0.12f, 0.12f, 0.92f);
        private const float TooltipGap = 4f;

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

        // Кнопка с иконкой слева и подписью (EPIC-66): null-иконка деградирует в текстовую кнопку.
        public static bool DrawButton(Rect rect, Texture2D icon, string label, bool highlighted)
        {
            Color saved = GUI.backgroundColor;
            if (highlighted)
            {
                GUI.backgroundColor = Color.yellow;
            }
            bool clicked = GUI.Button(rect, string.Empty);
            GUI.backgroundColor = saved;
            DrawIconAndLabel(rect, icon, label);
            if (clicked) AudioController.PlayClickGlobal();
            return clicked;
        }

        // Кнопка с иконкой и тултипом: подсказка показывается при наведении (EPIC-66).
        public static bool DrawButton(Rect rect, Texture2D icon, string label, string tooltip, bool highlighted)
        {
            DrawTooltip(rect, tooltip);
            return DrawButton(rect, icon, label, highlighted);
        }

        // Тултип над кнопкой при наведении: только в OnGUI-пути, пустой текст молча пропускается.
        public static void DrawTooltip(Rect rect, string text)
        {
            if (string.IsNullOrEmpty(text) || Event.current == null) return;
            if (!rect.Contains(Event.current.mousePosition)) return;
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Box(TooltipRect(rect, size.x + 10f, size.y + 4f, Screen.width, Screen.height), text);
        }

        // Чистая геометрия тултипа: над кнопкой, без выхода за края окна (для тестов без IMGUI).
        public static Rect TooltipRect(Rect buttonRect, float tipWidth, float tipHeight, float screenWidth, float screenHeight)
        {
            tipWidth = Mathf.Min(tipWidth, Mathf.Max(0f, screenWidth));
            float x = Mathf.Clamp(buttonRect.x, 0f, Mathf.Max(0f, screenWidth - tipWidth));
            float y = Mathf.Max(0f, buttonRect.y - tipHeight - TooltipGap);
            return new Rect(x, y, tipWidth, tipHeight);
        }

        // Белый Kenney-глиф тонируется через GUI.color под цвет текста темы.
        private static void DrawIconAndLabel(Rect rect, Texture2D icon, string label)
        {
            if (icon == null)
            {
                GUI.Label(rect, label);
                return;
            }
            float iconSize = rect.height - 6f;
            Rect iconRect = new Rect(rect.x + 4f, rect.y + 3f, iconSize, iconSize);
            Color saved = GUI.color;
            GUI.color = IconTint;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = saved;
            float textX = iconRect.xMax + 4f;
            GUI.Label(new Rect(textX, rect.y, Mathf.Max(0f, rect.xMax - textX - 2f), rect.height), label);
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
