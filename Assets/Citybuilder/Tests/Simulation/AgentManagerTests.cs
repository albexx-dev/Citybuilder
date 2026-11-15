using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Тесты AgentManager (ADR-011, EPIC-44): окна спавна, пул, прибытие, фид пробок.
    public class AgentManagerTests
    {
        private static readonly List<GridPosition> Homes = MakeList(new GridPosition(0, 1));
        private static readonly List<GridPosition> Works = MakeList(new GridPosition(4, 1));
        private static readonly List<GridPosition> Roads = MakeList(
            new GridPosition(1, 1), new GridPosition(2, 1), new GridPosition(3, 1));

        private static List<GridPosition> MakeList(params GridPosition[] cells) => new List<GridPosition>(cells);

        private static AgentManager MakeManager(Func<GridPosition, GridPosition, List<GridPosition>> finder, CongestionSystem congestion = null)
            => new AgentManager(congestion) { PathFinder = finder };

        // Прямой маршрут между клетками (для fake-провайдера путей).
        private static List<GridPosition> Walk(GridPosition from, GridPosition to)
        {
            List<GridPosition> path = new List<GridPosition> { from };
            int step = from.X < to.X ? 1 : -1;
            for (int x = from.X + step; x != to.X + step; x += step) path.Add(new GridPosition(x, from.Y));
            return path;
        }

        [Test]
        public void MorningWindow_SpawnsBatch_AnchoredToRoads()
        {
            AgentManager manager = MakeManager(Walk);
            manager.OnGameHour(7, Homes, Works, Roads);
            Assert.AreEqual(AgentManager.SpawnsPerHour, manager.ActiveCount);
            AgentManager.VehicleAgent agent = manager.AgentAt(0);
            Assert.IsTrue(agent.Active);
            Assert.AreEqual(new GridPosition(1, 1), agent.Path[0]);
            Assert.AreEqual(new GridPosition(3, 1), agent.Path[agent.Path.Count - 1]);
        }

        [Test]
        public void OutsideSpawnWindow_DoesNotSpawn()
        {
            AgentManager manager = MakeManager(Walk);
            int[] hours = { 0, 6, 9, 12, 16, 19, 23 };
            for (int i = 0; i < hours.Length; i++)
            {
                manager.OnGameHour(hours[i], Homes, Works, Roads);
            }
            Assert.AreEqual(0, manager.ActiveCount);
        }

        [Test]
        public void EveningWindow_SpawnsWorkToHome()
        {
            AgentManager manager = MakeManager(Walk);
            manager.OnGameHour(17, Homes, Works, Roads);
            AgentManager.VehicleAgent agent = manager.AgentAt(0);
            Assert.IsTrue(agent.Active);
            Assert.AreEqual(new GridPosition(3, 1), agent.Path[0]);
            Assert.AreEqual(new GridPosition(1, 1), agent.Path[agent.Path.Count - 1]);
        }

        [Test]
        public void NoPath_NoSpawn()
        {
            AgentManager noFinder = new AgentManager(null);
            noFinder.OnGameHour(7, Homes, Works, Roads);
            AgentManager nullPath = MakeManager((from, to) => null);
            nullPath.OnGameHour(7, Homes, Works, Roads);
            Assert.AreEqual(0, noFinder.ActiveCount);
            Assert.AreEqual(0, nullPath.ActiveCount);
        }

        [Test]
        public void MissingTargetsOrRoads_NoSpawn()
        {
            AgentManager manager = MakeManager(Walk);
            manager.OnGameHour(7, new List<GridPosition>(), Works, Roads);
            manager.OnGameHour(7, Homes, new List<GridPosition>(), Roads);
            manager.OnGameHour(7, Homes, Works, null);
            manager.OnGameHour(7, Homes, null, Roads);
            Assert.AreEqual(0, manager.ActiveCount);
        }

        [Test]
        public void SameRoadAnchor_NoSpawn()
        {
            AgentManager manager = MakeManager(Walk);
            manager.OnGameHour(7, Homes, MakeList(new GridPosition(1, 0)), Roads);
            Assert.AreEqual(0, manager.ActiveCount);
        }

        [Test]
        public void PoolLimit_CapsAtMaxAgents()
        {
            AgentManager manager = MakeManager(Walk);
            for (int hour = 0; hour < 5; hour++)
            {
                manager.OnGameHour(7, Homes, Works, Roads);
            }
            manager.OnGameHour(8, Homes, Works, Roads);
            Assert.AreEqual(AgentManager.MaxAgents, manager.ActiveCount);
        }

        [Test]
        public void Arrival_DespanwsAgent()
        {
            AgentManager manager = MakeManager(Walk);
            manager.OnGameHour(7, Homes, Works, Roads);
            manager.Advance(0.9f);
            Assert.AreEqual(AgentManager.SpawnsPerHour, manager.ActiveCount);
            manager.Advance(0.2f);
            Assert.AreEqual(0, manager.ActiveCount);
        }

        [Test]
        public void Slots_ReusedAfterArrival()
        {
            AgentManager manager = MakeManager(Walk);
            manager.OnGameHour(7, Homes, Works, Roads);
            manager.Advance(2f);
            Assert.AreEqual(0, manager.ActiveCount);
            manager.OnGameHour(7, Homes, Works, Roads);
            Assert.AreEqual(AgentManager.SpawnsPerHour, manager.ActiveCount);
        }

        [Test]
        public void CongestionFeed_CurrentCellGetsLoad()
        {
            GridSystem grid = new GridSystem(new GridSize(10, 10), 2f);
            List<GridPosition> roads = MakeList(
                new GridPosition(1, 0), new GridPosition(2, 0), new GridPosition(3, 0));
            RouteSystem routes = new RouteSystem(grid, roads);
            CongestionSystem congestion = new CongestionSystem(routes, grid);
            AgentManager manager = MakeManager(routes.FindPath, congestion);
            manager.OnGameHour(7, MakeList(new GridPosition(0, 0)), MakeList(new GridPosition(4, 0)), roads);
            Assert.GreaterOrEqual(congestion.GetLoad(new GridPosition(1, 0)), 1);
            manager.Advance(0.5f);
            Assert.GreaterOrEqual(congestion.GetLoad(new GridPosition(2, 0)), 1);
            manager.Advance(2f);
            Assert.AreEqual(0, manager.ActiveCount);
            Assert.GreaterOrEqual(congestion.GetLoad(new GridPosition(3, 0)), 1);
        }
    }
}
