using System;
using NUnit.Framework;

namespace Citybuilder
{
    // RU-выводы новых ключей меню (EPIC-72 шаг 2) байт-в-байт совпадают со старыми
    // захардкоженными строками; EN-варианты непусты; кисти зон идут через реестр.
    public class LocalizationUiTextsTests
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

        // Ключ на обоих языках: RU — ровно старый текст, EN — согласованный перевод.
        private void AssertRuEn(string key, string expectedRu, string expectedEn)
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual(expectedRu, Localization.T(key), key);
            Localization.SetLanguage(Language.En);
            Assert.AreEqual(expectedEn, Localization.T(key), key);
        }

        [Test]
        public void PauseMenu_Keys_MatchLegacyRuStrings()
        {
            AssertRuEn("pause_title", "=== Пауза ===", "=== Pause ===");
            AssertRuEn("pause_resume", "Продолжить [Esc]", "Resume [Esc]");
            AssertRuEn("pause_save", "Сохранить", "Save");
            AssertRuEn("pause_load", "Загрузить", "Load");
            AssertRuEn("pause_settings", "Настройки", "Settings");
            AssertRuEn("pause_main_menu", "В главное меню", "Main menu");
            AssertRuEn("pause_confirm_title", "Сохранить перед выходом?", "Save before exiting?");
            AssertRuEn("pause_confirm_yes", "Да (сохранить и выйти)", "Yes (save and exit)");
            AssertRuEn("pause_confirm_no", "Нет (выйти без сохранения)", "No (exit without saving)");
            AssertRuEn("pause_confirm_cancel", "Отмена", "Cancel");
        }

        [Test]
        public void PauseMenu_Audio_KeepsLegacyFormat()
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Громкость: 75%", Localization.T("pause_volume", 75));
            Assert.AreEqual(" Без звука", " " + Localization.T("pause_mute"));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Volume: 75%", Localization.T("pause_volume", 75));
            Assert.AreEqual(" Mute", " " + Localization.T("pause_mute"));
        }

        [Test]
        public void MainMenu_Keys_MatchLegacyRuStrings()
        {
            AssertRuEn("main_menu_title", "=== Citybuilder ===", "=== Citybuilder ===");
            AssertRuEn("main_menu_continue", "Продолжить", "Continue");
            AssertRuEn("main_menu_new_game", "Новая игра", "New game");
            AssertRuEn("main_menu_load", "Загрузить", "Load");
            AssertRuEn("main_menu_exit", "Выход", "Exit");
        }

        [Test]
        public void SaveMenu_Keys_MatchLegacyRuStrings()
        {
            AssertRuEn("save_title", "=== Сохранить город ===", "=== Save city ===");
            AssertRuEn("save_save", "Сохранить", "Save");
            AssertRuEn("save_close", "Закрыть", "Close");
            AssertRuEn("save_slot_empty", "пусто", "empty");
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("День 5 • 12:00", Localization.T("save_slot_info", 5, "12:00"));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Day 5 • 12:00", Localization.T("save_slot_info", 5, "12:00"));
        }

        [Test]
        public void LoadMenu_Keys_MatchLegacyRuStrings()
        {
            AssertRuEn("load_title", "=== Загрузить город ===", "=== Load city ===");
            AssertRuEn("load_load", "Загрузить", "Load");
            AssertRuEn("load_close", "Закрыть", "Close");
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Слот 1 — пусто", Localization.T("load_slot_empty", "Слот 1"));
            Assert.AreEqual("Слот 1 — День 5 (12:00)", Localization.T("load_slot_info", "Слот 1", 5, "12:00"));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Слот 1 — empty", Localization.T("load_slot_empty", "Слот 1"));
            Assert.AreEqual("Слот 1 — Day 5 (12:00)", Localization.T("load_slot_info", "Слот 1", 5, "12:00"));
        }

        [Test]
        public void ZoneBrushLabels_MatchZoneKeysInBothLanguages()
        {
            Localization.SetLanguage(Language.Ru);
            CollectionAssert.AreEqual(new[] { "Жилая", "Коммерция", "Промзона" }, BottomBarIcons.ZoneBrushLabels());
            Localization.SetLanguage(Language.En);
            CollectionAssert.AreEqual(new[] { "Residential", "Commercial", "Industrial" }, BottomBarIcons.ZoneBrushLabels());
        }
    }
}
