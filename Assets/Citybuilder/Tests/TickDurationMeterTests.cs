using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // issue #161: замер тика — p95, кольцо, сброс и отсутствие влияния на детерминизм.
    public class TickDurationMeterTests
    {
        [Test]
        public void EmptyMeter_ZeroCountAndP95()
        {
            TickDurationMeter meter = new TickDurationMeter();
            Assert.AreEqual(0, meter.Count);
            Assert.AreEqual(0f, meter.P95Milliseconds());
            Assert.AreEqual(TickDurationMeter.DefaultCapacity, meter.Capacity);
        }

        [Test]
        public void RecordMilliseconds_P95UsesNearestRank()
        {
            TickDurationMeter meter = new TickDurationMeter(20);
            for (int i = 1; i <= 20; i++) meter.RecordMilliseconds(i);

            Assert.AreEqual(20, meter.Count);
            Assert.AreEqual(19f, meter.P95Milliseconds(), 0.001f, "ceil(0.95*20)=19");
        }

        [Test]
        public void RecordMilliseconds_RingKeepsOnlyLastN()
        {
            TickDurationMeter meter = new TickDurationMeter(4);
            for (int i = 0; i < 4; i++) meter.RecordMilliseconds(100);
            for (int i = 1; i <= 4; i++) meter.RecordMilliseconds(i);

            Assert.AreEqual(4, meter.Count);
            Assert.AreEqual(4f, meter.P95Milliseconds(), 0.001f, "old 100 ms samples are gone");
        }

        [Test]
        public void BeginEnd_OnePairRecordsOneSample()
        {
            TickDurationMeter meter = new TickDurationMeter();
            meter.End();
            Assert.AreEqual(0, meter.Count, "unpaired End is ignored");

            meter.Begin();
            meter.Begin();
            meter.End();
            Assert.AreEqual(1, meter.Count);
            Assert.GreaterOrEqual(meter.P95Milliseconds(), 0f);
        }

        [Test]
        public void Reset_ClearsWindow_AndRecordingContinues()
        {
            TickDurationMeter meter = new TickDurationMeter(4);
            meter.RecordMilliseconds(50);
            meter.RecordMilliseconds(60);
            meter.Reset();

            Assert.AreEqual(0, meter.Count);
            Assert.AreEqual(0f, meter.P95Milliseconds());
            meter.RecordMilliseconds(7);
            Assert.AreEqual(7f, meter.P95Milliseconds(), 0.001f);
        }

        // Замер не добавляет/не теряет тики и не меняет время: замеры идут параллельно симуляции.
        [Test]
        public void AttachedToClock_TickSequenceUnchanged()
        {
            SimulationClock plain = new SimulationClock();
            SimulationClock measured = new SimulationClock();
            TickDurationMeter meter = new TickDurationMeter();
            measured.TickStarted += t => meter.Begin();
            measured.TickCompleted += t => meter.End();

            List<long> plainTicks = new List<long>();
            List<long> measuredTicks = new List<long>();
            plain.TickCompleted += t => plainTicks.Add(t.TickCount);
            measured.TickCompleted += t => measuredTicks.Add(t.TickCount);

            for (int i = 0; i < 200; i++)
            {
                plain.Update(0.1f);
                measured.Update(0.1f);
            }

            Assert.AreEqual(plainTicks, measuredTicks);
            Assert.AreEqual(plain.CurrentTime.TickCount, measured.CurrentTime.TickCount);
            Assert.AreEqual(plainTicks.Count, meter.Count);
        }
    }
}
