using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Citybuilder
{
    // Тесты трёх слотов, автосейва и индекса slots.json (EPIC-11 шаги 1-2, ADR-008).
    public class SaveSlotsTests
    {
        private string tempDir;

        [SetUp]
        public void SetUp()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "CitybuilderSaveSlots_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }

        private static CitySnapshot MakeSnapshot(int balance, long tick)
        {
            return SaveSystem.BuildSnapshot(
                new List<BuildingState>(), new List<HouseholdState>(),
                new List<KeyValuePair<GridPosition, ZoneType>>(),
                balance, tick, new List<RoadState>(), 1, new List<BuildingState>(), 1);
        }

        [Test]
        public void RoundTrip_AllThreeSlotsAndAutosave_PreservesSnapshotsAndIndex()
        {
            for (int slot = 0; slot < SaveSlotIndex.ManualSlotCount; slot++)
            {
                Assert.IsTrue(SaveSlotStorage.Save(tempDir, slot, MakeSnapshot(1000 + slot * 100, slot * 24), "Город " + slot));
            }
            Assert.IsTrue(SaveSlotStorage.Save(tempDir, SaveSlotIndex.AutosaveSlot, MakeSnapshot(999, 72)));

            for (int slot = 0; slot < SaveSlotIndex.ManualSlotCount; slot++)
            {
                CitySnapshot loaded = SaveSlotStorage.Load(tempDir, slot);
                Assert.IsNotNull(loaded);
                Assert.AreEqual(1000 + slot * 100, loaded.Balance);
                Assert.AreEqual("Город " + slot, SaveSlotIndex.GetMeta(tempDir, slot).name);
            }

            CitySnapshot auto = SaveSlotStorage.Load(tempDir, SaveSlotIndex.AutosaveSlot);
            Assert.IsNotNull(auto);
            Assert.AreEqual(999, auto.Balance);
            Assert.AreEqual(72, auto.TickCount);
        }

        [Test]
        public void BackwardCompatibility_LegacySlot0WithoutIndex_LoadsAndPopulatesIndex()
        {
            string slot0Path = Path.Combine(tempDir, SaveSlotStorage.SlotFileName);
            SaveSlotStorage.WriteSlot(slot0Path, MakeSnapshot(1500, 48));

            CitySnapshot loaded = SaveSlotStorage.Load(tempDir, 0);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(1500, loaded.Balance);

            SaveSlotIndex.SlotMeta meta = SaveSlotIndex.GetMeta(tempDir, 0);
            Assert.IsNotNull(meta);
            Assert.AreEqual("Слот 1", meta.name);
            Assert.AreEqual(48, meta.savedAtTick);
        }

        [Test]
        public void CorruptedSlot2AndCorruptedIndex_AreIsolatedWithoutDeletingFiles()
        {
            SaveSlotStorage.Save(tempDir, 0, MakeSnapshot(1200, 10), "Живой");
            File.WriteAllText(Path.Combine(tempDir, "slot2.json"), "{ broken json");
            File.WriteAllText(Path.Combine(tempDir, SaveSlotIndex.IndexFileName), "not a json");

            Assert.IsFalse(SaveSlotStorage.Validate(tempDir, 2));
            Assert.IsTrue(SaveSlotStorage.Validate(tempDir, 0));
            Assert.IsTrue(File.Exists(Path.Combine(tempDir, "slot2.json")));
        }

        [Test]
        public void AutosaveCounter_FiresAt24Ticks_PausesWhenClockPaused_AndSkipsManualSaveSameTick()
        {
            int counter = 0;
            for (int t = 1; t <= 23; t++)
            {
                Assert.IsFalse(AutosaveController.StepCounter(ref counter, 24, t, -1));
            }
            Assert.IsTrue(AutosaveController.StepCounter(ref counter, 24, 24, -1));
            Assert.AreEqual(0, counter);
            Assert.IsFalse(AutosaveController.StepCounter(ref counter, 24, 25, -1));

            // Если на 48-м тике уже был ручной сейв, автосейв пропускает запись.
            counter = 23;
            Assert.IsFalse(AutosaveController.StepCounter(ref counter, 24, 48, 48));
        }
    }
}
