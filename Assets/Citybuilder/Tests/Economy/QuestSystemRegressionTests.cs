using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Регрессии слотов сейма и генератора вех: дубль активного id и упадок города
    // ниже уже выполненной вехи (MEDIUM-ревью экономики/квестов).
    public class QuestSystemRegressionTests
    {
        // Дубль id в слоте давал две копии квеста и двойную награду (treasury.Earn не идемпотентен).
        [Test]
        public void RestoreActive_DuplicateId_RewardPaidOnce()
        {
            QuestSystem quests = new QuestSystem();
            Treasury treasury = new Treasury(0);
            CityEventLog log = new CityEventLog();
            quests.RestoreActive(new[] { "q_res_10", "q_res_10", "q_grow_5" });
            Assert.AreEqual(2, quests.Active.Count);

            quests.Tick(1, MakeContext(10, 0), treasury, log);

            Assert.AreEqual(200, treasury.Balance);
            Assert.AreEqual(1, quests.CompletedCount);
            Assert.AreEqual(1, CountMessages(log, EventMessageCatalog.QuestCompleted));
        }

        // Население упало ниже выполненной q_res_ms_250: вместо неё должна появиться q_res_ms_300.
        [Test]
        public void Tick_ResidentsFallBelowCompletedMilestone_NextMilestoneIssued()
        {
            QuestSystem quests = new QuestSystem();
            CompleteStarters(quests);
            QuestContext high = MakeContext(230, 10);
            quests.Tick(1, high, null, null);
            Assert.IsTrue(HasActive(quests, "q_res_ms_250"));

            int before = quests.CompletedCount;
            quests.Tick(2, MakeContext(250, 10), null, null);
            Assert.IsFalse(HasActive(quests, "q_res_ms_250"));
            Assert.AreEqual(before + 1, quests.CompletedCount);

            quests.RestoreActive(new string[0]);
            quests.Tick(3, high, null, null);
            Assert.IsTrue(HasActive(quests, "q_balance_ms_5000"));
            Assert.IsTrue(HasActive(quests, "q_res_ms_300"));
        }

        // Баланс упал ниже выполненной q_balance_ms_5000: вместо неё должна появиться q_balance_ms_10000.
        [Test]
        public void Tick_BalanceFallsBelowCompletedMilestone_NextMilestoneIssued()
        {
            QuestSystem quests = new QuestSystem();
            CompleteStarters(quests);
            QuestContext high = MakeContext(0, 10);
            high.Balance = 4000;
            quests.Tick(1, high, null, null);
            Assert.IsTrue(HasActive(quests, "q_balance_ms_5000"));

            int before = quests.CompletedCount;
            high.Balance = 5000;
            quests.Tick(2, high, null, null);
            Assert.IsFalse(HasActive(quests, "q_balance_ms_5000"));
            Assert.AreEqual(before + 1, quests.CompletedCount);

            quests.RestoreActive(new string[0]);
            high.Balance = 4000;
            quests.Tick(3, high, null, null);
            Assert.IsTrue(HasActive(quests, "q_balance_ms_10000"));
        }

        // Смена уровня рейтинга пишет одно событие в ленту и не повторяется без смены уровня.
        [Test]
        public void Tick_RatingTierChange_RecordsOneEvent()
        {
            QuestSystem quests = new QuestSystem();
            CityEventLog log = new CityEventLog();
            string key = EventMessageCatalog.MayorRatingChanged;

            quests.Tick(1, MakeContext(0, 0), null, log);
            Assert.AreEqual(0, CountMessages(log, key));

            QuestContext rich = MakeContext(0, 0);
            rich.Balance = 25000; // 25 очков → уровень 1
            quests.Tick(2, rich, null, log);
            Assert.AreEqual(1, CountMessages(log, key));

            quests.Tick(3, rich, null, log);
            Assert.AreEqual(1, CountMessages(log, key));
        }

        // Ключ рейтинга живёт в каталоге ленты и имеет оба перевода.
        [Test]
        public void MayorRatingEvent_RegisteredInCatalogWithBothLanguages()
        {
            bool found = false;
            foreach (string registered in EventMessageCatalog.Keys)
            {
                if (registered == "event_mayor_rating") { found = true; break; }
            }
            Assert.IsTrue(found);
            Assert.AreEqual(EventMessageCatalog.MayorRatingChanged, MayorRating.ChangedEventKey);
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("Рейтинг мэра вырос: 60 (уровень 2, +2 $/ч)",
                Localization.T(EventMessageCatalog.MayorRatingChanged, 60, 2, 2));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Mayor rating rose: 60 (tier 2, +2 $/h)",
                Localization.T(EventMessageCatalog.MayorRatingChanged, 60, 2, 2));
            Localization.SetLanguage(Language.Ru);
        }

        // Стартовые квесты помечены выполненными: генератор сразу работает с вехами.
        private static void CompleteStarters(QuestSystem quests)
        {
            quests.RestoreCompleted(new[]
            {
                "q_res_10", "q_grow_5", "q_res_50", "q_park_3", "q_grow_20", "q_res_100",
                "q_fire_cover", "q_police_cover", "q_res_200", "q_health_cover",
                QuestCatalog.ServicesAllId
            });
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

        private static int CountMessages(CityEventLog log, string messageKey)
        {
            int count = 0;
            IReadOnlyList<CityEvent> snapshot = log.Snapshot();
            for (int i = 0; i < snapshot.Count; i++)
            {
                if (snapshot[i].MessageKey == messageKey) count++;
            }
            return count;
        }
    }
}
