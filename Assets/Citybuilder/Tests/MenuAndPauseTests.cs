using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты логики Esc-приоритета, меню паузы/слотов, флага грязности, сноса и 0-cost стройки.
    public partial class MenuAndPauseTests
    {
        [Test]
        public void EscPriority_PauseAndBuildModes_FollowsEpic16Rules()
        {
            Assert.AreEqual(BuildModeHotkeys.EscAction.None, BuildModeHotkeys.EvaluateEsc(true, false, BuildModeMenu.Mode.Houses)); Assert.AreEqual(BuildModeHotkeys.EscAction.ClosePause, BuildModeHotkeys.EvaluateEsc(false, true, BuildModeMenu.Mode.Off));
            Assert.AreEqual(BuildModeHotkeys.EscAction.TurnOffMode, BuildModeHotkeys.EvaluateEsc(false, false, BuildModeMenu.Mode.Roads)); Assert.AreEqual(BuildModeHotkeys.EscAction.OpenPause, BuildModeHotkeys.EvaluateEsc(false, false, BuildModeMenu.Mode.Off));
            Assert.AreEqual(BuildModeHotkeys.EscAction.TurnOffMode, BuildModeHotkeys.EvaluateEsc(false, false, BuildModeMenu.Mode.Decor));
        }

        [Test]
        public void PauseMenu_OpenAndClose_TogglesSimulationPausedAndPlaying()
        {
            GameObject go = new GameObject("TestPauseMenu");
            try
            {
                SimulationRunner runner = go.AddComponent<SimulationRunner>();
                PauseMenuView pause = go.AddComponent<PauseMenuView>();
                pause.Runner = runner; runner.SetSpeed(SimulationSpeed.Playing);
                pause.Open();
                Assert.IsTrue(pause.IsOpen); Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);
                pause.Close();
                Assert.IsFalse(pause.IsOpen); Assert.AreEqual(SimulationSpeed.Playing, runner.Speed);
                pause.Toggle();
                Assert.IsTrue(pause.IsOpen); Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);
                pause.Toggle();
                Assert.IsFalse(pause.IsOpen); Assert.AreEqual(SimulationSpeed.Playing, runner.Speed);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void SaveAndLoadMenus_SpeedRestoreAndPauseReturn_PreventTraps()
        {
            GameObject go = new GameObject("TestSaveLoadTraps");
            try
            {
                SimulationRunner runner = go.AddComponent<SimulationRunner>();
                PauseMenuView pause = go.AddComponent<PauseMenuView>();
                SaveMenuView save = go.AddComponent<SaveMenuView>();
                LoadMenuView load = go.AddComponent<LoadMenuView>();
                pause.Runner = runner; save.Runner = runner; save.Pause = pause; load.Runner = runner; load.Pause = pause;
                save.Open();
                Assert.IsTrue(save.IsOpen); Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);
                save.Close();
                Assert.IsFalse(save.IsOpen); Assert.AreEqual(SimulationSpeed.Playing, runner.Speed);
                save.Open();
                save.ReturnToPause();
                Assert.IsFalse(save.IsOpen); Assert.IsTrue(pause.IsOpen); Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);
                pause.Close();
                load.Open();
                Assert.IsTrue(load.IsOpen); Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);
                load.ReturnToPause();
                Assert.IsFalse(load.IsOpen); Assert.IsTrue(pause.IsOpen); Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);
                pause.Close();
                load.Open(); load.Close();
                Assert.IsFalse(load.IsOpen); Assert.AreEqual(SimulationSpeed.Playing, runner.Speed);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void BuildModeHotkeys_SelectMode_DoesNotMarkSaveDirty()
        {
            GameObject go = new GameObject("TestHotkeysDirty");
            try
            {
                SaveController save = go.AddComponent<SaveController>();
                BuildModeMenu menu = go.AddComponent<BuildModeMenu>();
                BuildModeHotkeys hotkeys = go.AddComponent<BuildModeHotkeys>();
                hotkeys.Menu = menu; hotkeys.Save = save;
                Assert.IsFalse(save.IsDirty);
                hotkeys.SelectMode(menu, BuildModeMenu.Mode.Houses);
                Assert.AreEqual(BuildModeMenu.Mode.Houses, menu.CurrentMode); Assert.IsFalse(save.IsDirty);
                hotkeys.SelectMode(menu, BuildModeMenu.Mode.Roads);
                Assert.AreEqual(BuildModeMenu.Mode.Roads, menu.CurrentMode); Assert.IsFalse(save.IsDirty);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void DirtyFlag_MarkAndClear_ControlsExitConfirmation()
        {
            GameObject go = new GameObject("TestSave");
            try
            {
                SaveController save = go.AddComponent<SaveController>();
                Assert.IsFalse(save.IsDirty); Assert.IsFalse(PauseMenuView.NeedsExitConfirmation(save.IsDirty));
                save.MarkDirty();
                Assert.IsTrue(save.IsDirty); Assert.IsTrue(PauseMenuView.NeedsExitConfirmation(save.IsDirty));
                save.ClearDirty();
                Assert.IsFalse(save.IsDirty); Assert.IsFalse(PauseMenuView.NeedsExitConfirmation(save.IsDirty));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void TryPlaceAt_ZeroCostBuilding_PlacesSuccessfullyWithTreasury()
        {
            GameObject go = new GameObject("TestPlacementZeroCost");
            try
            {
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                TreasuryView tv = go.AddComponent<TreasuryView>(); tv.StartingBalance = 0;
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>(); def.Id = "zero_cost_house"; def.Footprint = new Vector2Int(1, 1); def.ConstructionCost = 0;
                BuildingPlacementController placement = go.AddComponent<BuildingPlacementController>(); placement.GridVisualizer = viz; placement.TreasuryView = tv; placement.Definition = def;
                Assert.IsTrue(placement.TryPlaceAt(new GridPosition(0, 0)));
                Assert.AreEqual(1, placement.Placed.Count); Assert.AreEqual(0, tv.Treasury.Balance);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Growth_ZeroCostBuilding_GrowsSuccessfullyWithTreasury()
        {
            GameObject go = new GameObject("TestGrowthZeroCost");
            try
            {
                GridVisualizer viz = go.AddComponent<GridVisualizer>(); SimulationRunner runner = go.AddComponent<SimulationRunner>();
                ZonePaintingController zp = go.AddComponent<ZonePaintingController>(); zp.GridVisualizer = viz;
                TreasuryView tv = go.AddComponent<TreasuryView>(); tv.StartingBalance = 0;
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>(); def.Id = "zero_cost_growth"; def.Footprint = new Vector2Int(1, 1); def.AllowedZone = ZoneType.Residential; def.ConstructionCost = 0;
                SettlementGrowthController growth = go.AddComponent<SettlementGrowthController>(); growth.GridVisualizer = viz; growth.ZonePainter = zp; growth.Runner = runner; growth.Treasury = tv;
                growth.Definitions = new List<BuildingDefinition> { def }; growth.TicksPerGrowth = 1;
                zp.Zones.SetZone(new GridPosition(0, 0), ZoneType.Residential); growth.OnClockTick(new SimulationTime(1));
                Assert.AreEqual(1, growth.GrownCount); Assert.AreEqual(0, tv.Treasury.Balance);
            }
            finally { Object.DestroyImmediate(go); }
        }

    }
}
