using NUnit.Framework;

namespace Citybuilder
{
    // issue #113: переключатель масштаба графика 8/30 дней и локализация подписи кнопки.
    public class MetricsGraphRangeTests
    {
        [SetUp]
        public void SetUp()
        {
            MetricsGraphView.ResetRange();
            MetricsGraphTexture.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            Localization.SetLanguage(Language.Ru);
            MetricsGraphView.ResetRange();
        }

        private static MetricsHistory History(int count)
        {
            MetricsHistory history = new MetricsHistory();
            for (int i = 0; i < count; i++)
            {
                history.Record(new MetricsSnapshot(i, 10 + i, i, 0f, 0f, 50f, 0f, 0f));
            }
            return history;
        }

        [Test]
        public void Range_DefaultsToEightDays_AndTogglesToThirtyAndBack()
        {
            Assert.AreEqual(8, MetricsGraphView.RangeDays);
            Assert.AreEqual(192, MetricsGraphView.SampleCount);

            Assert.IsTrue(MetricsGraphView.ToggleRange());
            Assert.AreEqual(30, MetricsGraphView.RangeDays);
            Assert.AreEqual(720, MetricsGraphView.SampleCount);

            Assert.IsFalse(MetricsGraphView.ToggleRange());
            Assert.AreEqual(8, MetricsGraphView.RangeDays);
            Assert.AreEqual(192, MetricsGraphView.SampleCount);
        }

        [Test]
        public void ToggleRange_InvalidatesThrottle_AndRedrawsImmediately()
        {
            MetricsHistory history = History(720);
            Assert.IsTrue(MetricsGraphTexture.RefreshIfDue(history, 10f));
            Assert.IsFalse(MetricsGraphTexture.RefreshIfDue(history, 10.4f));

            MetricsGraphView.ToggleRange();
            Assert.IsTrue(MetricsGraphTexture.RefreshIfDue(history, 10.4f));
            Assert.IsNotNull(MetricsGraphTexture.At(0));
        }

        [Test]
        public void Refresh_ExplicitSampleCounts_RespectEightAndThirtyDayWindows()
        {
            MetricsHistory history = History(720);
            Assert.IsTrue(MetricsGraphTexture.RefreshIfDue(history, 192, 10f));
            Assert.IsTrue(MetricsGraphTexture.RefreshIfDue(history, 720, 10.1f));
        }

        [Test]
        public void RangeLabel_HasNonEmptyRuAndEnTranslations()
        {
            Localization.SetLanguage(Language.Ru);
            string ru = Localization.T("metrics_range", MetricsGraphView.RangeDays);
            Localization.SetLanguage(Language.En);
            string en = Localization.T("metrics_range", MetricsGraphView.RangeDays);

            Assert.IsFalse(string.IsNullOrEmpty(ru));
            Assert.IsFalse(string.IsNullOrEmpty(en));
            StringAssert.Contains("8", ru);
            StringAssert.Contains("8", en);
        }
    }
}
