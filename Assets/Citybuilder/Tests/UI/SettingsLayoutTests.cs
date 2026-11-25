using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Rect-тесты окна настроек (EPIC-69): центр экрана, целиком внутри при минимальном
    // и базовом разрешении; математика степпера и подписи — без сцены.
    public partial class SettingsLayoutTests
    {
        private LanguageTestScope languageScope;

        [SetUp]
        public void InjectLanguageScope()
        {
            languageScope = new LanguageTestScope();
        }

        [TearDown]
        public void RestoreLanguageScope()
        {
            languageScope.Dispose();
        }
        [TestCase(1024f, 600f)]
        [TestCase(1920f, 1080f)]
        public void PanelRect_IsCenteredAndInsideScreen(float width, float height)
        {
            Rect panel = SettingsLayout.PanelRect(width, height);

            Assert.AreEqual((width - SettingsLayout.PanelWidth) * 0.5f, panel.x, 0.01f);
            Assert.AreEqual((height - SettingsLayout.PanelHeight) * 0.5f, panel.y, 0.01f);
            Assert.LessOrEqual(panel.xMax, width - HudLayout.BarPadding);
            Assert.LessOrEqual(panel.yMax, height);
        }
        // Эпик 97 и issue #123: кнопка повтора и три строки подсказок требуют высоты 480.
        [Test]
        public void PanelHeight_FitsRepeatButtonAndControlsHints()
        {
            Assert.AreEqual(480f, SettingsLayout.PanelHeight);
            Assert.LessOrEqual(SettingsLayout.PanelRect(1024f, 600f).yMax, 600f);
        }

        [Test]
        public void StepClamped_StepsInsideRange_AndStopsAtBorders()
        {
            Assert.AreEqual(48, SettingsLayout.StepClamped(40, SettingsLayout.AgentStep, GameSettings.MinAgentLimit, GameSettings.MaxAgentLimit));
            Assert.AreEqual(GameSettings.MinAgentLimit, SettingsLayout.StepClamped(GameSettings.MinAgentLimit, -SettingsLayout.AgentStep, GameSettings.MinAgentLimit, GameSettings.MaxAgentLimit));
            Assert.AreEqual(GameSettings.MaxAgentLimit, SettingsLayout.StepClamped(GameSettings.MaxAgentLimit - 4, SettingsLayout.AgentStep, GameSettings.MinAgentLimit, GameSettings.MaxAgentLimit));
            Assert.AreEqual(19, SettingsLayout.StepClamped(24, -SettingsLayout.AutosaveStep, GameSettings.MinAutosaveHours, GameSettings.MaxAutosaveHours));
            Assert.AreEqual(GameSettings.MinAutosaveHours, SettingsLayout.StepClamped(8, -SettingsLayout.AutosaveStep, GameSettings.MinAutosaveHours, GameSettings.MaxAutosaveHours));
        }

        [Test]
        public void Labels_FormatCurrentValues()
        {
            Assert.AreEqual("Громкость: 50%", SettingsLayout.VolumeLabel(0.5f));
            Assert.AreEqual("Машины: лимит 40", SettingsLayout.AgentLimitLabel(40));
            Assert.AreEqual("Автосейв: каждые 24 ч", SettingsLayout.AutosaveLabel(24));
        }

        [TestCase(1024f, 600f)]
        [TestCase(1280f, 800f)]
        [TestCase(1920f, 1080f)]
        public void MenuButtonsRect_FourButtons_FitNextToModeGroup(float width, float height)
        {
            Rect row = SettingsLayout.MenuButtonsRect(width, height);

            Assert.Less(HudLayout.ModeGroupRect(width, height).xMax, row.x);
            Assert.LessOrEqual(row.xMax, width - HudLayout.BarPadding);
            Assert.AreEqual(HudLayout.SpeedGroupRect(width, height).y, row.y, 0.01f);
        }

        [Test]
        public void MenuButtonRect_DividesRowEvenly_WithoutOverlap()
        {
            Rect row = SettingsLayout.MenuButtonsRect(1024f, 600f);
            Rect first = SettingsLayout.MenuButtonRect(row, 0);
            Rect second = SettingsLayout.MenuButtonRect(row, 1);
            Rect last = SettingsLayout.MenuButtonRect(row, SettingsLayout.MenuButtonCount - 1);

            Assert.AreEqual(row.x, first.x, 0.01f);
            Assert.IsFalse(first.Overlaps(second));
            Assert.LessOrEqual(last.xMax, row.xMax + 0.01f);
        }

        [Test]
        public void MenuButtonsRect_WideScreen_KeepsHudButtonWidth()
        {
            Rect row = SettingsLayout.MenuButtonsRect(1920f, 1080f);
            Assert.AreEqual(SettingsLayout.MenuButtonCount * HudLayout.MenuButtonWidth
                + (SettingsLayout.MenuButtonCount - 1) * HudLayout.ButtonGap, row.width, 0.01f);
        }

        [TestCase(800f, 600f)]
        [TestCase(640f, 480f)]
        [TestCase(400f, 320f)]
        public void PanelRect_SmallWindow_StaysInsideScreen(float width, float height)
        {
            Rect panel = SettingsLayout.PanelRect(width, height);
            Assert.GreaterOrEqual(panel.x, 0f);
            Assert.GreaterOrEqual(panel.y, 0f);
            Assert.LessOrEqual(panel.xMax, width);
            Assert.LessOrEqual(panel.yMax, height);
        }

        [TestCase(800f, 600f)]
        [TestCase(640f, 480f)]
        public void NarrowMenu_AllFourButtons_VisibleInsideScreen(float width, float height)
        {
            Assert.IsTrue(SettingsLayout.IsNarrow(width));
            for (int i = 0; i < SettingsLayout.MenuButtonCount; i++)
            {
                Rect button = SettingsLayout.MenuButtonRect(width, height, i);
                Assert.Greater(button.width, 0f);
                Assert.GreaterOrEqual(button.x, 0f);
                Assert.LessOrEqual(button.xMax, width + 0.01f);
                Assert.LessOrEqual(button.yMax, HudLayout.BottomBarRect(width, height).y);
            }
        }

        [TestCase(800f, 600f)]
        [TestCase(640f, 480f)]
        public void NarrowMenuRows_ClearBottomBarAndSubRow(float width, float height)
        {
            Rect bar = HudLayout.BottomBarRect(width, height);
            Rect subRow = HudLayout.PaletteRect(width, height, 1);
            for (int row = 0; row < SettingsLayout.MenuRowCount; row++)
            {
                Rect menu = SettingsLayout.MenuRowRect(width, height, row);
                Assert.LessOrEqual(menu.yMax, subRow.y);
                Assert.LessOrEqual(menu.yMax, HudLayout.CatalogTabsRect(width, height).y);
                Assert.LessOrEqual(menu.xMax, bar.xMax);
                Assert.GreaterOrEqual(menu.y, 0f);
            }
        }

    }
}
