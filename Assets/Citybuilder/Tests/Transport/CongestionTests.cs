using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    public class CongestionTests
    {
        private static GridSystem MakeGrid() => new GridSystem(new GridSize(10, 8), 2f);
        private static CongestionSystem MakeSystem(GridSystem grid) => new CongestionSystem(new RouteSystem(grid), grid);

        [Test]
        public void BelowThreshold_ReturnsOne()
        {
            CongestionSystem congestion = MakeSystem(MakeGrid());
            Assert.AreEqual(1f, congestion.MultiplierFor(0));
            Assert.AreEqual(1f, congestion.MultiplierFor(3));
            Assert.IsFalse(congestion.IsCongested(3));
        }

        [Test]
        public void AtThreshold_ReturnsOnePlusFactor()
        {
            CongestionSystem congestion = MakeSystem(MakeGrid());
            Assert.IsTrue(congestion.IsCongested(4));
            Assert.AreEqual(1.25f, congestion.MultiplierFor(4));
        }

        [Test]
        public void ClampToMax()
        {
            CongestionSystem congestion = MakeSystem(MakeGrid());
            Assert.AreEqual(3f, congestion.MultiplierFor(1000));
        }

        [Test]
        public void GrowthTickMultiplier_AtLeastOne()
        {
            CongestionSystem congestion = MakeSystem(MakeGrid());
            Assert.GreaterOrEqual(congestion.GrowthTickMultiplier(), 1);
        }

        [Test]
        public void WorstMultiplier_EmptyLoads_One()
        {
            Assert.AreEqual(1f, MakeSystem(MakeGrid()).WorstMultiplier());
        }

        [Test]
        public void Load01_EmptyCongestedAndClampedCells_ReturnsExpectedRatio()
        {
            GridPosition cell = new GridPosition(2, 3);
            int currentLoad = 0;
            CongestionSystem congestion = new CongestionSystem(p => p == cell ? currentLoad : 0, MakeGrid(), 4, 0.25f, 3f);
            Assert.AreEqual(0f, congestion.Load01(cell), 1e-4f);
            currentLoad = 4; Assert.AreEqual(0.125f, congestion.Load01(cell), 1e-4f);
            currentLoad = 7; Assert.AreEqual(0.5f, congestion.Load01(cell), 1e-4f);
            currentLoad = 25; Assert.AreEqual(1f, congestion.Load01(cell), 1e-4f);
        }

        [Test]
        public void Load01_NaNFactor_ReturnsZero()
        {
            CongestionSystem congestion = new CongestionSystem(p => 10, MakeGrid(), 4, float.NaN, 3f);
            Assert.AreEqual(0f, congestion.Load01(new GridPosition(0, 0)), 1e-4f);
        }

        [Test]
        public void RoadRecolor_ThresholdsAndOncePerTickGuard()
        {
            Assert.AreEqual(0, RoadView.BucketForLoad01(0.32f));
            Assert.AreEqual(1, RoadView.BucketForLoad01(0.34f));
            Assert.AreEqual(1, RoadView.BucketForLoad01(0.65f));
            Assert.AreEqual(2, RoadView.BucketForLoad01(0.67f));

            GameObject go = new GameObject("TestRoadView");
            try
            {
                RoadView view = go.AddComponent<RoadView>();
                int calls = 0;
                List<GridPosition> cells = new List<GridPosition> { new GridPosition(0, 0) };
                Assert.IsTrue(view.RefreshCongestionOncePerTick(10, cells, 1f, c => { calls++; return 0.5f; }));
                Assert.IsFalse(view.RefreshCongestionOncePerTick(10, cells, 1f, c => { calls++; return 0.5f; }));
                Assert.AreEqual(1, calls);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void FormatSummaryLine_MatchesCongestedCountWorstPercentAndGrowthMultiplier()
        {
            GridPosition jammed = new GridPosition(1, 1);
            CongestionSystem congestion = new CongestionSystem(p => p == jammed ? 7 : 0, MakeGrid(), 4, 0.25f, 3f);
            Assert.AreEqual(1, congestion.CongestedCellsCount());
            Assert.AreEqual("Пробки: 1 (худшая клетка 50%, рост ×2)", congestion.FormatSummaryLine());
        }

        [Test]
        public void ConstructorOverload_AdaptsFuncIntInt_WithoutNullReference()
        {
            CongestionSystem congestion = new CongestionSystem(x => x * 3, 4, 0.25f, 3f);
            Assert.AreEqual(1f, congestion.MultiplierFor(2));
            Assert.AreEqual(1.5f, congestion.MultiplierFor(5));
            Assert.DoesNotThrow(() => congestion.Load01(new GridPosition(2, 2)));
            CongestionSystem nullSystem = new CongestionSystem((Func<int, int>)null);
            Assert.AreEqual(0f, nullSystem.Load01(new GridPosition(0, 0)));
            Assert.AreEqual(1f, nullSystem.WorstMultiplier());
        }

        [Test]
        public void Multipliers_WithNaNInputs_ReturnSafeDefaults()
        {
            CongestionSystem congestion = MakeSystem(MakeGrid());
            congestion.Factor = float.NaN;
            Assert.AreEqual(0f, congestion.Factor);
            Assert.AreEqual(1f, congestion.MultiplierFor(10));
            Assert.AreEqual(1f, congestion.WorstMultiplier());
            Assert.AreEqual(1, congestion.GrowthTickMultiplier());
            congestion.MaxMultiplier = float.NaN;
            Assert.AreEqual(1f, congestion.MaxMultiplier);
            Assert.AreEqual(1f, congestion.MultiplierFor(10));
        }

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
