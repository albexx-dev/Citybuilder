using System;
using NUnit.Framework;

namespace Citybuilder
{
    // Статус роста (issue #100 шаг 3): склонение слова «дом» и RU/EN-шаблоны причин.
    public class LocalizationGrowthTests
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

        [Test]
        public void HousesWord_DeclensionInBothLanguages()
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("дом", GrowthStatusPolicy.HousesWord(1));
            Assert.AreEqual("дома", GrowthStatusPolicy.HousesWord(2));
            Assert.AreEqual("дома", GrowthStatusPolicy.HousesWord(3));
            Assert.AreEqual("домов", GrowthStatusPolicy.HousesWord(5));
            Assert.AreEqual("домов", GrowthStatusPolicy.HousesWord(11));
            Assert.AreEqual("дом", GrowthStatusPolicy.HousesWord(21));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("house", GrowthStatusPolicy.HousesWord(1));
            Assert.AreEqual("houses", GrowthStatusPolicy.HousesWord(2));
            Assert.AreEqual("houses", GrowthStatusPolicy.HousesWord(21));
        }

        [Test]
        public void GrowthPatterns_MatchLegacyRuStrings()
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Рост: 3 дома", Localization.T("growth_head", 3, GrowthStatusPolicy.HousesWord(3)));
            Assert.AreEqual(" — нет жилых зон, нажми [3]", Localization.T("growth_no_zones"));
            Assert.AreEqual(" — потолок 30 достигнут", Localization.T("growth_capacity", 30));
            Assert.AreEqual(" — нет денег: дом стоит $120", Localization.T("growth_no_money", 120));
            Assert.AreEqual(", следующий через 4 ч", Localization.T("growth_next", 4));
            Assert.AreEqual(" (медленнее ×2: дефицит снабжения и пробки)",
                Localization.T("growth_slow", 2, Localization.T("growth_slow_both")));
            Assert.AreEqual(" • низкая привлекательность (70%)", GrowthStatusPolicy.LowAttractivenessSuffix(0.7f));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Growth: 3 houses", Localization.T("growth_head", 3, GrowthStatusPolicy.HousesWord(3)));
            Assert.AreEqual(" — cap 30 reached", Localization.T("growth_capacity", 30));
            Assert.AreEqual(" • low attractiveness (70%)", GrowthStatusPolicy.LowAttractivenessSuffix(0.7f));
        }
    }
}
