using System;
using System.Diagnostics;
using System.Globalization;

namespace Citybuilder
{
    // Замер длительности тика (issue #161, ADR-001): только Stopwatch, без Unity Profiler API.
    // Кольцо на N тиков, без аллокаций в тике; состояние симуляции не меняется — детерминизм цел.
    public class TickDurationMeter
    {
        public const int DefaultCapacity = 120; // 2 минуты при 1 тике/с

        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly float[] samples;
        private readonly float[] sorted;
        private int next;
        private int count;
        private bool running;

        public TickDurationMeter(int capacity = DefaultCapacity)
        {
            if (capacity < 1) capacity = 1;
            samples = new float[capacity];
            sorted = new float[capacity];
        }

        public int Count { get { return count; } }
        public int Capacity { get { return samples.Length; } }

        // Начало тика: вызывать на TickStarted. Повторный вызов игнорируется.
        public void Begin()
        {
            if (running) return;
            stopwatch.Restart();
            running = true;
        }

        // Конец тика: вызывать на TickCompleted. Без пары Begin тихое игнорирование.
        public void End()
        {
            if (!running) return;
            running = false;
            stopwatch.Stop();
            RecordMilliseconds(stopwatch.Elapsed.TotalMilliseconds);
        }

        // Явная запись пробы: тесты и замеры вне Stopwatch.
        public void RecordMilliseconds(double milliseconds)
        {
            if (milliseconds < 0d || double.IsNaN(milliseconds)) milliseconds = 0d;
            samples[next] = (float)milliseconds;
            next = (next + 1) % samples.Length;
            if (count < samples.Length) count++;
        }

        // p95 по последним count пробам: nearest-rank, сортируем копию без аллокаций.
        public float P95Milliseconds()
        {
            if (count == 0) return 0f;
            for (int i = 0; i < count; i++) sorted[i] = samples[i];
            Array.Sort(sorted, 0, count);
            int rank = (int)Math.Ceiling(count * 0.95d) - 1;
            if (rank < 0) rank = 0;
            else if (rank >= count) rank = count - 1;
            return sorted[rank];
        }

        // Нейтральная строка для логов и E2E-моста; HUD переводит через Localization.
        public string FormatSummary()
        {
            return "p95 " + P95Milliseconds().ToString("F2", CultureInfo.InvariantCulture)
                + " ms / " + count + " ticks";
        }

        // «Новая игра»/загрузка: окно замеров начинается заново.
        public void Reset()
        {
            stopwatch.Reset();
            running = false;
            count = 0;
            next = 0;
        }
    }
}
