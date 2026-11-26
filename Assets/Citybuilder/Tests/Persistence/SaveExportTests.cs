using System;
using System.IO;
using NUnit.Framework;

namespace Citybuilder
{
    // Экспорт/импорт слота (#118, ADR-016): тот же формат v1–v6 и отклонение битых файлов.
    public class SaveExportTests
    {
        private string dir;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "cb118-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        [Test]
        public void RoundTrip_ExportImport_KeepsJsonVersionAndFields()
        {
            CitySnapshot slot = Sample(6);
            string file = SlotTransfer.DefaultPath(dir);
            Assert.IsTrue(SlotTransfer.Export(file, slot));
            CitySnapshot restored = SlotTransfer.Import(file);
            Assert.IsNotNull(restored);
            Assert.AreEqual(6, restored.Version);
            Assert.AreEqual(slot.TickCount, restored.TickCount);
            Assert.AreEqual(slot.Balance, restored.Balance);
            Assert.AreEqual(2, restored.Zones.Count);
            Assert.AreEqual("house", restored.Buildings[0].DefId);
            Assert.AreEqual(1, restored.Households.Count);
            Assert.AreEqual(5, restored.TheftCounter);
            Assert.AreEqual(9, restored.GarbageQueue[0].Accumulated);
            Assert.AreEqual(2, restored.ConstructionProgress[0].TicksElapsed);
            Assert.AreEqual(File.ReadAllText(file), SaveSlotStorage.ToJson(slot));
        }

        [Test]
        public void LegacyVersions_ImportWithDefaults()
        {
            for (int version = 1; version <= 5; version++)
            {
                CitySnapshot slot = Sample(version);
                slot.GarbageQueue.Clear();
                slot.ConstructionProgress.Clear();
                slot.ComfortStreak.Clear();
                slot.TheftCounter = 0;
                string file = Path.Combine(dir, "legacy" + version + ".json");
                Assert.IsTrue(SlotTransfer.Export(file, slot), "v" + version);
                CitySnapshot restored = SlotTransfer.Import(file);
                Assert.IsNotNull(restored, "v" + version);
                Assert.AreEqual(version, restored.Version, "v" + version);
                Assert.AreEqual(0, SaveV6Snapshot.ReadThefts(restored), "v" + version);
                Assert.AreEqual(0, SaveV6Snapshot.ReadGarbage(restored).Count, "v" + version);
            }
        }

        [Test]
        public void FutureVersion_ExportAndImport_Rejected()
        {
            CitySnapshot slot = Sample(7);
            slot.Version = 8;
            string file = SlotTransfer.DefaultPath(dir);
            Assert.IsFalse(SlotTransfer.Export(file, slot));
            Assert.IsFalse(File.Exists(file));
            File.WriteAllText(file, SaveSlotStorage.ToJson(slot));
            Assert.IsNull(SlotTransfer.Import(file));
        }

        [Test]
        public void BrokenCoreList_ImportRejected()
        {
            CitySnapshot slot = Sample(6);
            slot.Zones[0].X = -1;
            string file = SlotTransfer.DefaultPath(dir);
            File.WriteAllText(file, SaveSlotStorage.ToJson(slot));
            Assert.IsNull(SlotTransfer.Import(file));
        }

        [Test]
        public void BrokenV6Lists_ImportNormalizesWithoutLosingValidData()
        {
            CitySnapshot slot = Sample(6);
            slot.TheftCounter = -3;
            slot.GarbageQueue.Add(new CitySnapshot.GarbageQueueEntry { X = 3, Y = 3, Accumulated = 8 });
            slot.GarbageQueue.Add(new CitySnapshot.GarbageQueueEntry { X = -1, Y = 0, Accumulated = 4 });
            string file = SlotTransfer.DefaultPath(dir);
            File.WriteAllText(file, SaveSlotStorage.ToJson(slot));
            CitySnapshot restored = SlotTransfer.Import(file);
            Assert.IsNotNull(restored);
            Assert.AreEqual(0, restored.TheftCounter);
            Assert.AreEqual(1, restored.GarbageQueue.Count);
            Assert.AreEqual(8, restored.GarbageQueue[0].Accumulated);
        }

        private static CitySnapshot Sample(int version)
        {
            CitySnapshot slot = new CitySnapshot();
            slot.Version = version;
            slot.TickCount = 24;
            slot.Balance = 100;
            slot.TaxLevel = 2;
            slot.GoalStage = 1;
            slot.Zones.Add(new CitySnapshot.ZoneEntry { X = 0, Y = 0, Type = (int)ZoneType.Residential });
            slot.Zones.Add(new CitySnapshot.ZoneEntry { X = 1, Y = 1, Type = (int)ZoneType.Commercial });
            slot.Buildings.Add(new CitySnapshot.BuildingEntry { DefId = "house", X = 0, Y = 0, Level = 2 });
            slot.Households.Add(new CitySnapshot.HouseholdEntry { BuildingIndex = 0, Size = 3 });
            slot.Roads.Add(new CitySnapshot.RoadEntry { X = 2, Y = 2 });
            slot.TheftCounter = 5;
            slot.GarbageQueue.Add(new CitySnapshot.GarbageQueueEntry { X = 3, Y = 3, Accumulated = 9 });
            slot.ConstructionProgress.Add(new CitySnapshot.ConstructionProgressEntry
                { X = 4, Y = 4, DefId = "shop", TicksElapsed = 2 });
            return slot;
        }
    }
}
