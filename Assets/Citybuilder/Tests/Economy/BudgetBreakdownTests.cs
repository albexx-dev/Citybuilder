using System;
using NUnit.Framework;

namespace Citybuilder
{
    // Разложение бюджета (issue #96): net = income − upkeep − thefts + rewards.
    // Чистая политика без Unity API: отрицательный, нулевой и положительный случаи + RU/EN.
    public class BudgetBreakdownTests
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
        public void Net_Negative_CountsTheftsAndRewards()
        {
            Assert.AreEqual(-110, BudgetBreakdown.Net(100, 200, 20, 10));
            Assert.IsTrue(BudgetBreakdown.IsDeficit(100, 200, 20, 10));
        }

        [Test]
        public void Net_Zero_IsNotDeficit()
        {
            Assert.AreEqual(0, BudgetBreakdown.Net(100, 80, 20, 0));
            Assert.IsFalse(BudgetBreakdown.IsDeficit(100, 80, 20, 0));
        }

        [Test]
        public void Net_Positive_RewardsOutweighThefts()
        {
            Assert.AreEqual(230, BudgetBreakdown.Net(300, 100, 20, 50));
            Assert.IsFalse(BudgetBreakdown.IsDeficit(300, 100, 20, 50));
        }

        [Test]
        public void Line_RuAndEn_KeepAllFourComponents()
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Налоги +100 - Содержание 200 - Кражи 20 + Награды +10 = -110 $/ч",
                BudgetBreakdown.Line(100, 200, 20, 10));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Tax +100 - Upkeep 200 - Theft 20 + Rewards +10 = -110 $/h",
                BudgetBreakdown.Line(100, 200, 20, 10));
        }
    }
}
