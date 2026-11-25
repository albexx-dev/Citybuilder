using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса LoadSystemTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class LoadSystemTests
    {
        [Test]
        public void LoadController_ClearAll_ResetsSubsystems()
        {
            GameObject go = new GameObject("TestLC");
            BuildingDefinition def = NewHouseDef();
            try
            {
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                CongestionController cong = go.AddComponent<CongestionController>();
                PopulationController pop = go.AddComponent<PopulationController>();
                SettlementGrowthController growth = go.AddComponent<SettlementGrowthController>();
                LoadController lc = go.AddComponent<LoadController>();
                cong.GridVisualizer = viz; lc.Congestion = cong; lc.Population = pop; lc.Growth = growth;
                cong.TrackTrip(new[] { new GridPosition(0, 0), new GridPosition(0, 1) });
                BuildingState home = new BuildingState(EntityId.New(), def, new GridPosition(0, 0), def.CoversCells(new GridPosition(0, 0)));
                pop.Population.MoveIn(home, 2);
                lc.ClearAll();
                Assert.AreEqual(0, pop.Population.HouseholdCount);
                Assert.AreEqual(0, pop.Population.TotalResidents);
                Assert.AreEqual(0, cong.Routes.LoadOf(new GridPosition(0, 0)));
            }
            finally { Object.DestroyImmediate(def); Object.DestroyImmediate(go); }
        }

        private void NewCity()
        {
            grid = new GridSystem(new GridSize(16, 16), 1f);
            zones = new ZoneSystem(grid);
            settlement = new SettlementSystem(grid, zones, new BuildingSystem(grid));
            population = new PopulationSystem(); roads = new RoadSystem(grid);
        }

        private CitySnapshot BuildLiveSnapshot(BuildingDefinition def)
        {
            PaintHouseZones();
            BuildingState home0 = settlement.Settle(def, new GridPosition(0, 0));
            BuildingState home1 = settlement.Settle(def, new GridPosition(4, 0));
            population.MoveIn(home0, 2); population.MoveIn(home1, 3);
            List<RoadState> roadStates = new List<RoadState> { roads.Place(new GridPosition(8, 8)), roads.Place(new GridPosition(8, 9)) };
            List<BuildingState> liveHomes = new List<BuildingState> { home0, home1 };
            return SaveSystem.BuildSnapshot(liveHomes, population.Households, zones.GetZonedCells(), 500, 42, roadStates);
        }

        private static BuildingDefinition NewHouseDef()
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = "house"; def.AllowedZone = ZoneType.Residential; def.ResidentCapacity = 4;
            return def;
        }

        private void PaintHouseZones()
        {
            for (int x = 0; x < 2; x++) for (int y = 0; y < 2; y++)
            {
                zones.SetZone(new GridPosition(x, y), ZoneType.Residential);
                zones.SetZone(new GridPosition(4 + x, y), ZoneType.Residential);
            }
        }
    }
}
