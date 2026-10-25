using System;
using NUnit.Framework;

namespace Citybuilder
{
    // Пожарные в потребностях: седьмое слагаемое при провайдере, без него прежнее /6.
    // Чистый C#, без Unity API и без ScriptableObject.
    public class FireNeedTests
    {
        private const float Tolerance = 1e-4f;

        private static void MakeWorld(out PopulationSystem population, out SettlementSystem settlement)
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            population = new PopulationSystem();
        }

        private static NeedSystem MakeNeeds(Func<float> powerRate, Func<float> waterRate,
            Func<float> wasteRate, Func<float> fireRate)
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out population, out settlement);
            return new NeedSystem(population, settlement, powerRate, waterRate, wasteRate, fireRate);
        }

        private static Func<float> Constant(float value)
        {
            return () => value;
        }

        // Пустой город без домов: база 2 (жильё 0, работа 1, занятость 1).
        [Test]
        public void AllFour_Half_SevenPartAverage()
        {
            NeedSystem needs = MakeNeeds(Constant(0.5f), Constant(0.5f), Constant(0.5f), Constant(0.5f));
            Assert.IsTrue(needs.HasFire);
            Assert.AreEqual(0.5f, needs.FireRate, Tolerance);
            Assert.AreEqual(4f / 7f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void NoFireProvider_SixPartAverage()
        {
            NeedSystem needs = MakeNeeds(Constant(0.5f), Constant(0.5f), Constant(0.5f), null);
            Assert.IsFalse(needs.HasFire);
            Assert.AreEqual(1f, needs.FireRate, Tolerance);
            Assert.AreEqual(3.5f / 6f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void ZeroFire_LowersAverage()
        {
            NeedSystem needs = MakeNeeds(Constant(0.5f), Constant(0.5f), Constant(0.5f), Constant(0f));
            Assert.AreEqual(0f, needs.FireRate, Tolerance);
            Assert.AreEqual(3.5f / 7f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void FireProvider_CountsAndSumsALone()
        {
            float rate = 0.5f;
            NeedProviders providers = new NeedProviders(null, null, null, () => rate);
            Assert.IsTrue(providers.HasFire);
            Assert.AreEqual(1, providers.Count);
            Assert.AreEqual(0.5f, providers.Sum, Tolerance);
            rate = 0.25f;
            Assert.AreEqual(0.25f, providers.Sum, Tolerance);
        }

        [Test]
        public void LiveFire_ChangesAverage()
        {
            float rate = 1f;
            NeedSystem needs = MakeNeeds(Constant(0.5f), Constant(0.5f), Constant(0.5f), () => rate);
            Assert.AreEqual(4.5f / 7f, needs.AverageSatisfaction, Tolerance);
            rate = 0f;
            Assert.AreEqual(3.5f / 7f, needs.AverageSatisfaction, Tolerance);
        }

        [Test]
        public void OldFiveArg_NoFire_SixPartAverage()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out population, out settlement);
            NeedSystem needs = new NeedSystem(population, settlement,
                Constant(0.5f), Constant(0.5f), Constant(0.5f));
            Assert.IsFalse(needs.HasFire);
            Assert.AreEqual(3.5f / 6f, needs.AverageSatisfaction, Tolerance);
        }
    }
}
