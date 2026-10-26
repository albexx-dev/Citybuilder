namespace Citybuilder
{
    // Штраф темпа роста по глубине дефицита энергии, воды и мусора. Чистый C#, без Unity.
    // Худшая система задаёт темп: множители не перемножаются.
    public static class SupplyGrowthPolicy
    {
        public const float GoodRate = 0.99f;
        public const float SevereRate = 0.5f;

        // Глубина дефицита: 1 — норма, 2 — нехватка, 3 — сильная нехватка.
        public static int DepthMultiplier(float rate)
        {
            if (float.IsNaN(rate) || rate >= GoodRate) return 1;
            if (rate >= SevereRate) return 2;
            return 3;
        }

        // Максимум по трём системам: худшая система задаёт темп роста.
        public static int Multiplier(float powerRate, float waterRate, float wasteRate)
        {
            int worst = DepthMultiplier(powerRate);
            int water = DepthMultiplier(waterRate);
            if (water > worst) worst = water;
            int waste = DepthMultiplier(wasteRate);
            if (waste > worst) worst = waste;
            return worst;
        }
    }
}
