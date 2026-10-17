using System.Collections.Generic;
using NUnit.Framework;

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
    }
}
