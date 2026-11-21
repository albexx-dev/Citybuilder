using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Временные ключи покрытия не расходуют счётчик EntityId (ADR-009).
    public class CoverageIdDeterminismTests
    {
        private const int Tick = 17;
        private static readonly GridPosition Home = new GridPosition(12, 12);

        [Test]
        public void RepeatedRefresh_DoesNotConsumeEntityId()
        {
            EntityId.Reset();
            EntityId baseline = EntityId.New();
            ServiceBuildingSystem services = new ServiceBuildingSystem();
            services.SetSources(ServiceKind.Fire, new List<ServiceSource> { new ServiceSource(Home, 5) });
            for (int i = 0; i < 3; i++)
            {
                services.BuildSnapshot(new List<GridPosition> { Home });
                BuildingInfoQuery.GetRadiusCells(Home, 3, new GridSize(50, 50));
            }
            EntityId after = EntityId.New();
            Assert.AreEqual(baseline.Value + 1, after.Value);
        }

        // Дом на тех же координатах после пересчёта получает тот же id, что без пересчёта.
        [Test]
        public void SameCoordinates_IgniteAndSteal_StableAcrossRefresh()
        {
            EntityId.Reset();
            EntityId.New(); // дом до пересчёта

            ServiceBuildingSystem services = new ServiceBuildingSystem();
            services.SetSources(ServiceKind.Fire, new List<ServiceSource> { new ServiceSource(Home, 5) });
            services.BuildSnapshot(new List<GridPosition> { Home });
            services.BuildSnapshot(new List<GridPosition> { Home });
            EntityId createdAfter = EntityId.New();

            // Контроль: та же последовательность без пересчёта.
            EntityId.Reset();
            EntityId.New();
            EntityId createdControl = EntityId.New();
            Assert.AreEqual(createdControl.Value, createdAfter.Value);
            Assert.AreEqual(FireSystem.Hash(Tick, createdControl), FireSystem.Hash(Tick, createdAfter));
            Assert.AreEqual(CrimeSystem.Hash(Tick, createdControl), CrimeSystem.Hash(Tick, createdAfter));
        }
    }
}
