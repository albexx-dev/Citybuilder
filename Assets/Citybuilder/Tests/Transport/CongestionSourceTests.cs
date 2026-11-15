using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Источник пробок (EPIC-45, source-swap): агентный фид против синтетического фолбэка — без двойного учёта.
    public class CongestionSourceTests
    {
        private static List<GridPosition> MakeList(params GridPosition[] cells) => new List<GridPosition>(cells);

        [Test]
        public void AgentFeed_Active_SyntheticTripIgnored()
        {
            GameObject go = new GameObject("TestCongestionSource");
            try
            {
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                CongestionController controller = go.AddComponent<CongestionController>();
                controller.GridVisualizer = viz;
                GridPosition cell = new GridPosition(2, 2);
                List<GridPosition> trip = new List<GridPosition> { cell };

                CongestionSystem congestion = controller.System;
                congestion.TrackTrip(trip); // агентный фид (как AgentManager.FeedCongestion)
                controller.TrackTrip(trip); // синтетический вход — при активных агентах игнорируется

                Assert.IsTrue(congestion.HasAgentFeed);
                Assert.AreEqual(1, congestion.GetLoad(cell));
                Assert.AreEqual(0, controller.Routes.LoadOf(cell));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void Fallback_NoAgentFeed_SyntheticTripApplies()
        {
            GameObject go = new GameObject("TestCongestionFallback");
            try
            {
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                CongestionController controller = go.AddComponent<CongestionController>();
                controller.GridVisualizer = viz;
                controller.Threshold = 2;
                controller.Factor = 0.5f;
                GridPosition cell = new GridPosition(1, 1);
                List<GridPosition> trip = new List<GridPosition> { cell };

                controller.TrackTrip(trip);
                controller.TrackTrip(trip);

                Assert.IsFalse(controller.System.HasAgentFeed);
                Assert.AreEqual(2, controller.System.GetLoad(cell));
                Assert.AreEqual(2, controller.Routes.LoadOf(cell));
                Assert.Greater(controller.Load01(cell), 0f);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void ClearLoads_ResetsAgentFeedWindow()
        {
            CongestionSystem congestion = new CongestionSystem((System.Func<GridPosition, int>)(p => 0));
            congestion.TrackTrip(new List<GridPosition> { new GridPosition(0, 0) });
            Assert.IsTrue(congestion.HasAgentFeed);
            congestion.ClearLoads();
            Assert.IsFalse(congestion.HasAgentFeed);
        }

        [Test]
        public void AgentManager_Feed_MarksAgentSource()
        {
            GridSystem grid = new GridSystem(new GridSize(10, 10), 2f);
            List<GridPosition> roads = MakeList(new GridPosition(1, 0), new GridPosition(2, 0), new GridPosition(3, 0));
            RouteSystem routes = new RouteSystem(grid, roads);
            CongestionSystem congestion = new CongestionSystem(routes, grid);
            AgentManager manager = new AgentManager(congestion) { PathFinder = routes.FindPath };

            manager.OnGameHour(7, MakeList(new GridPosition(0, 0)), MakeList(new GridPosition(4, 0)), roads);

            Assert.IsTrue(congestion.HasAgentFeed);
            Assert.GreaterOrEqual(congestion.GetLoad(new GridPosition(1, 0)), 1);
        }

        [Test]
        public void AgentFeed_ShowsInSummaryLine()
        {
            GridSystem grid = new GridSystem(new GridSize(10, 8), 2f);
            CongestionSystem congestion = new CongestionSystem(new RouteSystem(grid), grid);
            List<GridPosition> trip = new List<GridPosition> { new GridPosition(3, 3) };
            for (int i = 0; i < 5; i++) congestion.TrackTrip(trip);

            Assert.AreEqual(1, congestion.CongestedCellsCount());
            Assert.AreEqual("Пробки: 1 (худшая клетка 25%, рост ×2)", congestion.FormatSummaryLine());
        }
    }
}
