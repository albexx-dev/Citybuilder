namespace Citybuilder
{
    // Масштабирование параметров здания уровнем (EPIC-51). Один источник математики
    // для ServiceBuildingSystem (радиус), SupplyFeed/WasteController (мощность),
    // UpkeepPolicy (содержание) и UpgradeController (цена). Чистый C#, без Unity.
    public static class BuildingLevelPolicy
    {
        // Радиус службы: +1 клетка за каждый уровень выше первого.
        public static int ServiceRadius(int baseRadius, int level)
        {
            return ClampNonNegative(baseRadius) + (HouseLevelPolicy.ClampLevel(level) - 1);
        }

        // Мощность станций сетей: ×1.5 за уровень выше первого, всегда в большую сторону.
        public static int ScaledOutput(int baseOutput, int level)
        {
            return ScaledByHalf(baseOutput, level);
        }

        // Содержание: ×1.5 за уровень выше первого, всегда в большую сторону.
        public static int ScaledUpkeep(int baseUpkeep, int level)
        {
            return ScaledByHalf(baseUpkeep, level);
        }

        // Цена апгрейда НА следующий уровень: на 2-й — 0.75 базовой цены,
        // на 3-й — 1.25. Округление к ближайшему, середина — от нуля.
        public static int UpgradeCost(int baseCost, int fromLevel)
        {
            if (fromLevel < HouseLevelPolicy.MinLevel) fromLevel = HouseLevelPolicy.MinLevel;
            if (fromLevel >= HouseLevelPolicy.MaxLevel) fromLevel = HouseLevelPolicy.MaxLevel - 1;
            int parts = fromLevel == HouseLevelPolicy.MinLevel ? 3 : 5; // 0.75 или 1.25 базы, знаменатель 4
            return (ClampNonNegative(baseCost) * parts + 2) / 4;
        }

        // ceil(value × (уровень+1) / 2) на целых: round(v×k/2) = (v×k+1)/2 без дробей.
        private static int ScaledByHalf(int value, int level)
        {
            if (value <= 0) return 0;
            return (value * (HouseLevelPolicy.ClampLevel(level) + 1) + 1) / 2;
        }

        private static int ClampNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }
    }
}
