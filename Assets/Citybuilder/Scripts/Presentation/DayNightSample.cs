namespace Citybuilder
{
    // Снимок освещения на час суток (EPIC-58): чистый тип без Unity-типов,
    // чтобы фазовая математика проверялась и через dotnet без редактора.
    // Контроллер переводит эти поля в Color и Quaternion.
    // ADR-021: presentation-поля тумана и сезонного оттенка не попадают в сейв.
    public readonly struct DayNightSample
    {
        public readonly float SunPitch;      // высота солнца, градусы
        public readonly float SunAzimuth;    // азимут солнца, градусы (оборот за сутки)
        public readonly float SunIntensity;  // солнце: ночью минимум, днём пик
        public readonly float MoonIntensity; // «луна»: 0 днём, максимум ночью
        public readonly float SunColorR;
        public readonly float SunColorG;
        public readonly float SunColorB;
        public readonly float AmbientR;
        public readonly float AmbientG;
        public readonly float AmbientB;

        // Presentation (ADR-021): tint — множитель оттенка, fog — цвет и плотность тумана.
        public readonly float SeasonTintR;
        public readonly float SeasonTintG;
        public readonly float SeasonTintB;
        public readonly float FogR;
        public readonly float FogG;
        public readonly float FogB;
        public readonly float FogDensity;

        // Без сезона: нейтральный оттенок и мягкий туман — старое поведение.#69
        public DayNightSample(float sunPitch, float sunAzimuth, float sunIntensity, float moonIntensity,
            float sunColorR, float sunColorG, float sunColorB,
            float ambientR, float ambientG, float ambientB)
            : this(sunPitch, sunAzimuth, sunIntensity, moonIntensity, sunColorR, sunColorG, sunColorB,
                ambientR, ambientG, ambientB, 1f, 1f, 1f, 0.62f, 0.70f, 0.82f, 0.005f)
        {
        }

        public DayNightSample(float sunPitch, float sunAzimuth, float sunIntensity, float moonIntensity,
            float sunColorR, float sunColorG, float sunColorB,
            float ambientR, float ambientG, float ambientB,
            float seasonTintR, float seasonTintG, float seasonTintB,
            float fogR, float fogG, float fogB, float fogDensity)
        {
            SunPitch = sunPitch;
            SunAzimuth = sunAzimuth;
            SunIntensity = sunIntensity;
            MoonIntensity = moonIntensity;
            SunColorR = sunColorR;
            SunColorG = sunColorG;
            SunColorB = sunColorB;
            AmbientR = ambientR;
            AmbientG = ambientG;
            AmbientB = ambientB;
            SeasonTintR = seasonTintR;
            SeasonTintG = seasonTintG;
            SeasonTintB = seasonTintB;
            FogR = fogR;
            FogG = fogG;
            FogB = fogB;
            FogDensity = fogDensity;
        }
    }
}
