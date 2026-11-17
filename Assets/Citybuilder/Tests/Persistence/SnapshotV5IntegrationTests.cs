using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // EPIC-64 шаг 3: интеграция сейва v5 на реальном пути F9 — LoadController.LoadNow читает слот с диска
    // (Application.persistentDataPath), возвращает уровни, три ставки и пересобранный вид дома 2+ уровня.
    public class SnapshotV5IntegrationTests
    {
        private static readonly GridPosition Home = new GridPosition(0, 0), Extra = new GridPosition(1, 0);
        private static readonly GridPosition Plant = new GridPosition(6, 6);

        private GameObject host; private SettlementView view; private string dir;
        private SettlementGrowthController growth; private BuildingPlacementController placement;
        private TaxPolicyView tax; private SaveController save; private LoadController load;
        private AutosaveController autosave; private BuildingDefinition houseDef, plantDef;
        private readonly List<string> foreign = new List<string>();

        [SetUp]
        public void SetUp()
        {
            dir = Application.persistentDataPath;
            Directory.CreateDirectory(dir); foreign.AddRange(Directory.GetFiles(dir));
            host = new GameObject("V5Integration"); view = new GameObject("V5View").AddComponent<SettlementView>();
            GridVisualizer viz = host.AddComponent<GridVisualizer>(); ZonePaintingController zones = host.AddComponent<ZonePaintingController>();
            zones.GridVisualizer = viz; houseDef = NewDef("house_v5", ZoneType.Residential); plantDef = NewDef("plant_v5", ZoneType.None);
            growth = host.AddComponent<SettlementGrowthController>();
            growth.GridVisualizer = viz; growth.ZonePainter = zones; growth.Runner = host.AddComponent<SimulationRunner>();
            growth.Definitions = new List<BuildingDefinition> { houseDef }; growth.View = view;
            placement = host.AddComponent<BuildingPlacementController>();
            placement.GridVisualizer = viz; placement.Definitions = new List<BuildingDefinition> { plantDef };
            tax = host.AddComponent<TaxPolicyView>(); save = host.AddComponent<SaveController>(); save.Policy = tax; save.Placement = placement;
            autosave = host.AddComponent<AutosaveController>(); autosave.Save = save;
            load = host.AddComponent<LoadController>(); load.Policy = tax; load.Placement = placement;
            host.AddComponent<SettlementDemolitionController>().Growth = growth;
            BuildCity();
        }

        // Удаляются только слоты, которых не было до теста: реальные сейвы в Editor остаются в покое.
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host); Object.DestroyImmediate(view);
            Object.DestroyImmediate(houseDef); Object.DestroyImmediate(plantDef);
            foreach (string file in Directory.GetFiles(dir)) if (!foreign.Contains(file)) File.Delete(file);
        }

        // Город игрока: дом 2 уровня, станция 3 уровня, Жилая низкая, Коммерция высокая, Промзона обычная.
        private void BuildCity()
        {
            growth.RestoreGrown("house_v5", Home).ApplyHouseTick(2, 10); Assert.IsTrue(placement.RestoreManual("plant_v5", Plant));
            placement.Placed[0].ApplyUpgrade(); placement.Placed[0].ApplyUpgrade();
            tax.Policy.SetZone(ZoneType.Residential, TaxPolicy.Level.Low); tax.Policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.High);
        }

        // Живое состояние сбивается на значения, которых в слоте нет: загрузка обязана вернуть файл.
        private void DistortCity()
        {
            growth.FindStateAt(Home).ApplyHouseTick(3, 0); placement.Placed[0].ApplyHouseTick(1, 0);
            tax.Policy.SetZone(ZoneType.Residential, TaxPolicy.Level.High); tax.Policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.Low);
            tax.Policy.SetZone(ZoneType.Industrial, TaxPolicy.Level.High);
        }

        private void AssertRestored()
        {
            Assert.AreEqual(2, growth.FindStateAt(Home).Level); Assert.AreEqual(3, placement.Placed[0].Level);
            Assert.AreEqual(2, tax.Policy.RateFor(ZoneType.Residential)); Assert.AreEqual(6, tax.Policy.RateFor(ZoneType.Commercial));
            Assert.AreEqual(4, tax.Policy.RateFor(ZoneType.Industrial));
        }

        private Transform CubeAt(GridPosition origin)
        {
            float x = origin.X + 0.5f;
            for (int i = 0; i < view.transform.childCount; i++) if (Mathf.Abs(view.transform.GetChild(i).localPosition.x - x) < 0.01f) return view.transform.GetChild(i);
            return null;
        }

        private static BuildingDefinition NewDef(string id, ZoneType zone)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = id; def.AllowedZone = zone; def.Footprint = new Vector2Int(1, 1); def.ResidentCapacity = 4;
            return def;
        }

        // Слоты 0 и 2: пишет реальный SaveController, читает реальный LoadController — тот же путь, что F9.
        [Test]
        public void LoadNow_ManualSlots0AndTwo_RestoresLevelsAndThreeRatesFromDisk()
        {
            for (int slot = 0; slot <= 2; slot += 2)
            {
                Assert.IsTrue(save.SaveNow(slot, "Слот"), "не записан слот " + slot);
                DistortCity(); Assert.IsTrue(load.LoadNow(slot), "не загружен слот " + slot); AssertRestored();
            }
        }

        [Test]
        public void Autosave_On24thTick_WritesV5Slot_AndLoadNowRestoresLevelsAndRates()
        {
            for (int t = 1; t <= AutosaveController.AutosaveIntervalTicks; t++) autosave.OnClockTick(new SimulationTime(t));
            Assert.IsTrue(SaveSlotStorage.Validate(dir, SaveSlotIndex.AutosaveSlot), "autosave.json не записан");
            DistortCity(); Assert.IsTrue(load.LoadNow(SaveSlotIndex.AutosaveSlot)); AssertRestored();
        }

        // Слот v4 в том же файле: уровней и трёх ставок нет → дома уровня 1, все ставки равны TaxLevel.
        [Test]
        public void LoadNow_LegacyV4SlotInSameSlot_FallsBackToLevelOneAndEqualRates()
        {
            CitySnapshot v4 = save.BuildCurrentSnapshot();
            v4.Version = 4; v4.TaxRates = new int[0]; v4.TaxLevel = 3;
            for (int i = 0; i < v4.Buildings.Count; i++) v4.Buildings[i].Level = 0;
            for (int i = 0; i < v4.ManualBuildings.Count; i++) v4.ManualBuildings[i].Level = 0;
            Assert.IsTrue(SaveSlotStorage.Save(dir, 1, v4, "v4")); Assert.IsTrue(load.LoadNow(1));
            Assert.AreEqual(1, growth.FindStateAt(Home).Level); Assert.AreEqual(1, placement.Placed[0].Level);
            Assert.AreEqual(tax.Policy.RateFor(ZoneType.Residential), tax.Policy.RateFor(ZoneType.Commercial));
            Assert.AreEqual(tax.Policy.RateFor(ZoneType.Commercial), tax.Policy.RateFor(ZoneType.Industrial));
        }

        // Дыра 2: после F9 куб дома 2 уровня обязан быть выше (×1.5), чем был бы у уровня 1 (×1).
        [Test]
        public void LoadNow_RebuildsViewOfHouseAboveLevelOne_AndLeavesLevelOneHouse()
        {
            Assert.IsNotNull(growth.RestoreGrown("house_v5", Extra));
            Assert.IsTrue(save.SaveNow(0)); Assert.IsTrue(load.LoadNow(0));
            Assert.AreEqual(2, view.transform.childCount);
            Assert.AreEqual(1.5f, CubeAt(Home).localScale.y, 0.001f); Assert.AreEqual(1f, CubeAt(Extra).localScale.y, 0.001f);
        }

        [Test]
        public void LoadNow_TwiceInARow_KeepsLevels_AndOneViewPerHouse()
        {
            Assert.IsTrue(save.SaveNow(0));
            Assert.IsTrue(load.LoadNow(0)); Assert.IsTrue(load.LoadNow(0));
            Assert.AreEqual(2, growth.FindStateAt(Home).Level); Assert.AreEqual(3, placement.Placed[0].Level);
            Assert.AreEqual(1, view.transform.childCount); Assert.AreEqual(1.5f, CubeAt(Home).localScale.y, 0.001f);
        }

        // Дом 1 уровня сохраняет свой куб: пересборка только для уровня > 1, как решено в шаге 3.
        [Test]
        public void ViewRefresh_RebuildsOnlyHomesAboveLevelOne()
        {
            Assert.IsNotNull(growth.RestoreGrown("house_v5", Extra));
            Transform low = CubeAt(Extra);
            LoadedHouseViewRefresh.Apply(growth, growth.Homes);
            Assert.AreSame(low, CubeAt(Extra)); Assert.AreEqual(1f, low.localScale.y, 0.001f);
            Assert.AreEqual(1.5f, CubeAt(Home).localScale.y, 0.001f);
        }
    }
}
