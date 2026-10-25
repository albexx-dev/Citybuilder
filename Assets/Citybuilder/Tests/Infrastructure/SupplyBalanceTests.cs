using NUnit.Framework;

namespace Citybuilder
{
    // Проверки общего баланса спроса и предложения. Чистый C#, без Unity API.
    public class SupplyBalanceTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Empty_DefaultsToFullSatisfaction()
        {
            SupplyBalance balance = new SupplyBalance();
            Assert.AreEqual(0f, balance.TotalDemand, Tolerance);
            Assert.AreEqual(0f, balance.TotalSupply, Tolerance);
            Assert.AreEqual(1f, balance.Rate01, Tolerance);
        }

        [Test]
        public void FullCoverage_AllSatisfied()
        {
            EntityId home = EntityId.New();
            EntityId shop = EntityId.New();
            SupplyBalance balance = new SupplyBalance();
            balance.SetSupply(EntityId.New(), 100f);
            balance.SetDemand(home, 40f);
            balance.SetDemand(shop, 60f);
            Assert.AreEqual(100f, balance.TotalDemand, Tolerance);
            Assert.AreEqual(100f, balance.TotalSupply, Tolerance);
            Assert.AreEqual(1f, balance.Rate01, Tolerance);
            Assert.IsTrue(balance.IsSatisfied(home));
            Assert.IsTrue(balance.IsSatisfied(shop));
        }

        [Test]
        public void HalfShortage_RateIsHalf()
        {
            EntityId home = EntityId.New();
            SupplyBalance balance = new SupplyBalance();
            balance.SetSupply(EntityId.New(), 50f);
            balance.SetDemand(home, 100f);
            Assert.AreEqual(0.5f, balance.Rate01, Tolerance);
            Assert.AreEqual(0.5f, balance.RateOf(home), Tolerance);
            Assert.IsFalse(balance.IsSatisfied(home));
        }

        [Test]
        public void Shortage_EqualShareForEveryConsumer()
        {
            EntityId small = EntityId.New();
            EntityId large = EntityId.New();
            SupplyBalance balance = new SupplyBalance();
            balance.SetSupply(EntityId.New(), 50f);
            balance.SetDemand(small, 30f);
            balance.SetDemand(large, 70f);
            Assert.AreEqual(0.5f, balance.RateOf(small), Tolerance);
            Assert.AreEqual(0.5f, balance.RateOf(large), Tolerance);
        }

        [Test]
        public void ZeroDemand_ConsumerSatisfied()
        {
            EntityId idle = EntityId.New();
            SupplyBalance balance = new SupplyBalance();
            balance.SetDemand(idle, 0f);
            Assert.AreEqual(0f, balance.TotalDemand, Tolerance);
            Assert.AreEqual(1f, balance.Rate01, Tolerance);
            Assert.AreEqual(1f, balance.RateOf(idle), Tolerance);
            Assert.IsTrue(balance.IsSatisfied(idle));
        }

        [Test]
        public void Remove_ForgetsDemandAndSupply()
        {
            EntityId home = EntityId.New();
            EntityId source = EntityId.New();
            SupplyBalance balance = new SupplyBalance();
            balance.SetDemand(home, 20f);
            balance.SetSupply(source, 20f);
            balance.Remove(home);
            Assert.AreEqual(0f, balance.TotalDemand, Tolerance);
            Assert.AreEqual(20f, balance.TotalSupply, Tolerance);
            Assert.AreEqual(0f, balance.RateOf(home), Tolerance);
            balance.Remove(source);
            Assert.AreEqual(0f, balance.TotalSupply, Tolerance);
        }

        [Test]
        public void Clear_ResetsEverything()
        {
            SupplyBalance balance = new SupplyBalance();
            balance.SetDemand(EntityId.New(), 15f);
            balance.SetSupply(EntityId.New(), 5f);
            balance.Clear();
            Assert.AreEqual(0f, balance.TotalDemand, Tolerance);
            Assert.AreEqual(0f, balance.TotalSupply, Tolerance);
            Assert.AreEqual(1f, balance.Rate01, Tolerance);
        }

        [Test]
        public void NegativeAndNaN_ClampedToZero()
        {
            EntityId home = EntityId.New();
            SupplyBalance balance = new SupplyBalance();
            balance.SetDemand(home, -10f);
            balance.SetSupply(EntityId.New(), -5f);
            balance.SetDemand(EntityId.New(), float.NaN);
            balance.SetSupply(EntityId.New(), float.NaN);
            Assert.AreEqual(0f, balance.TotalDemand, Tolerance);
            Assert.AreEqual(0f, balance.TotalSupply, Tolerance);
            Assert.AreEqual(1f, balance.RateOf(home), Tolerance);

            SupplyBalance brownout = new SupplyBalance();
            brownout.SetDemand(EntityId.New(), 10f);
            brownout.SetSupply(EntityId.New(), -5f);
            Assert.AreEqual(0f, brownout.Rate01, Tolerance);
        }

        [Test]
        public void IsSatisfied_ThresholdIs0999()
        {
            EntityId home = EntityId.New();
            SupplyBalance exact = new SupplyBalance();
            exact.SetDemand(home, 1000f);
            exact.SetSupply(EntityId.New(), 999f);
            Assert.IsTrue(exact.IsSatisfied(home));

            SupplyBalance below = new SupplyBalance();
            below.SetDemand(home, 1000f);
            below.SetSupply(EntityId.New(), 998f);
            Assert.IsFalse(below.IsSatisfied(home));
        }
    }
}
