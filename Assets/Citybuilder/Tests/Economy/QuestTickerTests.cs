using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Связка QuestTicker с системами (EPIC-48): снимок контекста из населения, счётчика
    // зданий, служб (парки, покрытие) и казны; отмотка времени назад сбрасывает цепочку.
    // Требует Unity: BuildingDefinition — ScriptableObject, вне редактора не собрать.
    public class QuestTickerTests
    {
        [Test]
        public void Tick_ContextFromSystems_QuestsCompleteWithRewards()
        {
            QuestTicker ticker = new QuestTicker();
            Treasury treasury = new Treasury(0);
            CityEventLog log = new CityEventLog();

            ticker.Tick(1, new PopulationSystem(), 0, MakeCoverage(0), treasury, log);
            Assert.AreEqual(3, ticker.Quests.Active.Count);

            ticker.Tick(2, MakePopulation(10), 0, MakeCoverage(0), treasury, log);
            Assert.AreEqual(1, ticker.Quests.CompletedCount);
            Assert.AreEqual(200, treasury.Balance);

            // Парки из служб досуга дошли до контекста: q_park_3 выполнена.
            ticker.Tick(3, MakePopulation(10), 0, MakeCoverage(3), treasury, log);
            Assert.AreEqual(2, ticker.Quests.CompletedCount);
            Assert.AreEqual(500, treasury.Balance);

            // Счётчик выросших зданий дошел: q_grow_5 выполнена.
            ticker.Tick(4, MakePopulation(10), 5, MakeCoverage(3), treasury, log);
            Assert.AreEqual(3, ticker.Quests.CompletedCount);
            Assert.AreEqual(750, treasury.Balance);
        }

        // Время отмотано назад («Новая игра»/загрузка): квесты не сериализуются до v5 —
        // цепочка начинается заново, выполненные id освобождаются.
        [Test]
        public void Tick_TimeRewound_ResetsQuestChain()
        {
            QuestTicker ticker = new QuestTicker();
            Treasury treasury = new Treasury(0);
            ticker.Tick(100, new PopulationSystem(), 0, null, treasury, null);
            ticker.Tick(101, MakePopulation(10), 0, null, treasury, null);
            Assert.AreEqual(1, ticker.Quests.CompletedCount);
            Assert.AreEqual(200, treasury.Balance);

            ticker.Tick(5, new PopulationSystem(), 0, null, treasury, null);

            Assert.AreEqual(0, ticker.Quests.CompletedCount);
            Assert.AreEqual(3, ticker.Quests.Active.Count);
            Assert.IsTrue(HasActive(ticker.Quests, "q_res_10"));
        }

        [Test]
        public void Tick_NullSystems_SafeZeroContext()
        {
            QuestTicker ticker = new QuestTicker();
            ticker.Tick(1, null, 0, null, null, null);
            Assert.AreEqual(3, ticker.Quests.Active.Count);
        }

        private static PopulationSystem MakePopulation(int residents)
        {
            PopulationSystem population = new PopulationSystem();
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.ResidentCapacity = residents;
            BuildingState home = new BuildingState(EntityId.New(), definition,
                new GridPosition(0, 0), new List<GridPosition> { new GridPosition(0, 0) });
            population.MoveIn(home, residents);
            return population;
        }

        // Парки — станции службы досуга; домов в снимке нет, покрытие видов равно 1.
        private static ServiceBuildingSystem MakeCoverage(int parks)
        {
            ServiceBuildingSystem coverage = new ServiceBuildingSystem();
            List<ServiceSource> sources = new List<ServiceSource>();
            for (int i = 0; i < parks; i++) sources.Add(new ServiceSource(new GridPosition(10 + i, 10), 5));
            coverage.SetSources(ServiceKind.Leisure, sources);
            coverage.BuildSnapshot(new List<GridPosition>());
            return coverage;
        }

        private static bool HasActive(QuestSystem quests, string id)
        {
            for (int i = 0; i < quests.Active.Count; i++)
            {
                if (quests.Active[i].Id == id) return true;
            }
            return false;
        }
    }
}
