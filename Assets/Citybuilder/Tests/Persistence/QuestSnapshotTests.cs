using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Цепочка квестов в слоте v5 (хвост #48): QuestSnapshot.Write/Apply поверх швов
    // QuestSystem. Без сцены: QuestSystem, QuestCatalog и CitySnapshot — чистый C#,
    // JSON-круговорот слота проверяется в Unity (SnapshotV5*Tests).
    public class QuestSnapshotTests
    {
        [Test]
        public void RoundTrip_TwoActiveThreeCompleted_RestoredInOrder()
        {
            QuestSystem quests = new QuestSystem();
            quests.RestoreCompleted(Ids("q_res_10", "q_grow_5", "q_res_50"));
            quests.RestoreActive(Ids("q_res_100", "q_grow_20"));
            CitySnapshot snapshot = new CitySnapshot();
            QuestSnapshot.Write(quests, snapshot);

            QuestSystem restored = new QuestSystem();
            QuestSnapshot.Apply(snapshot, restored);

            Assert.AreEqual(3, restored.CompletedCount);
            Assert.AreEqual(2, restored.ActiveIds.Count);
            Assert.AreEqual("q_res_100", restored.Active[0].Id);
            Assert.AreEqual("q_grow_20", restored.Active[1].Id);
            CollectionAssert.AreEquivalent(Ids("q_res_10", "q_grow_5", "q_res_50"), restored.CompletedIds);
        }

        // Живой путь: цепочка после тика пишется и восстанавливается, выполненная задача
        // не повторяется — следующий тик не начисляет за неё награду второй раз.
        [Test]
        public void RoundTrip_AfterTick_CompletedNotRepeatedOnNextTick()
        {
            QuestSystem quests = new QuestSystem();
            quests.Tick(1, Context(0, 0, 0), null, null);
            quests.Tick(2, Context(10, 0, 0), null, null);
            Assert.AreEqual(1, quests.CompletedCount);
            CitySnapshot snapshot = new CitySnapshot();
            QuestSnapshot.Write(quests, snapshot);

            QuestSystem restored = new QuestSystem();
            QuestSnapshot.Apply(snapshot, restored);
            restored.Tick(3, Context(10, 0, 0), null, null);

            Assert.AreEqual(1, restored.CompletedCount);
            Assert.AreEqual(3, restored.Active.Count);
            Assert.IsFalse(ContainsId(restored, "q_res_10"));
        }

        // Неизвестный id (вехи нет в каталоге) тихо пропускается: недостающее добирает Fill.
        [Test]
        public void Apply_UnknownActiveId_SkippedSilently()
        {
            QuestSystem quests = new QuestSystem();
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.QuestActive.Add("q_res_10");
            snapshot.QuestActive.Add("q_nope_999");
            QuestSnapshot.Apply(snapshot, quests);

            Assert.AreEqual(1, quests.Active.Count);
            Assert.AreEqual("q_res_10", quests.Active[0].Id);

            quests.Tick(1, Context(0, 0, 0), null, null);
            Assert.AreEqual(3, quests.Active.Count);
            Assert.AreEqual(0, quests.CompletedCount);
        }

        // Пустые списки в слоте: активные и выполненные очищаются — цепочка стартует заново.
        [Test]
        public void Apply_EmptyLists_ChainStartsFresh()
        {
            QuestSystem quests = new QuestSystem();
            quests.Tick(1, Context(0, 0, 0), null, null);
            quests.Tick(2, Context(10, 0, 0), null, null);
            QuestSnapshot.Apply(new CitySnapshot(), quests);

            Assert.AreEqual(0, quests.CompletedCount);
            Assert.AreEqual(0, quests.Active.Count);

            quests.Tick(3, Context(0, 0, 0), null, null);
            Assert.AreEqual(3, quests.Active.Count);
            Assert.AreEqual("q_res_10", quests.Active[0].Id);
        }

        // Старый слот: Version 5 без квестовых полей — Apply не падает, цепочка пуста до тика.
        [Test]
        public void Apply_LegacySlotWithoutQuestFields_DoesNotThrow()
        {
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.Version = 5;
            snapshot.Balance = 700;
            QuestSystem quests = new QuestSystem();
            Assert.DoesNotThrow(delegate { QuestSnapshot.Apply(snapshot, quests); });
            Assert.AreEqual(0, quests.CompletedCount);
            Assert.AreEqual(0, quests.Active.Count);
        }

        // Панель задач не найдена в сцене: списки пустые, слот остаётся валидным.
        [Test]
        public void Write_NullSystem_WritesEmptyLists()
        {
            CitySnapshot snapshot = new CitySnapshot();
            QuestSnapshot.Write(null, snapshot);
            Assert.AreEqual(0, snapshot.QuestActive.Count);
            Assert.AreEqual(0, snapshot.QuestCompleted.Count);
        }

        private static List<string> Ids(params string[] values)
        {
            return new List<string>(values);
        }

        // Снимок города: жители, здания, парки; баланс и покрытие для этих тестов не важны.
        private static QuestContext Context(int residents, int buildings, int parks)
        {
            QuestContext context = new QuestContext();
            context.Residents = residents;
            context.Buildings = buildings;
            context.Parks = parks;
            return context;
        }

        private static bool ContainsId(QuestSystem quests, string id)
        {
            for (int i = 0; i < quests.Active.Count; i++)
            {
                if (quests.Active[i].Id == id) return true;
            }
            return false;
        }
    }
}
