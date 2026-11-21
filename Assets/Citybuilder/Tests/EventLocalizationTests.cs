using NUnit.Framework;

namespace Citybuilder
{
    // Лента через ключи (ADR-013, issue #88): полнота реестра, байт-идентичный RU-вывод
    // и отсутствие дублей при смене языка. Чистый C#, прогоняется через dotnet.
    public class EventLocalizationTests
    {
        private static readonly string[] EventKeys =
        {
            EventMessageCatalog.CongestionSlowdown, EventMessageCatalog.TreasuryDeficit,
            EventMessageCatalog.UnemploymentHigh, EventMessageCatalog.QuestCompleted,
            EventMessageCatalog.SupplyPowerDeficit, EventMessageCatalog.SupplyWaterDeficit,
            EventMessageCatalog.SupplyWasteDeficit, EventMessageCatalog.SupplyRestored,
            EventMessageCatalog.Blackout, EventMessageCatalog.BlackoutRestored,
            EventMessageCatalog.FireStart, EventMessageCatalog.FireExtinguished,
            EventMessageCatalog.FireBurned, EventMessageCatalog.CrimeTheft,
            EventMessageCatalog.GarbageCollected
        };

        [SetUp]
        public void UseRu()
        {
            Localization.SetLanguage(Language.Ru);
        }

        [TearDown]
        public void RestoreRu()
        {
            Localization.SetLanguage(Language.Ru);
        }

        [Test]
        public void AllEventKeys_RegisteredAndNonEmptyInBothLanguages()
        {
            for (int i = 0; i < EventKeys.Length; i++)
            {
                string key = EventKeys[i];
                Assert.IsTrue(Localization.Has(key), key);
                Localization.SetLanguage(Language.Ru);
                Assert.IsFalse(string.IsNullOrEmpty(Localization.T(key)), key);
                Assert.AreNotEqual(key, Localization.T(key), key);
                Localization.SetLanguage(Language.En);
                Assert.IsFalse(string.IsNullOrEmpty(Localization.T(key)), key);
                Assert.AreNotEqual(key, Localization.T(key), key);
            }
        }

        [Test]
        public void AllRegistryKeys_AreNonEmpty()
        {
            foreach (string key in Localization.Keys)
            {
                Localization.SetLanguage(Language.Ru);
                Assert.IsFalse(string.IsNullOrEmpty(Localization.T(key)), "RU " + key);
                Localization.SetLanguage(Language.En);
                Assert.IsFalse(string.IsNullOrEmpty(Localization.T(key)), "EN " + key);
            }
        }

        [Test]
        public void RuOutput_ByteIdenticalToLegacyMessages()
        {
            Localization.SetLanguage(Language.Ru);
            CityEventLog log = new CityEventLog();
            Check(log, EventMessageCatalog.CongestionSlowdown, new object[] { 2 },
                "День 3, 09:00 — Пробки замедляют рост ×2");
            Check(log, EventMessageCatalog.TreasuryDeficit, new object[] { -5 },
                "День 3, 09:00 — Дефицит казны: баланс $-5");
            Check(log, EventMessageCatalog.UnemploymentHigh, new object[] { 38 },
                "День 3, 09:00 — Высокая безработица: занятость 38%");
            Check(log, EventMessageCatalog.QuestCompleted, new object[] { "Первые жители", 200 },
                "День 3, 09:00 — Задача выполнена: Первые жители (+200$)");
            Check(log, EventMessageCatalog.SupplyPowerDeficit, new object[] { 46 },
                "День 3, 09:00 — Энергия 46%: рост замедлен");
            Check(log, EventMessageCatalog.SupplyWaterDeficit, new object[] { 52 },
                "День 3, 09:00 — Вода 52%: рост замедлен");
            Check(log, EventMessageCatalog.SupplyWasteDeficit, new object[] { 61 },
                "День 3, 09:00 — Мусор 61%: рост замедлен");
            Check(log, EventMessageCatalog.SupplyRestored, null,
                "День 3, 09:00 — Снабжение восстановлено: 100%");
            Check(log, EventMessageCatalog.Blackout, new object[] { 3 },
                "День 3, 09:00 — Блэкаут: 3 зданий без энергии");
            Check(log, EventMessageCatalog.BlackoutRestored, null,
                "День 3, 09:00 — Электроснабжение восстановлено");
            Check(log, EventMessageCatalog.FireStart, new object[] { 12 },
                "День 3, 09:00 — Пожар в доме #12!");
            Check(log, EventMessageCatalog.FireExtinguished, new object[] { 3 },
                "День 3, 09:00 — Пожар в доме #3 потушен");
            Check(log, EventMessageCatalog.FireBurned, new object[] { 7 },
                "День 3, 09:00 — Дом #7 сгорел дотла");
            Check(log, EventMessageCatalog.CrimeTheft, new object[] { 9, 4 },
                "День 3, 09:00 — Кража в доме #9 (День 4)");
            Check(log, EventMessageCatalog.GarbageCollected, new object[] { 21 },
                "День 3, 09:00 — Мусоровоз вывез мусор из дома #21");
            Assert.AreEqual(EventKeys.Length, log.Count);
        }

        [Test]
        public void LanguageSwitch_ReformatsExistingEntry_WithoutAddingDuplicates()
        {
            CityEventLog log = new CityEventLog();
            Assert.IsTrue(log.Add(EventKind.Supply, EventMessageCatalog.SupplyPowerDeficit,
                new object[] { 46 }, 1));
            Localization.SetLanguage(Language.En);
            Assert.AreEqual(1, log.Count);
            Assert.AreEqual("Day 0, 01:00 — Power 46%: growth slowed",
                EventLogView.FormatLine(log.Snapshot()[0]));
            Localization.SetLanguage(Language.Ru);
            Assert.AreEqual(1, log.Count);
            Assert.AreEqual("День 0, 01:00 — Энергия 46%: рост замедлен",
                EventLogView.FormatLine(log.Snapshot()[0]));
        }

        private static void Check(CityEventLog log, string key, object[] args, string expected)
        {
            Assert.IsTrue(log.Add(EventKind.Info, key, args, 81), key);
            Assert.AreEqual(expected, EventLogView.FormatLine(log.Snapshot()[log.Count - 1]), key);
        }
    }
}
