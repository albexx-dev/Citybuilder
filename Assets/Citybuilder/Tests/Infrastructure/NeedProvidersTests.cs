using System;
using NUnit.Framework;

namespace Citybuilder
{
    // Провайдеры потребностей: флаги, живые ставки, сумма и счётчик. Чистый C#.
    public class NeedProvidersTests
    {
        private const float Tolerance = 1e-4f;

        private static Func<float> Constant(float value)
        {
            return () => value;
        }

        [Test]
        public void NoProviders_NeutralRatesAndEmptySum()
        {
            NeedProviders providers = new NeedProviders();
            Assert.IsFalse(providers.HasPower);
            Assert.IsFalse(providers.HasWater);
            Assert.IsFalse(providers.HasWaste);
            Assert.AreEqual(1f, providers.PowerRate, Tolerance);
            Assert.AreEqual(1f, providers.WaterRate, Tolerance);
            Assert.AreEqual(1f, providers.WasteRate, Tolerance);
            Assert.AreEqual(0f, providers.Sum, Tolerance);
            Assert.AreEqual(0, providers.Count);
        }

        [Test]
        public void PowerOnly_CountOne()
        {
            NeedProviders providers = new NeedProviders(Constant(0.5f), null);
            Assert.IsTrue(providers.HasPower);
            Assert.IsFalse(providers.HasWater);
            Assert.AreEqual(1, providers.Count);
            Assert.AreEqual(0.5f, providers.Sum, Tolerance);
        }

        [Test]
        public void PowerAndWater_CountTwo()
        {
            NeedProviders providers = new NeedProviders(Constant(0.5f), Constant(0.25f), null);
            Assert.IsTrue(providers.HasWater);
            Assert.IsFalse(providers.HasWaste);
            Assert.AreEqual(2, providers.Count);
            Assert.AreEqual(0.75f, providers.Sum, Tolerance);
            Assert.AreEqual(1f, providers.WasteRate, Tolerance);
        }

        [Test]
        public void AllThree_HalfEach_CountThree()
        {
            NeedProviders providers = new NeedProviders(Constant(0.5f), Constant(0.5f), Constant(0.5f));
            Assert.IsTrue(providers.HasWaste);
            Assert.AreEqual(3, providers.Count);
            Assert.AreEqual(0.5f, providers.WasteRate, Tolerance);
            Assert.AreEqual(1.5f, providers.Sum, Tolerance);
        }

        [Test]
        public void Rates_ClampedToUnitRange()
        {
            NeedProviders above = new NeedProviders(Constant(1.5f), null, null);
            Assert.AreEqual(1f, above.PowerRate, Tolerance);
            Assert.AreEqual(1f, above.Sum, Tolerance);
            NeedProviders below = new NeedProviders(null, Constant(-1f), null);
            Assert.AreEqual(0f, below.WaterRate, Tolerance);
            Assert.AreEqual(0f, below.Sum, Tolerance);
        }

        [Test]
        public void Rates_ReadLiveValue()
        {
            float rate = 1f;
            NeedProviders providers = new NeedProviders(null, null, () => rate);
            Assert.AreEqual(1f, providers.WasteRate, Tolerance);
            rate = 0.25f;
            Assert.AreEqual(0.25f, providers.WasteRate, Tolerance);
            Assert.AreEqual(0.25f, providers.Sum, Tolerance);
        }
    }
}
