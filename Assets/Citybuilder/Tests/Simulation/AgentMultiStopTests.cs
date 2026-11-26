using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Маршруты и аллокации агентов (issue #166): presentation-пулы и цели, домен не тронут.
    public class AgentMultiStopTests
    {
        private static List<GridPosition> Make(params int[] c)
        {
            List<GridPosition> cells = new List<GridPosition>();
            for (int i = 0; i < c.Length; i += 2) cells.Add(new GridPosition(c[i], c[i + 1]));
            return cells;
        }

        private static List<GridPosition> Walk(GridPosition from, GridPosition to)
        {
            List<GridPosition> path = new List<GridPosition> { from };
            int step = from.X < to.X ? 1 : -1;
            for (int x = from.X + step; x != to.X + step; x += step) path.Add(new GridPosition(x, from.Y));
            return path;
        }

        // Три прохода: первые два прогревают JIT, третий измеряет без шума (#124).
        private static void AssertNoAllocations(Action step)
        {
            long allocated = 0;
            for (int pass = 0; pass < 3; pass++)
            {
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 1000; i++) step();
                allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            }
            Assert.AreEqual(0L, allocated);
        }
        private static readonly List<GridPosition> Homes = Make(0, 1);
        private static readonly List<GridPosition> Works = Make(7, 1);
        private static readonly List<GridPosition> Shops = Make(6, 1);
        private static readonly List<GridPosition> Roads = Make(1, 1, 2, 1, 3, 1, 4, 1, 5, 1, 6, 1, 7, 1);
        [Test]
        public void VehicleChainEndsAtShop()
        {
            int calls = 0;
            AgentManager manager = new AgentManager(null)
            { PathFinder = delegate(GridPosition a, GridPosition b) { calls++; return Walk(a, b); } };
            Assert.IsTrue(manager.SpawnForNewRoad(new GridPosition(3, 1), Homes, Works, Shops, Roads));
            AgentManager.VehicleAgent agent = manager.AgentAt(0);
            Assert.AreEqual(2, calls);
            Assert.AreEqual(new GridPosition(7, 1), agent.Path[agent.Path.Count - 2]);
            Assert.AreEqual(new GridPosition(6, 1), agent.Path[agent.Path.Count - 1]);
        }

        [Test]
        public void VehicleAdvanceNoAllocations()
        {
            AgentManager manager = new AgentManager(null) { PathFinder = Walk };
            manager.SetActiveLimit(AgentManager.MaxAgents);
            for (int i = 0; i < AgentManager.MaxAgents; i++)
                manager.SpawnForNewRoad(new GridPosition(3, 1), Homes, Works, Shops, Roads);
            AssertNoAllocations(delegate { manager.Advance(0.001f); });
        }

        [Test]
        public void PedestrianGoalsRotate()
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 1f);
            Assert.IsTrue(grid.Occupy(new GridPosition(2, 2), EntityId.New()));
            PedestrianAgentManager manager = new PedestrianAgentManager();
            manager.Wander.IsBlocked = grid.IsOccupied;
            manager.Wander.SetDestinations(Make(5, 2), Make(6, 2));
            manager.OnPopulationChanged(5, Make(2, 2));
            GridPosition goal;
            Assert.AreEqual(new GridPosition(6, 2), manager.AgentAt(0).Goal);
            Assert.IsTrue(manager.Wander.TryPickGoal(new GridPosition(2, 2), out goal));
            Assert.AreEqual(new GridPosition(7, 2), goal);
            Assert.IsTrue(manager.Wander.TryPickGoal(new GridPosition(2, 2), out goal));
            Assert.AreEqual(new GridPosition(3, 2), goal);
        }

        [Test]
        public void PedestrianAdvanceNoAllocations()
        {
            GridSystem grid = new GridSystem(new GridSize(30, 30), 1f);
            Assert.IsTrue(grid.Occupy(new GridPosition(10, 10), EntityId.New()));
            PedestrianAgentManager manager = new PedestrianAgentManager();
            manager.Wander.IsBlocked = grid.IsOccupied;
            manager.Wander.SetDestinations(Make(14, 10), Make(15, 10));
            manager.OnPopulationChanged(PedestrianAgentManager.MaxPedestrians * 5, Make(10, 10));
            AssertNoAllocations(delegate { manager.Advance(0.001f); });
        }

        [Test]
        public void LimitsUnchanged()
        {
            AgentManager manager = new AgentManager(null);
            manager.SetActiveLimit(100);
            Assert.AreEqual(40, manager.ActiveLimit);
            manager.SetActiveLimit(0);
            Assert.AreEqual(1, manager.ActiveLimit);
        }
    }
}
