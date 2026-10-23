using System.Collections.Generic;

namespace Citybuilder
{
    // Баланс воды (V1, этап 063). Чистый C#, без Unity.
    // Нехватка делится поровну: все потребители получают одну и ту же долю 0..1.
    // Ничем не используется: поведение игры не меняет.
    public class WaterSystem
    {
        private const float SatisfiedThreshold = 0.999f;

        private readonly Dictionary<EntityId, float> consumerDemand = new Dictionary<EntityId, float>();
        private readonly Dictionary<EntityId, float> sourceCapacity = new Dictionary<EntityId, float>();

        // Суммарный спрос потребителей. Отрицательные значения уже обнулены.
        public float TotalDemand
        {
            get { return Sum(consumerDemand); }
        }

        // Суммарная мощность источников воды.
        public float TotalSupply
        {
            get { return Sum(sourceCapacity); }
        }

        // Доля покрытия города: 0..1. Спрос 0 — 1, покрытие полное — 1.
        public float SatisfactionRate01
        {
            get
            {
                float demand = TotalDemand;
                if (demand <= 0f)
                {
                    return 1f;
                }
                float supply = TotalSupply;
                if (supply >= demand)
                {
                    return 1f;
                }
                return supply / demand;
            }
        }

        // Дубликат id перезаписывает значение. Отрицательный спрос — 0.
        public void AddConsumer(EntityId id, float demand)
        {
            consumerDemand[id] = ClampNonNegative(demand);
        }

        // Дубликат id перезаписывает значение. Отрицательная мощность — 0.
        public void AddSource(EntityId id, float capacity)
        {
            sourceCapacity[id] = ClampNonNegative(capacity);
        }

        // Убирает id из потребителей и источников. Не найден — ничего.
        public void Remove(EntityId id)
        {
            consumerDemand.Remove(id);
            sourceCapacity.Remove(id);
        }

        public void Clear()
        {
            consumerDemand.Clear();
            sourceCapacity.Clear();
        }

        // Доля конкретного потребителя. Не найден — 0; спрос 0 — 1.
        public float SatisfactionOf(EntityId id)
        {
            float demand;
            if (!consumerDemand.TryGetValue(id, out demand))
            {
                return 0f;
            }
            if (demand <= 0f)
            {
                return 1f;
            }
            return SatisfactionRate01;
        }

        public bool IsSatisfied(EntityId id)
        {
            return SatisfactionOf(id) >= SatisfiedThreshold;
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
