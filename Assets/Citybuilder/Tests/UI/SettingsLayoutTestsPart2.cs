using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса SettingsLayoutTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class SettingsLayoutTests
    {
        [TestCase(800f, 600f)]
        [TestCase(640f, 480f)]
        public void CompactTutorialBanner_StaysInsideScreen(float width, float height)
        {
            Rect banner = SettingsLayout.CompactBannerRect(width, height);
            Rect button = SettingsLayout.CompactDismissRect(width, height);
            Assert.GreaterOrEqual(banner.x, 0f);
            Assert.Greater(banner.width, 0f);
            Assert.LessOrEqual(banner.xMax, button.x);
            Assert.LessOrEqual(button.xMax, width);
            Assert.GreaterOrEqual(button.y, 0f);
            Assert.LessOrEqual(button.yMax, height);
        }
        // Регресс 1024×600: компактный баннер больше не перекрывает квадрат мини-карты.
        [TestCase(1024f, 600f)]
        [TestCase(800f, 600f)]
        public void CompactTutorialBanner_ClearsMinimap(float width, float height)
        {
            Rect map = HudLayout.MinimapRect(width, height);
            Rect banner = SettingsLayout.CompactBannerRect(width, height);
            Rect button = SettingsLayout.CompactDismissRect(width, height);
            Assert.GreaterOrEqual(banner.y, map.yMax);
            Assert.GreaterOrEqual(button.y, map.yMax);
            Assert.LessOrEqual(button.yMax, height);
        }
    }
}
