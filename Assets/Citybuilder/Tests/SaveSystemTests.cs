using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Запись слота: снапшот, JSON-roundtrip, валидация. Без сцены и файлов.
    public class SaveSystemTests
    {
        [Test]
        public void Roundtrip_FullCity_Validates()
        {
            CitySnapshot snapshot = BuildSampleCity();
            CitySnapshot restored = SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(snapshot));
            Assert.IsTrue(SaveSlotStorage.Validate(restored));
            Assert.AreEqual(2, restored.Buildings.Count);
            Assert.AreEqual(1, restored.Households.Count);
            Assert.AreEqual(0, restored.Households[0].BuildingIndex);
            Assert.AreEqual(500, restored.Balance);
            Assert.AreEqual(42L, restored.TickCount);
        }

        [Test]
        public void BuildSnapshot_MapsHouseholdsToBuildingIndex()
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = "house";
            GridPosition origin = new GridPosition(1, 2);
            BuildingState home = new BuildingState(EntityId.New(), def, origin, def.CoversCells(origin));
            List<BuildingState> homes = new List<BuildingState>();
            homes.Add(home);
            List<HouseholdState> households = new List<HouseholdState>();
            households.Add(new HouseholdState(EntityId.New(), home, 3));
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(homes, households, null, 100, 7, null);
            Assert.IsTrue(SaveSlotStorage.Validate(snapshot));
            Assert.AreEqual("house", snapshot.Buildings[0].DefId);
            Assert.AreEqual(1, snapshot.Buildings[0].X);
            Assert.AreEqual(2, snapshot.Buildings[0].Y);
            Assert.AreEqual(0, snapshot.Households[0].BuildingIndex);
            Assert.AreEqual(3, snapshot.Households[0].Size);
            UnityEngine.Object.DestroyImmediate(def);
        }

        [Test]
        public void Validate_BadVersion_Fails()
        {
            CitySnapshot snapshot = BuildSampleCity();
            snapshot.Version = 99;
            Assert.IsFalse(SaveSlotStorage.Validate(snapshot));
        }

        [Test]
        public void Validate_HouseholdIndexOutOfRange_Fails()
        {
            CitySnapshot snapshot = BuildSampleCity();
            snapshot.Households[0].BuildingIndex = snapshot.Buildings.Count;
            Assert.IsFalse(SaveSlotStorage.Validate(snapshot));
        }

        [Test]
        public void Validate_EmptyCity_Passes()
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null);
            Assert.IsTrue(SaveSlotStorage.Validate(snapshot));
        }

        private static CitySnapshot BuildSampleCity()
        {
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.Balance = 500;
            snapshot.TickCount = 42;
            AddZone(snapshot, 0, 1, ZoneType.Residential);
            AddBuilding(snapshot, "house_a", 0, 0);
            AddBuilding(snapshot, "house_b", 3, 3);
            AddHousehold(snapshot, 0, 2);
            AddRoad(snapshot, 5, 5);
            return snapshot;
        }

        private static void AddZone(CitySnapshot snapshot, int x, int y, ZoneType type)
        {
            CitySnapshot.ZoneEntry zone = new CitySnapshot.ZoneEntry();
            zone.X = x;
            zone.Y = y;
            zone.Type = (int)type;
            snapshot.Zones.Add(zone);
        }

        private static void AddBuilding(CitySnapshot snapshot, string defId, int x, int y)
        {
            CitySnapshot.BuildingEntry building = new CitySnapshot.BuildingEntry();
            building.DefId = defId;
            building.X = x;
            building.Y = y;
            snapshot.Buildings.Add(building);
        }

        private static void AddHousehold(CitySnapshot snapshot, int buildingIndex, int size)
        {
            CitySnapshot.HouseholdEntry household = new CitySnapshot.HouseholdEntry();
            household.BuildingIndex = buildingIndex;
            household.Size = size;
            snapshot.Households.Add(household);
        }

        private static void AddRoad(CitySnapshot snapshot, int x, int y)
        {
            CitySnapshot.RoadEntry road = new CitySnapshot.RoadEntry();
            road.X = x;
            road.Y = y;
            snapshot.Roads.Add(road);
        }
    }
}
