using UnityEngine;

namespace Citybuilder
{
    // ADR-021: presentation-туман подменяется только в активном цикле и
    // возвращается владельцу на стопе/паузе. Настройки сцены не портятся.
    public struct FogPresentationState
    {
        private bool saved;
        private bool fog;
        private Color color;
        private float density;
        private FogMode mode;

        // Первая подмена запоминает исходное состояние сцены.
        public void Save()
        {
            if (saved) return;
            fog = RenderSettings.fog;
            color = RenderSettings.fogColor;
            density = RenderSettings.fogDensity;
            mode = RenderSettings.fogMode;
            saved = true;
        }

        public void Restore()
        {
            if (!saved) return;
            RenderSettings.fog = fog;
            RenderSettings.fogColor = color;
            RenderSettings.fogDensity = density;
            RenderSettings.fogMode = mode;
            saved = false;
        }

        // Выключенный цикл не трогает туман и возвращает прежние значения.
        public void Apply(bool enabled, DayNightSample sample)
        {
            if (!enabled)
            {
                Restore();
                return;
            }
            Save();
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(sample.FogR, sample.FogG, sample.FogB);
            RenderSettings.fogDensity = sample.FogDensity;
        }
    }
}
