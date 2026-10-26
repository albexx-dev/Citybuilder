namespace Citybuilder
{
    // Темп роста: тики на дом из базового значения и множителей пробок/дефицита.
    // Чистый C#, без Unity. Худший множитель задаёт темп, а не произведение.
    public static class GrowthPace
    {
        public const int MaxRequiredTicks = 1000;
        private const int MinMultiplier = 1;
        private const int MaxMultiplier = 3;

        // Максимум из двух множителей с клампом 1..3: ×3, а не ×9.
        public static int Combine(int congestionMultiplier, int supplyMultiplier)
        {
            int worst = congestionMultiplier > supplyMultiplier ? congestionMultiplier : supplyMultiplier;
            if (worst < MinMultiplier) return MinMultiplier;
            if (worst > MaxMultiplier) return MaxMultiplier;
            return worst;
        }

        // Тиков на дом. База и множитель снизу — 1, потолок — MaxRequiredTicks.
        public static int RequiredTicks(int baseTicks, int multiplier)
        {
            if (baseTicks <= 0) return 1;
            if (multiplier <= 0) multiplier = 1;
            long ticks = (long)baseTicks * multiplier;
            return ticks > MaxRequiredTicks ? MaxRequiredTicks : (int)ticks;
        }

        // Тиков на дом с учётом пробок и дефицита ресурсов: берём худший множитель.
        public static int RequiredTicks(int baseTicks, int congestionMultiplier, int supplyMultiplier)
        {
            return RequiredTicks(baseTicks, Combine(congestionMultiplier, supplyMultiplier));
        }
    }
}
