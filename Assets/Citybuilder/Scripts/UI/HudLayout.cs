using UnityEngine;

namespace Citybuilder
{
    // Адаптивные координаты нижних строк HUD.
    // На обычных окнах сохраняет привычные фиксированные y, на низких — поджимает строки к низу.
    public static class HudLayout
    {
        private const float BottomMargin = 30f;
        private const float LineStep = 24f;

        public static float FixedOrBottom(float preferredY, int linesFromBottom)
        {
            float maxY = Mathf.Max(10f, Screen.height - BottomMargin);
            if (preferredY <= maxY)
            {
                return preferredY;
            }
            return Mathf.Max(10f, maxY - linesFromBottom * LineStep);
        }
    }
}
