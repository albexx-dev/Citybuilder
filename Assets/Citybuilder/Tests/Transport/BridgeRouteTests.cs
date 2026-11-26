using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace Citybuilder
{
    public class BridgeRouteTests
    {
        private static BridgeRoadNetwork MakeNetwork(bool ramps, bool lowerGap)
        {
            GridSystem grid = new GridSystem(new GridSize(8, 4), 1f);
            BridgeRoadNetwork net = new BridgeRoadNetwork(grid);
            for (int x = 0; x <= 4; x++)
                if (!(lowerGap && x == 2))
                    net.Place(new RoadCellKey(x, 0, 0), "asphalt");
            for (int x = 1; x <= 3; x++) net.Place(new RoadCellKey(x, 0, 1), BridgeRoadNetwork.Wood);
            if (ramps)
            {
                net.SetRamp(new RoadCellKey(1, 0, 0), new RoadCellKey(1, 0, 1));
                net.SetRamp(new RoadCellKey(3, 0, 0), new RoadCellKey(3, 0, 1));
            }
            return net;
        }
        [Test] public void Ramps_CarryRouteOverGap()
        {
            List<RoadCellKey> path = MakeNetwork(true, true).FindPath(new RoadCellKey(0, 0, 0), new RoadCellKey(4, 0, 0));
            Assert.IsNotNull(path);
            Assert.AreEqual(7, path.Count);
            Assert.AreEqual(0, path[0].Level);
            Assert.AreEqual(1, path[3].Level);
            Assert.AreEqual(0, path[6].Level);
        }
        [Test] public void NoRamp_BridgeIsDeadEnd()
        {
            BridgeRoadNetwork net = MakeNetwork(false, true);
            Assert.IsNull(net.FindPath(new RoadCellKey(0, 0, 0), new RoadCellKey(4, 0, 0)));
            Assert.IsNull(net.FindPath(new RoadCellKey(1, 0, 1), new RoadCellKey(1, 0, 0)));
        }
        [Test] public void LowerRoad_RemainsRoutableUnderBridge()
        {
            List<RoadCellKey> path = MakeNetwork(true, false).FindPath(new RoadCellKey(0, 0, 0), new RoadCellKey(4, 0, 0));
            Assert.IsNotNull(path);
            Assert.AreEqual(5, path.Count);
            Assert.AreEqual(0, path[2].Level);
        }
        [Test] public void DemolishBridge_KeepsLowerRoad()
        {
            BridgeRoadNetwork net = MakeNetwork(true, false);
            GridPosition cell = new GridPosition(2, 0);
            Assert.AreEqual(1, net.TopLevel(cell));
            Assert.IsTrue(net.Demolish(new RoadCellKey(2, 0, 1)));
            Assert.IsFalse(net.Demolish(new RoadCellKey(2, 0, 1)));
            Assert.AreEqual(0, net.TopLevel(cell));
            Assert.IsTrue(net.IsRoad(new RoadCellKey(2, 0, 0)));
        }
        [Test] public void Mesh_RailingsStayWithinTriangleCap()
        {
            List<RoadCellKey> cells = new List<RoadCellKey>();
            for (int i = 0; i < 600; i++) cells.Add(new RoadCellKey(i % 25, i / 25, 1));
            Vector3[] vertices; int[] triangles;
            BridgeMeshBuilder.Build(cells, 1f, out vertices, out triangles);
            Assert.AreEqual(3000, triangles.Length / 3);
            Assert.AreEqual(vertices.Length, (triangles.Length / 3) * 2);
        }
    }
}
