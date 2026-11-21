namespace Citybuilder
{
    // Микс эмбиента (EPIC-71, шаг ambient): час суток и активность города ->
    // громкости дневного и ночного источников. Чистый C# без Unity — EditMode-тесты.
    // Края как у DayNightPolicy: 6 (рассвет), 9 (утро), 18 (закат), 22 (вечер).
    public struct AmbientMixSample
    {
        public readonly float DayVolume;
        public readonly float NightVolume;

        public AmbientMixSample(float dayVolume, float nightVolume)
        {
            DayVolume = dayVolume;
            NightVolume = nightVolume;
        }
    }

    public static class AmbientMixPolicy
    {
        public const float HoursPerDay = 24f;
        public const float NoonHour = 12f;
        // База громкости: эпик 71 задаёт эмбиент по умолчанию 0.4, ночь тише.
        public const float DayBaseVolume = 0.4f;
        public const float NightBaseVolume = 0.3f;
        // Пустой город звучит тише: активность 0 даёт 35% базы, полная — 100%.
        public const float MinActivityScale = 0.35f;

        // Дневная доля лупов: ночь 0 (22..6), подъём 6..9, день 1 (9..18), спад 18..22.
        public static float DayMix(float hour)
        {
            float normalized = NormalizeHour(hour);
            if (normalized < 6f || normalized >= 22f) return 0f;
            if (normalized < 9f) return (normalized - 6f) / 3f;
            if (normalized < 18f) return 1f;
            return 1f - (normalized - 18f) / 4f;
        }

        // Час приводится к [0, 24): NaN и бесконечность дают полдень (как DayNightPolicy).
        public static float NormalizeHour(float hour)
        {
            if (float.IsNaN(hour) || float.IsInfinity(hour)) return NoonHour;
            float wrapped = hour % HoursPerDay;
            return wrapped < 0f ? wrapped + HoursPerDay : wrapped;
        }

        // Громкости источников: активность города (доля населения, 0..1) масштабирует обе.
        public static AmbientMixSample Sample(float hour, float activity)
        {
            float mix = DayMix(hour);
            float scale = ScaleFor(activity);
            return new AmbientMixSample(mix * scale * DayBaseVolume,
                (1f - mix) * scale * NightBaseVolume);
        }

        private static float ScaleFor(float activity)
        {
            float clamped = activity < 0f ? 0f : (activity > 1f ? 1f : activity);
            return MinActivityScale + (1f - MinActivityScale) * clamped;
        }
    }
}
