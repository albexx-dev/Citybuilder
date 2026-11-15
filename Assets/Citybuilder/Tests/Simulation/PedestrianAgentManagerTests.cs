using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Тесты PedestrianAgentManager (EPIC-46): пропорция 1:5, пул, блуждание, застревание, очистка.
    public class PedestrianAgentManagerTests
    {
        private const int Size = 24;

        private static GridSystem MakeGrid() => new GridSystem(new GridSize(Size, Size), 1f);

        private static PedestrianAgentManager MakeManager(GridSystem grid)
        {
            PedestrianAgentManager manager = new PedestrianAgentManager();
            manager.Wander.IsBlocked = grid.IsOccupied;
            return manager;
        }

        private static List<GridPosition> MakeHomes(params GridPosition[] cells) => new List<GridPosition>(cells);

        private static void Occupy(GridSystem grid, int x, int y) => Assert.IsTrue(grid.Occupy(new GridPosition(x, y), EntityId.New()));

        private static void AssertInside(PedestrianAgentManager.PedestrianAgent agent, int min, int max)
        {
            Assert.IsTrue(agent.Current.X >= min && agent.Current.X <= max && agent.Current.Y >= min && agent.Current.Y <= max,
                "агент не должен покидать разрешённые клетки: " + agent.Current);
        }

        [Test]
        public void Population_ConvertsOneAgentPerFiveResidents()
        {
            GridSystem grid = MakeGrid();
            Occupy(grid, 10, 10);
            PedestrianAgentManager manager = MakeManager(grid);
            List<GridPosition> homes = MakeHomes(new GridPosition(10, 10));
            manager.OnPopulationChanged(4, homes);
            Assert.AreEqual(0, manager.ActiveCount);
            manager.OnPopulationChanged(5, homes);
            Assert.AreEqual(1, manager.ActiveCount);
            Assert.AreEqual(1, Math.Abs(manager.AgentAt(0).Current.X - 10) + Math.Abs(manager.AgentAt(0).Current.Y - 10));
            manager.OnPopulationChanged(9, homes);
            Assert.AreEqual(1, manager.ActiveCount);
            manager.OnPopulationChanged(10, homes);
            Assert.AreEqual(2, manager.ActiveCount);
        }

        [Test]
        public void Pool_CapsAtMaxPedestrians()
        {
            GridSystem grid = MakeGrid();
            Occupy(grid, 10, 10);
            PedestrianAgentManager manager = MakeManager(grid);
            manager.OnPopulationChanged(500, MakeHomes(new GridPosition(10, 10)));
            Assert.AreEqual(PedestrianAgentManager.MaxPedestrians, manager.ActiveCount);
        }

        [Test]
        public void SurroundedHome_NoSpawn()
        {
            GridSystem grid = MakeGrid();
            Occupy(grid, 12, 12);
            Occupy(grid, 13, 12);
            Occupy(grid, 11, 12);
            Occupy(grid, 12, 13);
            Occupy(grid, 12, 11);
            PedestrianAgentManager manager = MakeManager(grid);
            manager.OnPopulationChanged(5, MakeHomes(new GridPosition(12, 12)));
            Assert.AreEqual(0, manager.ActiveCount);
        }

        [Test]
        public void NoFreeGoalInRadius_NoSpawnBounded()
        {
            GridSystem grid = MakeGrid();
            Occupy(grid, 12, 12);
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    if (Math.Abs(x - 12) + Math.Abs(y - 12) >= 6 && Math.Abs(x - 12) + Math.Abs(y - 12) <= 10) Occupy(grid, x, y);
            PedestrianAgentManager manager = MakeManager(grid);
            manager.OnPopulationChanged(5, MakeHomes(new GridPosition(12, 12)));
            Assert.AreEqual(0, manager.ActiveCount);
        }

        [Test]
        public void Wandering_MovesReachesGoalsAndStaysInsideMap()
        {
            GridSystem grid = MakeGrid();
            Occupy(grid, 12, 12);
            PedestrianAgentManager manager = MakeManager(grid);
            List<GridPosition> homes = MakeHomes(new GridPosition(12, 12));
            manager.OnPopulationChanged(5, homes);
            GridPosition start = manager.AgentAt(0).Current;
            bool moved = false;
            bool reachedGoal = false;
            for (int step = 0; step < 300; step++)
            {
                manager.Advance(1.3f);
                Assert.AreEqual(1, manager.ActiveCount, "агент не должен исчезать на открытой сетке");
                PedestrianAgentManager.PedestrianAgent agent = manager.AgentAt(0);
                AssertInside(agent, 0, Size - 1);
                if (agent.Current != start) moved = true;
                if (agent.Current == agent.Goal || agent.Next == agent.Goal) reachedGoal = true;
            }
            Assert.IsTrue(moved, "блуждание должно уводить агента с точки спавна");
            Assert.IsTrue(reachedGoal, "агент должен достигать целей");
        }

        [Test]
        public void StuckGoal_PickedNewGoalWithoutHang()
        {
            GridSystem grid = MakeGrid();
            for (int x = 8; x <= 14; x++)
                for (int y = 8; y <= 14; y++)
                    if (x == 8 || x == 14 || y == 8 || y == 14) Occupy(grid, x, y);
            Occupy(grid, 11, 11);
            PedestrianAgentManager manager = MakeManager(grid);
            List<GridPosition> homes = MakeHomes(new GridPosition(11, 11));
            manager.OnPopulationChanged(5, homes);
            GridPosition firstGoal = manager.AgentAt(0).Goal;
            bool repicked = false;
            for (int step = 0; step < 150; step++)
            {
                manager.Advance(1.3f);
                Assert.AreEqual(1, manager.ActiveCount, "агент не должен исчезать: цели есть, просто недостижимы");
                PedestrianAgentManager.PedestrianAgent agent = manager.AgentAt(0);
                AssertInside(agent, 9, 13);
                if (agent.Goal != firstGoal) repicked = true;
            }
            Assert.IsTrue(repicked, "недостижимая цель заменяется после MaxStuckSteps промахов");
        }

        [Test]
        public void Clear_RemovesAllAgents()
        {
            GridSystem grid = MakeGrid();
            Occupy(grid, 10, 10);
            PedestrianAgentManager manager = MakeManager(grid);
            List<GridPosition> homes = MakeHomes(new GridPosition(10, 10));
            manager.OnPopulationChanged(10, homes);
            manager.OnPopulationChanged(0, homes);
            Assert.AreEqual(0, manager.ActiveCount);
            manager.OnPopulationChanged(10, homes);
            manager.Clear();
            Assert.AreEqual(0, manager.ActiveCount);
        }
    }
}
