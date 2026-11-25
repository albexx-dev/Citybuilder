using NUnit.Framework;

namespace Citybuilder
{
    // EPIC-63 metrics-core: кольцо истории метрик — перезапись, порядок, NaN-гарды, Clear, Latest.
    public class MetricsHistoryTests
    {
        private static MetricsSnapshot Sample(int hour, float needsAvg)
        {
            return new MetricsSnapshot(hour, 10 + hour, 100 + hour, needsAvg, 5f, 90f, 80f, 70f);
        }

        // 200 записей при ёмкости 192: остаются последние 192, порядок старый → новый.
        [Test]
        public void Record_200Snapshots_KeepsLast192OldestToNewest()
        {
            MetricsHistory history = new MetricsHistory(192);
            for (int i = 0; i < 200; i++) history.Record(Sample(i, 50f));

            MetricsSnapshot[] all = history.Snapshot();
            Assert.AreEqual(192, all.Length);
            Assert.AreEqual(8, all[0].Hour, "oldest kept hour");
            Assert.AreEqual(199, all[191].Hour, "newest hour");
            for (int i = 1; i < all.Length; i++)
            {
                Assert.Less(all[i - 1].Hour, all[i].Hour, "order at " + i);
            }
        }

        // Count растёт до ёмкости и держится на ней.
        [Test]
        public void Count_GrowsUpToCapacityAndStops()
        {
            MetricsHistory history = new MetricsHistory(192);
            for (int i = 0; i < 195; i++) history.Record(Sample(i, 50f));
            Assert.AreEqual(192, history.Count);
        }

        // NaN в любом процентном поле превращается в 0; баланс хранит минус как есть.
        [Test]
        public void Record_NaNPercentFields_AreStoredAsZero()
        {
            MetricsHistory history = new MetricsHistory();
            history.Record(new MetricsSnapshot(0, 5, -20,
                float.NaN, float.NaN, float.NaN, float.NaN, float.NaN));

            MetricsSnapshot stored = history.Latest();
            Assert.AreEqual(0f, stored.NeedsAvg);
            Assert.AreEqual(0f, stored.CrimePercent);
            Assert.AreEqual(0f, stored.PowerPercent);
            Assert.AreEqual(0f, stored.WaterPercent);
            Assert.AreEqual(0f, stored.WastePercent);
            Assert.AreEqual(-20, stored.Balance, "balance keeps sign");
        }

        // Latest пустой истории — пустой снимок; после записей — самая новая точка.
        [Test]
        public void Latest_EmptyHistoryIsDefault_ElseNewestRecord()
        {
            MetricsHistory history = new MetricsHistory();
            MetricsSnapshot empty = history.Latest();
            Assert.AreEqual(0, empty.Hour);
            Assert.AreEqual(0, empty.Population);

            history.Record(Sample(3, 50f));
            history.Record(Sample(4, 60f));
            Assert.AreEqual(4, history.Latest().Hour);
            Assert.AreEqual(104, history.Latest().Balance);
        }

        // Clear обнуляет историю, запись продолжается поверх (кнопка «Новая»).
        [Test]
        public void Clear_EmptiesHistory_AndRecordingContinues()
        {
            MetricsHistory history = new MetricsHistory();
            for (int i = 0; i < 10; i++) history.Record(Sample(i, 50f));
            history.Clear();

            Assert.AreEqual(0, history.Count);
            Assert.AreEqual(0, history.Snapshot().Length);
            Assert.AreEqual(0, history.Latest().Hour);

            history.Record(Sample(0, 40f));
            history.Record(Sample(1, 41f));
            Assert.AreEqual(2, history.Count);
            Assert.AreEqual(1, history.Latest().Hour);
        }

        // Snapshot() возвращает копию: правка массива не портит историю.
        [Test]
        public void Snapshot_ReturnsIndependentCopy()
        {
            MetricsHistory history = new MetricsHistory();
            history.Record(Sample(0, 50f));
            history.Record(Sample(1, 50f));

            MetricsSnapshot[] copy = history.Snapshot();
            copy[0] = default(MetricsSnapshot);

            Assert.AreEqual(0, history.Snapshot()[0].Hour);
        }

        // #112: новые поля переживают запись, NaN-гард и чтение через Snapshot().
        [Test]
        public void Record_FiresAndThefts_AreReadBack()
        {
            MetricsHistory history = new MetricsHistory();
            history.Record(new MetricsSnapshot(7, 20, 100, 50f, 12f, 90f, 80f, 70f, 3, 9));
            history.Record(new MetricsSnapshot(8, 20, 100, float.NaN, 12f, 90f, 80f, 70f, 1, 11));

            MetricsSnapshot[] all = history.Snapshot();
            Assert.AreEqual(2, all.Length);
            Assert.AreEqual(3, all[0].Fires);
            Assert.AreEqual(9, all[0].Thefts);
            Assert.AreEqual(1, all[1].Fires, "fires survive NaN guard");
            Assert.AreEqual(11, all[1].Thefts, "thefts survive NaN guard");
            Assert.AreEqual(1, history.Latest().Fires);
        }

        // Старый вызов конструктора остаётся рабочим: новые поля по умолчанию нули.
        [Test]
        public void Record_LegacyConstructor_LeavesNewFieldsZero()
        {
            MetricsHistory history = new MetricsHistory();
            history.Record(Sample(1, 50f));

            Assert.AreEqual(0, history.Latest().Fires);
            Assert.AreEqual(0, history.Latest().Thefts);
        }

        // Пустая история и пропущенные источники дают нули, а не мусор.
        [Test]
        public void Latest_EmptyHistory_NewFieldsAreZero()
        {
            MetricsHistory history = new MetricsHistory();
            Assert.AreEqual(0, history.Count);
            Assert.AreEqual(0, history.Latest().Fires);
            Assert.AreEqual(0, history.Latest().Thefts);
            history.Clear();
            Assert.AreEqual(0, history.Latest().Thefts);
        }
    }
}
