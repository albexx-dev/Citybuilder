using NUnit.Framework;

namespace Citybuilder
{
    // Верхний левый угол HUD: ярусы денег, населения, бюджета, часов и кнопки RU/EN.
    public class TopLeftLayoutTests
    {
        [Test]
        public void TopLeftRows_UseFixedYCoordinates()
        {
            Assert.AreEqual(HudLayout.TopLeftX, 10f, 0.01f);
            Assert.AreEqual(HudLayout.TopLeftY, HudLayout.TopLeftRowY(0), 0.01f);
            Assert.AreEqual(36f, HudLayout.TopLeftRowY(1), 0.01f);
            Assert.AreEqual(62f, HudLayout.TopLeftRowY(2), 0.01f);
            Assert.AreEqual(88f, HudLayout.TopLeftRowY(3), 0.01f);
            Assert.AreEqual(114f, HudLayout.TopLeftRowY(4), 0.01f);
        }

        [Test]
        public void TopLeftRows_LeaveRoomForBackingAndGap()
        {
            const float gap = 2f;
            for (int i = 0; i < 4; i++)
            {
                float step = HudLayout.TopLeftRowY(i + 1) - HudLayout.TopLeftRowY(i);
                Assert.IsTrue(step >= HudText.BackingRowHeight + gap, "row " + i + ": " + step);
            }
        }
    }
}
