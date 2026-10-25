using UnityEngine;

namespace Citybuilder
{
    // Единый HUD-текст: тёмная подложка по фактической ширине строки и белый текст.
    // Нужен, чтобы строки читались поверх светлой сетки. Состояние GUI не портит.
    public static class HudText
    {
        // Рисует строку с подложкой. Пустой текст молча пропускается.
        public static void Draw(float x, float y, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            Color savedColor = GUI.color;
            Color savedContentColor = GUI.contentColor;
            // Полупрозрачный чёрный: whiteTexture тонируется через GUI.color.
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(x - 4f, y - 2f, size.x + 8f, size.y + 4f), Texture2D.whiteTexture);
            GUI.color = savedColor;
            GUI.contentColor = Color.white;
            GUI.Label(new Rect(x, y, size.x, size.y), text);
            GUI.color = savedColor;
            GUI.contentColor = savedContentColor;
        }
    }
}
