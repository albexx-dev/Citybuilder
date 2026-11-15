using NUnit.Framework;
using UnityEngine;

namespace Citybuilder.Tests
{
    // Тесты чистой математики стрелки туториала v3 (EPIC-70): пульс-фаза, отскок
    // и зеркальность rect-кнопок относительно BottomBarView.
    public class TutorialArrowTests
    {
        [Test]
        public void Pulse_BoundedAndPhaseDependent()
        {
            Assert.AreEqual(0.5f, TutorialArrowView.Pulse(0f), 0.001f);
            Assert.AreEqual(1f, TutorialArrowView.Pulse(Mathf.PI * 0.125f), 0.01f);
            for (float t = 0f; t < 2f; t += 0.037f)
            {
                float pulse = TutorialArrowView.Pulse(t);
                Assert.GreaterOrEqual(pulse, 0f);
                Assert.LessOrEqual(pulse, 1f);
            }
        }

        [Test]
        public void ArrowBounce_BetweenZeroAndAmplitude_AndTracksPulse()
        {
            for (float t = 0f; t < 2f; t += 0.037f)
            {
                float bounce = TutorialArrowView.ArrowBounce(t);
                Assert.GreaterOrEqual(bounce, 0f);
                Assert.LessOrEqual(bounce, 4f);
                Assert.AreEqual(TutorialArrowView.Pulse(t), bounce * 0.25f, 0.001f);
            }
        }

        [Test]
        public void FrameRect_GrowsWithPulse_AndCoversButton()
        {
            Rect button = new Rect(100f, 200f, 84f, 28f);
            Rect calm = TutorialArrowView.FrameRect(button, 0f);
            Rect peak = TutorialArrowView.FrameRect(button, 1f);
            Assert.AreEqual(2f, button.x - calm.x, 0.01f);
            Assert.AreEqual(4f, button.x - peak.x, 0.01f);
            Assert.IsTrue(peak.Contains(button.center));
        }

        [TestCase(TutorialPolicy.TargetButton.Houses, 1)]
        [TestCase(TutorialPolicy.TargetButton.Zones, 3)]
        [TestCase(TutorialPolicy.TargetButton.Growth, 4)]
        public void ModeButtonRect_MirrorsBottomBarModeRow(TutorialPolicy.TargetButton target, int index)
        {
            Rect group = HudLayout.ModeGroupRect(1280f, 720f);
            Rect button = TutorialArrowView.ModeButtonRect(1280f, 720f, target);
            Assert.AreEqual(group.x + index * (HudLayout.ModeButtonWidth + HudLayout.ButtonGap), button.x, 0.01f);
            Assert.AreEqual(group.y, button.y, 0.01f);
            Assert.AreEqual(HudLayout.ModeButtonWidth, button.width, 0.01f);
            Assert.AreEqual(group.height, button.height, 0.01f);
        }

        [Test]
        public void ModeButtonRect_UnknownTarget_ReturnsEmptyRect()
        {
            Rect empty = TutorialArrowView.ModeButtonRect(1280f, 720f, TutorialPolicy.TargetButton.None);
            Assert.AreEqual(0f, empty.width);
        }

        [Test]
        public void SpeedButtonRect_MirrorsBottomBarSpeedRow_ForX4()
        {
            Rect group = HudLayout.SpeedGroupRect(1280f, 720f);
            Rect button = TutorialArrowView.SpeedButtonRect(1280f, 720f, TutorialArrowView.X4SpeedIndex);
            Assert.AreEqual(group.x + 2f * (HudLayout.SpeedButtonWidth + HudLayout.ButtonGap), button.x, 0.01f);
            Assert.AreEqual(group.y, button.y, 0.01f);
            Assert.AreEqual(HudLayout.SpeedButtonWidth, button.width, 0.01f);
            Assert.AreEqual(group.height, button.height, 0.01f);
        }

        [TestCase(1024f, 600f, 10)]
        [TestCase(1920f, 1080f, 10)]
        public void PaletteButtonRect_MirrorsDrawPaletteRow(float width, float height, int count)
        {
            Rect popup = HudLayout.PaletteRect(width, height, count);
            for (int i = 0; i < count; i++)
            {
                Rect button = TutorialArrowView.PaletteButtonRect(width, height, i, count);
                Assert.AreEqual(popup.x + i * (HudLayout.PaletteButtonWidth + HudLayout.ButtonGap), button.x, 0.01f);
                Assert.AreEqual(popup.y, button.y, 0.01f);
                Assert.AreEqual(HudLayout.PaletteButtonWidth, button.width, 0.01f);
                Assert.AreEqual(popup.height, button.height, 0.01f);
            }
        }
    }
}
