using NUnit.Framework;

namespace Citybuilder
{
    // Проверки баланса электроэнергии. Чистый C#, без Unity API.
    public class ElectricityTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Empty_DefaultsToFullSatisfaction()
        {
            ElectricitySystem power = new ElectricitySystem();
            Assert.AreEqual(0f, power.TotalDemand, Tolerance);
            Assert.AreEqual(0f, power.TotalSupply, Tolerance);
            Assert.AreEqual(1f, power.SatisfactionRate01, Tolerance);
            Assert.AreEqual(0f, power.SatisfactionOf(EntityId.New()), Tolerance);
            Assert.IsFalse(power.IsSatisfied(EntityId.New()));
        }

        [Test]
        public void FullCoverage_AllSatisfied()
        {
            EntityId home = EntityId.New();
            EntityId shop = EntityId.New();
            ElectricitySystem power = new ElectricitySystem();
            power.AddPlant(EntityId.New(), 100f);
            power.AddConsumer(home, 40f);
            power.AddConsumer(shop, 60f);
            Assert.AreEqual(100f, power.TotalDemand, Tolerance);
            Assert.AreEqual(100f, power.TotalSupply, Tolerance);
            Assert.AreEqual(1f, power.SatisfactionRate01, Tolerance);
            Assert.AreEqual(1f, power.SatisfactionOf(home), Tolerance);
            Assert.IsTrue(power.IsSatisfied(shop));
        }

        [Test]
        public void HalfShortage_RateIsHalf()
        {
            EntityId home = EntityId.New();
            ElectricitySystem power = new ElectricitySystem();
            power.AddPlant(EntityId.New(), 50f);
            power.AddConsumer(home, 100f);
            Assert.AreEqual(0.5f, power.SatisfactionRate01, Tolerance);
            Assert.AreEqual(0.5f, power.SatisfactionOf(home), Tolerance);
            Assert.IsFalse(power.IsSatisfied(home));
        }

        [Test]
        public void Shortage_EqualShareForEveryConsumer()
        {
            EntityId small = EntityId.New();
            EntityId large = EntityId.New();
            ElectricitySystem power = new ElectricitySystem();
            power.AddPlant(EntityId.New(), 50f);
            power.AddConsumer(small, 30f);
            power.AddConsumer(large, 70f);
            Assert.AreEqual(0.5f, power.SatisfactionOf(small), Tolerance);
            Assert.AreEqual(0.5f, power.SatisfactionOf(large), Tolerance);
        }

        [Test]
        public void ZeroDemand_ConsumerSatisfied()
        {
            EntityId idle = EntityId.New();
            ElectricitySystem power = new ElectricitySystem();
            power.AddConsumer(idle, 0f);
            Assert.AreEqual(0f, power.TotalDemand, Tolerance);
            Assert.AreEqual(1f, power.SatisfactionRate01, Tolerance);
            Assert.AreEqual(1f, power.SatisfactionOf(idle), Tolerance);
            Assert.IsTrue(power.IsSatisfied(idle));
        }

        [Test]
        public void Remove_ForgetsConsumerAndPlant()
        {
            EntityId home = EntityId.New();
            EntityId plant = EntityId.New();
            ElectricitySystem power = new ElectricitySystem();
            power.AddConsumer(home, 20f);
            power.AddPlant(plant, 20f);
            power.Remove(home);
            Assert.AreEqual(0f, power.TotalDemand, Tolerance);
            Assert.AreEqual(20f, power.TotalSupply, Tolerance);
            Assert.AreEqual(0f, power.SatisfactionOf(home), Tolerance);
            power.Remove(plant);
            Assert.AreEqual(0f, power.TotalSupply, Tolerance);
        }

        [Test]
        public void Clear_ResetsEverything()
        {
            ElectricitySystem power = new ElectricitySystem();
            power.AddConsumer(EntityId.New(), 15f);
            power.AddPlant(EntityId.New(), 5f);
            power.Clear();
            Assert.AreEqual(0f, power.TotalDemand, Tolerance);
            Assert.AreEqual(0f, power.TotalSupply, Tolerance);
            Assert.AreEqual(1f, power.SatisfactionRate01, Tolerance);
        }

        [Test]
        public void NegativeValues_ClampedToZero()
        {
            EntityId home = EntityId.New();
            ElectricitySystem power = new ElectricitySystem();
            power.AddConsumer(home, -10f);
            power.AddPlant(EntityId.New(), -5f);
            Assert.AreEqual(0f, power.TotalDemand, Tolerance);
            Assert.AreEqual(0f, power.TotalSupply, Tolerance);
            Assert.AreEqual(1f, power.SatisfactionOf(home), Tolerance);

            ElectricitySystem brownout = new ElectricitySystem();
            brownout.AddConsumer(EntityId.New(), 10f);
            brownout.AddPlant(EntityId.New(), -5f);
            Assert.AreEqual(0f, brownout.SatisfactionRate01, Tolerance);
        }

        [Test]
        public void DuplicateId_OverwritesValue()
        {
            EntityId home = EntityId.New();
            EntityId plant = EntityId.New();
            ElectricitySystem power = new ElectricitySystem();
            power.AddConsumer(home, 10f);
            power.AddConsumer(home, 25f);
            power.AddPlant(plant, 30f);
            power.AddPlant(plant, 70f);
            Assert.AreEqual(25f, power.TotalDemand, Tolerance);
            Assert.AreEqual(70f, power.TotalSupply, Tolerance);
        }
    }
}
