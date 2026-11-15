using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Строка выбора вида дерева над нижней панелью (режим «Декор», EPIC-56).
    // Вынесена из BottomBarView: панель уже на лимите строк.
    public static class DecorPaletteRow
    {
        // Рисует кнопку на каждый вид декора; клик выбирает вид для установки.
        public static void Draw(float sw, float sh, DecorPlacementController decor)
        {
            List<DecorDefinition> defs = decor != null ? decor.Definitions : null;
            if (defs == null || defs.Count == 0)
            {
                UiClickGuard.Set("bottom_subrow", new Rect());
                return;
            }
            Rect popup = HudLayout.PaletteRect(sw, sh, defs.Count);
            UiClickGuard.Set("bottom_subrow", popup);
            BottomBarStyle.DrawPopupBackground(popup);
            for (int i = 0; i < defs.Count; i++)
            {
                DecorDefinition def = defs[i];
                if (def == null) continue;
                float x = popup.x + i * (HudLayout.PaletteButtonWidth + HudLayout.ButtonGap);
                Rect rect = new Rect(x, popup.y, HudLayout.PaletteButtonWidth, popup.height);
                bool active = decor.Selected == def;
                if (BottomBarStyle.DrawButton(rect, def.DisplayName, active)) decor.SetSelected(def);
            }
        }
    }
}
