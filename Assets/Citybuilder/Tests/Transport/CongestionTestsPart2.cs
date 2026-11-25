using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса CongestionTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class CongestionTests
    {
        [Test]
        public void CongestionController_TrackTripAndClearLoads_UpdatesState()
        {
            GameObject go = new GameObject("TestCongestionController");
            try
            {
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                CongestionController ctrl = go.AddComponent<CongestionController>();
                ctrl.GridVisualizer = viz; ctrl.Threshold = 2; ctrl.Factor = 0.5f;
                GridPosition cell = new GridPosition(1, 1);
                List<GridPosition> trip = new List<GridPosition> { cell };
                ctrl.TrackTrip(trip);
                ctrl.TrackTrip(trip);
                Assert.Greater(ctrl.Load01(cell), 0f);
                Assert.GreaterOrEqual(ctrl.Multiplier, 1.5f);
                ctrl.ClearLoads();
                Assert.AreEqual(0f, ctrl.Load01(cell));
                Assert.AreEqual(1f, ctrl.Multiplier);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void ConnectivityController_ManualBuildings_IncludedInConnectivity()
        {
            GameObject go = new GameObject("TestConnectivityManual");
            try
            {
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                RoadNetworkStore roads = go.AddComponent<RoadNetworkStore>(); roads.GridVisualizer = viz;
                SettlementGrowthController growth = go.AddComponent<SettlementGrowthController>(); growth.GridVisualizer = viz;
                BuildingPlacementController placement = go.AddComponent<BuildingPlacementController>(); placement.GridVisualizer = viz;
                ConnectivityController conn = go.AddComponent<ConnectivityController>();
                conn.GridVisualizer = viz; conn.Store = roads; conn.Growth = growth; conn.Placement = placement;

                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Id = "m_house"; def.Footprint = new Vector2Int(1, 1);
                placement.Definitions = new List<BuildingDefinition> { def };
                placement.RestoreManual("m_house", new GridPosition(0, 0));
                roads.RestoreRoad(new GridPosition(1, 0));

                conn.RefreshNow();
                Assert.AreEqual(1, conn.Total);
                Assert.AreEqual(1, conn.Connected);
                Assert.AreEqual(1f, conn.Rate01);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void ServiceBuildingSystem_ManualResidences_IncludedInCoverageSnapshot()
        {
            BuildingDefinition stationDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            stationDef.Id = "station"; stationDef.ServiceKind = ServiceKind.Fire; stationDef.ServiceRadius = 5; stationDef.ResidentCapacity = 0;
            BuildingDefinition houseDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            houseDef.Id = "res"; houseDef.ServiceKind = ServiceKind.None; houseDef.ResidentCapacity = 4;

            BuildingState station = new BuildingState(EntityId.New(), stationDef, new GridPosition(0, 0), new List<GridPosition> { new GridPosition(0, 0) });
            BuildingState nearHouse = new BuildingState(EntityId.New(), houseDef, new GridPosition(2, 2), new List<GridPosition> { new GridPosition(2, 2) });
            BuildingState farHouse = new BuildingState(EntityId.New(), houseDef, new GridPosition(10, 10), new List<GridPosition> { new GridPosition(10, 10) });

            ServiceBuildingSystem system = new ServiceBuildingSystem();
            system.BuildSnapshotFromStates(new List<BuildingState> { station, nearHouse, farHouse }, null);
            Assert.AreEqual(2, system.TotalHomesCount());
            Assert.AreEqual(1, system.CoveredHomesCount(ServiceKind.Fire));
            Assert.AreEqual(0.5f, system.Coverage01(ServiceKind.Fire), 0.001f);
        }
    }
}
