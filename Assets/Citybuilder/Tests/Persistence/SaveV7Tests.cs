using System.IO;
using NUnit.Framework;

namespace Citybuilder
{
    // Сейв v7 (ADR-022): версия 7 на записи, чтение 1–7, fallback v6 и round-trip полей.
    public class SaveV7Tests
    {
        [Test]
        public void Version_WriteIsSeven_ReadOneToSevenAndRejectFuture()
        {
            Assert.AreEqual(7, SaveSystem.CurrentVersion);
            Assert.AreEqual(7, SaveSystem.BuildSnapshot(null, null, null, 5, 1, null).Version);
            for (int version = 1; version <= 7; version++)
            {
                CitySnapshot slot = Sample(); slot.Version = version;
                CitySnapshot restored = SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(slot));
                Assert.IsTrue(SaveSlotStorage.Validate(restored), "version=" + version);
            }
            CitySnapshot future = Sample(); future.Version = 8;
            CitySnapshot zero = Sample(); zero.Version = 0;
            CitySnapshot negative = Sample(); negative.Version = -1;
            Assert.IsFalse(SaveSlotStorage.Validate(future));
            Assert.IsFalse(SaveSlotStorage.Validate(zero));
            Assert.IsFalse(SaveSlotStorage.Validate(negative));
        }
        [Test]
        public void RoundTrip_V7_PreservesAllOptionalFields()
        {
            CitySnapshot slot = Sample();
            CitySnapshot restored = SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(slot));
            Assert.AreEqual(7, restored.Version);
            Assert.AreEqual("wood", SaveV7Snapshot.ReadRoadType(restored.Roads[0]));
            Assert.AreEqual(2, SaveV7Snapshot.ReadRoadLevel(restored.Roads[0]));
            Assert.AreEqual(12.5f, restored.Debt, 0.001f);
            Assert.AreEqual(7.25f, restored.DebtInterestPercent, 0.001f);
            Assert.AreEqual(1.4f, restored.MarketMultiplier, 0.001f);
            Assert.AreEqual("downtown", restored.DistrictTaxRates[0].DistrictId);
            Assert.AreEqual(3, restored.DistrictTaxRates[0].Rate);
            Assert.AreEqual(42, restored.MayorRating);
            Assert.AreEqual(6, restored.MilestoneCounterV2);
        }
        // Старый слот без полей v7 остаётся валидным и читается с дефолтами v6.
        [Test]
        public void LegacyV6_WithoutV7Fields_ReadsWithDefaults()
        {
            CitySnapshot slot = SaveSlotStorage.FromJson("{\"Version\":6,\"Balance\":7,"
                + "\"Roads\":[{\"X\":1,\"Y\":2}]}");
            Assert.IsTrue(SaveSlotStorage.Validate(slot));
            SaveV6Snapshot.Normalize(slot);
            Assert.AreEqual("asphalt", SaveV7Snapshot.ReadRoadType(slot.Roads[0]));
            Assert.AreEqual(1, SaveV7Snapshot.ReadRoadLevel(slot.Roads[0]));
            Assert.AreEqual(0f, slot.Debt, 0.001f);
            Assert.AreEqual(0f, slot.DebtInterestPercent, 0.001f);
            Assert.AreEqual(1f, slot.MarketMultiplier, 0.001f);
            Assert.AreEqual(0, slot.DistrictTaxRates.Count);
            Assert.AreEqual(0, slot.MayorRating);
            Assert.AreEqual(0, slot.MilestoneCounterV2);
        }
        // Битые значения и дубли получают дефолты; повторная нормализация не меняет счётчик.
        [Test]
        public void Normalize_BrokenAndDuplicates_NoExceptionAndIdempotent()
        {
            CitySnapshot slot = Sample();
            slot.Roads.Add(new CitySnapshot.RoadEntry { X = 1, Y = 2, Type = "unknown", Level = 7 });
            slot.Roads.Add(new CitySnapshot.RoadEntry { X = 1, Y = 2, Type = "", Level = 0 });
            slot.Debt = float.NaN; slot.DebtInterestPercent = 150f; slot.MarketMultiplier = -2f;
            slot.MayorRating = 101; slot.MilestoneCounterV2 = -3; slot.DistrictTaxRates.Add(null);
            slot.DistrictTaxRates.Add(new CitySnapshot.DistrictTaxEntry { DistrictId = "downtown", Rate = 0 });
            slot.DistrictTaxRates.Add(new CitySnapshot.DistrictTaxEntry { DistrictId = "downtown", Rate = 2 });
            Assert.DoesNotThrow(delegate { SaveV6Snapshot.Normalize(slot); });
            Assert.AreEqual(2, slot.Roads.Count);
            Assert.AreEqual("asphalt", SaveV7Snapshot.ReadRoadType(slot.Roads[1]));
            Assert.AreEqual(1, SaveV7Snapshot.ReadRoadLevel(slot.Roads[1]));
            Assert.AreEqual(0f, slot.Debt, 0.001f);
            Assert.AreEqual(0f, slot.DebtInterestPercent, 0.001f);
            Assert.AreEqual(1f, slot.MarketMultiplier, 0.001f);
            Assert.AreEqual(0, slot.MayorRating);
            Assert.AreEqual(1, slot.DistrictTaxRates.Count);
            Assert.AreEqual(2, slot.DistrictTaxRates[0].Rate);
            Assert.AreEqual(0, slot.MilestoneCounterV2);
            slot.MilestoneCounterV2 = 6;
            SaveV6Snapshot.Normalize(slot);
            Assert.AreEqual(6, slot.MilestoneCounterV2);
        }
        [Test]
        public void SlotTransfer_ExportImport_PreservesV7AndRejectsFuture()
        {
            string path = Path.Combine(Path.GetTempPath(), "cb-v145-07-export.json");
            try
            {
                Assert.IsTrue(SlotTransfer.Export(path, Sample()));
                CitySnapshot restored = SlotTransfer.Import(path);
                Assert.IsNotNull(restored);
                Assert.AreEqual(3, SaveV7Snapshot.ReadRoadLevel(restored.Roads[0]));
                Assert.AreEqual("downtown", restored.DistrictTaxRates[0].DistrictId);
                Assert.AreEqual(6, restored.MilestoneCounterV2);
                CitySnapshot future = Sample(); future.Version = 8;
                Assert.IsFalse(SlotTransfer.Export(path, future));
                File.WriteAllText(path, SaveSlotStorage.ToJson(future));
                Assert.IsNull(SlotTransfer.Import(path));
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }
        private static CitySnapshot Sample()
        {
            CitySnapshot slot = SaveSystem.BuildSnapshot(null, null, null, 5, 1, null);
            slot.Roads.Add(new CitySnapshot.RoadEntry { X = 1, Y = 2, Type = "wood", Level = 2 });
            slot.Debt = 12.5f; slot.DebtInterestPercent = 7.25f; slot.MarketMultiplier = 1.4f;
            slot.DistrictTaxRates.Add(new CitySnapshot.DistrictTaxEntry { DistrictId = "downtown", Rate = 3 });
            slot.MayorRating = 42; slot.MilestoneCounterV2 = 6;
            return slot;
        }
    }
}
