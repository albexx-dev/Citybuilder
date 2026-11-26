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

        // Переключатель типов дорог в строке панели (issue #163). Имена берутся из RU/EN ассетов.
        private void DrawRoadTypes(float sw, float sh)
        {
            RoadDefinition[] defs = Menu != null && Menu.Roads != null && Menu.Roads.Store != null
                ? Menu.Roads.Store.Definitions : null;
            if (defs == null || defs.Length == 0) { UiClickGuard.Set("bottom_subrow", new Rect()); return; }
            Rect popup = HudLayout.PaletteRect(sw, sh, defs.Length);
            UiClickGuard.Set("bottom_subrow", popup);
            BottomBarStyle.DrawPopupBackground(popup);
            for (int i = 0; i < defs.Length; i++)
            {
                if (defs[i] == null) continue;
                float x = popup.x + i * (HudLayout.PaletteButtonWidth + HudLayout.ButtonGap);
                Rect rect = new Rect(x, popup.y, HudLayout.PaletteButtonWidth, popup.height);
                string key = defs[i].DisplayNameKey;
                string label = !string.IsNullOrEmpty(key) && Localization.Has(key) ? Localization.T(key) : defs[i].DisplayName;
                bool active = Menu.ActiveRoadTypeId == defs[i].Id;
                if (BottomBarStyle.DrawButton(rect, IconBook.Get("road"), label, Localization.T("bottom_mode_roads"), active))
                    Menu.SetRoadType(defs[i].Id);
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
