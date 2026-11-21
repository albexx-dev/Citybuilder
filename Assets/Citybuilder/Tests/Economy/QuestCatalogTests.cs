using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Каталог и генератор вех (EPIC-48, ADR-012): границы доступности (0 зданий →
    // службы скипаются), скип выполненных, активных и выполнимых, вехи жителей
    // +50 после 200 и баланса от $5000, отказ когда выдавать нечего.
    public class QuestCatalogTests
    {
        // План тестов эпика: при 0 зданий «построй больницу» не выдаётся.
        [Test]
        public void TryNext_AtZeroBuildings_SkipsServiceCoverQuests()
        {
            QuestDefinition quest;
            Assert.IsTrue(QuestCatalog.TryNext(MakeContext(0, 0), new HashSet<string>(), new List<string>(), out quest));
            Assert.AreEqual("q_res_10", quest.Id);
        }

        [Test]
        public void TryNext_SkipsCompletedQuests()
        {
            HashSet<string> completed = new HashSet<string>();
            completed.Add("q_res_10");
            QuestDefinition quest;
            Assert.IsTrue(QuestCatalog.TryNext(MakeContext(0, 0), completed, new List<string>(), out quest));
            Assert.AreNotEqual("q_res_10", quest.Id);
        }

        [Test]
        public void TryNext_SkipsActiveQuests()
        {
            List<string> activeIds = new List<string>();
            activeIds.Add("q_res_10");
            activeIds.Add("q_grow_5");
            activeIds.Add("q_res_50");
            QuestDefinition quest;
            Assert.IsTrue(QuestCatalog.TryNext(MakeContext(0, 0), new HashSet<string>(), activeIds, out quest));
            CollectionAssert.DoesNotContain(activeIds, quest.Id);
        }

        [Test]
        public void TryNext_NeverReturnsSatisfiedQuest()
        {
            QuestDefinition quest;
            Assert.IsTrue(QuestCatalog.TryNext(MakeContext(300, 30), new HashSet<string>(), new List<string>(), out quest));
            Assert.IsFalse(quest.IsSatisfied(MakeContext(300, 30)), quest.Id);
        }

        // Полный сервис открывается только после трёх квестов покрытия служб.
        [Test]
        public void TryNext_AfterCoverQuests_OffersServicesAll()
        {
            HashSet<string> completed = MakeCompletedStarters();
            completed.Remove("q_services_all");
            QuestDefinition quest;
            Assert.IsTrue(QuestCatalog.TryNext(MakeContext(0, 8), completed, new List<string>(), out quest));
            Assert.AreEqual("q_services_all", quest.Id);
        }

        // Вехи жителей: после 200 — каждые +50 (250, 300, ...), бесконечно.
        [Test]
        public void TryNext_AfterTwoHundredResidents_OffersPlusFiftySteps()
        {
            HashSet<string> completed = MakeCompletedStarters();
            QuestDefinition quest;
            Assert.IsTrue(QuestCatalog.TryNext(MakeContext(210, 0), completed, new List<string>(), out quest));
            Assert.AreEqual("q_res_ms_250", quest.Id);
            Assert.IsFalse(quest.IsSatisfied(MakeContext(210, 0)));
        }

        // Вехи баланса: в подросшем городе от $5000 — удвоение (5000, 10000, ...).
        [Test]
        public void TryNext_EstablishedCity_OffersDoubledBalanceMilestones()
        {
            HashSet<string> completed = MakeCompletedStarters();
            QuestDefinition quest;
            Assert.IsTrue(QuestCatalog.TryNext(MakeContext(0, 12, 3000), completed, new List<string>(), out quest));
            Assert.AreEqual("q_balance_ms_5000", quest.Id);
            Assert.IsTrue(QuestCatalog.TryNext(MakeContext(0, 12, 6000), completed, new List<string>(), out quest));
            Assert.AreEqual("q_balance_ms_10000", quest.Id);
        }

        [Test]
        public void TryNext_CompletedMilestone_OffersNextStep()
        {
            HashSet<string> completed = MakeCompletedStarters();
            completed.Add("q_res_ms_250");
            completed.Add("q_balance_ms_5000");
            QuestDefinition quest;
            Assert.IsTrue(QuestCatalog.TryNext(MakeContext(220, 12, 4000), completed, new List<string>(), out quest));
            Assert.AreEqual("q_res_ms_300", quest.Id);
        }

        private static HashSet<string> MakeCompletedStarters()
        {
            HashSet<string> completed = new HashSet<string>();
            for (int i = 0; i < QuestCatalog.Starters.Count; i++) completed.Add(QuestCatalog.Starters[i].Id);
            completed.Add("q_services_all");
            return completed;
        }

        private static QuestContext MakeContext(int residents, int buildings)
        {
            return MakeContext(residents, buildings, 0);
        }

        private static QuestContext MakeContext(int residents, int buildings, int balance)
        {
            QuestContext context = new QuestContext();
            context.Residents = residents;
            context.Buildings = buildings;
            context.Balance = balance;
            return context;
        }
    }
}
