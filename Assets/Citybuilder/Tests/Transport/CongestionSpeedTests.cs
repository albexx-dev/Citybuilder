using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Замедление машин от нагрузки клетки (EPIC-45, agent-slowdown): политика и движение агентов.
    public class CongestionSpeedTests
    {
        private static readonly List<GridPosition> Homes = MakeList(new GridPosition(0, 1));
        private static readonly List<GridPosition> Works = MakeList(new GridPosition(4, 1));
        private static readonly List<GridPosition> Roads = MakeList(
            new GridPosition(1, 1), new GridPosition(2, 1), new GridPosition(3, 1));

        private static List<GridPosition> MakeList(params GridPosition[] cells) => new List<GridPosition>(cells);

        private static AgentManager MakeManager(Func<GridPosition, GridPosition, List<GridPosition>> finder)
            => new AgentManager(null) { PathFinder = finder };

        // Прямой маршрут между клетками (для fake-провайдера путей).
        private static List<GridPosition> Walk(GridPosition from, GridPosition to)
        {
            List<GridPosition> path = new List<GridPosition> { from };
            int step = from.X < to.X ? 1 : -1;
            for (int x = from.X + step; x != to.X + step; x += step) path.Add(new GridPosition(x, from.Y));
            return path;
        }

        [Test]
        public void SpeedForLoad01_FreeCell_FullSpeed()
        {
            Assert.AreEqual(1f, CongestionSpeedPolicy.SpeedForLoad01(0f));
        }

        [Test]
        public void SpeedForLoad01_JammedCell_ThirtyPercent()
        {
            Assert.AreEqual(0.3f, CongestionSpeedPolicy.SpeedForLoad01(1f), 1e-4f);
        }

        [Test]
        public void SpeedForLoad01_NaN_ReturnsFullSpeed()
        {
            Assert.AreEqual(1f, CongestionSpeedPolicy.SpeedForLoad01(float.NaN));
        }

        [Test]
        public void SpeedForLoad01_OutOfRange_Clamps()
        {
            Assert.AreEqual(1f, CongestionSpeedPolicy.SpeedForLoad01(-2f));
            Assert.AreEqual(0.3f, CongestionSpeedPolicy.SpeedForLoad01(2f), 1e-4f);
        }

        [Test]
        public void SpeedForLoad01_PaintingThresholds_StayOrdered()
        {
            float yellow = CongestionSpeedPolicy.SpeedForLoad01(RoadView.YellowThreshold);
            float red = CongestionSpeedPolicy.SpeedForLoad01(RoadView.RedThreshold);
            Assert.Less(yellow, 1f);
            Assert.Greater(yellow, red);
            Assert.Greater(red, 0.3f);
        }

        [Test]
        public void AgentManager_SpeedFactor_SlowsProgress()
        {
            AgentManager manager = MakeManager(Walk);
            manager.CellSpeedFactor = cell => 0.5f;
            manager.SpawnForNewRoad(new GridPosition(2, 1), Homes, Works, Roads);
            manager.Advance(0.9f);
            Assert.AreEqual(0, manager.AgentAt(0).CellIndex);
            manager.Advance(0.6f);
            Assert.AreEqual(1, manager.AgentAt(0).CellIndex);
        }

        [Test]
        public void AgentManager_NoSpeedFactor_FullSpeed()
        {
            AgentManager manager = MakeManager(Walk);
            manager.SpawnForNewRoad(new GridPosition(2, 1), Homes, Works, Roads);
            manager.Advance(0.9f);
            Assert.AreEqual(1, manager.AgentAt(0).CellIndex);
            manager.Advance(0.6f);
            Assert.AreEqual(0, manager.ActiveCount);
        }

        [Test]
        public void AgentManager_Load01_EndToEnd_JamSlowsAgent()
        {
            GridSystem grid = new GridSystem(new GridSize(10, 10), 2f);
            List<GridPosition> roads = MakeList(
                new GridPosition(1, 0), new GridPosition(2, 0), new GridPosition(3, 0),
                new GridPosition(4, 0), new GridPosition(5, 0));
            RouteSystem routes = new RouteSystem(grid, roads);
            CongestionSystem jam = new CongestionSystem(routes, grid);
            CongestionSystem clean = new CongestionSystem(routes, grid);
            List<GridPosition> jamTrip = new List<GridPosition> { new GridPosition(2, 0) };
            for (int i = 0; i < 25; i++) jam.TrackTrip(jamTrip);
            Assert.AreEqual(1f, jam.Load01(new GridPosition(2, 0)), 1e-4f);
            Assert.AreEqual(0.3f, CongestionSpeedPolicy.SpeedForLoad01(jam.Load01(new GridPosition(2, 0))), 1e-4f);

            AgentManager jammed = new AgentManager(jam) { PathFinder = routes.FindPath };
            AgentManager free = new AgentManager(clean) { PathFinder = routes.FindPath };
            List<GridPosition> homes = MakeList(new GridPosition(0, 0));
            List<GridPosition> works = MakeList(new GridPosition(6, 0));
            jammed.SpawnForNewRoad(roads[1], homes, works, roads);
            free.SpawnForNewRoad(roads[1], homes, works, roads);
            jammed.CellSpeedFactor = cell => CongestionSpeedPolicy.SpeedForLoad01(jam.Load01(cell));
            free.CellSpeedFactor = cell => CongestionSpeedPolicy.SpeedForLoad01(clean.Load01(cell));

            jammed.Advance(0.6f);
            free.Advance(0.6f);
            jammed.Advance(0.6f);
            free.Advance(0.6f);
            Assert.AreEqual(1, jammed.AgentAt(0).CellIndex);
            Assert.AreEqual(1, free.AgentAt(0).CellIndex);

            for (int i = 0; i < 4; i++)
            {
                jammed.Advance(0.6f);
                free.Advance(0.6f);
            }
            Assert.AreEqual(0, free.ActiveCount);
            Assert.AreEqual(1, jammed.ActiveCount); // затор: единственная машина не доехала
        }
    }
}
