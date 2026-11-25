using NUnit.Framework;

namespace Citybuilder
{
    // Чистая политика медленного списания: делитель 8 и целое деление без Unity и состояния.
    public class EffectiveUpkeepPolicyTests
    {
        [Test]
        public void Divisor_IsEight()
        {
            Assert.AreEqual(8, EffectiveUpkeepPolicy.Divisor);
        }

        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(7, 0)]
        [TestCase(8, 1)]
        [TestCase(26, 3)]
        [TestCase(236, 29)]
        public void Apply_DividesBaseUpkeep(int baseUpkeep, int expected)
        {
            Assert.AreEqual(expected, EffectiveUpkeepPolicy.Apply(baseUpkeep));
        }

        [Test]
        public void Apply_IsDeterministic()
        {
            Assert.AreEqual(EffectiveUpkeepPolicy.Apply(236), EffectiveUpkeepPolicy.Apply(236));
            Assert.GreaterOrEqual(EffectiveUpkeepPolicy.Apply(236), 0);
        }
    }
}
