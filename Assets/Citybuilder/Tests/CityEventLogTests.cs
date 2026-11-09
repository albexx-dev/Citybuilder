using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Тесты кольцевого буфера, дедупликации, очистки и источников CityEventLog (EPIC-09 шаги 1-3).
    public class CityEventLogTests
    {
        [Test]
        public void RingBuffer_Add60Events_KeepsLast50OldestToNewest()
        {
            CityEventLog log = new CityEventLog();
            for (int i = 0; i < 60; i++)
            {
                log.Add(EventKind.Info, "Событие #" + i, i * 10);
            }

            IReadOnlyList<CityEvent> snap = log.Snapshot();
            Assert.AreEqual(50, snap.Count);
            Assert.AreEqual("Событие #10", snap[0].Message);
            Assert.AreEqual("Событие #59", snap[49].Message);
        }

        [Test]
        public void Deduplication_SameKindAndMessageWithin10Ticks_RecordedOnceThenAllowedAtPlus10()
        {
            CityEventLog log = new CityEventLog();
            for (int i = 0; i < 15; i++)
            {
                log.Add(EventKind.Supply, "Энергия 46%: рост замедлен", i / 3);
            }

            Assert.AreEqual(1, log.Count);

            Assert.IsFalse(log.Add(EventKind.Supply, "Энергия 46%: рост замедлен", 9));
            Assert.IsTrue(log.Add(EventKind.Supply, "Энергия 46%: рост замедлен", 10));
            Assert.AreEqual(2, log.Count);
        }

        [Test]
        public void Deduplication_DifferentKindSameText_RecordsBoth()
        {
            CityEventLog log = new CityEventLog();
            log.Add(EventKind.Supply, "Дефицит", 5);
            log.Add(EventKind.Treasury, "Дефицит", 5);

            Assert.AreEqual(2, log.Count);
        }

        [Test]
        public void Clear_EmptiesLogAndResetsDeduplicationAndFormatsLine()
        {
            CityEventLog log = new CityEventLog();
            log.Add(EventKind.Employment, "Высокая безработица: занятость 38%", 81);
            Assert.AreEqual("День 3, 09:00 — Высокая безработица: занятость 38%", log.Snapshot()[0].FormatLine());

            log.Clear();
            Assert.AreEqual(0, log.Count);
            Assert.IsTrue(log.Add(EventKind.Employment, "Высокая безработица: занятость 38%", 82));
            Assert.AreEqual(1, log.Count);
        }

        [Test]
        public void SupplyEvent_ReportsDeficitAndThenRecoveryOnce()
        {
            bool wasInDeficit = false;
            Assert.AreEqual(string.Empty,
                SupplyGrowthController.EvaluateSupplyEvent(1f, 1f, 1f, ref wasInDeficit));
            Assert.IsFalse(wasInDeficit);

            Assert.AreEqual("Энергия 46%: рост замедлен",
                SupplyGrowthController.EvaluateSupplyEvent(0.46f, 1f, 1f, ref wasInDeficit));
            Assert.IsTrue(wasInDeficit);

            Assert.AreEqual("Снабжение восстановлено: 100%",
                SupplyGrowthController.EvaluateSupplyEvent(1f, 1f, 1f, ref wasInDeficit));
            Assert.IsFalse(wasInDeficit);

            Assert.AreEqual(string.Empty,
                SupplyGrowthController.EvaluateSupplyEvent(1f, 1f, 1f, ref wasInDeficit));
        }
    }
}
