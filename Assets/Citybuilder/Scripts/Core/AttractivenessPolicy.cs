namespace Citybuilder
{
    // Привлекательность города (EPIC-05 шаг 1): множитель скорости въезда жителей 0.5..1.5.
    // Чистый C#, без Unity и без состояния: вход — снимок факторов, выход — один множитель.
    // Фактор с NaN нейтрален: его вклад 0, база 1.0 от него не смещается; итог никогда не NaN.
    public static class AttractivenessPolicy
    {
        public const float MinMultiplier = 0.5f;
        public const float MaxMultiplier = 1.5f;

        private const float Base = 1f;
        private const float LeisureWeight = 0.2f;
        private const float HealthWeight = 0.1f;
        private const float SupplyWeight = 0.3f;
        private const float CongestionWeight = 0.1f;
        private const float DeficitPenalty = 0.2f;

        // Формула плана: база + досуг + здоровье − снабжение − пробки − дефицит.
        // Покрытия и ставки снабжения — доли 0..1 (1 = покрытие полное, 1 = нехватки нет),
        // worstCongestionMultiplier — худший множитель пробок (1 = свободно), inDeficit — флаг казны.
        public static float Evaluate(float leisureCoverage01, float healthCoverage01,
            float powerRate01, float waterRate01, float wasteRate01,
            float worstCongestionMultiplier, bool inDeficit)
        {
            float result = Base;
            result += Uplift(leisureCoverage01, LeisureWeight);
            result += Uplift(healthCoverage01, HealthWeight);
            result -= SupplyWeight * (Base - WorstSupplyRate(powerRate01, waterRate01, wasteRate01));
            result -= CongestionWeight * CongestionExcess(worstCongestionMultiplier);
            if (inDeficit) result -= DeficitPenalty;
            return Clamp(result);
        }

        // Прирост от покрытия: NaN — фактора нет, вклад 0.
        private static float Uplift(float coverage01, float weight)
        {
            if (float.IsNaN(coverage01)) return 0f;
            return coverage01 * weight;
        }

        // Ставка снабжения: NaN — «не ограничивает», как в NeedProviders.Rate01.
        private static float Rate01(float value)
        {
            if (float.IsNaN(value)) return Base;
            return value;
        }

        // Худшая из трёх ставок: штраф идёт по минимуму, а не по средней.
        private static float WorstSupplyRate(float power, float water, float waste)
        {
            float worst = Rate01(power);
            float value = Rate01(water);
            if (value < worst) worst = value;
            value = Rate01(waste);
            if (value < worst) worst = value;
            return worst;
        }

        // Смещение пробок сверх свободного хода: ниже 1 бонусом не бывает, NaN — 0.
        private static float CongestionExcess(float worstMultiplier)
        {
            if (float.IsNaN(worstMultiplier)) return 0f;
            return worstMultiplier > Base ? worstMultiplier - Base : 0f;
        }

        // Границы множителя: результат строго 0.5..1.5, NaN наружу не выходит.
        private static float Clamp(float value)
        {
            if (float.IsNaN(value)) return Base;
            if (value < MinMultiplier) return MinMultiplier;
            if (value > MaxMultiplier) return MaxMultiplier;
            return value;
        }
    }
}
