using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты FireTicker (EPIC-43, ADR-009): связка с ServiceBuildingSystem, запись
    // событий пожаров в CityEventLog, уникальность сообщений по дому, безопасный null.
    public class FireTickerTests
    {
        private const int ProbeTicks = 100000;

        // Два дома горят одновременно: сообщения уникальны по дому, дедуп ленты оба пишет.
        [Test]
        public void TwoBurningHomes_MessagesUnique_LogKeepsBoth()
        {
            BuildingState first = Home(new GridPosition(10, 10));
            BuildingState second = Home(new GridPosition(11, 11));
            FireSystem fires = new FireSystem();
            List<string> messages = new List<string>();
            CityEventLog log = new CityEventLog();
            fires.Fired += delegate (EventKind kind, string message, int tick)
            {
                messages.Add(message);
                log.Add(kind, message, tick);
            };
            fires.ForceIgnite(first, 50);
            fires.ForceIgnite(second, 50);
            Assert.AreEqual(2, log.Count);
            Assert.AreEqual(EventKind.Fire, ((CityEvent)log.Snapshot()[0]).Kind);
            Assert.AreNotEqual(messages[0], messages[1]);
        }

        // FireTicker: дом в радиусе станции снимка тушится, события старт+тушение в ленте.
        [Test]
        public void FireTicker_UsesServiceSnapshot_AndWritesLog()
        {
            BuildingState home = Home(new GridPosition(15, 15));
            ServiceBuildingSystem services = new ServiceBuildingSystem();
            services.SetSources(ServiceKind.Fire,
                new List<ServiceSource> { new ServiceSource(new GridPosition(15, 15), 3) });
            services.BuildSnapshot(new List<GridPosition> { home.Origin });
            FireTicker ticker = new FireTicker();
            CityEventLog log = new CityEventLog();
            List<BuildingState> homes = new List<BuildingState> { home };
            ticker.Tick(200, homes, services, log, null, delegate { });
            ticker.Fires.ForceIgnite(home, 200);
            ticker.Tick(201, homes, services, log, null, delegate { });
            Assert.IsFalse(ticker.Fires.IsBurning(home.Id));
            Assert.AreEqual(2, log.Count);
        }

        // FireTicker без снимка покрытия: дома считаются защищёнными, возгораний нет.
        [Test]
        public void FireTicker_NullCoverage_TreatsHomesProtected()
        {
            BuildingState home = Home(new GridPosition(16, 16));
            int igniteTick = FindIgniteTick(home.Id);
            FireTicker ticker = new FireTicker();
            ticker.Tick(igniteTick, new List<BuildingState> { home }, null, new CityEventLog(), null, delegate { });
            Assert.AreEqual(0, ticker.Fires.BurningCount);
        }

        // Разрушение через FireTicker: сгоревший дом уходит в делегат демонтажа, жители выселены.
        [Test]
        public void FireTicker_BurnDown_EvictsAndDemolishes()
        {
            BuildingState home = Home(new GridPosition(17, 17));
            PopulationSystem population = new PopulationSystem();
            population.MoveIn(home, 3);
            // Снимок без пожарных станций: покрытие дома 0 — дом может сгореть.
            ServiceBuildingSystem services = new ServiceBuildingSystem();
            services.BuildSnapshot(new List<GridPosition> { home.Origin });
            FireTicker ticker = new FireTicker();
            List<BuildingState> demolished = new List<BuildingState>();
            ticker.Fires.ForceIgnite(home, 300);
            for (int t = 301; t <= 306; t++)
            {
                ticker.Tick(t, new List<BuildingState> { home }, services, new CityEventLog(),
                    population, delegate (BuildingState b) { demolished.Add(b); });
            }
            Assert.AreEqual(1, demolished.Count);
            Assert.AreEqual(0, population.TotalResidents);
            Assert.IsFalse(ticker.Fires.IsBurning(home.Id));
        }

        private static BuildingState Home(GridPosition at)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = "house";
            return new BuildingState(EntityId.New(), def, at, new List<GridPosition> { at });
        }

        // Детерминированный поиск тика возгорания: первый t с порогом хеша (без System.Random).
        private static int FindIgniteTick(EntityId id)
        {
            for (int t = 0; t < ProbeTicks; t++)
            {
                if (FireSystem.ShouldIgnite(t, id)) return t;
            }
            Assert.Fail("Возгорания нет за " + ProbeTicks + " тиков — проверь IgniteChancePerTick.");
            return -1;
        }
    }
}
