using NUnit.Framework;

namespace Citybuilder
{
    // Тики роста с учётом пробок (067). Только чистая функция, без Unity-сцены.
    public class GrowthCongestionTests
    {
        [Test]
        public void BaseOnly_FiveTicks()
        {
            Assert.AreEqual(5, GrowthPace.RequiredTicks(5, 1));
        }

        [Test]
        public void DoubleMultiplier_TenTicks()
        {
            Assert.AreEqual(10, GrowthPace.RequiredTicks(5, 2));
        }

        [Test]
        public void TripleMultiplier_FifteenTicks()
        {
            Assert.AreEqual(15, GrowthPace.RequiredTicks(5, 3));
        }

        [Test]
        public void ZeroBase_ClampsToOne()
        {
            Assert.AreEqual(1, GrowthPace.RequiredTicks(0, 2));
        }

        [Test]
        public void ZeroMultiplier_KeepsBase()
        {
            Assert.AreEqual(5, GrowthPace.RequiredTicks(5, 0));
        }

        [Test]
        public void NegativeMultiplier_KeepsBase()
        {
            Assert.AreEqual(5, GrowthPace.RequiredTicks(5, -1));
        }

        [Test]
        public void HugeProduct_ClampsToOneThousand()
        {
            Assert.AreEqual(1000, GrowthPace.RequiredTicks(500, 10));
        }
    }
}
