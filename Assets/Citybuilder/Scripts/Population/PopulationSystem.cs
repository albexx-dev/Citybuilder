using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Учёт заселения домов домохозяйствами. Хранит только свои записи, чистый C#, без Unity.
    public class PopulationSystem
    {
        private readonly Dictionary<EntityId, HouseholdState> households = new Dictionary<EntityId, HouseholdState>();
        // Индекс дома → домохозяйства и жители: снимает полный перебор в горячих путях.
        private readonly Dictionary<BuildingState, List<HouseholdState>> byHome = new Dictionary<BuildingState, List<HouseholdState>>();
        private readonly Dictionary<BuildingState, int> residentsByHome = new Dictionary<BuildingState, int>();

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

        // Очистить все домохозяйства и счётчики населения при сбросе/загрузке.
        public void Clear()
        {
            households.Clear();
            byHome.Clear();
            residentsByHome.Clear();
            HouseholdCount = 0;
            TotalResidents = 0;
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
            AddToIndex(household);
            HouseholdCount++;
            TotalResidents += size;
            return household;
        }

        // Выселяет одно домохозяйство. Неизвестно или уже выехало — false.
        public bool MoveOut(HouseholdState household)
        {
            HouseholdState stored;
            if (household == null || !households.TryGetValue(household.Id, out stored)) return false;
            households.Remove(household.Id);
            RemoveFromIndex(stored);
            HouseholdCount = Math.Max(0, HouseholdCount - 1);
            TotalResidents = Math.Max(0, TotalResidents - stored.Size);
            return true;
        }

        // Выселяет все домохозяйства дома (нужно при сносе). Возвращает число выселенных.
        public int EvictHome(BuildingState home)
        {
            if (home == null)
            {
                return 0;
            }
            List<HouseholdState> residents;
            if (!byHome.TryGetValue(home, out residents)) return 0;
            int count = residents.Count;
            for (int i = count - 1; i >= 0; i--) MoveOut(residents[i]);
            return count;
        }

        // Жителей в конкретном доме. Дом null — 0.
        public int ResidentsIn(BuildingState home)
        {
            if (home == null)
            {
                return 0;
            }
            int residents;
            return residentsByHome.TryGetValue(home, out residents) ? residents : 0;
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

        // Заселение: индексы обновляются вместе со словарём домохозяйств.
        private void AddToIndex(HouseholdState household)
        {
            BuildingState home = household.Home;
            List<HouseholdState> list;
            if (!byHome.TryGetValue(home, out list))
            {
                list = new List<HouseholdState>();
                byHome.Add(home, list);
            }
            list.Add(household);
            int residents;
            residentsByHome.TryGetValue(home, out residents);
            residentsByHome[home] = residents + household.Size;
        }

        // Выселение: ищем дом по сохранённой в записи ссылке, а не перебором всех записей.
        private void RemoveFromIndex(HouseholdState household)
        {
            BuildingState home = household.Home;
            List<HouseholdState> list;
            if (byHome.TryGetValue(home, out list))
            {
                list.Remove(household);
                if (list.Count == 0) byHome.Remove(home);
            }
            int residents;
            if (residentsByHome.TryGetValue(home, out residents))
            {
                int left = residents - household.Size;
                if (left <= 0) residentsByHome.Remove(home);
                else residentsByHome[home] = left;
            }
        }

        private static int ClampNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }
    }
}
