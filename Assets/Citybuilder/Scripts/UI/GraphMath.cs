namespace Citybuilder
{
    // Математика графиков инфоменю (EPIC-68 шаг 1): прореживание серии и автоматические оси.
    // Чистый C#, без Unity и без аллокаций: результат пишется в буфер вызывающего.
    // Рисование линий — следующий шаг; здесь только числа, чтобы OnGUI был дешёвым.
    public static class GraphMath
    {
        // Точек на график после прореживания (план: 192 точки истории → ~64).
        public const int DefaultMaxPoints = 64;

        // Прореживание равномерным шагом с сохранением минимума и максимума серии:
        // экстремумы перезаписывают ближайшие выбранные точки, чтобы всплеск не потерялся.
        // Возвращает число заполненных точек output; буфер должен быть не короче maxPoints.
        public static int Decimate(float[] values, int count, float[] output, int maxPoints)
        {
            int safeCount = SafeCount(values, count);
            int limit = maxPoints < 1 ? 1 : maxPoints;
            if (output == null || safeCount == 0) return 0;
            int stride = Stride(safeCount, limit);
            int written = 0;
            for (int i = 0; i < safeCount && written < output.Length; i += stride)
            {
                output[written] = Safe(values[i]);
                written++;
            }
            KeepExtremes(values, safeCount, output, written, stride);
            return written;
        }

        // Децимация 30 дней: по паре min/max на интервал, всплески между выборками не теряются.
        // При maxPoints=64 даёт 32 интервала × 2 точки; результат — в буфер вызывающего, без аллокаций.
        public static int DecimateBuckets(float[] values, int count, float[] output, int maxPoints)
        {
            int safeCount = SafeCount(values, count);
            int limit = maxPoints < 2 ? 1 : maxPoints / 2;
            if (output == null || safeCount == 0) return 0;
            int buckets = safeCount < limit ? safeCount : limit;
            int written = 0;
            for (int b = 0; b < buckets && written + 1 < output.Length; b++)
            {
                int from = b * safeCount / buckets;
                int to = (b + 1) * safeCount / buckets;
                BucketExtremes(values, count, from, to, output, written);
                written += 2;
            }
            return written;
        }

        // Оси по данным: NaN пропускается, пустая серия даёт 0..0.
        // Плоская линия расширяется на единицу, чтобы нормализация не делила на ноль.
        public static void Axes(float[] values, int count, out float min, out float max)
        {
            int safeCount = SafeCount(values, count);
            min = 0f;
            max = 0f;
            if (safeCount == 0) return;
            min = float.MaxValue;
            max = float.MinValue;
            int found = 0;
            for (int i = 0; i < safeCount; i++)
            {
                if (float.IsNaN(values[i])) continue;
                if (values[i] < min) min = values[i];
                if (values[i] > max) max = values[i];
                found++;
            }
            if (found == 0)
            {
                min = 0f;
                max = 0f;
                return;
            }
            if (max - min <= 0f)
            {
                min -= 0.5f;
                max += 0.5f;
            }
        }

        // Значение в [0,1] по осям: края дают ровно 0 и 1, вырожденные оси — середину.
        public static float Normalize(float value, float min, float max)
        {
            float span = max - min;
            if (float.IsNaN(value)) return 0f;
            if (span <= 0f || float.IsNaN(span)) return 0.5f;
            float fraction = (value - min) / span;
            if (fraction < 0f) return 0f;
            if (fraction > 1f) return 1f;
            return fraction;
        }

        // Шаг выборки: ceil(count / limit), минимум 1 — серия короче лимита идёт как есть.
        private static int Stride(int count, int limit)
        {
            if (count <= limit) return 1;
            return (count + limit - 1) / limit;
        }

        // Минимум и максимум серии ставятся на места своих ближайших выбранных точек.
        private static void KeepExtremes(float[] values, int count, float[] output, int written, int stride)
        {
            if (written == 0) return;
            int minIndex = -1;
            int maxIndex = -1;
            for (int i = 0; i < count; i++)
            {
                if (float.IsNaN(values[i])) continue;
                if (minIndex < 0 || values[i] < values[minIndex]) minIndex = i;
                if (maxIndex < 0 || values[i] > values[maxIndex]) maxIndex = i;
            }
            if (minIndex < 0) return;
            Place(output, written, stride, Safe(values[minIndex]), minIndex);
            Place(output, written, stride, Safe(values[maxIndex]), maxIndex);
        }

        // Минимум и максимум интервала пишутся в две соседние ячейки; NaN → 0.
        private static void BucketExtremes(float[] values, int count, int from, int to, float[] output, int slot)
        {
            float min = float.MaxValue, max = float.MinValue;
            for (int i = from; i < to && i < count; i++)
            {
                float value = Safe(values[i]);
                if (value < min) min = value;
                if (value > max) max = value;
            }
            if (min == float.MaxValue) { min = 0f; max = 0f; }
            output[slot] = min;
            output[slot + 1] = max;
        }

        private static void Place(float[] output, int written, int stride, float value, int sourceIndex)
        {
            int slot = sourceIndex / stride;
            if (slot >= written) slot = written - 1;
            output[slot] = value;
        }

        private static int SafeCount(float[] values, int count)
        {
            if (values == null || count <= 0) return 0;
            return count > values.Length ? values.Length : count;
        }

        private static float Safe(float value)
        {
            return float.IsNaN(value) ? 0f : value;
        }
    }
}