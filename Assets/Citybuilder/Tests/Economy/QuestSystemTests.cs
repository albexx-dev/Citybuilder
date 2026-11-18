using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Домен квестов-вех (EPIC-48, ADR-012): переход «выполнена ровно один раз», награда
    // казне, событие в ленту, замена на не-выполненную задачу, лимит 3 активных и Reset.
    public class QuestSystemTests
    {
        [Test]
        public void Tick_QuestSatisfied_CompletesOnceWithRewardAndEvent()
        {
            QuestSystem quests = new QuestSystem();
            Treasury treasury = new Treasury(0);
            CityEventLog log = new CityEventLog();

            quests.Tick(1, MakeContext(0, 0), treasury, log);
            Assert.AreEqual(3, quests.Active.Count);
            Assert.IsTrue(HasActive(quests, "q_res_10"));

            quests.Tick(2, MakeContext(10, 0), treasury, log);

            Assert.AreEqual(200, treasury.Balance);
            Assert.AreEqual(1, quests.CompletedCount);
            Assert.IsFalse(HasActive(quests, "q_res_10"));
            Assert.AreEqual(3, quests.Active.Count);
            Assert.AreEqual(1, CountMessages(log, "Задача выполнена: Первые жители (+200$)"));
        }

        [Test]
        public void Tick_SameContextAgain_NoSecondReward()
        {
            QuestSystem quests = new QuestSystem();
            Treasury treasury = new Treasury(0);
            quests.Tick(1, MakeContext(0, 0), treasury, null);
            quests.Tick(2, MakeContext(10, 0), treasury, null);
            quests.Tick(3, MakeContext(10, 0), treasury, null);
            Assert.AreEqual(200, treasury.Balance);
            Assert.AreEqual(1, quests.CompletedCount);
        }

        // План тестов эпика: на замену приходит не-выполненная задача.
        [Test]
        public void Tick_Replacement_IsNotSatisfiedByCurrentContext()
        {
            QuestSystem quests = new QuestSystem();
            QuestContext context = MakeContext(10, 0);
            quests.Tick(1, context, null, null);
            quests.Tick(2, context, null, null);
            Assert.AreEqual(3, quests.Active.Count);
            for (int i = 0; i < quests.Active.Count; i++)
            {
                Assert.IsFalse(quests.Active[i].IsSatisfied(context), quests.Active[i].Id);
            }
        }

        [Test]
        public void Tick_LongRun_ActiveNeverExceedsThree()
        {
            QuestSystem quests = new QuestSystem();
            for (int tick = 1; tick <= 40; tick++)
            {
                quests.Tick(tick, MakeContext(tick * 2, tick / 2), new Treasury(0), null);
                Assert.LessOrEqual(quests.Active.Count, QuestSystem.MaxActive);
            }
            Assert.Greater(quests.CompletedCount, 0);
        }

        [Test]
        public void Tick_NullTreasuryAndLog_CompletesWithoutCrash()
        {
            QuestSystem quests = new QuestSystem();
            quests.Tick(1, MakeContext(0, 0), null, null);
            quests.Tick(2, MakeContext(10, 0), null, null);
            Assert.AreEqual(1, quests.CompletedCount);
        }

        [Test]
        public void Reset_ClearsState_NextTickRefillsFromScratch()
        {
            QuestSystem quests = new QuestSystem();
            Treasury treasury = new Treasury(0);
            quests.Tick(1, MakeContext(0, 0), treasury, null);
            quests.Tick(2, MakeContext(10, 0), treasury, null);
            Assert.AreEqual(1, quests.CompletedCount);

            quests.Reset();

            Assert.AreEqual(0, quests.CompletedCount);
            Assert.AreEqual(0, quests.Active.Count);
            quests.Tick(3, MakeContext(0, 0), treasury, null);
            Assert.IsTrue(HasActive(quests, "q_res_10"));
            Assert.AreEqual(200, treasury.Balance);
        }

        // Событие стреляет после мутаций Tick: Reset внутри подписчика не ломает обход.
        [Test]
        public void Tick_SubscriberResetsInsideEvent_StateStaysConsistent()
        {
            QuestSystem quests = new QuestSystem();
            Treasury treasury = new Treasury(0);
            quests.QuestCompleted += delegate { quests.Reset(); };
            quests.Tick(1, MakeContext(0, 0), treasury, null);
            quests.Tick(2, MakeContext(10, 0), treasury, null);
            Assert.AreEqual(200, treasury.Balance);
            Assert.AreEqual(0, quests.CompletedCount);
        }

        [Test]
        public void ProgressLine_ResidentsAndCoverage_FromContext()
        {
            QuestSystem quests = new QuestSystem();
            quests.Tick(1, MakeContext(12, 0), null, null);
            QuestDefinition residents = new QuestDefinition("q_res_50", "Т", "О", 1, c => false);
            Assert.AreEqual("Жители 12/50", QuestSystem.ProgressLine(residents, quests.LastContext));
            QuestContext covered = MakeContext(10, 5);
            covered.FireCoverage = 0.6f;
            QuestDefinition fire = new QuestDefinition("q_fire_cover", "Т", "О", 1, c => false);
            Assert.AreEqual("Пожарные 60/100", QuestSystem.ProgressLine(fire, covered));
        }

        private static QuestContext MakeContext(int residents, int buildings)
        {
            QuestContext context = new QuestContext();
            context.Residents = residents;
            context.Buildings = buildings;
            return context;
        }

        private static bool HasActive(QuestSystem quests, string id)
        {
            for (int i = 0; i < quests.Active.Count; i++)
            {
                if (quests.Active[i].Id == id) return true;
            }
            return false;
        }

        private static int CountMessages(CityEventLog log, string message)
        {
            int count = 0;
            IReadOnlyList<CityEvent> snapshot = log.Snapshot();
            for (int i = 0; i < snapshot.Count; i++)
            {
                if (snapshot[i].Message == message) count++;
            }
            return count;
        }
    }
}
