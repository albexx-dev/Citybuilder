using System;
using System.IO;
using NUnit.Framework;

namespace Citybuilder
{
    // Валидация полей v5 и откат чтения на TaxLevel: битый слот не проходит и не врёт при чтении.
    public class SnapshotV5ValidationTests
    {
        [Test]
        public void Validate_RejectsLevelOutOfRange_InBothBuildingLists()
        {
            Assert.IsFalse(SaveSlotStorage.Validate(SlotWithLevel(false, 7)), "Buildings level 7");
            Assert.IsFalse(SaveSlotStorage.Validate(SlotWithLevel(false, -1)), "Buildings level -1");
            Assert.IsFalse(SaveSlotStorage.Validate(SlotWithLevel(true, 9)), "ManualBuildings level 9");
            Assert.IsFalse(SaveSlotStorage.Validate(SlotWithLevel(true, -1)), "ManualBuildings level -1");
            Assert.IsTrue(SaveSlotStorage.Validate(SlotWithLevel(false, 3)), "Buildings level 3");
            Assert.IsTrue(SaveSlotStorage.Validate(SlotWithLevel(true, 0)), "ManualBuildings 0 = слот v4");
        }

        [Test]
        public void Validate_RejectsBrokenTaxRates()
        {
            Assert.IsFalse(SaveSlotStorage.Validate(SlotWithRates(new int[] { 1, 3 })), "два значения");
            Assert.IsFalse(SaveSlotStorage.Validate(SlotWithRates(new int[] { 1, 9, 2 })), "значение 9");
            Assert.IsFalse(SaveSlotStorage.Validate(SlotWithRates(new int[] { 0, 1, 2 })), "значение 0");
            Assert.IsTrue(SaveSlotStorage.Validate(SlotWithRates(new int[] { 1, 2, 3 })), "три ставки 1..3");
        }

        // Непустой, но битый набор ставок не читается по зонам: все три зоны берутся из TaxLevel.
        [Test]
        public void Read_BrokenRates_FallBackToTaxLevel()
        {
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.TaxLevel = 3;
            snapshot.TaxRates = new int[] { 9, 9, 9 };
            AssertSameRate(SnapshotTaxRates.Read(snapshot), TaxPolicy.Level.High);
            snapshot.TaxRates = new int[] { 3, 3 };
            AssertSameRate(SnapshotTaxRates.Read(snapshot), TaxPolicy.Level.High);
            snapshot.TaxLevel = 1;
            snapshot.TaxRates = null;
            AssertSameRate(SnapshotTaxRates.Read(snapshot), TaxPolicy.Level.Low);
        }

        // Файл слота v4 на диске: новые поля его не отбраковывают, ставки читаются из TaxLevel.
        [Test]
        public void Load_LegacyV4FileOnDisk_StillLoads()
        {
            string dir = Path.Combine(Path.GetTempPath(), "CitybuilderV5_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, SaveSlotStorage.SlotFileName),
                "{\"Version\":4,\"TaxLevel\":3,\"Buildings\":[{\"DefId\":\"house\",\"X\":1,\"Y\":2}]}");
            CitySnapshot loaded = SaveSlotStorage.Load(dir, 0);
            Directory.Delete(dir, true);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(0, loaded.Buildings[0].Level);
            AssertSameRate(SnapshotTaxRates.Read(loaded), TaxPolicy.Level.High);
        }

        private static void AssertSameRate(SnapshotTaxRates.Levels levels, TaxPolicy.Level expected)
        {
            Assert.AreEqual(expected, levels.Residential);
            Assert.AreEqual(expected, levels.Commercial);
            Assert.AreEqual(expected, levels.Industrial);
        }

        private static CitySnapshot SlotWithLevel(bool manual, int level)
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null);
            CitySnapshot.BuildingEntry entry = new CitySnapshot.BuildingEntry { DefId = "house", X = 1, Y = 1, Level = level };
            if (manual) snapshot.ManualBuildings.Add(entry);
            else snapshot.Buildings.Add(entry);
            return snapshot;
        }

        private static CitySnapshot SlotWithRates(int[] rates)
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null);
            snapshot.TaxRates = rates;
            return snapshot;
        }
    }
}
