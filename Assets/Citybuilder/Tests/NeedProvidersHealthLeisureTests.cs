using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Тесты провайдеров здоровья и досуга в NeedProviders (EPIC-03 шаг 1).
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
    }
}
