using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты обобщённой системы служб ServiceBuildingSystem (EPIC-01 шаг 1).
    public class ServiceBuildingSystemTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void EmptySnapshot_ZeroCounts_CoverageIsOneWithoutNaN()
        {
            ServiceBuildingSystem system = new ServiceBuildingSystem();
            system.BuildSnapshot(new List<GridPosition>());

            Assert.AreEqual(0, system.TotalHomesCount());
            Assert.AreEqual(0, system.CoveredHomesCount(ServiceKind.Fire));
            Assert.IsFalse(float.IsNaN(system.Coverage01(ServiceKind.Fire)));
            Assert.AreEqual(1f, system.Coverage01(ServiceKind.Fire), Tolerance);
        }

        [Test]
        public void SingleSource_InsideBoundaryAndOutside()
        {
            ServiceBuildingSystem system = new ServiceBuildingSystem();
            system.SetSources(ServiceKind.Fire, new List<ServiceSource>
            {
                new ServiceSource(new GridPosition(0, 0), 5)
            });
            GridPosition inside = new GridPosition(2, 2);
            GridPosition boundary = new GridPosition(3, 4);
            GridPosition outside = new GridPosition(4, 4);
            system.BuildSnapshot(new List<GridPosition> { inside, boundary, outside });

            Assert.AreEqual(3, system.TotalHomesCount());
            Assert.AreEqual(2, system.CoveredHomesCount(ServiceKind.Fire));
            Assert.AreEqual(1f, system.Coverage01(ServiceKind.Fire, inside), Tolerance);
            Assert.AreEqual(1f, system.Coverage01(ServiceKind.Fire, boundary), Tolerance);
            Assert.AreEqual(0f, system.Coverage01(ServiceKind.Fire, outside), Tolerance);
            Assert.AreEqual(2f / 3f, system.Coverage01(ServiceKind.Fire), Tolerance);
        }

        [Test]
        public void MixedKinds_FireAndPoliceDoNotBleedIntoEachOther()
        {
            ServiceBuildingSystem system = new ServiceBuildingSystem();
            system.SetSources(ServiceKind.Fire, new List<ServiceSource>
            {
                new ServiceSource(new GridPosition(0, 0), 2)
            });
            system.SetSources(ServiceKind.Police, new List<ServiceSource>
            {
                new ServiceSource(new GridPosition(20, 20), 2)
            });
            GridPosition nearFire = new GridPosition(1, 0);
            GridPosition nearPolice = new GridPosition(20, 19);
            system.BuildSnapshot(new List<GridPosition> { nearFire, nearPolice });

            Assert.AreEqual(1, system.CoveredHomesCount(ServiceKind.Fire));
            Assert.AreEqual(1, system.CoveredHomesCount(ServiceKind.Police));
            Assert.AreEqual(1f, system.Coverage01(ServiceKind.Fire, nearFire), Tolerance);
            Assert.AreEqual(0f, system.Coverage01(ServiceKind.Fire, nearPolice), Tolerance);
            Assert.AreEqual(1f, system.Coverage01(ServiceKind.Police, nearPolice), Tolerance);
            Assert.AreEqual(0f, system.Coverage01(ServiceKind.Police, nearFire), Tolerance);
        }

        [Test]
        public void RemovingSource_DropsCoverageOnRebuild()
        {
            ServiceBuildingSystem system = new ServiceBuildingSystem();
            List<GridPosition> houses = new List<GridPosition> { new GridPosition(0, 0) };
            system.SetSources(ServiceKind.Fire, new List<ServiceSource>
            {
                new ServiceSource(new GridPosition(0, 0), 3)
            });
            system.BuildSnapshot(houses);
            Assert.AreEqual(1, system.CoveredHomesCount(ServiceKind.Fire));

            system.SetSources(ServiceKind.Fire, new List<ServiceSource>());
            system.BuildSnapshot(houses);
            Assert.AreEqual(0, system.CoveredHomesCount(ServiceKind.Fire));
            Assert.AreEqual(0f, system.Coverage01(ServiceKind.Fire), Tolerance);
        }

        [Test]
        public void BuildSnapshotFromStates_UtilityManualBuilding_ExcludedFromDenominator()
        {
            BuildingDefinition fireStation = ScriptableObject.CreateInstance<BuildingDefinition>();
            fireStation.Id = "fire_station"; fireStation.DisplayName = "Fire Station";
            fireStation.ResidentCapacity = 0; fireStation.ServiceKind = ServiceKind.Fire; fireStation.ServiceRadius = 6;
            BuildingDefinition power = ScriptableObject.CreateInstance<BuildingDefinition>();
            power.Id = "power_station"; power.DisplayName = "Power Station";
            power.ResidentCapacity = 0; power.ServiceKind = ServiceKind.None;
            BuildingDefinition house = ScriptableObject.CreateInstance<BuildingDefinition>();
            house.Id = "house"; house.DisplayName = "House";
            house.ResidentCapacity = 4; house.ServiceKind = ServiceKind.None;
            List<BuildingState> placed = new List<BuildingState>
            {
                new BuildingState(EntityId.New(), fireStation, new GridPosition(0, 0),
                    new List<GridPosition> { new GridPosition(0, 0) }),
                new BuildingState(EntityId.New(), power, new GridPosition(4, 4),
                    new List<GridPosition> { new GridPosition(4, 4) }),
            };
            List<BuildingState> grown = new List<BuildingState>
            {
                new BuildingState(EntityId.New(), house, new GridPosition(2, 2),
                    new List<GridPosition> { new GridPosition(2, 2) }),
            };
            try
            {
                ServiceBuildingSystem system = new ServiceBuildingSystem();
                system.BuildSnapshotFromStates(placed, grown);

                Assert.AreEqual(1, system.TotalHomesCount());
                Assert.AreEqual(1, system.CoveredHomesCount(ServiceKind.Fire));
                Assert.AreEqual(1f, system.Coverage01(ServiceKind.Fire), Tolerance);
            }
            finally
            {
                ScriptableObject.DestroyImmediate(fireStation);
                ScriptableObject.DestroyImmediate(power);
                ScriptableObject.DestroyImmediate(house);
            }
        }

        // EPIC-51: станция уровня 2 покрывает дом на клетку дальше, уровня 1 — не покрывает.
        [Test]
        public void LevelTwoStation_RadiusGrowsByOneCell()
        {
            BuildingDefinition fireStation = ScriptableObject.CreateInstance<BuildingDefinition>();
            fireStation.Id = "fire_station"; fireStation.ResidentCapacity = 0;
            fireStation.ServiceKind = ServiceKind.Fire; fireStation.ServiceRadius = 3;
            BuildingState station = new BuildingState(EntityId.New(), fireStation, new GridPosition(0, 0),
                new List<GridPosition> { new GridPosition(0, 0) });
            BuildingDefinition house = ScriptableObject.CreateInstance<BuildingDefinition>();
            house.Id = "house"; house.ResidentCapacity = 4;
            List<BuildingState> placed = new List<BuildingState> { station };
            List<BuildingState> grown = new List<BuildingState>
            {
                new BuildingState(EntityId.New(), house, new GridPosition(4, 0),
                    new List<GridPosition> { new GridPosition(4, 0) }),
            };
            try
            {
                ServiceBuildingSystem system = new ServiceBuildingSystem();
                system.BuildSnapshotFromStates(placed, grown);
                Assert.AreEqual(1, system.TotalHomesCount());
                Assert.AreEqual(0, system.CoveredHomesCount(ServiceKind.Fire));

                station.ApplyUpgrade(); // уровень 2: радиус 3 → 4
                system.BuildSnapshotFromStates(placed, grown);
                Assert.AreEqual(1, system.CoveredHomesCount(ServiceKind.Fire));
                Assert.AreEqual(1f, system.Coverage01(ServiceKind.Fire, new GridPosition(4, 0)), Tolerance);
            }
            finally
            {
                ScriptableObject.DestroyImmediate(fireStation);
                ScriptableObject.DestroyImmediate(house);
            }
        }
    }
}
