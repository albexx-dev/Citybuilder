using NUnit.Framework;

namespace Citybuilder
{
    // issue #113: 30 дней истории (ADR-016), параметр ёмкости и децимация 720 точек.
    public class MetricsRangeTests
    {
        private static MetricsSnapshot Sample(int hour)
        {
            return new MetricsSnapshot(hour, hour, hour, 0f, 0f, 0f, 0f, 0f);
        }

        private static float[] RampWithSpikes(int count)
        {
            float[] values = new float[count];
            for (int i = 0; i < count; i++) values[i] = i;
            if (count > 6)
            {
                values[5] = -777f;
                values[count - 7] = 1234f;
            }
            return values;
        }

        [Test]
        public void DefaultCapacity_Is720Hours_AndKeepsLast720()
        {
            MetricsHistory history = new MetricsHistory();
            Assert.AreEqual(720, MetricsHistory.Capacity);
            Assert.AreEqual(720, history.BufferCapacity);
            for (int i = 0; i < 725; i++) history.Record(Sample(i));

            Assert.AreEqual(720, history.Count);
            MetricsSnapshot[] all = history.Snapshot();
            Assert.AreEqual(5, all[0].Hour);
            Assert.AreEqual(724, all[719].Hour);
        }

        [Test]
        public void CapacityParameter_Keeps192ForEightDayView()
        {
            MetricsHistory history = new MetricsHistory(192);
            for (int i = 0; i < 200; i++) history.Record(Sample(i));

            Assert.AreEqual(192, history.Count);
            Assert.AreEqual(8, history.Snapshot()[0].Hour);
        }

        [Test]
        public void CopyRecent_From720_TakesLast192WithoutAllocatingResult()
        {
            MetricsHistory history = new MetricsHistory();
            for (int i = 0; i < 720; i++) history.Record(Sample(i));

            MetricsSnapshot[] target = new MetricsSnapshot[720];
            int count = history.CopyRecent(target, 8 * MetricsHistory.HoursPerDay);
            Assert.AreEqual(192, count);
            Assert.AreEqual(528, target[0].Hour);
            Assert.AreEqual(719, target[191].Hour);
            Assert.AreEqual(0, history.CopyRecent(null, 192));
        }

        [Test]
        public void CopyRecent_ClampsToTargetAndCapacity()
        {
            MetricsHistory history = new MetricsHistory(8);
            for (int i = 0; i < 20; i++) history.Record(Sample(i));

            MetricsSnapshot[] target = new MetricsSnapshot[3];
            Assert.AreEqual(3, history.CopyRecent(target, 100));
            Assert.AreEqual(17, target[0].Hour);
            Assert.AreEqual(8, history.BufferCapacity);
        }

        [Test]
        public void DecimateBuckets_720Points_FitsWidthAndKeepsSpikes()
        {
            float[] values = RampWithSpikes(720);
            float[] output = new float[64];
            int written = GraphMath.DecimateBuckets(values, values.Length, output, 64);

            Assert.AreEqual(64, written);
            float min, max;
            GraphMath.Axes(output, written, out min, out max);
            Assert.AreEqual(-777f, min, 0.001f);
            Assert.AreEqual(1234f, max, 0.001f);
            for (int i = 0; i < written; i++) Assert.IsFalse(float.IsNaN(output[i]));
        }

        [Test]
        public void DecimateBuckets_192AndShortSeries_StayInsideBuffer()
        {
            float[] output = new float[64];
            Assert.AreEqual(64, GraphMath.DecimateBuckets(RampWithSpikes(192), 192, output, 64));
            Assert.AreEqual(2, GraphMath.DecimateBuckets(RampWithSpikes(3), 3, new float[2], 64));
            Assert.AreEqual(0, GraphMath.DecimateBuckets(null, 0, output, 64));
            Assert.AreEqual(0, GraphMath.DecimateBuckets(RampWithSpikes(4), 4, null, 64));
        }
    }
}
