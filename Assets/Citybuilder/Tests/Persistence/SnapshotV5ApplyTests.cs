using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Сейв v5 (EPIC-64 шаг 2): загрузка возвращает уровни зданий и три ставки; запись полей — SnapshotV5WriteTests.
    public class SnapshotV5ApplyTests
    {
        private readonly List<Object> assets = new List<Object>();

        [TearDown]
        public void TearDown() { for (int i = 0; i < assets.Count; i++) Object.DestroyImmediate(assets[i]); assets.Clear(); }

        // Игрок: дом вырос до 2, станцию улучшил до 3. С v6 серия комфорта тоже пишется в слот и восстанавливается.
        [Test]
        public void Roundtrip_GrownHouseAndUpgradedStation_AreRestoredByLevel()
        {
            BuildingState home = NewState(NewDef("house"), 0, 0); home.ApplyHouseTick(2, 40);
            BuildingState station = NewState(NewDef("power_plant"), 6, 6); station.ApplyUpgrade(); station.ApplyUpgrade();
            CitySnapshot slot = SlotOf(One(home), One(station));
            List<BuildingState> homes = ReloadHomes(slot, "house"); List<BuildingState> placed = ReloadManual(slot, "power_plant");
            Assert.AreEqual(2, homes[0].Level); Assert.AreEqual(40, homes[0].ComfortStreak); Assert.AreEqual(3, placed[0].Level);
            Assert.AreEqual(5, BuildingLevelPolicy.ServiceRadius(3, placed[0].Level)); Assert.AreEqual(12, BuildingLevelPolicy.ScaledOutput(6, placed[0].Level));
        }

        [Test]
        public void ManualLevels_MatchByCoordinates_NotByListOrder()
        {
            BuildingState first = NewState(NewDef("fire_station"), 0, 0);
            BuildingState second = NewState(NewDef("fire_station"), 2, 0); second.ApplyUpgrade();
            BuildingState third = NewState(NewDef("fire_station"), 4, 0); third.ApplyUpgrade(); third.ApplyUpgrade();
            List<BuildingState> placed = ReloadManual(SlotOf(null, new List<BuildingState> { first, second, third }), "fire_station");
            Assert.AreEqual(3, placed[0].Level); Assert.AreEqual(2, placed[1].Level); Assert.AreEqual(1, placed[2].Level);
        }

        // Слот v4: уровней и ставок в JSON нет — дома становятся 1 уровня, TaxLevel встаёт на все зоны.
        [Test]
        public void LegacyV4Slot_LevelsBecomeOne_AndSingleRateFillsAllZones()
        {
            CitySnapshot slot = SaveSlotStorage.FromJson("{\"Version\":4,\"TaxLevel\":3,"
                + "\"Buildings\":[{\"DefId\":\"house\",\"X\":0,\"Y\":0}],\"ManualBuildings\":[{\"DefId\":\"house\",\"X\":2,\"Y\":0}]}");
            Assert.IsTrue(SaveSlotStorage.Validate(slot));
            Assert.AreEqual(1, ReloadHomes(slot, "house")[0].Level); Assert.AreEqual(1, ReloadManual(slot, "house")[0].Level);
            TaxPolicy policy = new TaxPolicy(); SnapshotTaxRates.ApplyTo(slot, policy);
            Assert.AreEqual(4, policy.RateFor(ZoneType.Residential)); Assert.AreEqual(4, policy.RateFor(ZoneType.Commercial)); Assert.AreEqual(4, policy.RateFor(ZoneType.Industrial));
        }

        [Test]
        public void BrokenLevels_AreClampedToRangeOneToThree()
        {
            CitySnapshot slot = new CitySnapshot();
            slot.Buildings.Add(new CitySnapshot.BuildingEntry { DefId = "house", X = 0, Y = 0, Level = 9 });
            slot.Buildings.Add(new CitySnapshot.BuildingEntry { DefId = "house", X = 2, Y = 0, Level = -5 });
            List<BuildingState> homes = new List<BuildingState> { NewState(NewDef("house"), 0, 0), NewState(NewDef("house"), 2, 0) };
            SnapshotLevels.ApplyHomes(slot, homes); // битый уровень не проходит валидацию слота, но клампится и здесь
            Assert.AreEqual(3, homes[0].Level); Assert.AreEqual(1, homes[1].Level);
        }

        [Test]
        public void TaxRates_ThreeDifferentZones_AreAppliedPerZone()
        {
            CitySnapshot slot = new CitySnapshot(); slot.TaxLevel = 2; slot.TaxRates = new int[] { 1, 3, 2 };
            TaxPolicy policy = new TaxPolicy(); SnapshotTaxRates.ApplyTo(slot, policy);
            Assert.AreEqual(1, policy.RateFor(ZoneType.Residential)); Assert.AreEqual(4, policy.RateFor(ZoneType.Commercial)); Assert.AreEqual(2, policy.RateFor(ZoneType.Industrial));
        }

        [Test]
        public void TaxRates_BrokenOrMissing_FallBackToTaxLevel()
        {
            CitySnapshot slot = new CitySnapshot(); slot.TaxLevel = 1; slot.TaxRates = new int[] { 1, 9, 2 };
            TaxPolicy policy = new TaxPolicy(); policy.Set(TaxPolicy.Level.High);
            SnapshotTaxRates.ApplyTo(slot, policy);
            Assert.AreEqual(1, policy.RateFor(ZoneType.Commercial)); Assert.AreEqual(1, policy.RateFor(ZoneType.Industrial));
            SnapshotTaxRates.ApplyTo(null, policy); Assert.AreEqual(2, policy.RateFor(ZoneType.Residential));
        }

        // Повторная загрузка не наращивает уровень; null-списки, null-элементы и чужие клетки безопасны.
        [Test]
        public void Apply_IsIdempotent_AndSafeOnNullArguments()
        {
            CitySnapshot slot = new CitySnapshot();
            slot.Buildings.Add(new CitySnapshot.BuildingEntry { DefId = "house", X = 0, Y = 0, Level = 2 });
            slot.Buildings.Add(new CitySnapshot.BuildingEntry { DefId = "house", X = 9, Y = 9, Level = 3 });
            slot.Buildings.Add(null);
            List<BuildingState> homes = new List<BuildingState> { null, NewState(NewDef("house"), 0, 0) };
            SnapshotLevels.ApplyAll(slot, homes, null); SnapshotLevels.ApplyAll(slot, homes, homes);
            Assert.AreEqual(2, homes[1].Level);
            SnapshotLevels.ApplyAll(null, null, null); SnapshotTaxRates.ApplyTo(slot, null);
        }

        // Экран приёмки: станция 3 уровня + дом 2 уровня + Жилая низкая / Коммерция высокая / Промзона обычная.
        [Test]
        public void PlayerScenario_LevelsAndRates_SurviveSaveAndNewGame()
        {
            BuildingState home = NewState(NewDef("house"), 0, 0); home.ApplyHouseTick(2, 33);
            BuildingState station = NewState(NewDef("power_plant"), 6, 6); station.ApplyUpgrade(); station.ApplyUpgrade();
            CitySnapshot slot = SlotOf(One(home), One(station));
            Assert.IsTrue(SaveSlotStorage.Validate(slot));
            TaxPolicy policy = new TaxPolicy(); SnapshotTaxRates.ApplyTo(slot, policy);
            Assert.AreEqual(2, ReloadHomes(slot, "house")[0].Level); Assert.AreEqual(3, ReloadManual(slot, "power_plant")[0].Level);
            Assert.AreEqual(1, policy.RateFor(ZoneType.Residential)); Assert.AreEqual(4, policy.RateFor(ZoneType.Commercial)); Assert.AreEqual(2, policy.RateFor(ZoneType.Industrial));
        }

        // Дом возвращается тем же путём, что чистый C#-лоад: LoadSystem.ApplyBuildings, новый город.
        private List<BuildingState> ReloadHomes(CitySnapshot slot, string defId)
        {
            GridSystem grid = new GridSystem(new GridSize(16, 16), 1f);
            SettlementSystem settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            List<BuildingState> homes = new List<BuildingState>();
            LoadSystem.ApplyBuildings(slot, settlement, new List<BuildingDefinition> { NewDef(defId) }, homes);
            return homes;
        }

        // Ручные здания собираются в обратном порядке: угадать уровень по индексу нельзя, только по клетке.
        private List<BuildingState> ReloadManual(CitySnapshot slot, string defId)
        {
            List<BuildingState> placed = new List<BuildingState>();
            for (int i = slot.ManualBuildings.Count - 1; i >= 0; i--) placed.Add(NewState(NewDef(defId), slot.ManualBuildings[i].X, slot.ManualBuildings[i].Y));
            SnapshotLevels.ApplyManual(slot, placed);
            return placed;
        }

        // Слот так же, как его пишет SaveController: уровни из зданий, три ставки из политики, затем JSON.
        private static CitySnapshot SlotOf(List<BuildingState> homes, List<BuildingState> manual)
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(homes, null, null, 0, 0, null, 0, manual, 0);
            TaxPolicy policy = new TaxPolicy();
            policy.SetZone(ZoneType.Residential, TaxPolicy.Level.Low);
            policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.High);
            SnapshotTaxRates.Write(snapshot, policy); // Промзона остаётся обычной
            return SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(snapshot));
        }

        private BuildingState NewState(BuildingDefinition def, int x, int y)
        {
            GridPosition origin = new GridPosition(x, y);
            return new BuildingState(EntityId.New(), def, origin, def.CoversCells(origin));
        }

        private BuildingDefinition NewDef(string id)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = id; assets.Add(def);
            return def;
        }

        private static List<BuildingState> One(BuildingState state) { return new List<BuildingState> { state }; }
    }
}
