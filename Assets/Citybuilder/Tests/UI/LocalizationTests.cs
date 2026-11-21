using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Тесты локализации (EPIC-72 шаг 1): полнота таблицы, смена языка, событие,
    // сохранение выбора через шов и возврат самого ключа при отсутствии записи.
    public class LocalizationTests
    {
        private Func<string> savedLoad;
        private Action<string> savedSave;
        private string stored;

        [SetUp]
        public void InjectMemoryStore()
        {
            savedLoad = Localization.LoadPreference;
            savedSave = Localization.SavePreference;
            stored = string.Empty;
            Localization.LoadPreference = () => stored;
            Localization.SavePreference = value => stored = value;
            Localization.SetLanguage(Language.Ru);
        }

        [TearDown]
        public void RestoreDefaults()
        {
            Localization.SetLanguage(Language.Ru);
            Localization.LoadPreference = savedLoad;
            Localization.SavePreference = savedSave;
        }

        [Test]
        public void EveryKey_HasNonEmptyRuAndEn()
        {
            List<string> keys = new List<string>(Localization.Keys);
            Assert.Greater(keys.Count, 0);
            foreach (string key in keys)
            {
                Localization.SetLanguage(Language.Ru);
                string ru = Localization.T(key);
                Localization.SetLanguage(Language.En);
                string en = Localization.T(key);
                Assert.IsFalse(string.IsNullOrEmpty(ru), key);
                Assert.IsFalse(string.IsNullOrEmpty(en), key);
                Assert.AreNotEqual(key, ru, "RU-строка не должна совпадать с ключом: " + key);
                Assert.AreNotEqual(key, en, "EN-строка не должна совпадать с ключом: " + key);
            }
        }

        [Test]
        public void SwitchingLanguage_ChangesTranslation()
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Жилая", Localization.T("zone_residential"));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Residential", Localization.T("zone_residential"));
        }

        [Test]
        public void FormatPattern_SubstitutesArguments()
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("День 2, Час 8 — Игра", Localization.T("clock_line", 2, 8, "Игра"));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Day 2, Hour 8 — Playing", Localization.T("clock_line", 2, 8, "Playing"));
        }

        [Test]
        public void MissingKey_ReturnsKeyWithoutExceptions()
        {
            Assert.AreEqual("ui_missing_key", Localization.T("ui_missing_key"));
        }

        [Test]
        public void SetLanguage_PersistsChoiceThroughStore()
        {
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("En", stored);
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Ru", stored);
        }

        [Test]
        public void SetSameLanguage_DoesNotSaveOrFire()
        {
            int fired = 0;
            Action handler = () => fired++;
            Localization.LanguageChanged += handler;
            Localization.SetLanguage(Language.Ru);
            Localization.LanguageChanged -= handler;
            Assert.AreEqual(0, fired);
            Assert.AreEqual(string.Empty, stored);
        }

        [Test]
        public void SetLanguage_FiresChangedEventOnce()
        {
            int fired = 0;
            Action handler = () => fired++;
            Localization.LanguageChanged += handler;
            Localization.SetLanguage(Language.En);
            Localization.LanguageChanged -= handler;
            Assert.AreEqual(1, fired);
        }

        [Test]
        public void LoadSaved_RestoresLanguageFromStore()
        {
            stored = "En";
            Localization.LoadSaved();
            Assert.AreEqual(Language.En, Localization.Language);
            stored = "Ru";
            Localization.LoadSaved();
            Assert.AreEqual(Language.Ru, Localization.Language);
        }

        [Test]
        public void LoadSaved_UnknownOrEmptyValue_FallsBackToRu()
        {
            Localization.SetLanguage(Language.En);
            stored = "нет записи";
            Localization.LoadSaved();
            Assert.AreEqual(Language.Ru, Localization.Language);
            stored = string.Empty;
            Localization.LoadSaved();
            Assert.AreEqual(Language.Ru, Localization.Language);
        }
    }
}
