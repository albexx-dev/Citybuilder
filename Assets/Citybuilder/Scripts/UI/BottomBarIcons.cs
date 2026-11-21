using UnityEngine;

namespace Citybuilder
{
    // Ключи IconBook и ключи локализации подписей нижней панели (EPIC-66, issue #100 шаг 1).
    // Порядок массивов соответствует порядку кнопок в BottomBarView.
    public static class BottomBarIcons
    {
        // Скорости: пауза, игра, ускорение.
        public static readonly string[] SpeedKeys = { "pause", "play", "fast" };
        public static readonly string[] SpeedLabelKeys = { "bottom_speed_pause", "bottom_speed_play", "bottom_speed_fast" };
        public static readonly string[] SpeedTipKeys = { "bottom_tip_speed_pause", "bottom_tip_speed_play", "bottom_tip_speed_fast" };

        // Режимы стройки: выкл, дома, дороги, зоны, рост, декор.
        public static readonly string[] ModeKeys = { "off", "house", "road", "zones", "growth", "decor" };
        public static readonly string[] ModeLabelKeys =
        {
            "bottom_mode_off", "bottom_mode_houses", "bottom_mode_roads",
            "bottom_mode_zones", "bottom_mode_growth", "bottom_mode_decor"
        };
        public static readonly string[] ModeTipKeys =
        {
            "bottom_tip_mode_off", "bottom_tip_mode_houses", "bottom_tip_mode_roads",
            "bottom_tip_mode_zones", "bottom_tip_mode_growth", "bottom_tip_mode_decor"
        };

        // Кнопки меню: статистика, лента событий, задачи-вехи, настройки (EPIC-66/69).
        public static readonly string[] MenuKeys = { "stats", "feed", "quests", "settings" };
        public static readonly string[] MenuLabelKeys =
        {
            "bottom_menu_stats", "bottom_menu_feed", "bottom_menu_quests", "bottom_menu_settings"
        };
        public static readonly string[] MenuTipKeys =
        {
            "bottom_tip_menu_stats", "bottom_tip_menu_feed", "bottom_tip_menu_quests", "bottom_tip_menu_settings"
        };

        // Кисти зон: жилая, коммерция, промзона.
        public static readonly string[] BrushKeys = { "house", "shop", "factory" };
        public static readonly string[] BrushTipKeys =
        {
            "bottom_tip_brush_residential", "bottom_tip_brush_commercial", "bottom_tip_brush_industrial"
        };

        // Подписи кистей зон из реестра локализации (EPIC-72 шаг 2): вычисляются при вызове,
        // потому что язык может смениться между кадрами.
        public static string[] ZoneBrushLabels()
        {
            return new[] { Localization.T("zone_residential"), Localization.T("zone_commercial"), Localization.T("zone_industrial") };
        }
    }
}
