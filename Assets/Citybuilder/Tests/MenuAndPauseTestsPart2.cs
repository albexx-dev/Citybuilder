using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса MenuAndPauseTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class MenuAndPauseTests
    {
        [Test]
        public void EscLockout_ReenteringHousesMode_ResetsPlacementActive()
        {
            GameObject go = new GameObject("TestBuildModeMenuEsc");
            try
            {
                BuildModeMenu menu = go.AddComponent<BuildModeMenu>();
                BuildingPlacementController houses = go.AddComponent<BuildingPlacementController>();
                RoadPlacementController roads = go.AddComponent<RoadPlacementController>();
                menu.Houses = houses; menu.Roads = roads;
                menu.SetMode(BuildModeMenu.Mode.Houses);
                Assert.IsTrue(houses.enabled); Assert.IsTrue(houses.placementActive);
                houses.placementActive = false; menu.SetMode(BuildModeMenu.Mode.Houses);
                Assert.IsTrue(houses.placementActive);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Demolition_ModeAndModalGuards_FollowsPlacementAndModalRules()
        {
            Assert.IsFalse(SettlementDemolitionController.IsDemolitionAllowed(BuildModeMenu.Mode.Roads, false)); Assert.IsFalse(SettlementDemolitionController.IsDemolitionAllowed(BuildModeMenu.Mode.Houses, false));
            Assert.IsFalse(SettlementDemolitionController.IsDemolitionAllowed(BuildModeMenu.Mode.Zones, false)); Assert.IsFalse(SettlementDemolitionController.IsDemolitionAllowed(BuildModeMenu.Mode.Off, true));
            Assert.IsFalse(SettlementDemolitionController.IsDemolitionAllowed(BuildModeMenu.Mode.Growth, true)); Assert.IsTrue(SettlementDemolitionController.IsDemolitionAllowed(BuildModeMenu.Mode.Off, false));
            Assert.IsTrue(SettlementDemolitionController.IsDemolitionAllowed(BuildModeMenu.Mode.Growth, false));
        }

        [Test]
        public void Demolition_AutoResolvesPopulationSource_AndEvictsResidents()
        {
            GameObject go = new GameObject("TestDemolitionEviction");
            try
            {
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                ZonePaintingController zp = go.AddComponent<ZonePaintingController>(); zp.GridVisualizer = viz;
                SimulationRunner runner = go.AddComponent<SimulationRunner>();
                SettlementGrowthController growth = go.AddComponent<SettlementGrowthController>();
                PopulationController pop = go.AddComponent<PopulationController>();
                SettlementDemolitionController demo = go.AddComponent<SettlementDemolitionController>();
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>(); def.Id = "evict_def"; def.Footprint = new Vector2Int(1, 1); def.AllowedZone = ZoneType.Residential; def.ResidentCapacity = 4;
                growth.GridVisualizer = viz; growth.ZonePainter = zp; growth.Runner = runner; growth.Definitions = new List<BuildingDefinition> { def };
                demo.GridVisualizer = viz; demo.Growth = growth; demo.PopulationSource = null;
                zp.Zones.SetZone(new GridPosition(0, 0), ZoneType.Residential);
                BuildingState state = growth.RestoreGrown("evict_def", new GridPosition(0, 0));
                Assert.IsNotNull(state); pop.Population.MoveIn(state, 4);
                Assert.AreEqual(4, pop.Population.TotalResidents);
                demo.ClearGrown();
                Assert.IsNotNull(demo.PopulationSource);
                Assert.AreSame(pop, demo.PopulationSource);
                Assert.AreEqual(0, pop.Population.TotalResidents);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void PanelHotkeys_GuardAndEscPriority_UseSinglePureLogic()
        {
            Assert.IsFalse(BuildModeHotkeys.CanProcessHotkeys(true, 0)); Assert.IsFalse(BuildModeHotkeys.CanProcessHotkeys(false, 1)); Assert.IsTrue(BuildModeHotkeys.CanProcessHotkeys(false, 0));
            Assert.AreEqual(BuildModeHotkeys.EscAction.None, BuildModeHotkeys.EvaluateEsc(false, false, BuildModeMenu.Mode.Off, true)); Assert.AreEqual(BuildModeHotkeys.EscAction.OpenPause, BuildModeHotkeys.EvaluateEsc(false, false, BuildModeMenu.Mode.Off, false));
        }
    }
}
