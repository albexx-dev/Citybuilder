using NUnit.Framework;

namespace Citybuilder
{
    // Проверки баланса воды. Чистый C#, без Unity API.
    public class WaterTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Empty_DefaultsToFullSatisfaction()
        {
            WaterSystem water = new WaterSystem();
            Assert.AreEqual(0f, water.TotalDemand, Tolerance);
            Assert.AreEqual(0f, water.TotalSupply, Tolerance);
            Assert.AreEqual(1f, water.SatisfactionRate01, Tolerance);
            Assert.AreEqual(0f, water.SatisfactionOf(EntityId.New()), Tolerance);
            Assert.IsFalse(water.IsSatisfied(EntityId.New()));
        }

        [Test]
        public void FullCoverage_AllSatisfied()
        {
            EntityId home = EntityId.New();
            EntityId shop = EntityId.New();
            WaterSystem water = new WaterSystem();
            water.AddSource(EntityId.New(), 100f);
            water.AddConsumer(home, 40f);
            water.AddConsumer(shop, 60f);
            Assert.AreEqual(100f, water.TotalDemand, Tolerance);
            Assert.AreEqual(100f, water.TotalSupply, Tolerance);
            Assert.AreEqual(1f, water.SatisfactionRate01, Tolerance);
            Assert.AreEqual(1f, water.SatisfactionOf(home), Tolerance);
            Assert.IsTrue(water.IsSatisfied(shop));
        }

        [Test]
        public void HalfShortage_RateIsHalf()
        {
            EntityId home = EntityId.New();
            WaterSystem water = new WaterSystem();
            water.AddSource(EntityId.New(), 50f);
            water.AddConsumer(home, 100f);
            Assert.AreEqual(0.5f, water.SatisfactionRate01, Tolerance);
            Assert.AreEqual(0.5f, water.SatisfactionOf(home), Tolerance);
            Assert.IsFalse(water.IsSatisfied(home));
        }

        [Test]
        public void Shortage_EqualShareForEveryConsumer()
        {
            EntityId small = EntityId.New();
            EntityId large = EntityId.New();
            WaterSystem water = new WaterSystem();
            water.AddSource(EntityId.New(), 50f);
            water.AddConsumer(small, 30f);
            water.AddConsumer(large, 70f);
            Assert.AreEqual(0.5f, water.SatisfactionOf(small), Tolerance);
            Assert.AreEqual(0.5f, water.SatisfactionOf(large), Tolerance);
        }

        [Test]
        public void ZeroDemand_ConsumerSatisfied()
        {
            EntityId idle = EntityId.New();
            WaterSystem water = new WaterSystem();
            water.AddConsumer(idle, 0f);
            Assert.AreEqual(0f, water.TotalDemand, Tolerance);
            Assert.AreEqual(1f, water.SatisfactionRate01, Tolerance);
            Assert.AreEqual(1f, water.SatisfactionOf(idle), Tolerance);
            Assert.IsTrue(water.IsSatisfied(idle));
        }

        [Test]
        public void Remove_ForgetsConsumerAndSource()
        {
            EntityId home = EntityId.New();
            EntityId source = EntityId.New();
            WaterSystem water = new WaterSystem();
            water.AddConsumer(home, 20f);
            water.AddSource(source, 20f);
            water.Remove(home);
            Assert.AreEqual(0f, water.TotalDemand, Tolerance);
            Assert.AreEqual(20f, water.TotalSupply, Tolerance);
            Assert.AreEqual(0f, water.SatisfactionOf(home), Tolerance);
            water.Remove(source);
            Assert.AreEqual(0f, water.TotalSupply, Tolerance);
        }

        [Test]
        public void Clear_ResetsEverything()
        {
            WaterSystem water = new WaterSystem();
            water.AddConsumer(EntityId.New(), 15f);
            water.AddSource(EntityId.New(), 5f);
            water.Clear();
            Assert.AreEqual(0f, water.TotalDemand, Tolerance);
            Assert.AreEqual(0f, water.TotalSupply, Tolerance);
            Assert.AreEqual(1f, water.SatisfactionRate01, Tolerance);
        }

        [Test]
        public void NegativeValues_ClampedToZero()
        {
            EntityId home = EntityId.New();
            WaterSystem water = new WaterSystem();
            water.AddConsumer(home, -10f);
            water.AddSource(EntityId.New(), -5f);
            Assert.AreEqual(0f, water.TotalDemand, Tolerance);
            Assert.AreEqual(0f, water.TotalSupply, Tolerance);
            Assert.AreEqual(1f, water.SatisfactionOf(home), Tolerance);

            WaterSystem dry = new WaterSystem();
            dry.AddConsumer(EntityId.New(), 10f);
            dry.AddSource(EntityId.New(), -5f);
            Assert.AreEqual(0f, dry.SatisfactionRate01, Tolerance);
        }

        [Test]
        public void DuplicateId_OverwritesValue()
        {
            EntityId home = EntityId.New();
            EntityId source = EntityId.New();
            WaterSystem water = new WaterSystem();
            water.AddConsumer(home, 10f);
            water.AddConsumer(home, 25f);
            water.AddSource(source, 30f);
            water.AddSource(source, 70f);
            Assert.AreEqual(25f, water.TotalDemand, Tolerance);
            Assert.AreEqual(70f, water.TotalSupply, Tolerance);
        }
    }
}
