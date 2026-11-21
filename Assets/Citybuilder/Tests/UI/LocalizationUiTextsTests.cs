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

        // Новые ключи туториала и инфоменю: RU — старый литерал байт-в-байт, EN — без русского.
        [Test]
        public void TutorialAndInfoMenu_Keys_MatchLegacyRuStrings()
        {
            AssertRuEn("tutorial_dismiss", "Понятно", "Got it");
            AssertRuEn("minimap_tip", "Клик — перейти к клетке", "Click to focus the cell");
            AssertRuEn("tutorial_step_1", "Шаг 1/7. Нажми кнопку «Зоны [3]» на нижней панели, выбери кисть «Жилая» и закрась участок около 4×4 клетки на карте.", "Step 1/7. Press «Zones [3]» on the bottom bar, pick the «Residential» brush and paint an area of about 4x4 tiles on the map.");
            AssertRuEn("tutorial_step_4", "Шаг 4/7. Нажми кнопку «Дома [1]», выбери {0} в палитре НАД ПАНЕЛЬЮ и кликни свободную клетку на карте.", "Step 4/7. Press «Houses [1]», pick {0} in the palette ABOVE THE BAR and click a free tile on the map.");
            AssertRuEn("tutorial_event_fire", "Пожар! Построй пожарную станцию: открой вкладку «Службы» и выбери карточку в каталоге над панелью.", "Fire! Build a fire station: open the «Services» tab and pick the card in the catalog above the bar.");
            AssertRuEn("needs_base", "Потребности: {0}% (жильё {1}% • работа {2}% • занятость {3}%", "Needs: {0}% (housing {1}% • jobs {2}% • employment {3}%");
            AssertRuEn("service_label_police", "Полиция", "Police");
            AssertRuEn("goal_done", "Цель выполнена: {0} жителей", "Goal reached: {0} residents");
            AssertRuEn("progression_all", "Все типы открыты", "All types unlocked");
            AssertRuEn("metrics_title", "=== История ===", "=== History ===");
            AssertRuEn("metric_name_population", "Население", "Population");
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Шаг 4/7. Нажми кнопку «Дома [1]», выбери Электростанция в палитре НАД ПАНЕЛЬЮ и кликни свободную клетку на карте.",
                Localization.T("tutorial_step_4", Localization.T("building_power_station")));
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
