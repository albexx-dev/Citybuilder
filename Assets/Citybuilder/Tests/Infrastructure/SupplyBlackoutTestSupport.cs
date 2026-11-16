using System.Collections.Generic;

namespace Citybuilder
{
    internal static class SupplyBlackoutTestSupport
    {
        internal static SupplyBalance MakeUnitCity(int homes, float supply, List<EntityId> ids)
        {
            SupplyBalance balance = new SupplyBalance();
            balance.SetSupply(EntityId.New(), supply);
            for (int i = 0; i < homes; i++)
            {
                EntityId id = EntityId.New();
                ids.Add(id);
                balance.SetDemand(id, 1f, SupplyBalance.TierResidential);
            }
            return balance;
        }

        internal static int CountPowered(SupplyBalance balance, List<EntityId> ids)
        {
            int powered = 0;
            for (int i = 0; i < ids.Count; i++)
            {
                if (balance.IsPowered(ids[i])) powered++;
            }
            return powered;
        }
    }
}
