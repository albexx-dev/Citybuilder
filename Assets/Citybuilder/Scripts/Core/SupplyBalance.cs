using System.Collections.Generic;

namespace Citybuilder
{
    // Общий баланс спроса и предложения (энергия, вода). Чистый C#, без Unity.
    // Нехватка делится поровну (Rate01), при дефиците EPIC-50 — блэкаут IsPowered.
    public class SupplyBalance
    {
        private const float SatisfiedThreshold = 0.999f;

        // Порядок отключения (EPIC-50): службы первыми, жилые дома последними.
        public const int TierService = 0;
        public const int TierResidential = 1;

        private readonly Dictionary<EntityId, float> demand = new Dictionary<EntityId, float>();
        private readonly Dictionary<EntityId, float> supply = new Dictionary<EntityId, float>();
        private readonly Dictionary<EntityId, int> tiers = new Dictionary<EntityId, int>();
        private List<EntityId> cutoffCache;

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

        // Дубликат id перезаписывает значение. Отрицательный спрос и NaN — 0. Tier — службы.
        public void SetDemand(EntityId id, float value)
        {
            SetDemand(id, value, TierService);
        }

        // Tier: 0 — отключается первым (службы/станции), 1 — последним (жилые).
        public void SetDemand(EntityId id, float value, int tier)
        {
            demand[id] = ClampNonNegative(value);
            tiers[id] = tier;
            cutoffCache = null;
        }

        // Дубликат id перезаписывает значение. Отрицательная мощность и NaN — 0.
        public void SetSupply(EntityId id, float value)
        {
            supply[id] = ClampNonNegative(value);
            cutoffCache = null;
        }

        // Убирает id из потребителей и источников. Не найден — ничего.
        public void Remove(EntityId id)
        {
            demand.Remove(id);
            supply.Remove(id);
            tiers.Remove(id);
            cutoffCache = null;
        }

        public void Clear()
        {
            demand.Clear();
            supply.Clear();
            tiers.Clear();
            cutoffCache = null;
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

        // Питается ли потребитель (EPIC-50): вне баланса или без спроса — да; в наборе блэкаута — нет.
        public bool IsPowered(EntityId id)
        {
            float value;
            if (!demand.TryGetValue(id, out value) || value <= 0f) return true;
            return EnsureCutoff().IndexOf(id) < 0;
        }

        // Сколько потребителей отключено сейчас (EPIC-50): для ленты событий.
        public int UnpoweredCount { get { return EnsureCutoff().Count; } }

        // Снимок отключённых в порядке отключения. Копия: кеш защищён от чужих правок.
        public List<EntityId> UnpoweredIds()
        {
            return new List<EntityId>(EnsureCutoff());
        }

        private List<EntityId> EnsureCutoff()
        {
            if (cutoffCache == null) cutoffCache = SupplyCutoff.Compute(demand, tiers, TotalDemand - TotalSupply);
            return cutoffCache;
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
