using NUnit.Framework;

namespace Citybuilder
{
    // Хвост класса TutorialPolicyTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class TutorialPolicyTests
    {
        [Test]
        public void SelectedTargetType_SwitchesBuildStepTextToClickMap()
        {
            Assert.AreEqual(Localization.T("tutorial_select"),
                TutorialPolicy.FormatText(TutorialPolicy.Step.BuildPower, "power_station"));
        }

        // Новые ключи: RU совпадает со старым текстом, EN не содержит русских литералов.
        [Test]
        public void StepTexts_UseRegistry_InBothLanguages()
        {
            Language previous = Localization.Language;
            try
            {
                Localization.SetLanguage(Language.Ru);
                Assert.AreEqual(Localization.T("tutorial_step_1"), TutorialPolicy.FormatText(TutorialPolicy.Step.PaintZones));
                Localization.SetLanguage(Language.En);
                Assert.IsTrue(TutorialPolicy.FormatText(TutorialPolicy.Step.PaintZones).Contains("Zones [3]"));
                Assert.IsFalse(TutorialPolicy.FormatText(TutorialPolicy.Step.PaintZones).Contains("Зоны"));
                Assert.AreEqual(Localization.T("tutorial_select"), TutorialPolicy.FormatText(TutorialPolicy.Step.BuildPower, "power_station"));
            }
            finally { Localization.SetLanguage(previous); }
        }

        [Test]
        public void TypeSelected_OnlyWhenDefinitionMatchesStepTarget()
        {
            Assert.IsTrue(TutorialPolicy.IsTypeSelected(TutorialPolicy.Step.BuildWater, "water_tower"));
            Assert.IsFalse(TutorialPolicy.IsTypeSelected(TutorialPolicy.Step.BuildWater, "power_station"));
            Assert.IsFalse(TutorialPolicy.IsTypeSelected(TutorialPolicy.Step.BuildWater, null));
            Assert.IsFalse(TutorialPolicy.IsTypeSelected(TutorialPolicy.Step.BuildWater, ""));
            Assert.IsFalse(TutorialPolicy.IsTypeSelected(TutorialPolicy.Step.WaitForHouse, "water_tower"));
        }

        [Test]
        public void StateWithSelectedType_PicksSameStep_ButTextAsksToClickMap()
        {
            TutorialPolicy.State s = new TutorialPolicy.State
            {
                ResidentialCells = 16,
                GrowthMode = true,
                GrownCount = 1,
                SelectedBuildingId = "power_station"
            };
            Assert.AreEqual(TutorialPolicy.Step.BuildPower, TutorialPolicy.Pick(s));
            string text = TutorialPolicy.FormatText(TutorialPolicy.Pick(s), s.SelectedBuildingId);
            Assert.IsTrue(text.Contains("Тип выбран"), text);
        }
    }
}
