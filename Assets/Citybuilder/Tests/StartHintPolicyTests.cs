using NUnit.Framework;

namespace Citybuilder.Tests
{
    public class StartHintPolicyTests
    {
        [Test]
        public void NoZonesAsksToPaint()
        {
            Assert.AreEqual(StartHintPolicy.Step.PaintZones,
                StartHintPolicy.Pick(0, 0, false));
        }

        [Test]
        public void ZonesWithoutGrowthModeAskToEnableGrowth()
        {
            Assert.AreEqual(StartHintPolicy.Step.EnableGrowth,
                StartHintPolicy.Pick(16, 0, false));
        }

        [Test]
        public void ZonesAndGrowthModeWaitForFirstHouse()
        {
            Assert.AreEqual(StartHintPolicy.Step.WaitForHouse,
                StartHintPolicy.Pick(16, 0, true));
        }

        [Test]
        public void GrowthModeWithoutZonesStillAsksToPaint()
        {
            Assert.AreEqual(StartHintPolicy.Step.PaintZones,
                StartHintPolicy.Pick(0, 0, true));
        }

        [Test]
        public void FirstGrownHouseHidesHint()
        {
            Assert.AreEqual(StartHintPolicy.Step.Done,
                StartHintPolicy.Pick(16, 1, true));
        }
    }
}
