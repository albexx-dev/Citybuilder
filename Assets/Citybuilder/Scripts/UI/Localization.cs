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
    // Сторонние i18n-пакеты запрещены — таблица хранится здесь; лента событий остаётся RU до шага 2.
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
            { "catalog_locked", new[] { "\n— {0} зд.", "\n— {0} locked" } }
        };

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
