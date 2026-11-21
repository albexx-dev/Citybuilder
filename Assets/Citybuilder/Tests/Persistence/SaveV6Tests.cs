using NUnit.Framework;

namespace Citybuilder
{
    // Сейв v6 (ADR-014): round-trip v1–v6, fallback старых слотов, нормализация и без двойной награды.
    public class SaveV6Tests
    {
        [Test]
        public void RoundTrip_V1ToV6_PreservesVersionAndOptionalFields()
        {
            for (int version = 1; version <= 6; version++)
            {
                CitySnapshot slot = SaveSystem.BuildSnapshot(null, null, null, 20, 5, null, 2, null, 1);
                slot.Version = version;
                slot.TheftCounter = 3;
                slot.GarbageQueue.Add(new CitySnapshot.GarbageQueueEntry { X = 1, Y = 2, Accumulated = 9 });
                slot.ComfortStreak.Add(new CitySnapshot.ComfortStreakEntry { X = 1, Y = 2, Streak = -4 });
                slot.ConstructionProgress.Add(new CitySnapshot.ConstructionProgressEntry
                    { X = 3, Y = 4, DefId = "shop", TicksElapsed = 2 });
                CitySnapshot restored = SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(slot));
                Assert.AreEqual(version, restored.Version);
                Assert.IsTrue(SaveSlotStorage.Validate(restored), "version=" + version);
                Assert.AreEqual(3, SaveV6Snapshot.ReadThefts(restored));
                Assert.AreEqual(9, SaveV6Snapshot.ReadGarbage(restored)[0].Accumulated);
                Assert.AreEqual(-4, SaveV6Snapshot.ReadStreak(restored, 1, 2));
                Assert.AreEqual(2, SaveV6Snapshot.ReadSites(restored)[0].TicksElapsed);
            }
        }

        // Старый файл без новых полей остаётся валидным и читается как раньше.
        [Test]
        public void LegacySlot_WithoutV6Fields_ReadsWithDefaults()
        {
            CitySnapshot slot = SaveSlotStorage.FromJson("{\"Version\":5,\"Balance\":7,"
                + "\"Buildings\":[{\"DefId\":\"house\",\"X\":0,\"Y\":0}]}");
            Assert.IsTrue(SaveSlotStorage.Validate(slot));
            Assert.AreEqual(0, SaveV6Snapshot.ReadThefts(slot));
            Assert.AreEqual(0, SaveV6Snapshot.ReadGarbage(slot).Count);
            Assert.AreEqual(0, SaveV6Snapshot.ReadSites(slot).Count);
            Assert.AreEqual(0, SaveV6Snapshot.ReadStreak(slot, 0, 0));
        }

        // Битые записи отбрасываются, дубли по клетке оставляют последнюю, счётчик не уходит в минус.
        [Test]
        public void Normalize_BrokenEntriesAndDuplicates_KeepLastValid()
        {
            CitySnapshot slot = new CitySnapshot();
            slot.TheftCounter = -7;
            slot.GarbageQueue.Add(null);
            slot.GarbageQueue.Add(new CitySnapshot.GarbageQueueEntry { X = -1, Y = 0, Accumulated = 5 });
            slot.GarbageQueue.Add(new CitySnapshot.GarbageQueueEntry { X = 2, Y = 2, Accumulated = 1 });
            slot.GarbageQueue.Add(new CitySnapshot.GarbageQueueEntry { X = 2, Y = 2, Accumulated = 8 });
            slot.ConstructionProgress.Add(new CitySnapshot.ConstructionProgressEntry
                { X = 4, Y = 4, DefId = "", TicksElapsed = 1 });
            slot.ConstructionProgress.Add(new CitySnapshot.ConstructionProgressEntry
                { X = 4, Y = 4, DefId = "shop", TicksElapsed = 999999 });
            slot.ConstructionProgress.Add(new CitySnapshot.ConstructionProgressEntry
                { X = 4, Y = 4, DefId = "shop", TicksElapsed = 3 });
            slot.ComfortStreak.Add(new CitySnapshot.ComfortStreakEntry { X = 1, Y = 1, Streak = 20000 });
            slot.ComfortStreak.Add(new CitySnapshot.ComfortStreakEntry { X = 1, Y = 1, Streak = 6 });
            SaveV6Snapshot.Normalize(slot);
            Assert.AreEqual(0, slot.TheftCounter);
            Assert.AreEqual(1, slot.GarbageQueue.Count);
            Assert.AreEqual(8, slot.GarbageQueue[0].Accumulated);
            Assert.AreEqual(1, slot.ConstructionProgress.Count);
            Assert.AreEqual(3, slot.ConstructionProgress[0].TicksElapsed);
            Assert.AreEqual(1, slot.ComfortStreak.Count);
            Assert.AreEqual(6, slot.ComfortStreak[0].Streak);
        }

        // Повторная загрузка не перепроигрывает квесты, кражи и события ленты.
        [Test]
        public void ApplyTwice_NoRepeatedRewardsOrTheftEvents()
        {
            QuestSystem quests = new QuestSystem();
            quests.Tick(1, Context(10, 0, 0), null, null);
            Assert.AreEqual(1, quests.CompletedCount);
            CitySnapshot slot = new CitySnapshot();
            QuestSnapshot.Write(quests, slot);
            slot.TheftCounter = 2;
            int events = 0;
            CrimeSystem crimes = new CrimeSystem();
            crimes.Fired += delegate { events++; };
            for (int i = 0; i < 2; i++)
            {
                QuestSnapshot.Apply(slot, quests);
                crimes.RestoreTotal(SaveV6Snapshot.ReadThefts(slot));
            }
            Assert.AreEqual(1, quests.CompletedCount);
            Assert.AreEqual(2, crimes.TotalThefts);
            Assert.AreEqual(0, events);
        }

        [Test]
        public void ConstructionRestore_EmptySlotAndNullGrid_AreSafe()
        {
            Assert.DoesNotThrow(delegate { ConstructionSnapshot.RestoreAll(new CitySnapshot(), null, null, null); });
            Assert.AreEqual(0, ConstructionSnapshot.Collect().Count);
            ConstructionSnapshot.Clear();
        }

        private static QuestContext Context(int residents, int buildings, int parks)
        {
            QuestContext context = new QuestContext();
            context.Residents = residents;
            context.Buildings = buildings;
            context.Parks = parks;
            return context;
        }
    }
}
