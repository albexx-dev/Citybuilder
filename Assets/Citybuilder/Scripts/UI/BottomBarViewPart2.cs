using UnityEngine;

namespace Citybuilder
{
    // Хвост класса BottomBarView (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class BottomBarView : MonoBehaviour
    {
        private void DrawZoneBrushes(float sw, float sh)
        {
            ZoneType[] brushes = { ZoneType.Residential, ZoneType.Commercial, ZoneType.Industrial };
            string[] labels = BottomBarIcons.ZoneBrushLabels();
            Rect popup = HudLayout.PaletteRect(sw, sh, brushes.Length);
            UiClickGuard.Set("bottom_subrow", popup);
            BottomBarStyle.DrawPopupBackground(popup);
            for (int i = 0; i < brushes.Length; i++)
            {
                float x = popup.x + i * (HudLayout.PaletteButtonWidth + HudLayout.ButtonGap);
                Rect rect = new Rect(x, popup.y, HudLayout.PaletteButtonWidth, popup.height);
                bool active = Menu.Zones != null && Menu.Zones.Brush == brushes[i];
                if (BottomBarStyle.DrawButton(rect, IconBook.Get(BottomBarIcons.BrushKeys[i]), labels[i], Localization.T(BottomBarIcons.BrushTipKeys[i]), active)) Menu.SetBrush(brushes[i]);
            }
        }

        // Подписи считаются каждый кадр: язык мог смениться между отрисовками IMGUI.
        private static string[] Localized(string[] keys)
        {
            string[] labels = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++) labels[i] = Localization.T(keys[i]);
            return labels;
        }
    }
}
