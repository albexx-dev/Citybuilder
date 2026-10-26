using NUnit.Framework;

namespace Citybuilder.Tests
{
    public class GrowthStatusPolicyTests
    {
        [Test]
        public void NoHousesMeansNoStatus()
        {
            Assert.AreEqual(GrowthStatusPolicy.Kind.None,
                GrowthStatusPolicy.Pick(0, 16, true, 30, 100, 1000));
        }

        [Test]
        public void GrowthModeOffIsReportedFirst()
        {
            Assert.AreEqual(GrowthStatusPolicy.Kind.GrowthOff,
                GrowthStatusPolicy.Pick(3, 16, false, 30, 100, 0));
        }

        [Test]
        public void NoResidentialCellsBeatsCapacity()
        {
            Assert.AreEqual(GrowthStatusPolicy.Kind.NoZones,
                GrowthStatusPolicy.Pick(30, 0, true, 30, 100, 1000));
        }

        [Test]
        public void CapacityStopsGrowth()
        {
            Assert.AreEqual(GrowthStatusPolicy.Kind.AtCapacity,
                GrowthStatusPolicy.Pick(30, 16, true, 30, 100, 1000));
        }

        [Test]
        public void NotEnoughMoneyStopsGrowth()
        {
            Assert.AreEqual(GrowthStatusPolicy.Kind.NoMoney,
                GrowthStatusPolicy.Pick(3, 16, true, 30, 100, 99));
        }

        [Test]
        public void AffordableAndZonedMeansGrowing()
        {
            Assert.AreEqual(GrowthStatusPolicy.Kind.Growing,
                GrowthStatusPolicy.Pick(3, 16, true, 30, 100, 100));
        }

        [Test]
        public void HousesWordFollowsRussianPlural()
        {
            Assert.AreEqual("дом", GrowthStatusPolicy.HousesWord(1));
            Assert.AreEqual("дома", GrowthStatusPolicy.HousesWord(3));
            Assert.AreEqual("домов", GrowthStatusPolicy.HousesWord(5));
            Assert.AreEqual("домов", GrowthStatusPolicy.HousesWord(11));
            Assert.AreEqual("дом", GrowthStatusPolicy.HousesWord(21));
            Assert.AreEqual("дома", GrowthStatusPolicy.HousesWord(22));
        }
    }
}
