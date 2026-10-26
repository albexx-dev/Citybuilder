using NUnit.Framework;

namespace Citybuilder
{
    // Налог в слоте: TaxLevel, версия 2 и совместимость со слотом v1. Без сцены и файлов.
    public class TaxSaveTests
    {
        [Test]
        public void Snapshot_Defaults_ZeroTaxAndVersionTwo()
        {
            CitySnapshot snapshot = new CitySnapshot();
            Assert.AreEqual(0, snapshot.TaxLevel);
            Assert.AreEqual(2, snapshot.Version);
        }

        [Test]
        public void Json_Roundtrip_KeepsHighTax()
        {
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.TaxLevel = 3;
            CitySnapshot restored = SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(snapshot));
            Assert.AreEqual(3, restored.TaxLevel);
            Assert.AreEqual(2, restored.Version);
            Assert.IsTrue(SaveSlotStorage.Validate(restored));
        }

        [Test]
        public void BuildSnapshot_NoTaxOverload_WritesZero()
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 100, 5, null);
            Assert.AreEqual(0, snapshot.TaxLevel);
            Assert.AreEqual(2, snapshot.Version);
        }

        [Test]
        public void BuildSnapshot_WithTax_WritesLevel()
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 100, 5, null, 3);
            Assert.AreEqual(3, snapshot.TaxLevel);
        }

        [Test]
        public void BuildSnapshot_OutOfRangeTax_WritesZero()
        {
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null, 99);
            Assert.AreEqual(0, snapshot.TaxLevel);
        }

        [Test]
        public void Encode_AllLevels_MapToOneToThree()
        {
            Assert.AreEqual(1, SaveSystem.EncodeTaxLevel(TaxPolicy.Level.Low));
            Assert.AreEqual(2, SaveSystem.EncodeTaxLevel(TaxPolicy.Level.Normal));
            Assert.AreEqual(3, SaveSystem.EncodeTaxLevel(TaxPolicy.Level.High));
        }

        [Test]
        public void Decode_MapsSlotValues()
        {
            Assert.AreEqual(TaxPolicy.Level.Normal, SaveSystem.DecodeTaxLevel(0));
            Assert.AreEqual(TaxPolicy.Level.Low, SaveSystem.DecodeTaxLevel(1));
            Assert.AreEqual(TaxPolicy.Level.Normal, SaveSystem.DecodeTaxLevel(2));
            Assert.AreEqual(TaxPolicy.Level.High, SaveSystem.DecodeTaxLevel(3));
            Assert.AreEqual(TaxPolicy.Level.Normal, SaveSystem.DecodeTaxLevel(99));
        }

        [Test]
        public void Validate_VersionOneSlot_PassesAsNormal()
        {
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.Version = 1;
            Assert.IsTrue(SaveSlotStorage.Validate(snapshot));
            Assert.AreEqual(0, snapshot.TaxLevel);
            Assert.AreEqual(TaxPolicy.Level.Normal, SaveSystem.DecodeTaxLevel(snapshot.TaxLevel));
        }

        [Test]
        public void Validate_VersionZero_Fails()
        {
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.Version = 0;
            Assert.IsFalse(SaveSlotStorage.Validate(snapshot));
        }
    }
}
