using NUnit.Framework;

namespace Citybuilder.Tests
{
    // Тесты политики туториала v3 (EPIC-70): матрица переходов, тексты-подсказки
    // с точными названиями кнопок и состояние «тип уже выбран».
    public partial class TutorialPolicyTests
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

        [Test]
        public void EarlyStepTexts_NameExactButtons()
        {
            Assert.IsTrue(TutorialPolicy.FormatText(TutorialPolicy.Step.PaintZones).Contains("Зоны [3]"));
            Assert.IsTrue(TutorialPolicy.FormatText(TutorialPolicy.Step.EnableGrowth).Contains("Рост [4]"));
            Assert.IsTrue(TutorialPolicy.FormatText(TutorialPolicy.Step.WaitForHouse).Contains("x4"));
        }

        // Регрессия на UX-провал v2: владелец не понял, что станцию надо выбрать в палитре.
        [Test]
        public void PowerStepText_MentionsPaletteAboveTheBar()
        {
            string text = TutorialPolicy.FormatText(TutorialPolicy.Step.BuildPower);
            Assert.IsTrue(text.Contains("палитре"), text);
            Assert.IsTrue(text.Contains("НАД ПАНЕЛЬЮ"), text);
            Assert.IsTrue(text.Contains(Localization.T("building_power_station")), text);
            Assert.IsTrue(text.Contains("Здания [1]"), text);
        }

        [Test]
        public void EveryBuildStep_NamesPaletteAndExactDefinition()
        {
            AssertPaletteStep(TutorialPolicy.Step.BuildWater, Localization.T("building_water_tower"));
            AssertPaletteStep(TutorialPolicy.Step.BuildWaste, Localization.T("building_waste_depot"));
            AssertPaletteStep(TutorialPolicy.Step.BuildPark, Localization.T("building_park"));
        }

        private static void AssertPaletteStep(TutorialPolicy.Step step, string displayName)
        {
            string text = TutorialPolicy.FormatText(step);
            Assert.IsTrue(text.Contains("палитре"), step + ": " + text);
            Assert.IsTrue(text.Contains("НАД ПАНЕЛЬЮ"), step + ": " + text);
            Assert.IsTrue(text.Contains("Здания [1]"), step + ": " + text);
            Assert.IsTrue(text.Contains(displayName), step + ": " + text);
        }

        // RU-подсказки называют типы по-русски, имя берётся из реестра локализации (эпик 97).
        [Test]
        public void BuildStepTexts_UseLocalizedRussianTypeNames()
        {
            Language previous = Localization.Language;
            Localization.SetLanguage(Language.Ru);
            Assert.IsTrue(TutorialPolicy.FormatText(TutorialPolicy.Step.BuildPower).Contains("Электростанция"));
            Assert.IsTrue(TutorialPolicy.FormatText(TutorialPolicy.Step.BuildWater).Contains("Водонапорная башня"));
            Assert.IsTrue(TutorialPolicy.FormatText(TutorialPolicy.Step.BuildWaste).Contains("Мусорный склад"));
            Assert.IsTrue(TutorialPolicy.FormatText(TutorialPolicy.Step.BuildPark).Contains("Парк"));
            Localization.SetLanguage(previous);
        }

        [Test]
        public void BuildSteps_TargetExactPaletteDefinitions()
        {
            Assert.AreEqual("power_station", TutorialPolicy.TargetBuildingId(TutorialPolicy.Step.BuildPower));
            Assert.AreEqual("water_tower", TutorialPolicy.TargetBuildingId(TutorialPolicy.Step.BuildWater));
            Assert.AreEqual("waste_depot", TutorialPolicy.TargetBuildingId(TutorialPolicy.Step.BuildWaste));
            Assert.AreEqual("park", TutorialPolicy.TargetBuildingId(TutorialPolicy.Step.BuildPark));
            Assert.IsNull(TutorialPolicy.TargetBuildingId(TutorialPolicy.Step.WaitForHouse));
        }

        [Test]
        public void HighlightTargets_FollowBottomBarButtons()
        {
            Assert.AreEqual(TutorialPolicy.TargetButton.Zones, TutorialPolicy.HighlightTarget(TutorialPolicy.Step.PaintZones));
            Assert.AreEqual(TutorialPolicy.TargetButton.Growth, TutorialPolicy.HighlightTarget(TutorialPolicy.Step.EnableGrowth));
            Assert.AreEqual(TutorialPolicy.TargetButton.Speed4, TutorialPolicy.HighlightTarget(TutorialPolicy.Step.WaitForHouse));
            Assert.AreEqual(TutorialPolicy.TargetButton.Houses, TutorialPolicy.HighlightTarget(TutorialPolicy.Step.BuildPower));
            Assert.AreEqual(TutorialPolicy.TargetButton.None, TutorialPolicy.HighlightTarget(TutorialPolicy.Step.Done));
        }

    }
}
