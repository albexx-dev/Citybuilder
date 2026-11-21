using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Тесты кольцевого буфера, дедупликации, очистки и источников CityEventLog (EPIC-09 шаги 1-3).
    public class CityEventLogTests
    {
        private Func<string> savedLoad;
        private Action<string> savedSave;
        private string stored;

        [SetUp]
        public void InjectMemoryStore()
        {
            savedLoad = Localization.LoadPreference;
            savedSave = Localization.SavePreference;
            stored = string.Empty;
            Localization.LoadPreference = () => stored;
            Localization.SavePreference = value => stored = value;
            Localization.SetLanguage(Language.Ru);
        }

        [TearDown]
        public void RestoreDefaults()
        {
            Localization.SetLanguage(Language.Ru);
            Localization.LoadPreference = savedLoad;
            Localization.SavePreference = savedSave;
        }

        [Test]
        public void RingBuffer_Add60Events_KeepsLast50OldestToNewest()
        {
            CityEventLog log = new CityEventLog();
            for (int i = 0; i < 60; i++)
            {
                log.Add(EventKind.Info, "test_event", new object[] { i }, i * 10);
            }

            IReadOnlyList<CityEvent> snap = log.Snapshot();
            Assert.AreEqual(50, snap.Count);
            Assert.AreEqual(10, snap[0].Args[0]);
            Assert.AreEqual(59, snap[49].Args[0]);
        }

        [Test]
        public void Deduplication_SameKindKeyAndArgsWithin10Ticks_RecordedOnceThenAllowedAtPlus10()
        {
            CityEventLog log = new CityEventLog();
            for (int i = 0; i < 15; i++)
            {
                log.Add(EventKind.Supply, EventMessageCatalog.SupplyPowerDeficit,
                    new object[] { 46 }, i / 3);
            }

            Assert.AreEqual(1, log.Count);

            Assert.IsFalse(log.Add(EventKind.Supply, EventMessageCatalog.SupplyPowerDeficit,
                new object[] { 46 }, 9));
            Assert.IsTrue(log.Add(EventKind.Supply, EventMessageCatalog.SupplyPowerDeficit,
                new object[] { 46 }, 10));
            Assert.AreEqual(2, log.Count);
        }

        [Test]
        public void Deduplication_DifferentKindOrArgs_RecordsBoth()
        {
            CityEventLog log = new CityEventLog();
            log.Add(EventKind.Supply, "same_key", null, 5);
            log.Add(EventKind.Treasury, "same_key", null, 5);
            log.Add(EventKind.Supply, "same_key", new object[] { 46 }, 5);
            log.Add(EventKind.Supply, "same_key", new object[] { 47 }, 5);

            Assert.AreEqual(4, log.Count);
        }

        [Test]
        public void Deduplication_LanguageSwitch_IsNotAffected()
        {
            CityEventLog log = new CityEventLog();
            Localization.SetLanguage(Language.Ru);
            log.Add(EventKind.Supply, EventMessageCatalog.SupplyPowerDeficit, new object[] { 46 }, 1);
            Localization.SetLanguage(Language.En);
            Assert.IsFalse(log.Add(EventKind.Supply, EventMessageCatalog.SupplyPowerDeficit,
                new object[] { 46 }, 2));
            Assert.AreEqual(1, log.Count);
            Assert.AreEqual("Day 0, 01:00 — Power 46%: growth slowed",
                EventLogView.FormatLine(log.Snapshot()[0]));
        }

        [Test]
        public void Clear_EmptiesLogAndResetsDeduplicationAndFormatsLine()
        {
            CityEventLog log = new CityEventLog();
            log.Add(EventKind.Employment, EventMessageCatalog.UnemploymentHigh, new object[] { 38 }, 81);
            Assert.AreEqual("День 3, 09:00 — Высокая безработица: занятость 38%",
                EventLogView.FormatLine(log.Snapshot()[0]));

            log.Clear();
            Assert.AreEqual(0, log.Count);
            Assert.IsTrue(log.Add(EventKind.Employment, EventMessageCatalog.UnemploymentHigh,
                new object[] { 38 }, 82));
            Assert.AreEqual(1, log.Count);
        }

        [Test]
        public void FormatLine_EnglishLanguage_UsesEnglishText()
        {
            CityEvent e = new CityEvent(EventKind.Employment, EventMessageCatalog.UnemploymentHigh,
                new object[] { 38 }, 81);
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("Day 3, 09:00 — High unemployment: employment 38%", EventLogView.FormatLine(e));
        }

        [Test]
        public void EventLogKeys_HaveLegacyRuAndExpectedEn()
        {
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual("=== События города ===", Localization.T("event_log_title"));
            Assert.AreEqual("Событий пока нет.", Localization.T("event_log_empty"));
            Assert.AreEqual("День 3, 09:00 — ", Localization.T("event_log_prefix", 3, 9));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual("=== City events ===", Localization.T("event_log_title"));
            Assert.AreEqual("No events yet.", Localization.T("event_log_empty"));
            Assert.AreEqual("Day 3, 09:00 — ", Localization.T("event_log_prefix", 3, 9));
        }

        [Test]
        public void SupplyEvent_ReportsDeficitKeyAndRecoveryOnce()
        {
            bool wasInDeficit = false;
            Assert.IsEmpty(SupplyGrowthController.EvaluateSupplyEvent(1f, 1f, 1f, ref wasInDeficit).MessageKey);
            Assert.IsFalse(wasInDeficit);

            CityEvent deficit = SupplyGrowthController.EvaluateSupplyEvent(0.46f, 1f, 1f, ref wasInDeficit);
            Assert.AreEqual(EventMessageCatalog.SupplyPowerDeficit, deficit.MessageKey);
            Assert.AreEqual(46, deficit.Args[0]);
            Assert.IsTrue(wasInDeficit);

            CityEvent restored = SupplyGrowthController.EvaluateSupplyEvent(1f, 1f, 1f, ref wasInDeficit);
            Assert.AreEqual(EventMessageCatalog.SupplyRestored, restored.MessageKey);
            Assert.IsFalse(wasInDeficit);

            Assert.IsEmpty(SupplyGrowthController.EvaluateSupplyEvent(1f, 1f, 1f, ref wasInDeficit).MessageKey);
        }
    }
}
