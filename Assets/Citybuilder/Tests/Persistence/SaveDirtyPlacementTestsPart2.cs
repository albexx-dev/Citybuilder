using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса SaveDirtyPlacementTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class SaveDirtyPlacementTests
    {
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
