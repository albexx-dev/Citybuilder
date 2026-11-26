using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Единый стиль нижней панели (EPIC-13/66): подложка, кнопки с иконкой и тултипы с задержкой 0.4 с.
    public static class BottomBarStyle
    {
        private static readonly Color BarBackground = new Color(0f, 0f, 0f, 0.72f);
        private static readonly Color SubBarBackground = new Color(0f, 0f, 0f, 0.58f);
        private static readonly Color IconTint = new Color(0.12f, 0.12f, 0.12f, 0.92f);
        private const float TooltipGap = 4f;
        // Подписи кнопок: 10 px без переноса — «Дороги [2]» не уходит на вторую строку.
        public const int ButtonFontSize = 10;
        private const float IconInset = 2f, IconGap = 2f;
        private const float TextRightInset = 1f;
        private const float IconSizeReduction = 12f;

        private static GUIStyle buttonLabel;
        private static GUIStyle ButtonLabel => buttonLabel ?? (buttonLabel = new GUIStyle(GUI.skin.label) { fontSize = ButtonFontSize, wordWrap = false });
        private static readonly Dictionary<Rect, float> hoverStarts = new Dictionary<Rect, float>();

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

        // Кнопка (EPIC-19): общий хелпер даёт пульс и возврат цвета.
        public static bool DrawButton(Rect rect, string label, bool highlighted)
        {
            return DrawWithPulse(highlighted, () => GUI.Button(rect, label));
        }

        // Кнопка с иконкой (EPIC-66): активная пульсирует так же, как текстовая.
        public static bool DrawButton(Rect rect, Texture2D icon, string label, bool highlighted)
        {
            bool clicked = DrawWithPulse(highlighted, () => GUI.Button(rect, string.Empty));
            DrawIconAndLabel(rect, icon, label);
            return clicked;
        }

        // Единая точка подмены/возврата GUI.backgroundColor для всех кнопок (ADR-021).
        private static bool DrawWithPulse(bool highlighted, Func<bool> draw)
        {
            Color saved = GUI.backgroundColor;
            try
            {
                if (highlighted) GUI.backgroundColor = HudText.PulseColor(Time.unscaledTime);
                bool clicked = draw();
                if (clicked) AudioController.PlayClickGlobal();
                return clicked;
            }
            finally
            {
                GUI.backgroundColor = saved;
            }
        }

        // Кнопка с иконкой и тултипом: подсказка показывается при наведении (EPIC-66).
        public static bool DrawButton(Rect rect, Texture2D icon, string label, string tooltip, bool highlighted)
        {
            DrawTooltip(rect, tooltip);
            return DrawButton(rect, icon, label, highlighted);
        }

        // Тултип в OnGUI-пути: пустой текст пропускается, задержка через TooltipHoverPolicy.
        public static void DrawTooltip(Rect rect, string text)
        {
            if (string.IsNullOrEmpty(text) || Event.current == null) return;
            if (!TooltipHoverPolicy.Track(hoverStarts, rect, Event.current.mousePosition, Time.unscaledTime)) return;
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

        // Ширина зоны текста в кнопке: иконка и отступы вычитаются из ширины кнопки (для тестов без IMGUI).
        public static float LabelWidth(float buttonWidth, float buttonHeight)
        {
            float iconSize = Mathf.Max(0f, buttonHeight - IconSizeReduction);
            return Mathf.Max(0f, buttonWidth - IconInset - iconSize - IconGap - TextRightInset);
        }

        // Оценка ширины строки без IMGUI: 0.5 em на букву, пробел 0.28, цифра 0.56, скобка 0.34.
        public static float EstimateLabelWidth(string label, int fontSize)
        {
            if (string.IsNullOrEmpty(label)) return 0f;
            float em = 0f;
            for (int i = 0; i < label.Length; i++)
            {
                char c = label[i];
                if (c == ' ') em += 0.28f;
                else if (c == '[' || c == ']') em += 0.34f;
                else if (c >= '0' && c <= '9') em += 0.56f;
                else em += 0.5f;
            }
            return em * fontSize;
        }

        // Белый Kenney-глиф тонируется через GUI.color под цвет текста темы.
        private static void DrawIconAndLabel(Rect rect, Texture2D icon, string label)
        {
            if (icon == null)
            {
                GUI.Label(rect, label, ButtonLabel);
                return;
            }
            float iconSize = Mathf.Max(0f, rect.height - IconSizeReduction);
            Rect iconRect = new Rect(rect.x + IconInset, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
            Color saved = GUI.color;
            GUI.color = IconTint;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = saved;
            GUI.Label(new Rect(iconRect.xMax + IconGap, rect.y, LabelWidth(rect.width, rect.height), rect.height), label, ButtonLabel);
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
