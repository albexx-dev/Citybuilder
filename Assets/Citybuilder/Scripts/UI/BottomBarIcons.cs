using UnityEngine;

namespace Citybuilder
{
    // Ключи IconBook и тексты кнопок нижней панели (EPIC-66): подсказки и подписи.
    // Порядок массивов соответствует порядку кнопок в BottomBarView.
    public static class BottomBarIcons
    {
        // Скорости: пауза, игра, ускорение.
        public static readonly string[] SpeedKeys = { "pause", "play", "fast" };
        public static readonly string[] SpeedTips = { "Остановить время", "Обычная скорость", "Ускорение x4" };

        // Режимы стройки: выкл, дома, дороги, зоны, рост, декор.
        public static readonly string[] ModeKeys = { "off", "house", "road", "zones", "growth", "decor" };
        public static readonly string[] ModeTips =
        {
            "Выйти из режима стройки", "Строить дома", "Строить дороги",
            "Красить зоны", "Ускорять рост домов", "Декорации"
        };

        // Кнопки меню: статистика, лента событий, задачи-вехи, настройки (EPIC-66/69).
        public static readonly string[] MenuKeys = { "stats", "feed", "quests", "settings" };
        public static readonly string[] MenuTips = { "Сводка города", "События города", "Задачи-вехи", "Настройки игры" };

        // Кисти зон: жилая, коммерция, промзона.
        public static readonly string[] BrushKeys = { "house", "shop", "factory" };
        public static readonly string[] BrushTips = { "Жилая зона: растут дома", "Коммерция: магазины", "Промзона: заводы" };

        // Подписи кистей зон из реестра локализации (EPIC-72 шаг 2): вычисляются при вызове,
        // потому что язык может смениться между кадрами.
        public static string[] ZoneBrushLabels()
        {
            return new[] { Localization.T("zone_residential"), Localization.T("zone_commercial"), Localization.T("zone_industrial") };
        }
    }
}
