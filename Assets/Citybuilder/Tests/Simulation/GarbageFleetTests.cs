using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Флот мусоровозов (EPIC-52): лимит домов на рейс и уникальные события ленты
    // при вывозе двух домов за один прогон. Накопление/порог/очередь — GarbageSystemTests.
    public partial class GarbageFleetTests
    {
        private static readonly GridPosition Depot = new GridPosition(0, 0);

        // Рейс берёт не больше HousesPerRun домов; остаток очереди ждёт следующего выезда.
        // Очередь копится БЕЗ полигона, депо появляется после — один Dispatch разбирает очередь.
        [Test]
        public void Dispatch_TakesAtMostHousesPerRun()
        {
            List<BuildingState> homes = new List<BuildingState>();
            int horizon = 0;
            for (int i = 0; i < 5; i++)
            {
                BuildingState home = Home(new GridPosition(2 + i * 2, 2 + i * 2));
                homes.Add(home);
                horizon = Math.Max(horizon, ReadyTick(home.Id));
            }
            GarbageSystem garbage = Run(homes, horizon + 1, null, null);
            Assert.AreEqual(5, garbage.WaitingCount, "без полигона все ждут");
            garbage.Tick(horizon + 1, homes, Depot, FakePath);
            Assert.AreEqual(1, garbage.ActiveTrucks);
            GarbageTrucks.Truck truck = garbage.TruckAt(0);
            Assert.AreEqual(GarbageTrucks.HousesPerRun, truck.Houses.Count, "лимит домов на рейс");
            Assert.AreEqual(1, garbage.WaitingCount, "пятый дом ждёт следующего рейса");
        }

        // Дедуп ленты: два дома вывезены в один прогон — два разных сообщения.
        [Test]
        public void TwoHomes_SameRun_TwoDistinctEventsInLog()
        {
            BuildingState first = Home(new GridPosition(1, 1));
            BuildingState second = Home(new GridPosition(3, 3));
            CityEventLog log = new CityEventLog();
            GarbageSystem garbage = new GarbageSystem();
            garbage.Collected += delegate (CityEvent cityEvent) { log.Add(cityEvent); };
            List<BuildingState> homes = new List<BuildingState> { first, second };
            int horizon = Math.Max(ReadyTick(first.Id), ReadyTick(second.Id)) + 1;
            for (int t = 0; t <= horizon; t++) garbage.Tick(t, homes, Depot, FakePath);
            garbage.Advance(30f);
            Assert.AreEqual(2, log.Count);
            Assert.AreNotEqual(log.Snapshot()[0].Args[0], log.Snapshot()[1].Args[0]);
        }

        // Делегат null: ноги считаются прямым L-маршрутом — вывоз работает без дорог.
        [Test]
        public void Dispatch_NullFinder_FallsBackToDirectPath()
        {
            BuildingState home = Home(new GridPosition(3, 3));
            GarbageSystem garbage = Run(new List<BuildingState> { home }, ReadyTick(home.Id) + 1, Depot, null);
            Assert.AreEqual(1, garbage.ActiveTrucks);
            GarbageTrucks.Truck truck = garbage.TruckAt(0);
            Assert.IsTrue(truck.Route[0].Equals(Depot), "старт от полигона");
            Assert.IsTrue(truck.Route[truck.Route.Count - 1].Equals(Depot), "финиш на полигоне");
            Assert.IsTrue(truck.Route.Contains(new GridPosition(3, 3)), "дом на L-маршруте");
        }

        // Реальная сетка дорог (RouteSystem(grid, roads)): маршрут идёт по дорогам игрока
        // и объезжает прямой L-маршрут — образец CongestionFeed-теста AgentManagerTests.
        [Test]
        public void Dispatch_RouteSystemFinder_FollowsRoadCells()
        {
            GridSystem grid = new GridSystem(new GridSize(10, 10), 2f);
            List<GridPosition> roads = MakeList(new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(2, 0),
                new GridPosition(2, 1), new GridPosition(2, 2), new GridPosition(1, 2), new GridPosition(0, 2));
            RouteSystem routes = new RouteSystem(grid, roads);
            BuildingState home = Home(new GridPosition(0, 2));
            GarbageSystem garbage = Run(new List<BuildingState> { home }, ReadyTick(home.Id) + 1, Depot, routes.FindPath);
            Assert.AreEqual(1, garbage.ActiveTrucks);
            GarbageTrucks.Truck truck = garbage.TruckAt(0);
            Assert.Greater(truck.Route.Count, 5, "объезд по дорогам длиннее L-маршрута в 3 клетки");
            Assert.IsTrue(truck.Route.Contains(new GridPosition(2, 2)), "маршрут прошёл по дороге");
            for (int i = 0; i < truck.Route.Count; i++) Assert.IsTrue(roads.Contains(truck.Route[i]), "клетка " + i + " не дорога");
        }

        // Дом не соединён с дорогой: RouteSystem вернул null — прямой L-маршрут, очередь не залипает.
        [Test]
        public void Dispatch_RouteSystemNoConnection_FallsBackToDirectPath()
        {
            GridSystem grid = new GridSystem(new GridSize(10, 10), 2f);
            RouteSystem routes = new RouteSystem(grid, MakeList(new GridPosition(0, 0), new GridPosition(1, 0)));
            BuildingState home = Home(new GridPosition(5, 5));
            GarbageSystem garbage = Run(new List<BuildingState> { home }, ReadyTick(home.Id) + 1, Depot, routes.FindPath);
            Assert.AreEqual(1, garbage.ActiveTrucks, "фолбэк: мусоровоз выехал напрямик");
            Assert.AreEqual(0, garbage.WaitingCount, "очередь разобрана, вывоз не встал");
        }

        private static List<GridPosition> MakeList(params GridPosition[] cells)
        {
            return new List<GridPosition>(cells);
        }

        // Маппинг конца ноги на ближайшую дорогу (ревью task/214): дом/депо не занимают
        // дорожные клетки — маршрут стартует с ближайшей дороги к зданию.
        [Test]
        public void TryNearestRoad_PicksClosestCell_AndFailsOnEmpty()
        {
            List<GridPosition> cells = MakeList(new GridPosition(1, 0), new GridPosition(4, 2), new GridPosition(2, 5));
            GridPosition nearest;
            Assert.IsTrue(GarbageTruckView.TryNearestRoad(new GridPosition(3, 1), cells, out nearest));
            Assert.AreEqual(new GridPosition(4, 2), nearest);
            Assert.IsTrue(GarbageTruckView.TryNearestRoad(new GridPosition(1, 1), cells, out nearest));
            Assert.AreEqual(new GridPosition(1, 0), nearest);
            List<GridPosition> empty = new List<GridPosition>();
            Assert.IsFalse(GarbageTruckView.TryNearestRoad(new GridPosition(0, 0), empty, out nearest));
        }

        private static BuildingState Home(GridPosition at)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = "house";
            def.WasteOutput = 1;
            return new BuildingState(EntityId.New(), def, at, new List<GridPosition> { at });
        }

        // Fake-маршрут из двух клеток: ноги конкатенируются без дублей стыковых клеток.
        private static List<GridPosition> FakePath(GridPosition from, GridPosition to)
        {
            return new List<GridPosition> { from, to };
        }

        // Детерминированный поиск тика готовности: первый t, когда сумма прироста ≥ порога.
        private static int ReadyTick(EntityId id)
        {
            int total = 0;
            for (int t = 0; t < 10000; t++)
            {
                total += GarbageSystem.GainPerTick(t, id, 1);
                if (total >= GarbageSystem.ReadyThreshold) return t;
            }
            Assert.Fail("Дом не готов к вывозу за 10000 тиков — проверь GainPerTick.");
            return -1;
        }

    }
}
