using UnityEngine;

namespace Citybuilder
{
    // Ключи IconBook и тексты подсказок кнопок нижней панели (EPIC-66).
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

        // Кнопки меню: статистика, лента событий, задачи-вехи.
        public static readonly string[] MenuKeys = { "stats", "feed", "quests" };
        public static readonly string[] MenuTips = { "Сводка города", "События города", "Задачи-вехи" };

        // Кисти зон: жилая, коммерция, промзона.
        public static readonly string[] BrushKeys = { "house", "shop", "factory" };
        public static readonly string[] BrushTips = { "Жилая зона: растут дома", "Коммерция: магазины", "Промзона: заводы" };
    }
}
