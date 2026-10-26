using System.Collections.Generic;

namespace Citybuilder
{
    // Запись домохозяйств в слот: индекс дома (совместимость) и координаты (v4+).
    public static class SnapshotHouseholds
    {
        public static void Write(CitySnapshot snapshot, List<HouseholdState> households,
            Dictionary<BuildingState, int> indexByHome)
        {
            if (snapshot == null || households == null || indexByHome == null)
            {
                return;
            }
            for (int i = 0; i < households.Count; i++)
            {
                HouseholdState household = households[i];
                if (household == null || household.Home == null || household.Size <= 0)
                {
                    continue;
                }
                int index;
                if (!indexByHome.TryGetValue(household.Home, out index))
                {
                    continue;
                }
                CitySnapshot.HouseholdEntry entry = new CitySnapshot.HouseholdEntry();
                entry.BuildingIndex = index;
                entry.Size = household.Size;
                entry.HasHome = true;
                entry.HomeX = household.Home.Origin.X;
                entry.HomeY = household.Home.Origin.Y;
                snapshot.Households.Add(entry);
            }
        }
    }
}
