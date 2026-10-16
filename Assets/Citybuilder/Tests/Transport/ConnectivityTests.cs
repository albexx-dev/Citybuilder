using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Проверки инфо-связности. Чистый C#, без Unity API.
    public class ConnectivityTests
    {
        private static GridSystem MakeGrid()
        {
            return new GridSystem(new GridSize(10, 8), 2f);
        }

        private static ConnectivitySystem MakeSystem(List<GridPosition> roads, List<GridPosition> buildings)
        {
            return new ConnectivitySystem(MakeGrid(), roads, buildings);
        }

        [Test]
        public void Empty_ZeroCounts()
        {
            ConnectivitySystem connectivity = MakeSystem(null, null);
            Assert.AreEqual(0, connectivity.TotalCount);
            Assert.AreEqual(0, connectivity.ConnectedCount);
            Assert.AreEqual(0f, connectivity.ConnectionRate01);
        }

        [Test]
        public void AdjacentRoad_Connected()
        {
            List<GridPosition> roads = new List<GridPosition> { new GridPosition(1, 0) };
            List<GridPosition> buildings = new List<GridPosition> { new GridPosition(0, 0) };
            ConnectivitySystem connectivity = MakeSystem(roads, buildings);
            Assert.IsTrue(connectivity.IsConnected(new GridPosition(0, 0)));
            Assert.AreEqual(1, connectivity.ConnectedCount);
            Assert.AreEqual(1f, connectivity.ConnectionRate01);
        }

        [Test]
        public void DiagonalOnly_NotConnected()
        {
            List<GridPosition> roads = new List<GridPosition> { new GridPosition(1, 1) };
            List<GridPosition> buildings = new List<GridPosition> { new GridPosition(0, 0) };
            ConnectivitySystem connectivity = MakeSystem(roads, buildings);
            Assert.IsFalse(connectivity.IsConnected(new GridPosition(0, 0)));
            Assert.AreEqual(0, connectivity.ConnectedCount);
            Assert.AreEqual(0f, connectivity.ConnectionRate01);
        }

        [Test]
        public void Rebuild_ForgetsOldRoads()
        {
            List<GridPosition> buildings = new List<GridPosition> { new GridPosition(0, 0) };
            ConnectivitySystem connectivity = MakeSystem(
                new List<GridPosition> { new GridPosition(1, 0) }, buildings);
            Assert.IsTrue(connectivity.IsConnected(new GridPosition(0, 0)));
            connectivity.Rebuild(new List<GridPosition> { new GridPosition(5, 5) }, buildings);
            Assert.IsFalse(connectivity.IsConnected(new GridPosition(0, 0)));
            Assert.AreEqual(0, connectivity.ConnectedCount);
        }

        [Test]
        public void Rate_OneOfTwo()
        {
            List<GridPosition> roads = new List<GridPosition> { new GridPosition(1, 0) };
            List<GridPosition> buildings = new List<GridPosition>
            {
                new GridPosition(0, 0),
                new GridPosition(7, 7)
            };
            ConnectivitySystem connectivity = MakeSystem(roads, buildings);
            Assert.AreEqual(2, connectivity.TotalCount);
            Assert.AreEqual(1, connectivity.ConnectedCount);
            Assert.AreEqual(0.5f, connectivity.ConnectionRate01);
        }
    }
}
