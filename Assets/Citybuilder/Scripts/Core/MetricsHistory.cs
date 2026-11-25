namespace Citybuilder
{
    // История метрик города (EPIC-63, issue #113): кольцо снимков на предвыделенном массиве.
    // Ёмкость 720 = 30 игровых дней по 24 часа (ADR-016); 8 дней по умолчанию выбирает UI.
    // Запись без аллокаций; CopyRecent() копирует в буфер вызывающего, Snapshot() выделяет массив.
    // История сессии: в сейв не сериализуется, после загрузки копится заново (EPIC-63).
    public class MetricsHistory
    {
        public const int Capacity = 720; // 30 дней × 24 часа (ADR-016)
        public const int HoursPerDay = 24;

        private readonly MetricsSnapshot[] buffer;
        private readonly int capacity;
        private int writeIndex; // индекс следующей записи в кольце
        private int recorded;   // сколько точек накоплено (растёт до ёмкости)

        // По умолчанию полная история 30 дней; тесты и частные случаи задают ёмкость явно.
        public MetricsHistory() : this(Capacity) { }

        public MetricsHistory(int capacity)
        {
            this.capacity = capacity < 1 ? 1 : capacity;
            buffer = new MetricsSnapshot[this.capacity];
        }

        public int Count { get { return recorded; } }
        public int BufferCapacity { get { return capacity; } }

        // Записать снимок; NaN в любом процентном поле заменяется нулём (паттерн задачи 084).
        public void Record(MetricsSnapshot snapshot)
        {
            buffer[writeIndex] = Sanitize(snapshot);
            writeIndex = (writeIndex + 1) % capacity;
            if (recorded < capacity) recorded++;
        }

        // Последний записанный снимок; пустая история — пустой снимок (все нули).
        public MetricsSnapshot Latest()
        {
            if (recorded == 0) return default(MetricsSnapshot);
            return buffer[(writeIndex - 1 + capacity) % capacity];
        }

        // Копия истории от старых к новым; аллоцирующее место API для внешних потребителей.
        public MetricsSnapshot[] Snapshot()
        {
            MetricsSnapshot[] result = new MetricsSnapshot[recorded];
            int oldest = (writeIndex - recorded + capacity) % capacity;
            for (int i = 0; i < recorded; i++)
            {
                result[i] = buffer[(oldest + i) % capacity];
            }
            return result;
        }

        // Последние maxCount точек в буфер вызывающего (от старых к новым), без аллокаций.
        // Возвращает фактически записанное число; лишние точки и нулевой буфер не ломают кольцо.
        public int CopyRecent(MetricsSnapshot[] target, int maxCount)
        {
            if (target == null || maxCount <= 0) return 0;
            int take = recorded < maxCount ? recorded : maxCount;
            if (take > capacity) take = capacity;
            if (take > target.Length) take = target.Length;
            int oldest = (writeIndex - take + capacity) % capacity;
            for (int i = 0; i < take; i++) target[i] = buffer[(oldest + i) % capacity];
            return take;
        }

        // Очистка по кнопке «Новая»; запись продолжается поверх.
        public void Clear()
        {
            writeIndex = 0;
            recorded = 0;
        }

        // Гард чисел: NaN-проценты сломали бы график (#68), поэтому заменяются нулём.
        private static MetricsSnapshot Sanitize(MetricsSnapshot snapshot)
        {
            return new MetricsSnapshot(
                snapshot.Hour,
                snapshot.Population,
                snapshot.Balance,
                float.IsNaN(snapshot.NeedsAvg) ? 0f : snapshot.NeedsAvg,
                float.IsNaN(snapshot.CrimePercent) ? 0f : snapshot.CrimePercent,
                float.IsNaN(snapshot.PowerPercent) ? 0f : snapshot.PowerPercent,
                float.IsNaN(snapshot.WaterPercent) ? 0f : snapshot.WaterPercent,
                float.IsNaN(snapshot.WastePercent) ? 0f : snapshot.WastePercent,
                snapshot.Fires,
                snapshot.Thefts);
        }
    }
}
