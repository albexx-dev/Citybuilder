using System.Collections.Generic;

namespace Citybuilder
{
    // Таблица реестра локализации (issue #100): ключи вынесены из фасада Localization,
    // чтобы хвосты UI можно было добавлять, не приближая фасад к лимиту строк файла.
    public static class LocalizationTable
    {
        private static readonly Dictionary<string, string[]> values = new Dictionary<string, string[]>
        {
            // Верхние строки HUD: казна, население, часы.
            { "budget_line", new[] { "Доход: {0} $/ч - Расход: {1} $/ч = Итог: {2} $/ч", "Income: {0} $/h - Upkeep: {1} $/h = Net: {2} $/h" } },
            { "population_line", new[] { "Население: {0} / {1} — {2} домохозяйств", "Pop: {0} / {1} — {2} households" } },
            { "clock_line", new[] { "День {0}, Час {1} — {2}", "Day {0}, Hour {1} — {2}" } },
            { "clock_paused", new[] { "Пауза", "Paused" } },
            { "clock_playing", new[] { "Игра", "Playing" } },
            // Зоны и уровни налога: строки бюджета инфоменю.
            { "zone_residential", new[] { "Жилая", "Residential" } },
            { "zone_commercial", new[] { "Коммерция", "Commercial" } },
            { "zone_industrial", new[] { "Промзона", "Industrial" } },
            { "tax_level_low", new[] { "низкий", "low" } },
            { "tax_level_normal", new[] { "обычный", "normal" } },
            { "tax_level_high", new[] { "высокий", "high" } },
            { "zone_tax_label", new[] { "{0}: {1} ({2}) +{3} $/ч", "{0}: {1} ({2}) +{3} $/h" } },
            // Каталог постройки: вкладки категорий и суффикс заблокированной карточки.
            { "catalog_tab_roads", new[] { "Дороги", "Roads" } },
            { "catalog_tab_houses", new[] { "Дома", "Houses" } },
            { "catalog_tab_commerce", new[] { "Коммерция", "Commerce" } },
            { "catalog_tab_services", new[] { "Службы", "Services" } },
            { "catalog_tab_decor", new[] { "Декор", "Decor" } },
            { "catalog_tab_transport", new[] { "Транспорт", "Transport" } },
            { "catalog_locked", new[] { "\n— {0} зд.", "\n— {0} locked" } },
            // Меню паузы: заголовок, кнопки, звук и подтверждение выхода.
            { "pause_title", new[] { "=== Пауза ===", "=== Pause ===" } },
            { "pause_resume", new[] { "Продолжить [Esc]", "Resume [Esc]" } },
            { "pause_save", new[] { "Сохранить", "Save" } },
            { "pause_load", new[] { "Загрузить", "Load" } },
            { "pause_settings", new[] { "Настройки", "Settings" } },
            { "pause_main_menu", new[] { "В главное меню", "Main menu" } },
            { "pause_volume", new[] { "Громкость: {0}%", "Volume: {0}%" } },
            { "pause_mute", new[] { "Без звука", "Mute" } },
            { "pause_confirm_title", new[] { "Сохранить перед выходом?", "Save before exiting?" } },
            { "pause_confirm_yes", new[] { "Да (сохранить и выйти)", "Yes (save and exit)" } },
            { "pause_confirm_no", new[] { "Нет (выйти без сохранения)", "No (exit without saving)" } },
            { "pause_confirm_cancel", new[] { "Отмена", "Cancel" } },
            { "main_menu_title", new[] { "=== Citybuilder ===", "=== Citybuilder ===" } },
            { "main_menu_continue", new[] { "Продолжить", "Continue" } },
            { "main_menu_new_game", new[] { "Новая игра", "New game" } },
            { "main_menu_load", new[] { "Загрузить", "Load" } },
            { "main_menu_exit", new[] { "Выход", "Exit" } },
            { "save_title", new[] { "=== Сохранить город ===", "=== Save city ===" } },
            { "save_save", new[] { "Сохранить", "Save" } },
            { "save_close", new[] { "Закрыть", "Close" } },
            { "save_slot_empty", new[] { "пусто", "empty" } },
            { "save_slot_info", new[] { "День {0} • {1}", "Day {0} • {1}" } },
            { "load_title", new[] { "=== Загрузить город ===", "=== Load city ===" } },
            { "load_load", new[] { "Загрузить", "Load" } },
            { "load_close", new[] { "Закрыть", "Close" } },
            { "load_slot_empty", new[] { "{0} — пусто", "{0} — empty" } },
            { "load_slot_info", new[] { "{0} — День {1} ({2})", "{0} — Day {1} ({2})" } },
            // Лента событий (issue #87): ключи регистрируются EventMessageCatalog.
            { "event_log_title", new[] { "=== События города ===", "=== City events ===" } },
            { "event_log_empty", new[] { "Событий пока нет.", "No events yet." } },
            { "event_log_prefix", new[] { "День {0}, {1:00}:00 — ", "Day {0}, {1:00}:00 — " } },
            // Имена зданий каталога из ассетов BuildingDefinition.DisplayNameKey (задача 89).
            { "building_asphalt", new[] { "Асфальтовая дорога", "Asphalt Road" } },
            { "building_factory", new[] { "Завод", "Factory" } },
            { "building_fire_station", new[] { "Пожарная станция", "Fire Station" } },
            { "building_hospital", new[] { "Больница", "Hospital" } },
            { "building_house", new[] { "Дом", "House" } },
            { "building_park", new[] { "Парк", "Park" } },
            { "building_police_station", new[] { "Полицейская станция", "Police Station" } },
            { "building_power_station", new[] { "Электростанция", "Power Station" } },
            { "building_shop", new[] { "Магазин", "Shop" } },
            { "building_waste_depot", new[] { "Мусорный склад", "Waste Depot" } },
            { "building_compost_station", new[] { "Компостная станция", "Compost Station" } },
            { "building_water_tower", new[] { "Водонапорная башня", "Water Tower" } },
            { "decor_bush", new[] { "Куст", "Bush" } },
            { "decor_detailed", new[] { "Дерево (детальное)", "Tree (detailed)" } },
            { "decor_oak", new[] { "Дуб", "Oak" } },
            { "decor_pine", new[] { "Сосна", "Pine" } },
            { "budget_breakdown", new[] { "Налоги {0} - Содержание {1} - Кражи {2} + Награды {3} = {4} $/ч", "Tax {0} - Upkeep {1} - Theft {2} + Rewards {3} = {4} $/h" } },
            // Нижняя панель (issue #100 шаг 1): подписи и тултипы. Хоткей-суффиксы остаются в шаблоне.
            { "bottom_speed_pause", new[] { "Пауза", "Pause" } },
            { "bottom_speed_play", new[] { "Игра", "Play" } },
            { "bottom_speed_fast", new[] { "x4", "x4" } },
            { "bottom_mode_off", new[] { "Выкл [Esc]", "Off [Esc]" } },
            { "bottom_mode_houses", new[] { "Дома [1]", "Houses [1]" } },
            { "bottom_mode_roads", new[] { "Дороги [2]", "Roads [2]" } },
            { "bottom_mode_zones", new[] { "Зоны [3]", "Zones [3]" } },
            { "bottom_mode_growth", new[] { "Рост [4]", "Growth [4]" } },
            { "bottom_mode_decor", new[] { "Декор [5]", "Decor [5]" } },
            { "bottom_menu_stats", new[] { "Статистика", "Statistics" } },
            { "bottom_menu_feed", new[] { "Лента", "Feed" } },
            { "bottom_menu_quests", new[] { "Задачи", "Quests" } },
            { "bottom_menu_settings", new[] { "Настройки", "Settings" } },
            { "bottom_tip_speed_pause", new[] { "Остановить время", "Stop time" } },
            { "bottom_tip_speed_play", new[] { "Обычная скорость", "Normal speed" } },
            { "bottom_tip_speed_fast", new[] { "Ускорение x4", "Fast forward x4" } },
            { "bottom_tip_mode_off", new[] { "Выйти из режима стройки", "Exit build mode" } },
            { "bottom_tip_mode_houses", new[] { "Строить дома", "Build houses" } },
            { "bottom_tip_mode_roads", new[] { "Строить дороги", "Build roads" } },
            { "bottom_tip_mode_zones", new[] { "Красить зоны", "Paint zones" } },
            { "bottom_tip_mode_growth", new[] { "Ускорять рост домов", "Speed up house growth" } },
            { "bottom_tip_mode_decor", new[] { "Декорации", "Decorations" } },
            { "bottom_tip_menu_stats", new[] { "Сводка города", "City overview" } },
            { "bottom_tip_menu_feed", new[] { "События города", "City events" } },
            { "bottom_tip_menu_quests", new[] { "Задачи-вехи", "Milestone quests" } },
            { "bottom_tip_menu_settings", new[] { "Настройки игры", "Game settings" } },
            { "bottom_tip_brush_residential", new[] { "Жилая зона: растут дома", "Residential zone: houses grow" } },
            { "bottom_tip_brush_commercial", new[] { "Коммерция: магазины", "Commercial: shops" } },
            { "bottom_tip_brush_industrial", new[] { "Промзона: заводы", "Industrial: factories" } }
        };

        // Перенос в фасад: исходный реестр остаётся тем же словарём, ключи не дублируются.
        public static void AddTo(Dictionary<string, string[]> target)
        {
            foreach (KeyValuePair<string, string[]> pair in values) target[pair.Key] = pair.Value;
        }
    }
}
