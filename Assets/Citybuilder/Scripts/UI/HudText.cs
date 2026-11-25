using UnityEngine;

namespace Citybuilder
{
    // Единый HUD-текст: тёмная подложка по фактической ширине строки и белый текст.
    // Нужен, чтобы строки читались поверх светлой сетки. Состояние GUI не портит.
    // Доступность (issue #122): профиль связывает SettingsAppliers.ApplyHud.
    public static class HudText
    {
        // Вертикальный запас подложки сверху и снизу от метки.
        public const float BackingPadY = 2f;
        // Оценочная высота подложки одной строки: метка ~18 px + запас 4 px.
        public const float BackingRowHeight = 22f;

        private const float NormalBackingAlpha = 0.55f;
        private const float ContrastBackingAlpha = 0.9f;

        private static GameSettings profile;

        public static GameSettings Profile { get { return profile; } set { profile = value; } }
        public static float FontScale { get { return profile == null ? 1f : profile.HudFontScale; } }
        public static bool HighContrast { get { return profile != null && profile.HighContrast; } }
        public static float BackingAlpha { get { return HighContrast ? ContrastBackingAlpha : NormalBackingAlpha; } }

        // Рисует строку с подложкой. Пустой текст молча пропускается.
        public static void Draw(float x, float y, string text)
        {
            Draw(x, y, text, Color.white);
        }

        // Тот же рисунок с цветом текста: нужен, чтобы дефицитная строка отличалась,
        // не меняя подложку и все существующие вызовы. Размер и контраст берутся из профиля.
        public static void Draw(float x, float y, string text, Color textColor)
        {
            if (string.IsNullOrEmpty(text)) return;
            int savedFontSize = GUI.skin.label.fontSize;
            float scale = FontScale;
            if (scale > 1f && savedFontSize > 0)
                GUI.skin.label.fontSize = Mathf.RoundToInt(savedFontSize * scale);
            try
            {
                Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
                DrawBacking(x, y, size);
                DrawLabel(x, y, size, text, textColor);
            }
            finally
            {
                GUI.skin.label.fontSize = savedFontSize;
            }
        }

        private static void DrawBacking(float x, float y, Vector2 size)
        {
            Color savedColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, BackingAlpha);
            float height = HudLayout.BackingHeight(size.y);
            GUI.DrawTexture(new Rect(x - 4f, y - BackingPadY, size.x + 8f, height), Texture2D.whiteTexture);
            GUI.color = savedColor;
        }

        private static void DrawLabel(float x, float y, Vector2 size, string text, Color textColor)
        {
            Color savedContentColor = GUI.contentColor;
            GUI.contentColor = textColor;
            GUI.Label(new Rect(x, y, size.x, size.y), text);
            GUI.contentColor = savedContentColor;
        }
    }
}
