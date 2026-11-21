using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Геометрия тултипов кнопок (EPIC-66) и задержка показа (хвост #66): чистые функции
    // TooltipRect / TooltipShown / Track без IMGUI. Тултип лежит над кнопкой, не выходит
    // за края окна и показывается только после TooltipHoverPolicy.HoverDelaySeconds.
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

        [Test]
        public void TooltipShown_HoverUnder400ms_IsHidden()
        {
            Assert.IsFalse(TooltipHoverPolicy.TooltipShown(10f, 10.39f));
        }

        [Test]
        public void TooltipShown_HoverOver400ms_IsShown()
        {
            Assert.IsTrue(TooltipHoverPolicy.TooltipShown(10f, 10.41f));
            // Граница ровно 0.4 с: проверяем от нуля, чтобы не ловить ошибку float-вычитания.
            Assert.IsTrue(TooltipHoverPolicy.TooltipShown(0f, TooltipHoverPolicy.HoverDelaySeconds));
        }

        [Test]
        public void TooltipShown_NegativeStart_HasNoHover_IsHidden()
        {
            Assert.IsFalse(TooltipHoverPolicy.TooltipShown(-1f, 100f));
        }

        [Test]
        public void Track_ContinuousHover_WaitsDelayThenShows()
        {
            Dictionary<Rect, float> starts = new Dictionary<Rect, float>();
            Rect button = new Rect(100f, 500f, 84f, 28f);
            Vector2 mouse = new Vector2(120f, 510f);
            Assert.IsFalse(TooltipHoverPolicy.Track(starts, button, mouse, 10f));
            Assert.IsFalse(TooltipHoverPolicy.Track(starts, button, mouse, 10.3f));
            Assert.IsTrue(TooltipHoverPolicy.Track(starts, button, mouse, 10.5f));
        }

        [Test]
        public void Track_MouseLeavesButton_ResetsDelay()
        {
            Dictionary<Rect, float> starts = new Dictionary<Rect, float>();
            Rect button = new Rect(100f, 500f, 84f, 28f);
            Vector2 inside = new Vector2(120f, 510f);
            TooltipHoverPolicy.Track(starts, button, inside, 10f);
            TooltipHoverPolicy.Track(starts, button, inside, 10.6f);
            // Курсор ушёл — запись сбрасывается, возврат начинает задержку заново.
            Assert.IsFalse(TooltipHoverPolicy.Track(starts, button, new Vector2(900f, 100f), 12f));
            Assert.AreEqual(0, starts.Count);
            Assert.IsFalse(TooltipHoverPolicy.Track(starts, button, inside, 12.2f));
            Assert.IsTrue(TooltipHoverPolicy.Track(starts, button, inside, 12.7f));
        }

        [Test]
        public void Track_SwitchToAnotherButton_StartsFreshDelay()
        {
            Dictionary<Rect, float> starts = new Dictionary<Rect, float>();
            Rect first = new Rect(100f, 500f, 84f, 28f);
            Rect second = new Rect(200f, 500f, 84f, 28f);
            Vector2 firstMouse = new Vector2(120f, 510f);
            Vector2 secondMouse = new Vector2(220f, 510f);
            Assert.IsFalse(TooltipHoverPolicy.Track(starts, first, firstMouse, 10f));
            // Переход на соседнюю кнопку: у той свой отсчёт, тултип сразу не показывается.
            Assert.IsFalse(TooltipHoverPolicy.Track(starts, second, secondMouse, 20f));
            Assert.IsFalse(TooltipHoverPolicy.Track(starts, second, secondMouse, 20.2f));
            Assert.IsTrue(TooltipHoverPolicy.Track(starts, second, secondMouse, 20.5f));
        }

        [Test]
        public void Track_NullDictionary_IsSafe()
        {
            Assert.IsFalse(TooltipHoverPolicy.Track(null, new Rect(0f, 0f, 10f, 10f), new Vector2(1f, 1f), 1f));
        }
    }
}
