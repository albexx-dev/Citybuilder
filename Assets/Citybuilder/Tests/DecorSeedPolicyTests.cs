using NUnit.Framework;

namespace Citybuilder
{
    // Политика декора (EPIC-56): детерминизм по клетке, границы шанса и вариантов.
    public class DecorSeedPolicyTests
    {
        [Test]
        public void HasDecor_SameCellAlwaysSameDecision()
        {
            bool decision = DecorSeedPolicy.HasDecor(7, 13, 0.15f);
            bool negative = DecorSeedPolicy.HasDecor(-4, 31, 0.15f);
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(decision, DecorSeedPolicy.HasDecor(7, 13, 0.15f));
                Assert.AreEqual(negative, DecorSeedPolicy.HasDecor(-4, 31, 0.15f));
            }
        }

        [Test]
        public void HasDecor_ExtremeChances()
        {
            Assert.IsFalse(DecorSeedPolicy.HasDecor(3, 4, 0f));
            Assert.IsFalse(DecorSeedPolicy.HasDecor(3, 4, -0.5f));
            Assert.IsTrue(DecorSeedPolicy.HasDecor(3, 4, 1f));
        }

        // Статистика на 2500 клетках: доля декора держится в 10-25%.
        [Test]
        public void HasDecor_FiftyByFifty_ShareWithinTenAndTwentyFivePercent()
        {
            int decorated = 0;
            for (int x = 0; x < 50; x++)
                for (int z = 0; z < 50; z++)
                    if (DecorSeedPolicy.HasDecor(x, z, 0.15f)) decorated++;
            Assert.GreaterOrEqual(decorated, 250);
            Assert.Less(decorated, 625);
        }

        [Test]
        public void VariantIndex_AlwaysWithinPrefabCountAndDeterministic()
        {
            for (int x = 0; x < 50; x++)
                for (int z = 0; z < 50; z++)
                {
                    int index = DecorSeedPolicy.VariantIndex(x, z, 4);
                    Assert.GreaterOrEqual(index, 0);
                    Assert.Less(index, 4);
                    Assert.AreEqual(index, DecorSeedPolicy.VariantIndex(x, z, 4));
                }
            Assert.AreEqual(0, DecorSeedPolicy.VariantIndex(1, 2, 0));
        }
    }
}
