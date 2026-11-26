using System;
using NUnit.Framework;

namespace Citybuilder
{
    // ADR-019: дефолт ×1 — тождество, долговой тик — честный минус казны (issue #168).
    public class MarketCreditTests
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
        public void DefaultMultiplier_IsIdentity_AtPriceAndUpkeepPoints()
        {
            MarketPricePolicy market = new MarketPricePolicy();
            Assert.AreEqual(1.0, market.Multiplier);
            Assert.IsTrue(market.IsDefault);
            Assert.AreEqual(137, market.Apply(137));
            Assert.AreEqual(0, market.Apply(0));
            // Порядок ADR-019: цена ×1, затем ÷8 ровно один раз.
            Assert.AreEqual(17, EffectiveUpkeepPolicy.Apply(market.Apply(137)));
            Assert.AreEqual(17, EffectiveUpkeepPolicy.Apply(137));
        }

        [Test]
        public void NonDefaultMultiplier_FloorsToNonNegativeInteger()
        {
            Assert.AreEqual(151, new MarketPricePolicy(1.5).Apply(101));
            Assert.AreEqual(3, new MarketPricePolicy(0.5).Apply(7));
            Assert.AreEqual(0, new MarketPricePolicy(2.0).Apply(-5));
        }

        [Test]
        public void InvalidMultiplier_FallsBackToNeutral()
        {
            Assert.IsTrue(new MarketPricePolicy(0.0).IsDefault);
            Assert.IsTrue(new MarketPricePolicy(-2.0).IsDefault);
            Assert.IsTrue(new MarketPricePolicy(double.NaN).IsDefault);
            Assert.IsTrue(new MarketPricePolicy(double.PositiveInfinity).IsDefault);
        }

        [Test]
        public void Borrow_PutsPrincipalIntoBalance()
        {
            Treasury treasury = new Treasury(0);
            CreditLedger ledger = new CreditLedger(0, 10);
            Assert.IsTrue(ledger.Borrow(treasury, 100));
            Assert.AreEqual(100, treasury.Balance);
            Assert.AreEqual(100, ledger.Principal);
            Assert.AreEqual(100, ledger.Outstanding);
            Assert.IsFalse(ledger.Borrow(treasury, 0));
        }

        [Test]
        public void DebtTick_ChargesInterestFromEmptyTreasury()
        {
            Treasury treasury = new Treasury(0);
            CreditLedger ledger = new CreditLedger(100, 10);
            Assert.AreEqual(10, ledger.InterestForTick());
            Assert.AreEqual(10, ledger.TickInterest(treasury));
            Assert.AreEqual(-10, treasury.Balance);
            Assert.IsTrue(treasury.IsInDeficit);
            Assert.AreEqual(100, ledger.Outstanding);
            Assert.AreEqual(10, ledger.TickInterest(treasury));
            Assert.AreEqual(-20, treasury.Balance);
        }

        [Test]
        public void SmallInterest_TruncatesToZero_AndDoesNotCharge()
        {
            Treasury treasury = new Treasury(7);
            CreditLedger ledger = new CreditLedger(9, 10);
            Assert.AreEqual(0, ledger.InterestForTick());
            Assert.AreEqual(0, ledger.TickInterest(treasury));
            Assert.AreEqual(7, treasury.Balance);
        }

        [Test]
        public void TrySpend_StillRefuses_WhileChargePasses()
        {
            Treasury treasury = new Treasury(5);
            Assert.IsFalse(treasury.TrySpend(50, "build").Success);
            Assert.AreEqual(5, treasury.Balance);
            Assert.IsTrue(treasury.Charge(50).Success);
            Assert.AreEqual(-45, treasury.Balance);
        }

        [Test]
        public void Repay_ReducesOutstanding_AndNeverOverpays()
        {
            Treasury treasury = new Treasury(200);
            CreditLedger ledger = new CreditLedger(100, 10);
            Assert.IsTrue(ledger.Repay(treasury, 40));
            Assert.AreEqual(60, ledger.Outstanding);
            Assert.AreEqual(160, treasury.Balance);
            Assert.IsFalse(ledger.Repay(treasury, 61));
            Assert.AreEqual(60, ledger.Outstanding);
            Assert.AreEqual(6, ledger.InterestForTick());
        }

        [Test]
        public void LineWithCredit_RuAndEn_LeavesOldLineUnchanged()
        {
            Assert.IsTrue(Localization.Has("budget_breakdown_credit"));
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Налоги +100 - Содержание 200 - Кражи 20 + Награды +10 - Процент 5 = -115 $/ч | Долг 50",
                BudgetBreakdown.LineWithCredit(100, 200, 20, 10, 50, 5));
            Assert.AreEqual("Налоги +100 - Содержание 200 - Кражи 20 + Награды +10 = -110 $/ч",
                BudgetBreakdown.Line(100, 200, 20, 10));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Tax +100 - Upkeep 200 - Theft 20 + Rewards +10 - Interest 5 = -115 $/h | Debt 50",
                BudgetBreakdown.LineWithCredit(100, 200, 20, 10, 50, 5));
            Assert.AreEqual("Tax +100 - Upkeep 200 - Theft 20 + Rewards +10 = -110 $/h",
                BudgetBreakdown.Line(100, 200, 20, 10));
        }
    }
}
