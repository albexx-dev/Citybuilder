using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Rect-тесты окна задач (EPIC-48): та же зона, что инфоменю и лента, — не наезжает
    // на нижнюю панель и её подстроку при минимальной и базовой ширине окна.
    public class QuestLogLayoutTests
    {
        [TestCase(1024f, 768f)]
        [TestCase(1920f, 1080f)]
        public void QuestLogRect_StaysAboveBottomBar(float width, float height)
        {
            Rect panel = HudLayout.QuestLogRect(width, height);
            Rect bar = HudLayout.BottomBarRect(width, height);

            Assert.LessOrEqual(panel.yMax, bar.y - HudLayout.SubRowOffset);
            Assert.GreaterOrEqual(panel.y, HudLayout.InfoMenuTop);
            Assert.LessOrEqual(panel.xMax, width - HudLayout.BarPadding);
        }

        [TestCase(1024f, 768f)]
        [TestCase(1920f, 1080f)]
                public void MenuButtonsRect_WithQuestsButton_FitsInsideBar(float width, float height)
        {
            Rect menu = SettingsLayout.MenuButtonsRect(width, height);

            // На узких окнах ряд сжимается — ширина не больше натуральной, но панель не наезжает.
            float natural = SettingsLayout.MenuButtonCount * HudLayout.MenuButtonWidth
                + (SettingsLayout.MenuButtonCount - 1) * HudLayout.ButtonGap;
            Assert.LessOrEqual(menu.width, natural, 0.01f);
            Assert.LessOrEqual(menu.xMax, width - HudLayout.BarPadding);
            Assert.Less(HudLayout.ModeGroupRect(width, height).xMax, menu.x);
        }
    }
}
