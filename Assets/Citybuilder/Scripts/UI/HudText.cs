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
        // Fallback для скинов, где fontSize == 0 (дефолтный IMGUI-скин).
        public const int DefaultFontSize = 16;

        private const float NormalBackingAlpha = 0.55f;
        private const float ContrastBackingAlpha = 0.9f;
        private const float PulseSpeed = 4f;
        private const float PulseGainMax = 0.22f;

        // ADR-021: ночь холодная, рассвет и закат тёплые.
        private static readonly Color NightColor = new Color(0.62f, 0.78f, 1f);
        private static readonly Color DawnColor = new Color(1f, 0.74f, 0.48f);
        private static readonly Color DuskColor = new Color(1f, 0.66f, 0.42f);

        private static GameSettings profile;

        public static GameSettings Profile { get { return profile; } set { profile = value; } }
        public static float FontScale { get { return profile == null ? 1f : profile.HudFontScale; } }
        public static bool HighContrast { get { return profile != null && profile.HighContrast; } }
        public static float BackingAlpha { get { return HighContrast ? ContrastBackingAlpha : NormalBackingAlpha; } }

        // Чистая математика масштаба: нулевой базовый размер заменяется на DefaultFontSize.
        public static int ScaledFontSize(int baseFontSize)
        {
            int size = baseFontSize > 0 ? baseFontSize : DefaultFontSize;
            return Mathf.RoundToInt(size * FontScale);
        }

        // Рисует строку с подложкой. Пустой текст молча пропускается.
        public static void Draw(float x, float y, string text)
        {
            Draw(x, y, text, Color.white);
        }

        // Тот же рисунок с цветом текста; ADR-021: все временные подмены GUI возвращаются в finally.
        public static void Draw(float x, float y, string text, Color textColor)
        {
            if (string.IsNullOrEmpty(text)) return;
            int savedFontSize = GUI.skin.label.fontSize;
            Color savedColor = GUI.color;
            Color savedContentColor = GUI.contentColor;
            if (FontScale > 1f)
                GUI.skin.label.fontSize = ScaledFontSize(savedFontSize);
            try
            {
                Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
                DrawBacking(x, y, size);
                DrawLabel(x, y, size, text, textColor);
            }
            finally
            {
                GUI.skin.label.fontSize = savedFontSize;
                GUI.color = savedColor;
                GUI.contentColor = savedContentColor;
            }
        }

        private static void DrawBacking(float x, float y, Vector2 size)
        {
            GUI.color = new Color(0f, 0f, 0f, BackingAlpha);
            float height = HudLayout.BackingHeight(size.y);
            GUI.DrawTexture(new Rect(x - 4f, y - BackingPadY, size.x + 8f, height), Texture2D.whiteTexture);
        }

        private static void DrawLabel(float x, float y, Vector2 size, string text, Color textColor)
        {
            GUI.contentColor = textColor;
            GUI.Label(new Rect(x, y, size.x, size.y), text);
        }

        // Чистая математика пульса: 0..1, детерминирована от переданного времени.
        public static float PulseGain(float timeSeconds)
        {
            return (Mathf.Sin(timeSeconds * PulseSpeed) + 1f) * 0.5f;
        }

        // Тёплый пульс активной кнопки; цвет всегда в границах [0, 1].
        public static Color PulseColor(float timeSeconds)
        {
            float gain = PulseGain(timeSeconds);
            return new Color(1f, 0.78f + gain * PulseGainMax, 0.25f + gain * 0.35f, 1f);
        }

        // Цвет строки часов: ночь холодная, рассвет/закат тёплые, день белый.
        public static Color TimeColor(int hour)
        {
            int h = ((hour % 24) + 24) % 24;
            if (h < 6 || h >= 22) return NightColor;
            if (h < 9) return DawnColor;
            if (h > 17) return DuskColor;
            return Color.white;
        }
    }
}
