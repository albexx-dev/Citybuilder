using System.Collections.Generic;

namespace Citybuilder
{
    // Общий баланс спроса и предложения (энергия, вода). Чистый C#, без Unity.
    // Нехватка делится поровну: все потребители получают одну и ту же долю 0..1.
    public class SupplyBalance
    {
        private const float SatisfiedThreshold = 0.999f;

        private readonly Dictionary<EntityId, float> demand = new Dictionary<EntityId, float>();
        private readonly Dictionary<EntityId, float> supply = new Dictionary<EntityId, float>();

        // Суммарный спрос потребителей. Отрицательные значения и NaN уже обнулены.
        public float TotalDemand
        {
            get { return Sum(demand); }
        }

        // Суммарная мощность источников.
        public float TotalSupply
        {
            get { return Sum(supply); }
        }

        // Доля покрытия: 0..1. Спрос 0 — 1, покрытие полное — 1.
        public float Rate01
        {
            get
            {
                float demandTotal = TotalDemand;
                if (demandTotal <= 0f)
                {
                    return 1f;
                }
                float supplyTotal = TotalSupply;
                if (supplyTotal >= demandTotal)
                {
                    return 1f;
                }
                return supplyTotal / demandTotal;
            }
        }

        // Дубликат id перезаписывает значение. Отрицательный спрос и NaN — 0.
        public void SetDemand(EntityId id, float value)
        {
            demand[id] = ClampNonNegative(value);
        }

        // Дубликат id перезаписывает значение. Отрицательная мощность и NaN — 0.
        public void SetSupply(EntityId id, float value)
        {
            supply[id] = ClampNonNegative(value);
        }

        // Убирает id из потребителей и источников. Не найден — ничего.
        public void Remove(EntityId id)
        {
            demand.Remove(id);
            supply.Remove(id);
        }

        public void Clear()
        {
            demand.Clear();
            supply.Clear();
        }

        // Доля конкретного потребителя. Не найден — 0; спрос 0 — 1.
        public float RateOf(EntityId id)
        {
            float value;
            if (!demand.TryGetValue(id, out value))
            {
                return 0f;
            }
            if (value <= 0f)
            {
                return 1f;
            }
            return Rate01;
        }

        public bool IsSatisfied(EntityId id)
        {
            return RateOf(id) >= SatisfiedThreshold;
        }

        private static float ClampNonNegative(float value)
        {
            return value > 0f ? value : 0f;
        }

        private static float Sum(Dictionary<EntityId, float> values)
        {
            float total = 0f;
            foreach (KeyValuePair<EntityId, float> pair in values)
            {
                total = total + pair.Value;
            }
            return total;
        }
    }
}
