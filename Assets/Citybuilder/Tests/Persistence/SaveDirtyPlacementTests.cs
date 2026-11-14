using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты маркировки SaveController.IsDirty при мутациях стройки, дорог, зон и роста.
    public class SaveDirtyPlacementTests
    {
        [Test]
        public void ManualPlacement_SuccessAndDemolish_MarksSaveDirty()
        {
            GameObject go = new GameObject("TestManualPlacementDirty");
            try
            {
                SaveController save = go.AddComponent<SaveController>();
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Id = "bld_1x1"; def.Footprint = new Vector2Int(1, 1); def.ConstructionCost = 0;
                BuildingPlacementController placement = go.AddComponent<BuildingPlacementController>();
                placement.GridVisualizer = viz; placement.Definition = def; placement.Save = save;

                Assert.IsFalse(save.IsDirty);
                Assert.IsTrue(placement.TryPlaceAt(new GridPosition(0, 0))); Assert.IsTrue(save.IsDirty);
                save.ClearDirty();
                Assert.IsTrue(placement.TryDemolish(new GridPosition(0, 0))); Assert.IsTrue(save.IsDirty);
                save.ClearDirty();
                Assert.IsFalse(placement.TryDemolish(new GridPosition(5, 5))); Assert.IsFalse(save.IsDirty);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void ManualPlacement_FailedPlacement_DoesNotMarkSaveDirty()
        {
            GameObject go = new GameObject("TestPlacementFailClean");
            try
            {
                SaveController save = go.AddComponent<SaveController>();
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                TreasuryView tv = go.AddComponent<TreasuryView>(); tv.StartingBalance = 10;
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Id = "costly"; def.Footprint = new Vector2Int(1, 1); def.ConstructionCost = 100;
                BuildingPlacementController placement = go.AddComponent<BuildingPlacementController>();
                placement.GridVisualizer = viz; placement.TreasuryView = tv; placement.Definition = def; placement.Save = save;

                Assert.IsFalse(placement.TryPlaceAt(new GridPosition(0, 0))); Assert.IsFalse(save.IsDirty);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void RoadPlacement_PlaceAndDemolish_MarksSaveDirty()
        {
            GameObject go = new GameObject("TestRoadDirty");
            try
            {
                SaveController save = go.AddComponent<SaveController>();
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                RoadNetworkStore store = go.AddComponent<RoadNetworkStore>(); store.GridVisualizer = viz;
                RoadPlacementController roads = go.AddComponent<RoadPlacementController>();
                roads.GridVisualizer = viz; roads.Store = store; roads.Save = save;

                Assert.IsFalse(save.IsDirty);
                Assert.IsTrue(roads.PlaceLine(new GridPosition(0, 0), new GridPosition(2, 0))); Assert.IsTrue(save.IsDirty);
                save.ClearDirty();
                roads.PlaceLine(new GridPosition(0, 0), new GridPosition(2, 0)); Assert.IsFalse(save.IsDirty);
                Assert.IsTrue(roads.TryDemolish(new GridPosition(1, 0))); Assert.IsTrue(save.IsDirty);
                save.ClearDirty();
                Assert.IsFalse(roads.TryDemolish(new GridPosition(9, 9))); Assert.IsFalse(save.IsDirty);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void ZonePainting_PaintAndClear_MarksSaveDirty()
        {
            GameObject go = new GameObject("TestZoneDirty");
            try
            {
                SaveController save = go.AddComponent<SaveController>();
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                ZonePaintingController painter = go.AddComponent<ZonePaintingController>();
                painter.GridVisualizer = viz; painter.Save = save;

                Assert.IsFalse(save.IsDirty);
                Assert.IsTrue(painter.PaintZoneAt(new GridPosition(1, 1), ZoneType.Residential)); Assert.IsTrue(save.IsDirty);
                save.ClearDirty();
                Assert.IsFalse(painter.PaintZoneAt(new GridPosition(1, 1), ZoneType.Residential)); Assert.IsFalse(save.IsDirty);
                Assert.IsTrue(painter.PaintZoneAt(new GridPosition(1, 1), ZoneType.None)); Assert.IsTrue(save.IsDirty);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void SettlementGrowth_TryGrowAndDemolishState_MarksSaveDirty()
        {
            GameObject go = new GameObject("TestGrowthDirty");
            try
            {
                SaveController save = go.AddComponent<SaveController>();
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                ZonePaintingController zp = go.AddComponent<ZonePaintingController>(); zp.GridVisualizer = viz;
                SimulationRunner runner = go.AddComponent<SimulationRunner>();
                TreasuryView tv = go.AddComponent<TreasuryView>(); tv.StartingBalance = 0;
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Id = "home"; def.Footprint = new Vector2Int(1, 1); def.AllowedZone = ZoneType.Residential; def.ConstructionCost = 0;
                SettlementGrowthController growth = go.AddComponent<SettlementGrowthController>();
                growth.GridVisualizer = viz; growth.ZonePainter = zp; growth.Runner = runner;
                growth.Treasury = tv; growth.Definitions = new List<BuildingDefinition> { def };
                growth.TicksPerGrowth = 1; growth.Save = save;

                zp.Zones.SetZone(new GridPosition(0, 0), ZoneType.Residential);
                save.ClearDirty();
                growth.OnClockTick(new SimulationTime(1));
                Assert.AreEqual(1, growth.GrownCount); Assert.IsTrue(save.IsDirty);

                BuildingState home = growth.FindStateAt(new GridPosition(0, 0));
                Assert.IsNotNull(home);
                save.ClearDirty();
                growth.DemolishState(home);
                Assert.AreEqual(0, growth.GrownCount); Assert.IsTrue(save.IsDirty);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void SettlementGrowth_RestoreAndReset_DoesNotMarkSaveDirty()
        {
            GameObject go = new GameObject("TestGrowthClean");
            try
            {
                SaveController save = go.AddComponent<SaveController>();
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                ZonePaintingController zp = go.AddComponent<ZonePaintingController>(); zp.GridVisualizer = viz;
                SimulationRunner runner = go.AddComponent<SimulationRunner>();
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Id = "home_res"; def.Footprint = new Vector2Int(1, 1); def.AllowedZone = ZoneType.Residential; def.ConstructionCost = 0;
                SettlementGrowthController growth = go.AddComponent<SettlementGrowthController>();
                growth.GridVisualizer = viz; growth.ZonePainter = zp; growth.Runner = runner;
                growth.Definitions = new List<BuildingDefinition> { def }; growth.Save = save;

                save.ClearDirty();
                BuildingState restored = growth.RestoreGrown("home_res", new GridPosition(2, 2));
                Assert.IsNotNull(restored); Assert.IsFalse(save.IsDirty);
                growth.ResetGrowth(); Assert.IsFalse(save.IsDirty);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void BuildModeHotkeys_ModeSwitching_DoesNotMarkSaveDirty()
        {
            GameObject go = new GameObject("TestHotkeysClean");
            try
            {
                SaveController save = go.AddComponent<SaveController>();
                BuildModeMenu menu = go.AddComponent<BuildModeMenu>();
                BuildModeHotkeys hotkeys = go.AddComponent<BuildModeHotkeys>();
                hotkeys.Menu = menu; hotkeys.Save = save;

                Assert.IsFalse(save.IsDirty);
                hotkeys.SelectMode(menu, BuildModeMenu.Mode.Houses); Assert.IsFalse(save.IsDirty);
                hotkeys.SelectMode(menu, BuildModeMenu.Mode.Roads); Assert.IsFalse(save.IsDirty);
                hotkeys.SelectMode(menu, BuildModeMenu.Mode.Zones); Assert.IsFalse(save.IsDirty);
                hotkeys.SelectMode(menu, BuildModeMenu.Mode.Off); Assert.IsFalse(save.IsDirty);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
