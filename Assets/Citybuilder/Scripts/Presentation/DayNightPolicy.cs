namespace Citybuilder
{
    // Чистая математика цикла дня и ночи (EPIC-58): 6 ключевых точек суток,
    // линейная интерполяция между ними. Без Unity-типов — покрыта EditMode-тестами.
    // Точки: 0 (ночь), 6 (рассвет), 9 (утро), 12 (полдень), 18 (закат), 22 (вечер).
    public static class DayNightPolicy
    {
        public const float HoursPerDay = 24f;
        public const float NoonHour = 12f;
        public const int KeyPointCount = 6;
        public const float MinSunIntensity = 0.15f;
        public const float MaxSunIntensity = 1.1f;
        public const float MaxMoonIntensity = 0.3f;
        public const float DegreesPerHour = 15f; // полный оборот солнца за сутки

        // Столбцы: pitch, sunI, moonI, sunR, sunG, sunB, ambR, ambG, ambB.
        // Ночью ambient темнее и холоднее (доминирует синий), днём — тёплый.
        private static readonly float[,] Table =
        {
            {  8f, 0.15f, 0.30f, 0.62f, 0.70f, 1.00f, 0.16f, 0.19f, 0.30f },
            { 12f, 0.55f, 0.10f, 1.00f, 0.76f, 0.52f, 0.36f, 0.33f, 0.36f },
            { 32f, 0.95f, 0.00f, 1.00f, 0.90f, 0.78f, 0.55f, 0.52f, 0.47f },
            { 58f, 1.10f, 0.00f, 1.00f, 0.97f, 0.90f, 0.62f, 0.59f, 0.52f },
            { 14f, 0.50f, 0.05f, 1.00f, 0.60f, 0.34f, 0.46f, 0.35f, 0.30f },
            {  8f, 0.15f, 0.25f, 0.62f, 0.70f, 1.00f, 0.18f, 0.21f, 0.33f },
        };

        private static readonly float[] KeyHours = { 0f, 6f, 9f, 12f, 18f, 22f };

        // Фиксированный полдень: безопасное состояние при выключенном цикле (#69).
        public static DayNightSample Noon()
        {
            return Sample(NoonHour);
        }

        // Час приводится к [0, 24): NaN и бесконечность дают полдень.
        public static float NormalizeHour(float hour)
        {
            if (float.IsNaN(hour) || float.IsInfinity(hour)) return NoonHour;
            float wrapped = hour % HoursPerDay;
            return wrapped < 0f ? wrapped + HoursPerDay : wrapped;
        }

        // Параметры света на час суток: сегмент таблицы + линейная интерполяция.
        public static DayNightSample Sample(float hour)
        {
            float normalized = NormalizeHour(hour);
            int segment = SegmentFor(normalized);
            float start = KeyHours[segment];
            float t = (normalized - start) / (SegmentEnd(segment) - start);
            return LerpRows(segment, (segment + 1) % KeyPointCount, t, normalized);
        }

        // Сегмент: последняя ключевая точка, чей час не позже текущего.
        private static int SegmentFor(float hour)
        {
            for (int i = KeyPointCount - 1; i > 0; i--)
            {
                if (hour >= KeyHours[i]) return i;
            }
            return 0;
        }

        // Конец сегмента: последний смыкается с началом суток (22 -> 24 = 0).
        private static float SegmentEnd(int segment)
        {
            return segment == KeyPointCount - 1 ? HoursPerDay : KeyHours[segment + 1];
        }

        private static DayNightSample LerpRows(int from, int to, float t, float hour)
        {
            return new DayNightSample(
                Lerp(Table[from, 0], Table[to, 0], t),
                AzimuthAt(hour),
                Clamp(Lerp(Table[from, 1], Table[to, 1], t), MinSunIntensity, MaxSunIntensity),
                Lerp(Table[from, 2], Table[to, 2], t),
                Clamp01(Lerp(Table[from, 3], Table[to, 3], t)),
                Clamp01(Lerp(Table[from, 4], Table[to, 4], t)),
                Clamp01(Lerp(Table[from, 5], Table[to, 5], t)),
                Clamp01(Lerp(Table[from, 6], Table[to, 6], t)),
                Clamp01(Lerp(Table[from, 7], Table[to, 7], t)),
                Clamp01(Lerp(Table[from, 8], Table[to, 8], t)));
        }

        // Солнце делает полкруга за световой день: -90° восход, 0° полдень, +90° закат.
        private static float AzimuthAt(float hour)
        {
            return (hour - NoonHour) * DegreesPerHour;
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        private static float Clamp(float value, float min, float max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        private static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }
    }
}
