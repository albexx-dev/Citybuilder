using NUnit.Framework;

namespace Citybuilder.Tests
{
    // Тесты многошаговой политики туториала v2 (EPIC-18 шаг 1, заменяет StartHintPolicyTests).
    public class TutorialPolicyTests
    {
        [Test]
        public void NoZonesAsksToPaint()
        {
            Assert.AreEqual(TutorialPolicy.Step.PaintZones,
                TutorialPolicy.PickEarly(0, 0, false));
        }

        [Test]
        public void ZonesWithoutGrowthModeAskToEnableGrowth()
        {
            Assert.AreEqual(TutorialPolicy.Step.EnableGrowth,
                TutorialPolicy.PickEarly(16, 0, false));
        }

        [Test]
        public void ZonesAndGrowthModeWaitForFirstHouse()
        {
            Assert.AreEqual(TutorialPolicy.Step.WaitForHouse,
                TutorialPolicy.PickEarly(16, 0, true));
        }

        [Test]
        public void GrowthModeWithoutZonesStillAsksToPaint()
        {
            Assert.AreEqual(TutorialPolicy.Step.PaintZones,
                TutorialPolicy.PickEarly(0, 0, true));
        }

        [Test]
        public void FullChain_AdvancesStepByStepThroughStationsAndParkToDone()
        {
            TutorialPolicy.State s = new TutorialPolicy.State
            {
                ResidentialCells = 16,
                GrowthMode = true,
                GrownCount = 1,
                ParkAvailable = true
            };

            Assert.AreEqual(TutorialPolicy.Step.BuildPower, TutorialPolicy.Pick(s));
            s.HasPowerStation = true;
            Assert.AreEqual(TutorialPolicy.Step.BuildWater, TutorialPolicy.Pick(s));
            s.HasWaterTower = true;
            Assert.AreEqual(TutorialPolicy.Step.BuildWaste, TutorialPolicy.Pick(s));
            s.HasWasteDepot = true;
            Assert.AreEqual(TutorialPolicy.Step.BuildPark, TutorialPolicy.Pick(s));
            s.HasPark = true;
            Assert.AreEqual(TutorialPolicy.Step.Done, TutorialPolicy.Pick(s));
        }

        [Test]
        public void DismissedOrBuiltEverythingAtOnce_CollapsesToDone()
        {
            TutorialPolicy.State dismissed = new TutorialPolicy.State { Dismissed = true };
            Assert.AreEqual(TutorialPolicy.Step.Done, TutorialPolicy.Pick(dismissed));

            TutorialPolicy.State allBuilt = new TutorialPolicy.State
            {
                GrownCount = 5,
                HasPowerStation = true,
                HasWaterTower = true,
                HasWasteDepot = true,
                ParkAvailable = false
            };
            Assert.AreEqual(TutorialPolicy.Step.Done, TutorialPolicy.Pick(allBuilt));
        }
    }
}
