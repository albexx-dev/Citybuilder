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
    // Сторонние i18n-пакеты запрещены — таблица хранится здесь; сообщения ленты приходят
    // из EventMessageCatalog как ключ + аргументы и переводятся только в момент отрисовки.
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
        // Пар ключей лежит в LocalizationTable; EventMessageCatalog добавляет свои при старте типа.
        private static readonly Dictionary<string, string[]> table = new Dictionary<string, string[]>();
        // Сообщения ленты регистрируются в том же реестре: Localization.Keys их видит (ADR-013).
        static Localization()
        {
            LocalizationTable.AddTo(table);
            EventMessageCatalog.RegisterTo(table);
        }
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
