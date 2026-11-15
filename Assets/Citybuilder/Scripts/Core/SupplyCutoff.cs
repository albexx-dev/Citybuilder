using System.Collections.Generic;

namespace Citybuilder
{
    // Детерминированное распределение блэкаута (EPIC-50): минимальный набор потребителей,
    // чей спрос закрывает нехватку. Службы (tier 0) обесточиваются первыми, жилые (tier 1)
    // последними; внутри tier — стабильный хеш id (паттерн ADR-009, как SelectVariant).
    public static class SupplyCutoff
    {
        // Префикс порядка отключения с суммарным спросом не меньше shortage. Shortage <= 0 — пусто.
        public static List<EntityId> Compute(Dictionary<EntityId, float> demand,
            Dictionary<EntityId, int> tiers, float shortage)
        {
            List<EntityId> cutoff = new List<EntityId>();
            if (shortage <= 0f) return cutoff;
            List<EntityId> order = new List<EntityId>(demand.Keys);
            order.Sort(delegate (EntityId a, EntityId b) { return Compare(a, b, demand, tiers); });
            float cut = 0f;
            for (int i = 0; i < order.Count && cut < shortage; i++)
            {
                float value = demand[order[i]];
                if (value <= 0f) continue;
                cutoff.Add(order[i]);
                cut = cut + value;
            }
            return cutoff;
        }

        // Сначала меньший tier (службы раньше жилых), внутри — хеш id, ничья — по значению id.
        private static int Compare(EntityId a, EntityId b,
            Dictionary<EntityId, float> demand, Dictionary<EntityId, int> tiers)
        {
            int tier = tiers[a].CompareTo(tiers[b]);
            if (tier != 0) return tier;
            int hash = Hash(a).CompareTo(Hash(b));
            return hash != 0 ? hash : a.Value.CompareTo(b.Value);
        }

        // Стабильный хеш перемешивания порядковых id: соседние дома не гаснут волнами.
        private static int Hash(EntityId id)
        {
            uint v = (uint)id.Value;
            v = v * 2654435761u ^ (v >> 13);
            v = v * 1274126177u;
            v ^= v >> 16;
            return (int)(v & 0x7fffffff);
        }
    }
}
