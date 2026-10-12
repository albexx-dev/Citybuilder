using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Прямоугольники меню для гарда кликов. Координаты GUI: origin слева-сверху.
    public static class UiClickGuard
    {
        private static readonly Dictionary<string, Rect> rects = new Dictionary<string, Rect>();

        public static void Set(string key, Rect rect)
        {
            rects[key] = rect;
        }

        // True, если точка внутри любого rect. Y конвертируется из экранных координат.
        public static bool IsOverMenu(Vector2 mousePosition)
        {
            Vector2 guiPoint = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            foreach (KeyValuePair<string, Rect> pair in rects)
            {
                if (pair.Value.Contains(guiPoint))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
