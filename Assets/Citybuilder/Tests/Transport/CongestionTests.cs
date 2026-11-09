using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    public class CongestionTests
    {
        private static GridSystem MakeGrid()
        {
            return new GridSystem(new GridSize(10, 8), 2f);
        }

        private static CongestionSystem MakeSystem(GridSystem grid)
        {
            RouteSystem routes = new RouteSystem(grid);
            return new CongestionSystem(routes, grid);
        }

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
            GridSystem grid = MakeGrid();
            CongestionSystem congestion = MakeSystem(grid);
            Assert.AreEqual(1f, congestion.WorstMultiplier());
        }

        [Test]
        public void Load01_EmptyCongestedAndClampedCells_ReturnsExpectedRatio()
        {
            GridPosition cell = new GridPosition(2, 3);
            int currentLoad = 0;
            CongestionSystem congestion = new CongestionSystem(p => p == cell ? currentLoad : 0, MakeGrid(), 4, 0.25f, 3f);

            Assert.AreEqual(0f, congestion.Load01(cell), 1e-4f);
            currentLoad = 4;
            Assert.AreEqual(0.125f, congestion.Load01(cell), 1e-4f);
            currentLoad = 7;
            Assert.AreEqual(0.5f, congestion.Load01(cell), 1e-4f);
            currentLoad = 25;
            Assert.AreEqual(1f, congestion.Load01(cell), 1e-4f);
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
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
