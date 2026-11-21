using System;

namespace Citybuilder
{
    // Тестовая шина языка (F9): память вместо PlayerPrefs и фиксация RU на время теста.
    // Возвращает язык и шов персистенции в Dispose; вызывается из SetUp/TearDown тестов.
    public sealed class LanguageTestScope : IDisposable
    {
        private readonly Language savedLanguage;
        private readonly Func<string> savedLoad;
        private readonly Action<string> savedSave;
        private string stored;

        public LanguageTestScope()
        {
            savedLanguage = Localization.Language;
            savedLoad = Localization.LoadPreference;
            savedSave = Localization.SavePreference;
            stored = string.Empty;
            Localization.LoadPreference = () => stored;
            Localization.SavePreference = value => stored = value;
            Localization.SetLanguage(Language.Ru);
        }

        public void Dispose()
        {
            Localization.SetLanguage(savedLanguage);
            Localization.LoadPreference = savedLoad;
            Localization.SavePreference = savedSave;
        }
    }
}
