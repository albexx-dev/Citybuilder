using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Геометрия тултипов кнопок (EPIC-66): чистая функция TooltipRect без IMGUI.
    // Тултип лежит над кнопкой и не выходит за края окна на любом размере экрана.
    public class TooltipLayoutTests
    {
        private static Rect MenuButtonAt(float width, float height)
        {
            Rect group = SettingsLayout.MenuButtonsRect(width, height);
            return SettingsLayout.MenuButtonRect(group, 0);
        }

        [TestCase(1024f, 600f)]
        [TestCase(1280f, 700f)]
        [TestCase(1920f, 1080f)]
        [TestCase(2560f, 1440f)]
        public void Tooltip_SitsAboveButton_AndInsideWindow(float width, float height)
        {
            Rect button = MenuButtonAt(width, height);
            Rect tip = BottomBarStyle.TooltipRect(button, 160f, 22f, width, height);
            Assert.LessOrEqual(tip.yMax, button.y + 0.01f, "Тултип должен быть над кнопкой");
            Assert.GreaterOrEqual(tip.x, 0f);
            Assert.GreaterOrEqual(tip.y, 0f);
            Assert.LessOrEqual(tip.xMax, width + 0.01f);
        }

        [Test]
        public void Tooltip_RightButton_ClampsToRightEdge()
        {
            Rect button = MenuButtonAt(1024f, 600f);
            Rect tip = BottomBarStyle.TooltipRect(button, 400f, 22f, 1024f, 600f);
            Assert.LessOrEqual(tip.xMax, 1024f + 0.01f);
            Assert.Greater(tip.width, 0f);
        }

        [Test]
        public void Tooltip_AtTopOfScreen_ClampsToZeroY()
        {
            Rect button = new Rect(100f, 2f, 84f, 28f);
            Rect tip = BottomBarStyle.TooltipRect(button, 160f, 22f, 1024f, 600f);
            Assert.AreEqual(0f, tip.y, 0.01f);
        }
    }
}
