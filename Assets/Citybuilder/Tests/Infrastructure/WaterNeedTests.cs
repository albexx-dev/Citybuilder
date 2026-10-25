using System;
using NUnit.Framework;

namespace Citybuilder
{
    // Вода в потребностях: пятое слагаемое при провайдере, 3-4 частное без него.
    // Чистый C#, без Unity API и без ScriptableObject.
    public class WaterNeedTests
    {
        private const float Tolerance = 1e-4f;

        private static void MakeWorld(out PopulationSystem population, out SettlementSystem settlement)
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            population = new PopulationSystem();
        }

        private static NeedSystem MakeNeeds(Func<float> powerRate, Func<float> waterRate)
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out population, out settlement);
            return new NeedSystem(population, settlement, powerRate, waterRate);
        }

        private static Func<float> Constant(float value)
        {
            return () => value;
        }

        // Пустой город без домов: жильё 0, работа 1, занятость 1 — сумма трёх равна 2.
        [Test]
        public void OldConstructor_NoWater_ThreePartAverage()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out population, out settlement);
            NeedSystem needs = new NeedSystem(population, settlement);
            Assert.IsFalse(needs.HasWater);
            Assert.AreEqual(1f, needs.WaterRate, Tolerance);
            Assert.AreEqual(2f / 3f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void PowerOnly_HasNoWater_FourPartAverage()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out population, out settlement);
            NeedSystem needs = new NeedSystem(population, settlement, Constant(0.5f));
            Assert.IsFalse(needs.HasWater);
            Assert.AreEqual(1f, needs.WaterRate, Tolerance);
            Assert.AreEqual(0.625f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void PowerAndWater_Half_FivePartAverage()
        {
            NeedSystem needs = MakeNeeds(Constant(0.5f), Constant(0.5f));
            Assert.IsTrue(needs.HasWater);
            Assert.AreEqual(0.5f, needs.WaterRate, Tolerance);
            Assert.AreEqual(0.6f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void WaterOnly_Half_FourPartAverage()
        {
            NeedSystem needs = MakeNeeds(null, Constant(0.5f));
            Assert.IsFalse(needs.HasPower);
            Assert.IsTrue(needs.HasWater);
            Assert.AreEqual(0.625f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void FullPower_ZeroWater_FivePartAverage()
        {
            NeedSystem needs = MakeNeeds(Constant(1f), Constant(0f));
            Assert.AreEqual(0f, needs.WaterRate, Tolerance);
            Assert.AreEqual(0.6f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void WaterOutsideRange_ClampedToOneAndZero()
        {
            NeedSystem above = MakeNeeds(null, Constant(1.5f));
            Assert.AreEqual(1f, above.WaterRate, Tolerance);
            Assert.AreEqual(0.75f, above.AverageSatisfaction, Tolerance);
            NeedSystem below = MakeNeeds(null, Constant(-1f));
            Assert.AreEqual(0f, below.WaterRate, Tolerance);
            Assert.AreEqual(0.5f, below.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void Water_ReadsLiveValue()
        {
            float rate = 1f;
            NeedSystem needs = MakeNeeds(null, () => rate);
            Assert.AreEqual(0.75f, needs.AverageSatisfaction, Tolerance);
            rate = 0.25f;
            Assert.AreEqual(0.25f, needs.WaterRate, Tolerance);
            Assert.AreEqual(0.5625f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void Ctor_NullArguments_Throw()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out population, out settlement);
            Assert.Throws<ArgumentNullException>(
                () => new NeedSystem(null, settlement, null, Constant(1f)));
            Assert.Throws<ArgumentNullException>(
                () => new NeedSystem(population, null, null, Constant(1f)));
        }
    }
}
