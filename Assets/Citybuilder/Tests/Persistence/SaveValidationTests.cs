using NUnit.Framework;

namespace Citybuilder
{
    // Границы слота v4: координаты, налог и ступень цели не должны выходить из допустимых значений.
    public class SaveValidationTests
    {
        private static CitySnapshot ValidSnapshot()
        {
            return SaveSystem.BuildSnapshot(null, null, null, 100, 5, null, 2, null, 1);
        }

        [Test]
        public void Validate_AcceptsCurrentSnapshot()
        {
            Assert.IsTrue(SaveSlotStorage.Validate(ValidSnapshot()));
        }

        [Test]
        public void Validate_RejectsZoneOutsideMap()
        {
            CitySnapshot snapshot = ValidSnapshot();
            snapshot.Zones.Add(new CitySnapshot.ZoneEntry { X = 50, Y = 0, Type = (int)ZoneType.Residential });
            Assert.IsFalse(SaveSlotStorage.Validate(snapshot));
        }

        [Test]
        public void Validate_RejectsNegativeRoadCoordinate()
        {
            CitySnapshot snapshot = ValidSnapshot();
            snapshot.Roads.Add(new CitySnapshot.RoadEntry { X = -1, Y = 0 });
            Assert.IsFalse(SaveSlotStorage.Validate(snapshot));
        }

        [Test]
        public void Validate_RejectsBuildingOutsideMap()
        {
            CitySnapshot snapshot = ValidSnapshot();
            snapshot.Buildings.Add(new CitySnapshot.BuildingEntry { DefId = "House", X = 0, Y = 50 });
            Assert.IsFalse(SaveSlotStorage.Validate(snapshot));
        }

        [Test]
        public void Validate_RejectsHouseholdHomeOutsideMap()
        {
            CitySnapshot snapshot = ValidSnapshot();
            snapshot.Buildings.Add(new CitySnapshot.BuildingEntry { DefId = "House", X = 1, Y = 1 });
            snapshot.Households.Add(new CitySnapshot.HouseholdEntry
            {
                BuildingIndex = 0,
                Size = 2,
                HasHome = true,
                HomeX = 99,
                HomeY = 0
            });
            Assert.IsFalse(SaveSlotStorage.Validate(snapshot));
        }

        [Test]
        public void Validate_RejectsUnknownTaxLevel()
        {
            CitySnapshot snapshot = ValidSnapshot();
            snapshot.TaxLevel = 4;
            Assert.IsFalse(SaveSlotStorage.Validate(snapshot));
        }

        [Test]
        public void Validate_RejectsUnknownGoalStage()
        {
            CitySnapshot snapshot = ValidSnapshot();
            snapshot.GoalStage = 4;
            Assert.IsFalse(SaveSlotStorage.Validate(snapshot));
        }
    }
}
