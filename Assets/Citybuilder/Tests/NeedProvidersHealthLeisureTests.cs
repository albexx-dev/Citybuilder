using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Тесты провайдеров здоровья и досуга в NeedProviders и 9 показателей в NeedSystem (EPIC-03 шаги 1-2).
    public class NeedProvidersHealthLeisureTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void SixProviders_CountIsSix_AndSumMatches()
        {
            NeedProviders providers = new NeedProviders(
                () => 0.5f, () => 0.5f, () => 0.5f, () => 0.5f, () => 0.25f, () => 0.75f);

            Assert.AreEqual(6, providers.Count);
            Assert.IsTrue(providers.HasHealth);
            Assert.IsTrue(providers.HasLeisure);
            Assert.AreEqual(0.25f, providers.HealthRate, Tolerance);
            Assert.AreEqual(0.75f, providers.LeisureRate, Tolerance);
            Assert.AreEqual(3f, providers.Sum, Tolerance);
        }

        [Test]
        public void NaNInHealthOrLeisure_SanitizedToOne()
        {
            NeedProviders providers = new NeedProviders(
                null, null, null, null, () => float.NaN, () => float.NaN);

            Assert.AreEqual(2, providers.Count);
            Assert.AreEqual(1f, providers.HealthRate, Tolerance);
            Assert.AreEqual(1f, providers.LeisureRate, Tolerance);
            Assert.AreEqual(2f, providers.Sum, Tolerance);
        }

        [Test]
        public void ServiceSystemFactories_ReadLiveHealthAndLeisureCoverage()
        {
            ServiceBuildingSystem services = new ServiceBuildingSystem();
            services.SetSources(ServiceKind.Health, new List<ServiceSource> { new ServiceSource(new GridPosition(0, 0), 5) });
            services.BuildSnapshot(new List<GridPosition> { new GridPosition(0, 0), new GridPosition(20, 20) });

            NeedProviders providers = new NeedProviders(
                null, null, null, null, NeedProviders.Health(services), NeedProviders.Leisure(services));

            Assert.AreEqual(0.5f, providers.HealthRate, Tolerance);
            Assert.AreEqual(0f, providers.LeisureRate, Tolerance);
        }

        [Test]
        public void NeedSystem_EightArgCtor_AveragesAcrossNineIndicators_AndFormatsHudLine()
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            SettlementSystem settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            PopulationSystem population = new PopulationSystem();
            // Пустой город: housing=0, jobs=1, employment=1 -> базовые дают 2.0
            NeedSystem needs = new NeedSystem(population, settlement,
                () => 1f, () => 1f, () => 1f, () => 1f, () => 0f, () => 0.5f);

            Assert.IsTrue(needs.HasHealth);
            Assert.IsTrue(needs.HasLeisure);
            Assert.AreEqual(0f, needs.HealthRate, Tolerance);
            Assert.AreEqual(0.5f, needs.LeisureRate, Tolerance);
            Assert.AreEqual(6.5f / 9f, needs.AverageSatisfaction, Tolerance);

            string line = NeedsView.BuildText(needs);
            StringAssert.Contains("здоровье 0%", line);
            StringAssert.Contains("досуг 50%", line);
        }
    }
}
