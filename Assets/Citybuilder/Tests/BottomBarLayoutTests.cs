using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Rect-тесты нижней панели (EPIC-13): группы не пересекаются в диапазоне 1024..2560 x 600..1440.
    public class BottomBarLayoutTests
    {
        [TestCase(1024f, 600f)]
        [TestCase(1280f, 600f)]
        [TestCase(1280f, 700f)]
        [TestCase(1280f, 800f)]
        [TestCase(1920f, 1080f)]
        [TestCase(2560f, 1440f)]
        public void Groups_DoNotOverlap_AndStayInsideBar(float width, float height)
        {
            Rect bar = HudLayout.BottomBarRect(width, height);
            Rect speed = HudLayout.SpeedGroupRect(width, height);
            Rect mode = HudLayout.ModeGroupRect(width, height);
            Rect menu = HudLayout.MenuButtonsRect(width, height);

            Assert.AreEqual(width, bar.width, 0.01f);
            Assert.AreEqual(height - HudLayout.BarHeight, bar.y, 0.01f);
            Assert.Less(speed.xMax, mode.x);
            Assert.Less(mode.xMax, menu.x);
            Assert.LessOrEqual(menu.xMax, bar.xMax);
            Assert.IsFalse(speed.Overlaps(mode));
            Assert.IsFalse(mode.Overlaps(menu));
        }

        [TestCase(1024f, 600f)]
        [TestCase(1920f, 1080f)]
        public void PalettePopup_StaysAboveBottomBar(float width, float height)
        {
            Rect bar = HudLayout.BottomBarRect(width, height);
            Rect popup = HudLayout.PaletteRect(width, height, 6);
            Assert.Greater(popup.width, 0f);
            Assert.LessOrEqual(popup.yMax, bar.y);
            Assert.IsFalse(popup.Overlaps(bar));
        }

        [Test]
        public void PalettePopup_ZeroItems_ReturnsEmptyRect()
        {
            Rect popup = HudLayout.PaletteRect(1920f, 1080f, 0);
            Assert.AreEqual(0f, popup.width);
        }

        [TestCase(600f)]
        [TestCase(700f)]
        [TestCase(800f)]
        [TestCase(1080f)]
        public void BottomHudRows_DoNotOverlapBottomBar(float height)
        {
            Rect bar = HudLayout.BottomBarRect(1280f, height);
            float y0 = HudLayout.FixedOrBottom(454f, 0, height);
            float y1 = HudLayout.FixedOrBottom(430f, 1, height);
            float y2 = HudLayout.FixedOrBottom(406f, 2, height);
            Assert.Less(y2, y1);
            Assert.Less(y1, y0);
            Assert.LessOrEqual(y0 + 24f, bar.y);
        }
    }
}
