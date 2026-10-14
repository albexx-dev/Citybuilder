using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Учёт заселения домов домохозяйствами. Хранит только свои записи, чистый C#, без Unity.
    public class PopulationSystem
    {
        private readonly Dictionary<EntityId, HouseholdState> households = new Dictionary<EntityId, HouseholdState>();

        public int HouseholdCount { get; private set; }
        public int TotalResidents { get; private set; }

        // Копия домохозяйств для сохранений. Отдаём копию, а не сам словарь.
        public List<HouseholdState> Households
        {
            get
            {
                return new List<HouseholdState>(households.Values);
            }
        }

        public PopulationSystem()
        {
        }

        // Заселяет домохозяйство в дом в пределах вместимости. Не влезло — null.
        public HouseholdState MoveIn(BuildingState home, int size)
        {
            if (home == null || size <= 0)
            {
                return null;
            }
            if (ResidentsIn(home) + size > CapacityOf(home))
            {
                return null;
            }
            HouseholdState household = new HouseholdState(EntityId.New(), home, size);
            households.Add(household.Id, household);
            HouseholdCount++;
            TotalResidents += size;
            return household;
        }

        // Выселяет одно домохозяйство. Неизвестно или уже выехало — false.
        public bool MoveOut(HouseholdState household)
        {
            if (household == null || !households.Remove(household.Id))
            {
                return false;
            }
            HouseholdCount = Math.Max(0, HouseholdCount - 1);
            TotalResidents = Math.Max(0, TotalResidents - household.Size);
            return true;
        }

        // Выселяет все домохозяйства дома (нужно при сносе). Возвращает число выселенных.
        public int EvictHome(BuildingState home)
        {
            if (home == null)
            {
                return 0;
            }
            List<EntityId> toRemove = new List<EntityId>();
            foreach (KeyValuePair<EntityId, HouseholdState> pair in households)
            {
                if (pair.Value.Home == home)
                {
                    toRemove.Add(pair.Key);
                }
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                MoveOut(households[toRemove[i]]);
            }
            return toRemove.Count;
        }

        // Жителей в конкретном доме. Дом null — 0.
        public int ResidentsIn(BuildingState home)
        {
            if (home == null)
            {
                return 0;
            }
            int total = 0;
            foreach (HouseholdState household in households.Values)
            {
                if (household.Home == home)
                {
                    total += household.Size;
                }
            }
            return total;
        }

        // Вместимость дома из Definition.ResidentCapacity. Нет дома или типа — 0.
        public int CapacityOf(BuildingState home)
        {
            if (home == null || home.Definition == null)
            {
                return 0;
            }
            return ClampNonNegative(home.Definition.ResidentCapacity);
        }

        private static int ClampNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }
    }
}
