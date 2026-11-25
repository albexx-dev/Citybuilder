using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса ServiceBuildingSystemTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class ServiceBuildingSystemTests
    {
        // EPIC-51: станция уровня 2 покрывает дом на клетку дальше, уровня 1 — не покрывает.
        [Test]
        public void LevelTwoStation_RadiusGrowsByOneCell()
        {
            BuildingDefinition fireStation, house;
            BuildingState station;
            List<BuildingState> placed, grown;
            MakeStationFixture(out fireStation, out house, out station, out placed, out grown);
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

        // Фикстура станции и дома на расстоянии 4 (issue #127).
        private static void MakeStationFixture(out BuildingDefinition fireStation, out BuildingDefinition house,
            out BuildingState station, out List<BuildingState> placed, out List<BuildingState> grown)
        {
            fireStation = ScriptableObject.CreateInstance<BuildingDefinition>();
            fireStation.Id = "fire_station"; fireStation.ResidentCapacity = 0;
            fireStation.ServiceKind = ServiceKind.Fire; fireStation.ServiceRadius = 3;
            station = new BuildingState(EntityId.New(), fireStation, new GridPosition(0, 0),
                new List<GridPosition> { new GridPosition(0, 0) });
            house = ScriptableObject.CreateInstance<BuildingDefinition>();
            house.Id = "house"; house.ResidentCapacity = 4;
            placed = new List<BuildingState> { station };
            grown = new List<BuildingState>
            {
                new BuildingState(EntityId.New(), house, new GridPosition(4, 0),
                    new List<GridPosition> { new GridPosition(4, 0) }),
            };
        }
    }
}
