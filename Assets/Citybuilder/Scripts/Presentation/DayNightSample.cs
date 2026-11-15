namespace Citybuilder
{
    // Снимок освещения на час суток (EPIC-58): чистый тип без Unity-типов,
    // чтобы фазовая математика проверялась и через dotnet без редактора.
    // Контроллер переводит эти поля в Color и Quaternion.
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

        public DayNightSample(float sunPitch, float sunAzimuth, float sunIntensity, float moonIntensity,
            float sunColorR, float sunColorG, float sunColorB,
            float ambientR, float ambientG, float ambientB)
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
        }
    }
}
