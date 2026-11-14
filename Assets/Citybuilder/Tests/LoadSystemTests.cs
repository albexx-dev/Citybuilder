using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Загрузка слота: roundtrip живых систем, битые данные, пустой город, тихие часы. Без сцены и файлов.
    public class LoadSystemTests
    {
        private GridSystem grid;
        private ZoneSystem zones;
        private SettlementSystem settlement;
        private PopulationSystem population;
        private RoadSystem roads;

        [Test]
        public void Roundtrip_LiveSystems_CountsMatch()
        {
            BuildingDefinition def = NewHouseDef();
            NewCity();
            CitySnapshot snapshot = BuildLiveSnapshot(def);
            Assert.IsTrue(SaveSlotStorage.Validate(snapshot));
            NewCity();
            List<BuildingState> homes = LoadSystem.Apply(snapshot, zones, roads, settlement, population, new List<BuildingDefinition> { def });
            Assert.AreEqual(2, homes.Count); Assert.AreEqual(2, settlement.SettledCount);
            Assert.AreEqual(2, population.HouseholdCount); Assert.AreEqual(5, population.TotalResidents);
            Assert.AreEqual(8, zones.CountZone(ZoneType.Residential));
            Assert.IsTrue(grid.IsOccupied(new GridPosition(8, 8))); Assert.IsTrue(grid.IsOccupied(new GridPosition(8, 9)));
            Assert.AreEqual(10, grid.OccupiedCount);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void Validate_BrokenSnapshot_Fails()
        {
            Assert.IsFalse(SaveSlotStorage.Validate(null));
            Assert.IsNull(SaveSlotStorage.FromJson(string.Empty));
            CitySnapshot badVersion = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null);
            badVersion.Version = 99;
            Assert.IsFalse(SaveSlotStorage.Validate(badVersion));
            CitySnapshot badHousehold = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null);
            badHousehold.Households.Add(new CitySnapshot.HouseholdEntry { BuildingIndex = 0, Size = 2 });
            Assert.IsFalse(SaveSlotStorage.Validate(badHousehold));
        }

        [Test]
        public void Apply_EmptySnapshot_NoErrors()
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null);
            Assert.IsTrue(SaveSlotStorage.Validate(snapshot));
            NewCity();
            List<BuildingState> homes = LoadSystem.Apply(snapshot, zones, roads, settlement, population, null);
            Assert.AreEqual(0, homes.Count); Assert.AreEqual(0, grid.OccupiedCount);
            Assert.AreEqual(0, LoadSystem.Apply(null, zones, null, null, null, null).Count);
            LoadSystem.ApplyZones(snapshot, null);
            LoadSystem.ApplyHouseholds(snapshot, null, homes);
        }

        [Test]
        public void Apply_UnknownDefId_Skipped()
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 100, 10, null);
            snapshot.Buildings.Add(new CitySnapshot.BuildingEntry { DefId = "ghost", X = 1, Y = 1 });
            NewCity();
            List<BuildingState> homes = LoadSystem.Apply(snapshot, zones, roads, settlement, population, new List<BuildingDefinition>());
            Assert.AreEqual(0, homes.Count); Assert.AreEqual(0, population.HouseholdCount);
        }

        [Test]
        public void RestoreTime_SetsClockWithoutEvents()
        {
            SimulationClock clock = new SimulationClock();
            int started = 0; int completed = 0;
            clock.TickStarted += delegate { started++; };
            clock.TickCompleted += delegate { completed++; };
            clock.RestoreTime(100);
            Assert.AreEqual(100L, clock.CurrentTime.TickCount);
            Assert.AreEqual(0, started); Assert.AreEqual(0, completed);
        }

        [Test]
        public void PopulationSystem_Clear_ResetsCountsAndHouseholds()
        {
            PopulationSystem pop = new PopulationSystem();
            BuildingDefinition def = NewHouseDef();
            BuildingState home = new BuildingState(EntityId.New(), def, new GridPosition(0, 0), def.CoversCells(new GridPosition(0, 0)));
            pop.MoveIn(home, 3);
            pop.Clear();
            Assert.AreEqual(0, pop.HouseholdCount);
            Assert.AreEqual(0, pop.TotalResidents);
            Assert.AreEqual(0, pop.Households.Count);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void SettlementSystem_Restore_BypassesZoneMismatch()
        {
            NewCity();
            BuildingDefinition def = NewHouseDef();
            GridPosition pos = new GridPosition(2, 2);
            Assert.IsNull(settlement.Settle(def, pos));
            Assert.IsNotNull(settlement.Restore(def, pos));
            Assert.AreEqual(1, settlement.SettledCount);
            Assert.AreEqual(4, settlement.TotalCapacity);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void SettlementSystem_Restore_FailsOnOccupied()
        {
            NewCity();
            BuildingDefinition def = NewHouseDef();
            GridPosition pos = new GridPosition(2, 2);
            Assert.IsNotNull(settlement.Restore(def, pos));
            Assert.IsNull(settlement.Restore(def, pos));
            Assert.AreEqual(1, settlement.SettledCount);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void LoadSystem_ApplyBuildings_RestoresBuildingsAcrossZoneChanges()
        {
            NewCity();
            BuildingDefinition def = NewHouseDef();
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.Buildings.Add(new CitySnapshot.BuildingEntry { DefId = "house", X = 2, Y = 2 });
            List<BuildingState> homes = new List<BuildingState>();
            LoadSystem.ApplyBuildings(snapshot, settlement, new List<BuildingDefinition> { def }, homes);
            Assert.AreEqual(1, homes.Count);
            Assert.AreEqual(1, settlement.SettledCount);
            Object.DestroyImmediate(def);
        }

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
