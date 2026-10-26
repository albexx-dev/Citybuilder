using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Ручные здания в слоте v3: запись, JSON-roundtrip, пропуски и версии. Без сцены и файлов.
    public class ManualSaveTests
    {
        [Test]
        public void BuildSnapshot_WithManual_WritesBothAndVersionFour()
        {
            List<BuildingState> manual = MakeManual("house", 1, 2, "fire_station", 5, 6);
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 100, 7, null, 0, manual);
            Assert.AreEqual(4, snapshot.Version);
            Assert.AreEqual(2, snapshot.ManualBuildings.Count);
            Assert.AreEqual("house", snapshot.ManualBuildings[0].DefId);
            Assert.AreEqual(1, snapshot.ManualBuildings[0].X);
            Assert.AreEqual(5, snapshot.ManualBuildings[1].X);
            Assert.AreEqual(6, snapshot.ManualBuildings[1].Y);
            Assert.IsTrue(SaveSlotStorage.Validate(snapshot));
            DestroyManual(manual);
        }

        [Test]
        public void Json_Roundtrip_KeepsManualIdsAndCells()
        {
            List<BuildingState> manual = MakeManual("house", 2, 3, "fire_station", 7, 8);
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 50, 9, null, 2, manual);
            CitySnapshot restored = SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(snapshot));
            Assert.IsTrue(SaveSlotStorage.Validate(restored));
            Assert.AreEqual(4, restored.Version);
            Assert.AreEqual(2, restored.ManualBuildings.Count);
            Assert.AreEqual("fire_station", restored.ManualBuildings[1].DefId);
            Assert.AreEqual(7, restored.ManualBuildings[1].X);
            Assert.AreEqual(8, restored.ManualBuildings[1].Y);
            Assert.AreEqual(2, restored.TaxLevel);
            DestroyManual(manual);
        }

        [Test]
        public void BuildSnapshot_SkipsNullAndEmptyId()
        {
            BuildingDefinition emptyId = ScriptableObject.CreateInstance<BuildingDefinition>();
            emptyId.Id = "";
            List<BuildingState> manual = new List<BuildingState>();
            manual.Add(null);
            manual.Add(new BuildingState(EntityId.New(), null, new GridPosition(0, 0), new List<GridPosition>()));
            manual.Add(new BuildingState(EntityId.New(), emptyId, new GridPosition(1, 1), emptyId.CoversCells(new GridPosition(1, 1))));
            manual.Add(MakeState("house", 4, 4));
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null, 0, manual);
            Assert.AreEqual(1, snapshot.ManualBuildings.Count);
            Assert.AreEqual("house", snapshot.ManualBuildings[0].DefId);
            DestroyManual(manual);
        }

        [Test]
        public void BuildSnapshot_OldOverloads_HaveEmptyManual()
        {
            CitySnapshot six = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null);
            CitySnapshot seven = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null, 1);
            Assert.IsNotNull(six.ManualBuildings);
            Assert.AreEqual(0, six.ManualBuildings.Count);
            Assert.IsNotNull(seven.ManualBuildings);
            Assert.AreEqual(0, seven.ManualBuildings.Count);
        }

        [Test]
        public void Validate_VersionsOneToFour_Pass()
        {
            CitySnapshot snapshot = new CitySnapshot();
            for (int version = 1; version <= 4; version++)
            {
                snapshot.Version = version;
                Assert.IsTrue(SaveSlotStorage.Validate(snapshot), "version=" + version);
            }
        }

        [Test]
        public void Validate_VersionFive_Fails()
        {
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.Version = 5;
            Assert.IsFalse(SaveSlotStorage.Validate(snapshot));
        }

        // Старый слот v1/v2: поля ManualBuildings в JSON нет, это не ошибка.
        [Test]
        public void Validate_NullManualField_PassesAsOlderSlot()
        {
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.ManualBuildings = null;
            Assert.IsTrue(SaveSlotStorage.Validate(snapshot));
        }

        private static List<BuildingState> MakeManual(string idA, int xa, int ya, string idB, int xb, int yb)
        {
            List<BuildingState> manual = new List<BuildingState>();
            manual.Add(MakeState(idA, xa, ya));
            manual.Add(MakeState(idB, xb, yb));
            return manual;
        }

        private static BuildingState MakeState(string defId, int x, int y)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = defId;
            GridPosition origin = new GridPosition(x, y);
            return new BuildingState(EntityId.New(), def, origin, def.CoversCells(origin));
        }

        private static void DestroyManual(List<BuildingState> manual)
        {
            for (int i = 0; i < manual.Count; i++)
            {
                if (manual[i] == null || manual[i].Definition == null) continue;
                Object.DestroyImmediate(manual[i].Definition);
            }
        }
    }
}
