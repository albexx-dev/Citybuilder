using NUnit.Framework;

namespace Citybuilder
{
    // Налоговая политика v2 (эпик #49): три зональные лестницы 1/2/4 (баланс v1.2, эпик #53),
    // дефолт 2/2/2, независимые циклы зон, формат строки бюджета, фолбэки. Без Unity API.
    public class TaxPolicyTests
    {
        private LanguageTestScope languageScope;

        [SetUp]
        public void InjectLanguageScope()
        {
            languageScope = new LanguageTestScope();
        }

        [TearDown]
        public void RestoreLanguageScope()
        {
            languageScope.Dispose();
        }

        [Test]
        public void Default_EachZone_IsNormalTwo()
        {
            TaxPolicy policy = new TaxPolicy();
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.LevelFor(ZoneType.Residential));
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.LevelFor(ZoneType.Commercial));
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.LevelFor(ZoneType.Industrial));
            Assert.AreEqual(2, policy.RateFor(ZoneType.Residential));
            Assert.AreEqual(2, policy.RateFor(ZoneType.Commercial));
            Assert.AreEqual(2, policy.RateFor(ZoneType.Industrial));
            Assert.AreEqual(2, policy.PerWorker);
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.Current);
        }

        [Test]
        public void RateFor_FollowsLadder_PerZone()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.SetZone(ZoneType.Residential, TaxPolicy.Level.Low);
            policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.High);
            Assert.AreEqual(1, policy.RateFor(ZoneType.Residential));
            Assert.AreEqual(4, policy.RateFor(ZoneType.Commercial));
            Assert.AreEqual(2, policy.RateFor(ZoneType.Industrial));
        }

        [Test]
        public void NextZone_CyclesEachZoneIndependently()
        {
            TaxPolicy policy = new TaxPolicy();
            Assert.AreEqual(TaxPolicy.Level.High, policy.NextZone(ZoneType.Commercial));
            Assert.AreEqual(TaxPolicy.Level.Low, policy.NextZone(ZoneType.Commercial));
            Assert.AreEqual(1, policy.RateFor(ZoneType.Commercial));
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.LevelFor(ZoneType.Residential));
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.LevelFor(ZoneType.Industrial));
        }

        [Test]
        public void NextZone_UnknownZone_KeepsRates()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.SetZone(ZoneType.Industrial, TaxPolicy.Level.High);
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.NextZone((ZoneType)99));
            Assert.AreEqual(2, policy.RateFor(ZoneType.Residential));
            Assert.AreEqual(4, policy.RateFor(ZoneType.Industrial));
        }

        [Test]
        public void SetZone_UnknownLevel_FallsBackToNormal()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.SetZone(ZoneType.Industrial, (TaxPolicy.Level)99);
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.LevelFor(ZoneType.Industrial));
            Assert.AreEqual(2, policy.RateFor(ZoneType.Industrial));
        }

        [Test]
        public void Set_AppliesOneLevelToAllZones()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.Set(TaxPolicy.Level.High);
            Assert.AreEqual(4, policy.RateFor(ZoneType.Residential));
            Assert.AreEqual(4, policy.RateFor(ZoneType.Commercial));
            Assert.AreEqual(4, policy.RateFor(ZoneType.Industrial));
            policy.Set((TaxPolicy.Level)99);
            Assert.AreEqual(2, policy.PerWorker);
        }

        [Test]
        public void RateFor_NoneZone_IsZero()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.Set(TaxPolicy.Level.High);
            Assert.AreEqual(0, policy.RateFor(ZoneType.None));
        }

        [Test]
        public void IncomeForZone_ScalesWithWorkersAndRate()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.High);
            Assert.AreEqual(12, policy.IncomeForZone(ZoneType.Commercial, 3));
            Assert.AreEqual(0, policy.IncomeForZone(ZoneType.Commercial, 0));
            Assert.AreEqual(0, policy.IncomeForZone(ZoneType.Commercial, -2));
            Assert.AreEqual(0, policy.IncomeForZone(ZoneType.None, 5));
        }

        [Test]
        public void ZoneLabel_ShowsZoneLevelRateAndIncome()
        {
            TaxPolicy policy = new TaxPolicy();
            Assert.AreEqual("Жилая: обычный (2) +4 $/ч", policy.ZoneLabel(ZoneType.Residential, 2));
            policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.High);
            Assert.AreEqual("Коммерция: высокий (4) +12 $/ч", policy.ZoneLabel(ZoneType.Commercial, 3));
            Assert.AreEqual("Промзона: обычный (2) +0 $/ч", policy.ZoneLabel(ZoneType.Industrial, 0));
        }
    }
}
