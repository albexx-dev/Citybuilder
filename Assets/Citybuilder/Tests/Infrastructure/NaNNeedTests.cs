using System;
using NUnit.Framework;

namespace Citybuilder
{
    // NaN от провайдера = «нет ограничения»: ставка 1, как у отсутствующего провайдера.
    // Бесконечности закрываются обычными сравнениями: +∞ → 1, −∞ → 0.
    public class NaNNeedTests
    {
        private const float Tolerance = 1e-4f;

        private static Func<float> Constant(float value)
        {
            return () => value;
        }

        private static NeedSystem MakeNeeds(Func<float> powerRate, Func<float> waterRate,
            Func<float> wasteRate, Func<float> fireRate)
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            SettlementSystem settlement =
                new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            return new NeedSystem(new PopulationSystem(), settlement,
                powerRate, waterRate, wasteRate, fireRate);
        }

        [Test]
        public void NaNProvider_RateIsOneAndHasTrue()
        {
            NeedProviders providers = new NeedProviders(Constant(float.NaN), null, null, null);
            Assert.IsTrue(providers.HasPower);
            Assert.IsFalse(float.IsNaN(providers.PowerRate));
            Assert.AreEqual(1f, providers.PowerRate, Tolerance);
        }

        [Test]
        public void NaNSumAndCount_CountedAsOne()
        {
            NeedProviders providers = new NeedProviders(Constant(float.NaN), Constant(0.5f), null, null);
            Assert.AreEqual(2, providers.Count);
            Assert.IsFalse(float.IsNaN(providers.Sum));
            Assert.AreEqual(1.5f, providers.Sum, Tolerance);
        }

        [Test]
        public void NaNAverage_FiniteAndSameAsUnlimited()
        {
            NeedSystem nan = MakeNeeds(Constant(float.NaN), Constant(0.5f), Constant(0.5f), Constant(0.5f));
            NeedSystem plain = MakeNeeds(Constant(1f), Constant(0.5f), Constant(0.5f), Constant(0.5f));
            Assert.IsFalse(float.IsNaN(nan.AverageSatisfaction));
            Assert.IsFalse(float.IsInfinity(nan.AverageSatisfaction));
            Assert.AreEqual(4.5f / 7f, nan.AverageSatisfaction, Tolerance);
            Assert.AreEqual(plain.AverageSatisfaction, nan.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void PositiveInfinity_ClampedToOne()
        {
            NeedProviders providers = new NeedProviders(null, Constant(float.PositiveInfinity), null, null);
            Assert.AreEqual(1f, providers.WaterRate, Tolerance);
            Assert.AreEqual(1f, providers.Sum, Tolerance);
        }

        [Test]
        public void NegativeInfinity_ClampedToZero()
        {
            NeedProviders providers = new NeedProviders(null, Constant(float.NegativeInfinity), null, null);
            Assert.AreEqual(0f, providers.WaterRate, Tolerance);
            Assert.AreEqual(0f, providers.Sum, Tolerance);
        }
    }
}
