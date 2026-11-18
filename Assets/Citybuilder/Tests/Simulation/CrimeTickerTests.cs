using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты CrimeTicker (EPIC-47): связка со снимком ServiceBuildingSystem — штраф казне
    // за кражу, запись события в CityEventLog, полиция в радиусе останавливает кражи,
    // безопасный null-снимок и уровень преступности для строки инфоменю.
    public class CrimeTickerTests
    {
        private const int ProbeTicks = 200000;

        // Кража дома без полиции: штраф $50 списан, событие Crime в ленте, уровень 100%.
        [Test]
        public void Theft_FinesTreasury_AndWritesLog()
        {
            BuildingState home = Home(new GridPosition(20, 20));
            ServiceBuildingSystem services = new ServiceBuildingSystem();
            services.BuildSnapshot(new List<GridPosition> { home.Origin });
            CrimeTicker ticker = new CrimeTicker();
            CityEventLog log = new CityEventLog();
            Treasury treasury = new Treasury(1000);
            int robTick = FindRobTick(home.Id);
            List<BuildingState> homes = new List<BuildingState> { home };
            ticker.Tick(robTick, homes, services, log, treasury);
            Assert.AreEqual(950, treasury.Balance);
            Assert.AreEqual(1, log.Count);
            Assert.AreEqual(EventKind.Crime, ((CityEvent)log.Snapshot()[0]).Kind);
            Assert.AreEqual(1f, ticker.Crimes.CrimeLevel01);
        }

        // Полиция в радиусе: дом защищён, казна не тронута, уровень преступности 0.
        [Test]
        public void PoliceCoverage_StopsThefts_AndCrimeLevelIsZero()
        {
            BuildingState home = Home(new GridPosition(23, 23));
            ServiceBuildingSystem services = new ServiceBuildingSystem();
            services.SetSources(ServiceKind.Police,
                new List<ServiceSource> { new ServiceSource(new GridPosition(23, 23), 3) });
            services.BuildSnapshot(new List<GridPosition> { home.Origin });
            CrimeTicker ticker = new CrimeTicker();
            CityEventLog log = new CityEventLog();
            Treasury treasury = new Treasury(1000);
            int robTick = FindRobTick(home.Id);
            List<BuildingState> homes = new List<BuildingState> { home };
            ticker.Tick(robTick, homes, services, log, treasury);
            Assert.AreEqual(1000, treasury.Balance);
            Assert.AreEqual(0, log.Count);
            Assert.AreEqual(0f, ticker.Crimes.CrimeLevel01);
        }

        // Null-снимок покрытия: город считается безопасным, краж нет (план тестов эпика).
        [Test]
        public void NullCoverage_NoThefts_NoFine()
        {
            BuildingState home = Home(new GridPosition(24, 24));
            CrimeTicker ticker = new CrimeTicker();
            CityEventLog log = new CityEventLog();
            Treasury treasury = new Treasury(1000);
            int robTick = FindRobTick(home.Id);
            List<BuildingState> homes = new List<BuildingState> { home };
            ticker.Tick(robTick, homes, null, log, treasury);
            Assert.AreEqual(1000, treasury.Balance);
            Assert.AreEqual(0, log.Count);
            Assert.AreEqual(0f, ticker.Crimes.CrimeLevel01);
        }

        // Временный −досуг доступен через тикер: 0.2 после кражи, восстановился через 12 тиков.
        [Test]
        public void Ticker_ExposesLeisurePenalty_WithRecovery()
        {
            BuildingState home = Home(new GridPosition(25, 25));
            CrimeTicker ticker = new CrimeTicker();
            ticker.Crimes.ForceSteal(home, 200, delegate { });
            Assert.AreEqual(0.2f, ticker.Crimes.LeisurePenalty01(home.Id, 211));
            Assert.AreEqual(0f, ticker.Crimes.LeisurePenalty01(home.Id, 212));
        }

        private static BuildingState Home(GridPosition at)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = "house";
            return new BuildingState(EntityId.New(), def, at, new List<GridPosition> { at });
        }

        // Детерминированный поиск тика кражи: первый t с порогом хеша (без System.Random).
        private static int FindRobTick(EntityId id)
        {
            for (int t = 0; t < ProbeTicks; t++)
            {
                if (CrimeSystem.ShouldSteal(t, id)) return t;
            }
            Assert.Fail("Кражи нет за " + ProbeTicks + " тиков — проверь TheftChancePerTick.");
            return -1;
        }
    }
}
