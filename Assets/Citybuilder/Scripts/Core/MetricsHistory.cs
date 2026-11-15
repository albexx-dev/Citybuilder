namespace Citybuilder
{
    // История метрик города (EPIC-63): кольцо снимков на предвыделенном массиве.
    // Ёмкость 192 = 8 игровых дней по 24 часа; дальше старые точки перезаписываются.
    // Запись без аллокаций; Snapshot() выделяет массив — UI вызывает раз в час, приемлемо.
    // История сессии: в сейв не сериализуется, после загрузки копится заново (EPIC-63).
    public class MetricsHistory
    {
        public const int Capacity = 192; // 8 дней × 24 часа

        private readonly MetricsSnapshot[] buffer = new MetricsSnapshot[Capacity];
        private int writeIndex; // индекс следующей записи в кольце
        private int recorded;   // сколько точек накоплено (растёт до ёмкости)

        public int Count { get { return recorded; } }

        // Записать снимок; NaN в любом процентном поле заменяется нулём (паттерн задачи 084).
        public void Record(MetricsSnapshot snapshot)
        {
            buffer[writeIndex] = Sanitize(snapshot);
            writeIndex = (writeIndex + 1) % Capacity;
            if (recorded < Capacity) recorded++;
        }

        // Последний записанный снимок; пустая история — пустой снимок (все нули).
        public MetricsSnapshot Latest()
        {
            if (recorded == 0) return default(MetricsSnapshot);
            return buffer[(writeIndex - 1 + Capacity) % Capacity];
        }

        // Копия истории от старых к новым; единственное аллоцирующее место API.
        public MetricsSnapshot[] Snapshot()
        {
            MetricsSnapshot[] result = new MetricsSnapshot[recorded];
            int oldest = (writeIndex - recorded + Capacity) % Capacity;
            for (int i = 0; i < recorded; i++)
            {
                result[i] = buffer[(oldest + i) % Capacity];
            }
            return result;
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
                float.IsNaN(snapshot.WastePercent) ? 0f : snapshot.WastePercent);
        }
    }
}
