using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Rect-тесты инфопанели в маленьком окне (issue #99): 7 строк помещаются в подложку.
    public class SmallWindowLayoutTests
    {
        [TestCase(800f, 600f)]
        [TestCase(640f, 480f)]
        public void InfoPanelFitsSevenLinesAboveBar(float width, float height)
        {
            Rect bar = HudLayout.BottomBarRect(width, height);
            Rect panel = HudLayout.InfoPanelRect(width, height);
            Assert.GreaterOrEqual(panel.height, 8f + 6f * 24f + 20f);
            Assert.LessOrEqual(panel.yMax, bar.y - HudLayout.SubRowOffset);
            Assert.LessOrEqual(panel.xMax, width);
        }
    }
}
