using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Сейв v5 (EPIC-64 шаг 1): уровни зданий и три ставки пишутся в JSON слота. Без сцены и файлов.
    // Битые значения полей проверяет SnapshotV5ValidationTests.
    public class SnapshotV5WriteTests
    {
        private readonly List<Object> assets = new List<Object>();

        [TearDown]
        public void TearDown() { for (int i = 0; i < assets.Count; i++) Object.DestroyImmediate(assets[i]); assets.Clear(); }

        [Test]
        public void Version_IsSix()
        {
            Assert.AreEqual(6, SaveSystem.CurrentVersion);
            Assert.AreEqual(6, new CitySnapshot().Version);
            Assert.AreEqual(6, SaveSystem.BuildSnapshot(null, null, null, 0, 0, null).Version);
        }

        // Игрок: дом вырос до 2, станцию улучшил до 2, ставки Жилая=1 / Коммерция=3 / Промзона=2.
        [Test]
        public void PlayerAction_LevelsAndRates_AreVisibleInJsonSlot()
        {
            BuildingState home = MakeState("house", 1, 2);
            BuildingState station = MakeState("fire_station", 5, 6);
            home.ApplyHouseTick(2, 0);
            station.ApplyUpgrade();
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(Single(home), null, null, 100, 7, null, 1, Single(station), 0);
            SnapshotTaxRates.Write(snapshot, PlayerPolicy());
            string json = SaveSlotStorage.ToJson(snapshot);
            Assert.IsTrue(json.Contains("\"Level\":2"), json);
            Assert.IsTrue(json.Contains("\"TaxRates\":[1,3,2]"), json);
            CitySnapshot restored = SaveSlotStorage.FromJson(json);
            Assert.IsTrue(SaveSlotStorage.Validate(restored));
            Assert.AreEqual(2, restored.Buildings[0].Level);
            Assert.AreEqual(2, restored.ManualBuildings[0].Level);
        }

        [Test]
        public void BuildSnapshot_WritesLevelsOfBothBuildingLists()
        {
            BuildingState grown = MakeState("house", 0, 0);
            BuildingState fresh = MakeState("house", 1, 0);
            BuildingState station = MakeState("fire_station", 2, 0);
            grown.ApplyHouseTick(3, 0);
            station.ApplyUpgrade();
            List<BuildingState> homes = new List<BuildingState> { grown, fresh };
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(homes, null, null, 0, 0, null, 0, Single(station), 0);
            Assert.AreEqual(3, snapshot.Buildings[0].Level);
            Assert.AreEqual(1, snapshot.Buildings[1].Level);
            Assert.AreEqual(2, snapshot.ManualBuildings[0].Level);
        }

        [Test]
        public void Rates_ThreeDifferentZones_RoundtripAndDecodeByZone()
        {
            CitySnapshot snapshot = new CitySnapshot();
            SnapshotTaxRates.Write(snapshot, PlayerPolicy());
            CitySnapshot restored = SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(snapshot));
            SnapshotTaxRates.Levels levels = SnapshotTaxRates.Read(restored);
            Assert.AreEqual(TaxPolicy.Level.Low, levels.Residential);
            Assert.AreEqual(TaxPolicy.Level.High, levels.Commercial);
            Assert.AreEqual(TaxPolicy.Level.Normal, levels.Industrial);
        }

        [Test]
        public void Write_NullPolicy_WritesEmptyRatesAndSlotValidates()
        {
            CitySnapshot snapshot = new CitySnapshot();
            SnapshotTaxRates.Write(snapshot, null);
            Assert.AreEqual(0, snapshot.TaxRates.Length);
            Assert.IsTrue(SaveSlotStorage.Validate(snapshot));
        }

        // Слот v4: полей Level/TaxRates в JSON нет — это «не задано», а не битый файл.
        [Test]
        public void LegacyV4Slot_ValidatesAndDecodesToOneRateForAllZones()
        {
            CitySnapshot restored = SaveSlotStorage.FromJson(
                "{\"Version\":4,\"TaxLevel\":1,\"Buildings\":[{\"DefId\":\"house\",\"X\":0,\"Y\":0}]}");
            Assert.IsTrue(SaveSlotStorage.Validate(restored));
            Assert.AreEqual(0, restored.Buildings[0].Level);
            SnapshotTaxRates.Levels levels = SnapshotTaxRates.Read(restored);
            Assert.AreEqual(TaxPolicy.Level.Low, levels.Residential);
            Assert.AreEqual(TaxPolicy.Level.Low, levels.Commercial);
            Assert.AreEqual(TaxPolicy.Level.Low, levels.Industrial);
        }

        [Test]
        public void SaveController_WritesRatesFromScenePolicy()
        {
            GameObject go = new GameObject("SnapshotV5Rates");
            SaveController save = go.AddComponent<SaveController>();
            TaxPolicyView view = go.AddComponent<TaxPolicyView>();
            save.Policy = view;
            view.Policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.High);
            CitySnapshot snapshot = save.BuildCurrentSnapshot();
            Assert.AreEqual(3, snapshot.TaxRates.Length);
            Assert.AreEqual(3, snapshot.TaxRates[1]);
            Object.DestroyImmediate(go);
        }

        private static TaxPolicy PlayerPolicy()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.SetZone(ZoneType.Residential, TaxPolicy.Level.Low);
            policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.High);
            policy.SetZone(ZoneType.Industrial, TaxPolicy.Level.Normal);
            return policy;
        }

        private static List<BuildingState> Single(BuildingState state) { return new List<BuildingState> { state }; }

        private BuildingState MakeState(string defId, int x, int y)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = defId;
            assets.Add(def);
            GridPosition origin = new GridPosition(x, y);
            return new BuildingState(EntityId.New(), def, origin, def.CoversCells(origin));
        }
    }
}
