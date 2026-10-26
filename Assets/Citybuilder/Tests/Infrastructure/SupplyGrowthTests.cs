using NUnit.Framework;

namespace Citybuilder
{
    // Глубина дефицита энергии/воды/мусора и множители темпа роста (095).
    // Чистый C#, без Unity API: только политика и расчёт тиков.
    public class SupplyGrowthTests
    {
        [Test]
        public void FullRate_NoPenalty()
        {
            Assert.AreEqual(1, SupplyGrowthPolicy.DepthMultiplier(1f));
        }

        [Test]
        public void GoodBoundary_NoPenalty()
        {
            Assert.AreEqual(1, SupplyGrowthPolicy.DepthMultiplier(0.99f));
        }

        [Test]
        public void JustBelowGood_Double()
        {
            Assert.AreEqual(2, SupplyGrowthPolicy.DepthMultiplier(0.98f));
        }

        [Test]
        public void PartialShortage_Double()
        {
            Assert.AreEqual(2, SupplyGrowthPolicy.DepthMultiplier(0.75f));
        }

        [Test]
        public void SevereBoundary_Double()
        {
            Assert.AreEqual(2, SupplyGrowthPolicy.DepthMultiplier(0.5f));
        }

        [Test]
        public void JustBelowSevere_Triple()
        {
            Assert.AreEqual(3, SupplyGrowthPolicy.DepthMultiplier(0.499f));
        }

        [Test]
        public void ZeroRate_Triple()
        {
            Assert.AreEqual(3, SupplyGrowthPolicy.DepthMultiplier(0f));
        }

        [Test]
        public void NegativeRate_Triple()
        {
            Assert.AreEqual(3, SupplyGrowthPolicy.DepthMultiplier(-0.2f));
        }

        [Test]
        public void NaNRate_TreatedAsNormal()
        {
            Assert.AreEqual(1, SupplyGrowthPolicy.DepthMultiplier(float.NaN));
        }

        [Test]
        public void WorstSystem_PowerTriple()
        {
            Assert.AreEqual(3, SupplyGrowthPolicy.Multiplier(0.4f, 1f, 1f));
        }

        [Test]
        public void WorstSystem_WaterDouble()
        {
            Assert.AreEqual(2, SupplyGrowthPolicy.Multiplier(1f, 0.7f, 1f));
        }

        [Test]
        public void WorstSystem_BothDouble()
        {
            Assert.AreEqual(2, SupplyGrowthPolicy.Multiplier(0.7f, 0.7f, 1f));
        }

        [Test]
        public void Combine_BothNormal()
        {
            Assert.AreEqual(1, GrowthPace.Combine(1, 1));
        }

        [Test]
        public void Combine_TakesWorst()
        {
            Assert.AreEqual(3, GrowthPace.Combine(3, 2));
        }

        [Test]
        public void Combine_EqualDouble()
        {
            Assert.AreEqual(2, GrowthPace.Combine(2, 2));
        }

        [Test]
        public void Combine_ClampsHugeToThree()
        {
            Assert.AreEqual(3, GrowthPace.Combine(0, 5));
        }

        [Test]
        public void RequiredTicks_BaseOnly()
        {
            Assert.AreEqual(5, GrowthPace.RequiredTicks(5, 1));
        }

        [Test]
        public void RequiredTicks_TripleMultiplier()
        {
            Assert.AreEqual(15, GrowthPace.RequiredTicks(5, 3));
        }

        [Test]
        public void RequiredTicks_ClampsToOneThousand()
        {
            Assert.AreEqual(1000, GrowthPace.RequiredTicks(500, 9));
        }

        [Test]
        public void RequiredTicks_MaxOfTwoSystemsNotProduct()
        {
            Assert.AreEqual(15, GrowthPace.RequiredTicks(5, 3, 2));
        }
    }
}
