using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Язык интерфейса (EPIC-72): тип на уровне namespace, чтобы свойство Localization.Language
    // могло называться как тип (паттерн Color Color) и код писал просто Language.Ru.
    public enum Language
    {
        Ru = 0,
        En = 1
    }

    // Реестр строк RU/EN (EPIC-72 шаг 1): ключ → пара переводов, выбор языка и событие смены.
    // Сторонние i18n-пакеты запрещены — таблица хранится здесь; сообщения ленты остаются RU до шага 2 (#88), оформление — через ключи локализации.
    public static class Localization
    {
        // Ключ PlayerPrefs, под которым хранится выбор языка ("Ru" / "En").
        public const string PrefKey = "citybuilder_language";

        // Шов персистенции для EditMode-тестов: по умолчанию PlayerPrefs, в тестах подменяется.
        public static Func<string> LoadPreference = () => PlayerPrefs.GetString(PrefKey, string.Empty);
        public static Action<string> SavePreference = value => PlayerPrefs.SetString(PrefKey, value);

        // Событие смены языка: для слушателей вне IMGUI (там строки перерисовываются каждый кадр).
        public static event Action LanguageChanged;

        private static Language language = Language.Ru;
        // [0] — русский, [1] — английский: порядок совпадает с enum Language.
        private static readonly Dictionary<string, string[]> table = new Dictionary<string, string[]>
        {
            // Верхние строки HUD: казна, население, часы.
            { "budget_line", new[] { "Доход: {0} $/ч - Расход: {1} $/ч = Итог: {2} $/ч", "Income: {0} $/h - Upkeep: {1} $/h = Net: {2} $/h" } },
            { "population_line", new[] { "Население: {0} / {1} — {2} домохозяйств", "Pop: {0} / {1} — {2} households" } },
            { "clock_line", new[] { "День {0}, Час {1} — {2}", "Day {0}, Hour {1} — {2}" } },
            { "clock_paused", new[] { "Пауза", "Paused" } },
            { "clock_playing", new[] { "Игра", "Playing" } },
            // Зоны и уровни налога: строки бюджета инфоменю (кисти зон в нижней панели — шаг 2).
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
            // Меню паузы: заголовок, кнопки, звук и подтверждение выхода (шаг 2).
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
            // Главное меню поверх живого города: строки «Продолжить/Новая игра/Загрузить/Выход».
            { "main_menu_title", new[] { "=== Citybuilder ===", "=== Citybuilder ===" } },
            { "main_menu_continue", new[] { "Продолжить", "Continue" } },
            { "main_menu_new_game", new[] { "Новая игра", "New game" } },
            { "main_menu_load", new[] { "Загрузить", "Load" } },
            { "main_menu_exit", new[] { "Выход", "Exit" } },
            // Меню сохранения в слоты: заголовок, кнопка, пустой слот и строка метаинформации.
            { "save_title", new[] { "=== Сохранить город ===", "=== Save city ===" } },
            { "save_save", new[] { "Сохранить", "Save" } },
            { "save_close", new[] { "Закрыть", "Close" } },
            { "save_slot_empty", new[] { "пусто", "empty" } },
            { "save_slot_info", new[] { "День {0} • {1}", "Day {0} • {1}" } },
            // Меню загрузки из слотов: заголовок, кнопка, пустой слот и строка метаинформации.
            { "load_title", new[] { "=== Загрузить город ===", "=== Load city ===" } },
            { "load_load", new[] { "Загрузить", "Load" } },
            { "load_close", new[] { "Закрыть", "Close" } },
            { "load_slot_empty", new[] { "{0} — пусто", "{0} — empty" } },
            { "load_slot_info", new[] { "{0} — День {1} ({2})", "{0} — Day {1} ({2})" } },
            // Лента событий (ADR-013, issue #87): заголовок, пустое состояние и префикс строки.
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
            // Разложение бюджета за час (issue #96): налоги − содержание − кражи + награды.
            { "budget_breakdown", new[] { "Налоги {0} - Содержание {1} - Кражи {2} + Награды {3} = {4} $/ч", "Tax {0} - Upkeep {1} - Theft {2} + Rewards {3} = {4} $/h" } }
        };

        // Есть ли ключ в таблице: фолбэк имени здания на DisplayName, если перевода нет.
        public static bool Has(string key)
        {
            return table.ContainsKey(key);
        }

        // Текущий язык интерфейса. По умолчанию русский; выбор игрока подтягивает LoadSaved.
        public static Language Language
        {
            get { return language; }
            set { SetLanguage(value); }
        }

        // Все ключи таблицы: тест полноты «в обоих языках нет пустых» и будущие проверки.
        public static ICollection<string> Keys
        {
            get { return table.Keys; }
        }

        // Смена языка: сохраняет выбор и сообщает слушателям. Повторное присваивание — тихий пропуск.
        public static void SetLanguage(Language value)
        {
            if (language == value) return;
            language = value;
            SavePreference(value == Language.En ? "En" : "Ru");
            Action handler = LanguageChanged;
            if (handler != null) handler();
        }

        // Читает сохранённый выбор игрока при старте сцены. Нет записи или битое значение — русский.
        public static void LoadSaved()
        {
            language = LoadPreference() == "En" ? Language.En : Language.Ru;
        }

        // Строка на текущем языке. Нет ключа — возвращается сам ключ, без исключений.
        // Аргументы подставляются в паттерн {0}..{n}, как в строках верхнего HUD.
        public static string T(string key, params object[] args)
        {
            string[] pair;
            string text = table.TryGetValue(key, out pair) ? pair[(int)language] : key;
            if (args != null && args.Length > 0) return string.Format(text, args);
            return text;
        }
    }
}
