using System;
using NUnit.Framework;

namespace Citybuilder
{
    // Мусор в потребностях: шестое слагаемое при провайдере, прежние /3, /4, /5 без него.
    // Чистый C#, без Unity API и без ScriptableObject.
    public class WasteNeedTests
    {
        private const float Tolerance = 1e-4f;

        private static void MakeWorld(out PopulationSystem population, out SettlementSystem settlement)
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            population = new PopulationSystem();
        }

        private static NeedSystem MakeNeeds(Func<float> powerRate, Func<float> waterRate, Func<float> wasteRate)
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out population, out settlement);
            return new NeedSystem(population, settlement, powerRate, waterRate, wasteRate);
        }

        private static Func<float> Constant(float value)
        {
            return () => value;
        }

        // Пустой город без домов: жильё 0, работа 1, занятость 1 — база равна 2.
        [Test]
        public void AllThree_Half_SixPartAverage()
        {
            NeedSystem needs = MakeNeeds(Constant(0.5f), Constant(0.5f), Constant(0.5f));
            Assert.IsTrue(needs.HasWaste);
            Assert.AreEqual(0.5f, needs.WasteRate, Tolerance);
            Assert.AreEqual(3.5f / 6f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void NoWasteProvider_FivePartAverage()
        {
            NeedSystem needs = MakeNeeds(Constant(0.5f), Constant(0.5f), null);
            Assert.IsFalse(needs.HasWaste);
            Assert.AreEqual(1f, needs.WasteRate, Tolerance);
            Assert.AreEqual(0.6f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void ZeroWater_ZeroWaste_SixPartAverage()
        {
            NeedSystem needs = MakeNeeds(Constant(0.5f), Constant(0f), Constant(0f));
            Assert.AreEqual(0f, needs.WaterRate, Tolerance);
            Assert.AreEqual(0f, needs.WasteRate, Tolerance);
            Assert.AreEqual(2.5f / 6f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void OldConstructors_KeepThreeFourFivePartAverages()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out population, out settlement);
            NeedSystem three = new NeedSystem(population, settlement);
            Assert.IsFalse(three.HasWaste);
            Assert.AreEqual(2f / 3f, three.AverageSatisfaction, Tolerance);
            NeedSystem four = new NeedSystem(population, settlement, Constant(0.5f));
            Assert.IsFalse(four.HasWaste);
            Assert.AreEqual(0.625f, four.AverageSatisfaction, Tolerance);
            NeedSystem five = new NeedSystem(population, settlement, Constant(0.5f), Constant(0.5f));
            Assert.IsFalse(five.HasWaste);
            Assert.AreEqual(0.6f, five.AverageSatisfaction, Tolerance);
        }
    }
}
