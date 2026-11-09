using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Rect-тесты информационного меню (EPIC-14): не пересекается с нижней панелью и попапом палитры.
    public class InfoMenuLayoutTests
    {
        [TestCase(1024f, 600f)]
        [TestCase(1280f, 600f)]
        [TestCase(1280f, 700f)]
        [TestCase(1280f, 800f)]
        [TestCase(1920f, 1080f)]
        [TestCase(2560f, 1440f)]
        public void InfoMenu_DoesNotOverlapBottomBarOrPopup(float width, float height)
        {
            Rect menu = HudLayout.InfoMenuRect(width, height);
            Rect bar = HudLayout.BottomBarRect(width, height);
            Rect popup = HudLayout.PaletteRect(width, height, 6);

            Assert.Greater(menu.width, 300f);
            Assert.Greater(menu.height, 100f);
            Assert.LessOrEqual(menu.xMax, width);
            Assert.LessOrEqual(menu.yMax, popup.y);
            Assert.IsFalse(menu.Overlaps(bar));
            Assert.IsFalse(menu.Overlaps(popup));
        }

        [Test]
        public void LowWindow600_MenuScrollsAndStaysAboveBottomBar()
        {
            Rect menu = HudLayout.InfoMenuRect(1280f, 600f);
            Assert.Less(menu.height, 600f);
            Assert.IsTrue(HudLayout.IsHudRowVisible(454f, 0, 600f));
        }
    }
}
