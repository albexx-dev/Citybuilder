using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Флот мусоровозов (EPIC-52): лимит домов на рейс и уникальные события ленты
    // при вывозе двух домов за один прогон. Накопление/порог/очередь — GarbageSystemTests.
    public class GarbageFleetTests
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
            garbage.Collected += delegate (EventKind kind, string message, int tick) { log.Add(kind, message, tick); };
            List<BuildingState> homes = new List<BuildingState> { first, second };
            int horizon = Math.Max(ReadyTick(first.Id), ReadyTick(second.Id)) + 1;
            for (int t = 0; t <= horizon; t++) garbage.Tick(t, homes, Depot, FakePath);
            garbage.Advance(30f);
            Assert.AreEqual(2, log.Count);
            Assert.AreNotEqual(log.Snapshot()[0].Message, log.Snapshot()[1].Message);
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

        private static GarbageSystem Run(List<BuildingState> homes, int ticks, GridPosition? depot,
            Func<GridPosition, GridPosition, List<GridPosition>> pathFinder)
        {
            GarbageSystem garbage = new GarbageSystem();
            for (int t = 0; t < ticks; t++) garbage.Tick(t, homes, depot, pathFinder);
            return garbage;
        }
    }
}
