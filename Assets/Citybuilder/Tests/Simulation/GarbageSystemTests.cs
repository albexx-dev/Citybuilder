using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты GarbageSystem (EPIC-52): детерминизм накопления по FNV-1a, формула прироста,
    // порог готовности и очередь, границы (0 домов / 0 полигонов), вывоз сбрасывает
    // накопление, уникальные по дому события, Reset при отмотке времени назад.
    public partial class GarbageSystemTests
    {
        private static readonly GridPosition Depot = new GridPosition(0, 0);
        private static readonly GridPosition HouseAt = new GridPosition(6, 6);

        // Формула прироста: отрицательный WasteOutput клампится в ноль, остальное — бит FNV-1a хеша.
        [Test]
        public void GainPerTick_NegativeOutputClamped_HashBitAdded()
        {
            EntityId id = EntityId.New();
            for (int t = 0; t < 1000; t++)
            {
                Assert.AreEqual((int)(GarbageSystem.Hash(t, id) & 1u), GarbageSystem.GainPerTick(t, id, -5), "tick " + t);
            }
        }

        // Детерминизм: два прогона по одним и тем же входам — одинаковое накопление и события.
        [Test]
        public void TwoRuns_SameInputs_SameAccumulationAndEvents()
        {
            BuildingState home = Home(HouseAt);
            List<CityEvent> firstEvents = new List<CityEvent>();
            List<CityEvent> secondEvents = new List<CityEvent>();
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
            GarbageSystem before = Run(new List<BuildingState> { home }, readyTick, null, null, new List<CityEvent>());
            Assert.AreEqual(0, before.WaitingCount);
            // +10 тиков после порога: дом уже в очереди — повторно не добавляется (грид queued).
            GarbageSystem after = Run(new List<BuildingState> { home }, readyTick + 11, null, null, new List<CityEvent>());
            Assert.AreEqual(1, after.WaitingCount);
            Assert.GreaterOrEqual(after.AccOf(home.Id), GarbageSystem.ReadyThreshold);
        }

        // Граница: нет полигона — мусоровозов нет, накопление и очередь продолжаются.
        [Test]
        public void NoDepot_NoTrucks_AccumulationContinues()
        {
            BuildingState home = Home(HouseAt);
            int readyTick = ReadyTick(home.Id);
            GarbageSystem garbage = Run(new List<BuildingState> { home }, readyTick + 5, null, null, new List<CityEvent>());
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
                new List<CityEvent>());
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
            List<CityEvent> events = new List<CityEvent>();
            int readyTick = ReadyTick(home.Id);
            GarbageSystem garbage = Run(new List<BuildingState> { home }, readyTick + 1, Depot, FakePath, events);
            garbage.Advance(10f);
            Assert.AreEqual(0, garbage.AccOf(home.Id));
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(EventMessageCatalog.GarbageCollected, events[0].MessageKey);
            Assert.AreEqual(home.Id.Value, events[0].Args[0]);
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
                new List<CityEvent>());
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

    }
}
