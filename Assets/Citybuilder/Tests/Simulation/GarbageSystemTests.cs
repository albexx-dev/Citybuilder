using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты GarbageSystem (EPIC-52): детерминизм накопления по FNV-1a, формула прироста,
    // порог готовности и очередь, границы (0 домов / 0 полигонов), вывоз сбрасывает
    // накопление, уникальные по дому события, Reset при отмотке времени назад.
    public class GarbageSystemTests
    {
        private static readonly GridPosition Depot = new GridPosition(0, 0);
        private static readonly GridPosition HouseAt = new GridPosition(6, 6);

        // Формула прироста: WasteOutput плюс младший бит FNV-1a хеша — детерминирована.
        [Test]
        public void GainPerTick_FollowsHashFormula()
        {
            EntityId id = EntityId.New();
            for (int t = 0; t < 1000; t++)
            {
                int expected = 1 + (int)(GarbageSystem.Hash(t, id) & 1u);
                Assert.AreEqual(expected, GarbageSystem.GainPerTick(t, id, 1), "tick " + t);
            }
        }

        // Детерминизм: два прогона по одним и тем же входам — одинаковое накопление и события.
        [Test]
        public void TwoRuns_SameInputs_SameAccumulationAndEvents()
        {
            BuildingState home = Home(HouseAt);
            List<string> firstEvents = new List<string>();
            List<string> secondEvents = new List<string>();
            GarbageSystem first = Run(new List<BuildingState> { home }, 20, null, null, firstEvents);
            GarbageSystem second = Run(new List<BuildingState> { home }, 20, null, null, secondEvents);
            Assert.AreEqual(first.AccOf(home.Id), second.AccOf(home.Id));
            Assert.AreEqual(secondEvents, firstEvents);
        }

        // Порог и очередь: до порога дом не в очереди, на пороге — ровно один раз.
        [Test]
        public void Home_EntersQueue_Once_WhenThresholdCrossed()
        {
            BuildingState home = Home(HouseAt);
            int readyTick = ReadyTick(home.Id);
            GarbageSystem before = Run(new List<BuildingState> { home }, readyTick, null, null, new List<string>());
            Assert.AreEqual(0, before.WaitingCount);
            GarbageSystem after = Run(new List<BuildingState> { home }, readyTick + 1, null, null, new List<string>());
            Assert.AreEqual(1, after.WaitingCount);
            Assert.GreaterOrEqual(after.AccOf(home.Id), GarbageSystem.ReadyThreshold);
        }

        // Граница: нет полигона — мусоровозов нет, накопление и очередь продолжаются.
        [Test]
        public void NoDepot_NoTrucks_AccumulationContinues()
        {
            BuildingState home = Home(HouseAt);
            int readyTick = ReadyTick(home.Id);
            GarbageSystem garbage = Run(new List<BuildingState> { home }, readyTick + 5, null, null, new List<string>());
            Assert.AreEqual(0, garbage.ActiveTrucks);
            Assert.AreEqual(1, garbage.WaitingCount);
            Assert.GreaterOrEqual(garbage.AccOf(home.Id), GarbageSystem.ReadyThreshold);
        }

        // Граница: нет домов — ни очереди, ни мусоровозов даже с полигоном.
        [Test]
        public void ZeroHomes_NoQueueNoTrucks()
        {
            GarbageSystem garbage = new GarbageSystem();
            garbage.Tick(0, new List<BuildingState>(), Depot, FakePath);
            garbage.Advance(1f);
            Assert.AreEqual(0, garbage.WaitingCount);
            Assert.AreEqual(0, garbage.ActiveTrucks);
        }

        // Выезд: полигон и готовый дом — мусоровоз в пути, маршрут полигон → дом → полигон.
        [Test]
        public void Depot_DispatchesTruck_OverRoute()
        {
            BuildingState home = Home(HouseAt);
            int readyTick = ReadyTick(home.Id);
            GarbageSystem garbage = Run(new List<BuildingState> { home }, readyTick + 1, Depot, FakePath,
                new List<string>());
            Assert.AreEqual(1, garbage.ActiveTrucks);
            Assert.AreEqual(0, garbage.WaitingCount);
            GarbageTrucks.Truck truck = garbage.TruckAt(0);
            Assert.IsTrue(truck.Route[0].Equals(Depot), "start at depot");
            Assert.IsTrue(truck.Route[truck.Route.Count - 1].Equals(Depot), "finish at depot");
        }

        // Вывоз: у дома мусор сбрасывается в ноль, событие уникально по дому.
        [Test]
        public void PickUp_ResetsAccumulation_EmitsUniqueEvent()
        {
            BuildingState home = Home(HouseAt);
            List<string> events = new List<string>();
            int readyTick = ReadyTick(home.Id);
            GarbageSystem garbage = Run(new List<BuildingState> { home }, readyTick + 1, Depot, FakePath, events);
            garbage.Advance(10f);
            Assert.AreEqual(0, garbage.AccOf(home.Id));
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0].Contains("дома #" + home.Id.Value));
        }

        // Дедуп ленты: два дома вывезены в один прогон — два разных сообщения, оба в ленте.
        [Test]
        public void TwoHomes_SameRun_TwoDistinctEventsInLog()
        {
            BuildingState first = Home(new GridPosition(1, 1));
            BuildingState second = Home(new GridPosition(3, 3));
            CityEventLog log = new CityEventLog();
            GarbageSystem garbage = new GarbageSystem();
            garbage.Collected += delegate (EventKind kind, string message, int tick) { log.Add(kind, message, tick); };
            List<BuildingState> homes = new List<BuildingState> { first, second };
            int horizon = Math.Max(ReadyTick(first.Id), ReadyTick(second.Id)) + 1;
            for (int t = 0; t <= horizon; t++) garbage.Tick(t, homes, Depot, FakePath);
            garbage.Advance(30f);
            Assert.AreEqual(2, log.Count);
            Assert.AreNotEqual(log.Snapshot()[0].Message, log.Snapshot()[1].Message);
        }

        // Тикер: отмотка времени назад обнуляет накопление, очередь и мусоровозы.
        [Test]
        public void Ticker_RewindTime_ResetsGarbage()
        {
            BuildingState home = Home(HouseAt);
            GarbageTicker ticker = new GarbageTicker();
            List<BuildingState> homes = new List<BuildingState> { home };
            for (int t = 0; t <= ReadyTick(home.Id) + 1; t++) ticker.Tick(t, homes, Depot, null, FakePath);
            Assert.AreEqual(1, ticker.Garbage.ActiveTrucks);
            Assert.Greater(ticker.Garbage.AccOf(home.Id) + ticker.Garbage.WaitingCount, 0);
            ticker.Tick(1, homes, Depot, null, FakePath);
            // После отмотки накопление началось заново: ровно прирост первого тика.
            Assert.AreEqual(GarbageSystem.GainPerTick(1, home.Id, 1), ticker.Garbage.AccOf(home.Id));
            Assert.AreEqual(0, ticker.Garbage.WaitingCount);
            Assert.AreEqual(0, ticker.Garbage.ActiveTrucks);
        }

        // Прямой Reset: накопление, очередь и мусоровозы обнулены (команда «Новая игра»).
        [Test]
        public void Reset_ClearsAccumulationQueueAndTrucks()
        {
            BuildingState home = Home(HouseAt);
            GarbageSystem garbage = Run(new List<BuildingState> { home }, ReadyTick(home.Id) + 1, Depot, FakePath,
                new List<string>());
            garbage.Reset();
            Assert.AreEqual(0, garbage.AccOf(home.Id));
            Assert.AreEqual(0, garbage.WaitingCount);
            Assert.AreEqual(0, garbage.ActiveTrucks);
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

        // Прогон тиков; depot/pathFinder опциональны, события собираются в список.
        private static GarbageSystem Run(List<BuildingState> homes, int ticks, GridPosition? depot,
            Func<GridPosition, GridPosition, List<GridPosition>> pathFinder, List<string> events)
        {
            GarbageSystem garbage = new GarbageSystem();
            garbage.Collected += delegate (EventKind kind, string message, int tick) { events.Add(message); };
            for (int t = 0; t < ticks; t++) garbage.Tick(t, homes, depot, pathFinder);
            return garbage;
        }
    }
}
