using NUnit.Framework;

namespace Citybuilder
{
    // Налоговая политика: ставки, переключение по кругу, фолбэк. Без Unity API.
    public class TaxPolicyTests
    {
        [Test]
        public void Default_IsNormalTwo()
        {
            TaxPolicy policy = new TaxPolicy();
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.Current);
            Assert.AreEqual(2, policy.PerWorker);
            Assert.AreEqual("Налог: обычный (2)", policy.Label);
        }

        [Test]
        public void Next_CyclesHighLowNormal()
        {
            TaxPolicy policy = new TaxPolicy();
            Assert.AreEqual(TaxPolicy.Level.High, policy.Next());
            Assert.AreEqual(3, policy.PerWorker);
            Assert.AreEqual(TaxPolicy.Level.Low, policy.Next());
            Assert.AreEqual(1, policy.PerWorker);
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.Next());
            Assert.AreEqual(2, policy.PerWorker);
        }

        [Test]
        public void Previous_CyclesLowHighNormal()
        {
            TaxPolicy policy = new TaxPolicy();
            Assert.AreEqual(TaxPolicy.Level.Low, policy.Previous());
            Assert.AreEqual(1, policy.PerWorker);
            Assert.AreEqual(TaxPolicy.Level.High, policy.Previous());
            Assert.AreEqual(3, policy.PerWorker);
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.Previous());
            Assert.AreEqual(2, policy.PerWorker);
        }

        [Test]
        public void Set_Unknown_FallsBackToNormal()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.Set(TaxPolicy.Level.High);
            policy.Set((TaxPolicy.Level)99);
            Assert.AreEqual(TaxPolicy.Level.Normal, policy.Current);
            Assert.AreEqual(2, policy.PerWorker);
        }

        [Test]
        public void Set_EachLevel_SetsRate()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.Set(TaxPolicy.Level.Low);
            Assert.AreEqual(1, policy.PerWorker);
            Assert.AreEqual("Налог: низкий (1)", policy.Label);
            policy.Set(TaxPolicy.Level.High);
            Assert.AreEqual(3, policy.PerWorker);
            Assert.AreEqual("Налог: высокий (3)", policy.Label);
        }

        [Test]
        public void Income_ScalesWithLevelAndWorkers()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.Set(TaxPolicy.Level.Low);
            Assert.AreEqual(4, policy.Income(4));
            policy.Set(TaxPolicy.Level.Normal);
            Assert.AreEqual(8, policy.Income(4));
            policy.Set(TaxPolicy.Level.High);
            Assert.AreEqual(12, policy.Income(4));
        }

        [Test]
        public void Income_WithoutWorkers_IsZero()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.Set(TaxPolicy.Level.High);
            Assert.AreEqual(0, policy.Income(0));
            Assert.AreEqual(0, policy.Income(-3));
        }

        [Test]
        public void Label_ContainsRate()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.Set(TaxPolicy.Level.Low);
            StringAssert.Contains("(1)", policy.Label);
            policy.Set(TaxPolicy.Level.Normal);
            StringAssert.Contains("(2)", policy.Label);
            policy.Set(TaxPolicy.Level.High);
            StringAssert.Contains("(3)", policy.Label);
        }
    }
}
