using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты чистого запроса сведений о здании и клеток радиуса (EPIC-12 шаг 1 и 3).
    public class BuildingInfoQueryTests
    {
        [Test]
        public void Query_GrownHomeWithResidentsAndServices_ReturnsAccurateSnapshot()
        {
            GridSize size = new GridSize(50, 50);
            BuildingDefinition houseDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            houseDef.Id = "house";
            houseDef.DisplayName = "House";
            houseDef.ResidentCapacity = 4;
            houseDef.ConstructionCost = 100;

            BuildingState home = new BuildingState(
                EntityId.New(), houseDef, new GridPosition(2, 2),
                new List<GridPosition> { new GridPosition(2, 2), new GridPosition(3, 2) });

            PopulationSystem pop = new PopulationSystem();
            pop.MoveIn(home, 4);

            ServiceBuildingSystem services = new ServiceBuildingSystem();
            services.SetSources(ServiceKind.Fire, new List<ServiceSource> { new ServiceSource(new GridPosition(2, 2), 6) });
            services.BuildSnapshot(new List<GridPosition> { new GridPosition(2, 2) });

            BuildingInfo info = BuildingInfoQuery.Query(
                new GridPosition(3, 2), size,
                new List<BuildingState> { home }, null,
                pop, services, 1f, 0.8f, 0.6f, s => true);

            Assert.IsTrue(info.Found);
            Assert.IsTrue(info.IsGrown);
            Assert.AreEqual("House", info.DisplayName);
            Assert.AreEqual(4, info.Residents);
            Assert.AreEqual(4, info.Capacity);
            Assert.AreEqual(1f, info.FireCoverage01, 1e-4f);
            Assert.AreEqual(0f, info.HealthCoverage01, 1e-4f);
            Assert.IsTrue(info.RoadConnected);
        }

        [Test]
        public void Query_ServiceStationR6_CountsCoveredHomesMatchingServiceCoverage()
        {
            GridSize size = new GridSize(50, 50);
            BuildingDefinition houseDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            BuildingDefinition fireDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            fireDef.Id = "fire_station";
            fireDef.DisplayName = "Fire Station";
            fireDef.ServiceKind = ServiceKind.Fire;
            fireDef.ServiceRadius = 6;
            fireDef.UpkeepPerHour = 6;

            BuildingState h1 = new BuildingState(EntityId.New(), houseDef, new GridPosition(2, 2), new List<GridPosition> { new GridPosition(2, 2) });
            BuildingState h2 = new BuildingState(EntityId.New(), houseDef, new GridPosition(30, 30), new List<GridPosition> { new GridPosition(30, 30) });
            BuildingState st = new BuildingState(EntityId.New(), fireDef, new GridPosition(0, 0), new List<GridPosition> { new GridPosition(0, 0) });

            BuildingInfo info = BuildingInfoQuery.Query(
                new GridPosition(0, 0), size,
                new List<BuildingState> { h1, h2 },
                new List<BuildingState> { st },
                null, null, 1f, 1f, 1f, null);

            Assert.IsTrue(info.Found);
            Assert.IsTrue(info.HasServiceRadius);
            Assert.AreEqual(6, info.ServiceRadius);
            Assert.AreEqual(1, info.CoveredHomes);
            Assert.AreEqual(2, info.TotalHomes);
            Assert.AreEqual(6, info.UpkeepPerHour);
        }

        [Test]
        public void Query_EmptyCellOrOutOfBounds_ReturnsNotFoundWithoutException()
        {
            GridSize size = new GridSize(50, 50);
            Assert.IsFalse(BuildingInfoQuery.Query(new GridPosition(10, 10), size, null, null, null, null, 1f, 1f, 1f, null).Found);
            Assert.IsFalse(BuildingInfoQuery.Query(new GridPosition(-1, 99), size, null, null, null, null, 1f, 1f, 1f, null).Found);
        }

        [Test]
        public void GetRadiusCells_MatchesServiceCoverageBoundaryExactly()
        {
            GridSize size = new GridSize(50, 50);
            GridPosition center = new GridPosition(10, 10);
            List<GridPosition> cells = BuildingInfoQuery.GetRadiusCells(center, 6, size);

            CollectionAssert.Contains(cells, new GridPosition(16, 10));
            CollectionAssert.DoesNotContain(cells, new GridPosition(17, 10));
        }
    }
}
