using System;
using NUnit.Framework;

namespace Citybuilder
{
    // Инфоменю и инспекция (issue #100 шаг 3): RU-строки совпадают со старыми,
    // EN-варианты непусты, геометрия панелей не меняется.
    public class LocalizationInfoTests
    {
        private Func<string> savedLoad;
        private Action<string> savedSave;

        [SetUp]
        public void InjectMemoryStore()
        {
            savedLoad = Localization.LoadPreference;
            savedSave = Localization.SavePreference;
            Localization.LoadPreference = () => string.Empty;
            Localization.SavePreference = value => { };
            Localization.SetLanguage(Language.Ru);
        }

        [TearDown]
        public void RestoreDefaults()
        {
            Localization.SetLanguage(Language.Ru);
            Localization.LoadPreference = savedLoad;
            Localization.SavePreference = savedSave;
        }

        private void AssertRuEn(string key, string ru, string en)
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual(ru, Localization.T(key), key);
            Localization.SetLanguage(Language.En);
            Assert.AreEqual(en, Localization.T(key), key);
        }

        [Test]
        public void InfoMenu_Sections_MatchLegacyRuStrings()
        {
            AssertRuEn("info_section_city", "=== Город ===", "=== City ===");
            AssertRuEn("info_section_goal", "=== Цель ===", "=== Goal ===");
            AssertRuEn("info_section_networks", "=== Сети ===", "=== Utilities ===");
            AssertRuEn("info_section_services", "=== Службы ===", "=== Services ===");
            AssertRuEn("info_section_budget", "=== Бюджет ===", "=== Budget ===");
            AssertRuEn("info_section_development", "=== Развитие ===", "=== Development ===");
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Преступность: 38%", Localization.T("info_crime", 38));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Crime: 38%", Localization.T("info_crime", 38));
        }

        [Test]
        public void InfoPanel_Lines_MatchLegacyRuStrings()
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("=== Дом ===", Localization.T("info_panel_title", "Дом"));
            Assert.AreEqual("Стоимость: $120 • Содержание: $5/ч", Localization.T("info_panel_cost", 120, 5));
            Assert.AreEqual("Жители: 3/6 • Дорога: да", Localization.T("info_panel_residents", 3, 6, Localization.T("info_yes")));
            Assert.AreEqual("Жители: 3/6 • Дорога: нет", Localization.T("info_panel_residents", 3, 6, Localization.T("info_no")));
            Assert.AreEqual("Уровень: 2", Localization.T("info_panel_level", 2));
            Assert.AreEqual("Свет 80% • Вода 90% • Мусор 100%", Localization.T("info_panel_supply", 80, 90, 100));
            Assert.AreEqual("Пожарные 70% • Полиция 60%", Localization.T("info_panel_safety", 70, 60));
            Assert.AreEqual("Больница 50% • Парк 40%", Localization.T("info_panel_care", 50, 40));
            Assert.AreEqual("Радиус: 5 кл. • Покрыто домов: 7/9", Localization.T("info_panel_radius", 5, 7, 9));
            Assert.AreEqual("Рабочих мест: 12", Localization.T("info_panel_jobs", 12));
            Assert.AreEqual("Улучшить — $240", Localization.T("info_panel_upgrade", 240));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Residents: 3/6 • Road: yes", Localization.T("info_panel_residents", 3, 6, Localization.T("info_yes")));
            Assert.AreEqual("Cost: $120 • Upkeep: $5/h", Localization.T("info_panel_cost", 120, 5));
        }
    }
}
