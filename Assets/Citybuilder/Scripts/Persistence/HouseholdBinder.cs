using System.Collections.Generic;

namespace Citybuilder
{
    // Восстановление домохозяйств в дома: по координатам (новые слоты) или по индексу (старые).
    // Индекс ненадёжен: пропущенное при загрузке здание сдвигает порядок и уводит жителей в чужие дома.
    public static class HouseholdBinder
    {
        public static void Apply(CitySnapshot snapshot, PopulationSystem population, List<BuildingState> homes)
        {
            if (snapshot == null || snapshot.Households == null || population == null || homes == null)
            {
                return;
            }
            for (int i = 0; i < snapshot.Households.Count; i++)
            {
                CitySnapshot.HouseholdEntry entry = snapshot.Households[i];
                if (entry == null || entry.Size <= 0)
                {
                    continue;
                }
                BuildingState home = FindHome(entry, homes);
                if (home != null)
                {
                    population.MoveIn(home, entry.Size);
                }
            }
        }

        private static BuildingState FindHome(CitySnapshot.HouseholdEntry entry, List<BuildingState> homes)
        {
            if (entry.HasHome)
            {
                return FindByOrigin(homes, entry.HomeX, entry.HomeY);
            }
            if (entry.BuildingIndex < 0 || entry.BuildingIndex >= homes.Count)
            {
                return null;
            }
            return homes[entry.BuildingIndex];
        }

        private static BuildingState FindByOrigin(List<BuildingState> homes, int x, int y)
        {
            for (int i = 0; i < homes.Count; i++)
            {
                BuildingState home = homes[i];
                if (home != null && home.Origin.X == x && home.Origin.Y == y)
                {
                    return home;
                }
            }
            return null;
        }
    }
}
