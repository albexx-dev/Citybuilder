using System;
using NUnit.Framework;

namespace Citybuilder
{
    // Энергия в потребностях: 4-частное среднее при провайдере, 3-частное без него.
    // Чистый C#, без Unity API и без ScriptableObject.
    public class PowerNeedTests
    {
        private const float Tolerance = 1e-4f;

        private static void MakeWorld(out PopulationSystem population, out SettlementSystem settlement)
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            population = new PopulationSystem();
        }

        private static NeedSystem MakeNeeds(Func<float> powerRate)
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out population, out settlement);
            return new NeedSystem(population, settlement, powerRate);
        }

        private static Func<float> Constant(float value)
        {
            return () => value;
        }

        // Пустой город без домов: жильё 0, работа 1, занятость 1 — сумма трёх равна 2.
        [Test]
        public void NoProvider_ThreePartAverage()
        {
            NeedSystem needs = MakeNeeds(null);
            Assert.IsFalse(needs.HasPower);
            Assert.AreEqual(1f, needs.PowerRate, Tolerance);
            Assert.AreEqual(2f / 3f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void OldConstructor_KeepsThreePartAverage()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out population, out settlement);
            NeedSystem needs = new NeedSystem(population, settlement);
            Assert.IsFalse(needs.HasPower);
            Assert.AreEqual(1f, needs.PowerRate, Tolerance);
            Assert.AreEqual(2f / 3f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void Provider_HalfPower_FourPartAverage()
        {
            NeedSystem needs = MakeNeeds(Constant(0.5f));
            Assert.IsTrue(needs.HasPower);
            Assert.AreEqual(0.5f, needs.PowerRate, Tolerance);
            Assert.AreEqual(0.625f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void Provider_ZeroPower_DropsSatisfaction()
        {
            NeedSystem needs = MakeNeeds(Constant(0f));
            Assert.IsTrue(needs.HasPower);
            Assert.AreEqual(0.5f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void Provider_AboveOne_ClampedToOne()
        {
            NeedSystem needs = MakeNeeds(Constant(1.5f));
            Assert.AreEqual(1f, needs.PowerRate, Tolerance);
            Assert.AreEqual(0.75f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void Provider_BelowZero_ClampedToZero()
        {
            NeedSystem needs = MakeNeeds(Constant(-1f));
            Assert.AreEqual(0f, needs.PowerRate, Tolerance);
            Assert.AreEqual(0.5f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void Provider_ReadsLiveValue()
        {
            float rate = 1f;
            NeedSystem needs = MakeNeeds(() => rate);
            rate = 0.25f;
            Assert.AreEqual(0.25f, needs.PowerRate, Tolerance);
            Assert.AreEqual(0.5625f, needs.AverageSatisfaction, Tolerance);
        }
    }
}
