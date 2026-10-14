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
            List<BuildingDefinition> defs = new List<BuildingDefinition>();
            defs.Add(def);
            List<BuildingState> homes = LoadSystem.Apply(snapshot, zones, roads, settlement, population, defs);
            Assert.AreEqual(2, homes.Count);
            Assert.AreEqual(2, settlement.SettledCount);
            Assert.AreEqual(2, population.HouseholdCount);
            Assert.AreEqual(5, population.TotalResidents);
            Assert.AreEqual(8, zones.CountZone(ZoneType.Residential));
            Assert.IsTrue(grid.IsOccupied(new GridPosition(8, 8)));
            Assert.IsTrue(grid.IsOccupied(new GridPosition(8, 9)));
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
            CitySnapshot.HouseholdEntry entry = new CitySnapshot.HouseholdEntry();
            entry.BuildingIndex = 0;
            entry.Size = 2;
            badHousehold.Households.Add(entry);
            Assert.IsFalse(SaveSlotStorage.Validate(badHousehold));
        }

        [Test]
        public void Apply_EmptySnapshot_NoErrors()
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null);
            Assert.IsTrue(SaveSlotStorage.Validate(snapshot));
            NewCity();
            List<BuildingState> homes = LoadSystem.Apply(snapshot, zones, roads, settlement, population, null);
            Assert.AreEqual(0, homes.Count);
            Assert.AreEqual(0, grid.OccupiedCount);
            Assert.AreEqual(0, LoadSystem.Apply(null, zones, null, null, null, null).Count);
            LoadSystem.ApplyZones(snapshot, null);
            LoadSystem.ApplyHouseholds(snapshot, null, homes);
        }

        [Test]
        public void Apply_UnknownDefId_Skipped()
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 100, 10, null);
            CitySnapshot.BuildingEntry building = new CitySnapshot.BuildingEntry();
            building.DefId = "ghost";
            building.X = 1;
            building.Y = 1;
            snapshot.Buildings.Add(building);
            NewCity();
            List<BuildingDefinition> defs = new List<BuildingDefinition>();
            List<BuildingState> homes = LoadSystem.Apply(snapshot, zones, roads, settlement, population, defs);
            Assert.AreEqual(0, homes.Count);
            Assert.AreEqual(0, population.HouseholdCount);
        }

        [Test]
        public void RestoreTime_SetsClockWithoutEvents()
        {
            SimulationClock clock = new SimulationClock();
            int started = 0;
            int completed = 0;
            clock.TickStarted += delegate { started++; };
            clock.TickCompleted += delegate { completed++; };
            clock.RestoreTime(100);
            Assert.AreEqual(100L, clock.CurrentTime.TickCount);
            Assert.AreEqual(0, started);
            Assert.AreEqual(0, completed);
        }

        private void NewCity()
        {
            grid = new GridSystem(new GridSize(16, 16), 1f);
            zones = new ZoneSystem(grid);
            settlement = new SettlementSystem(grid, zones, new BuildingSystem(grid));
            population = new PopulationSystem();
            roads = new RoadSystem(grid);
        }

        private CitySnapshot BuildLiveSnapshot(BuildingDefinition def)
        {
            PaintHouseZones();
            BuildingState home0 = settlement.Settle(def, new GridPosition(0, 0));
            BuildingState home1 = settlement.Settle(def, new GridPosition(4, 0));
            population.MoveIn(home0, 2);
            population.MoveIn(home1, 3);
            List<RoadState> roadStates = new List<RoadState>();
            roadStates.Add(roads.Place(new GridPosition(8, 8)));
            roadStates.Add(roads.Place(new GridPosition(8, 9)));
            List<BuildingState> liveHomes = new List<BuildingState>();
            liveHomes.Add(home0);
            liveHomes.Add(home1);
            return SaveSystem.BuildSnapshot(liveHomes, population.Households, zones.GetZonedCells(), 500, 42, roadStates);
        }

        private static BuildingDefinition NewHouseDef()
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = "house";
            def.AllowedZone = ZoneType.Residential;
            def.ResidentCapacity = 4;
            return def;
        }

        private void PaintHouseZones()
        {
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    zones.SetZone(new GridPosition(x, y), ZoneType.Residential);
                    zones.SetZone(new GridPosition(4 + x, y), ZoneType.Residential);
                }
            }
        }
    }
}
