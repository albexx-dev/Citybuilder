using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Строит снапшот города из живых систем. Чистый C#, без Unity.
    public static class SaveSystem
    {
        public const int CurrentVersion = 2;

        // Слот без налога (старая сигнатура): TaxLevel = 0, при чтении это «обычный».
        public static CitySnapshot BuildSnapshot(
            List<BuildingState> homes,
            List<HouseholdState> households,
            List<KeyValuePair<GridPosition, ZoneType>> zones,
            int balance,
            long tickCount,
            List<RoadState> roads)
        {
            return BuildSnapshot(homes, households, zones, balance, tickCount, roads, 0);
        }

        public static CitySnapshot BuildSnapshot(
            List<BuildingState> homes,
            List<HouseholdState> households,
            List<KeyValuePair<GridPosition, ZoneType>> zones,
            int balance,
            long tickCount,
            List<RoadState> roads,
            int taxLevel)
        {
            CitySnapshot snapshot = new CitySnapshot();
            snapshot.Version = CurrentVersion;
            snapshot.Balance = Math.Max(0, balance);
            snapshot.TickCount = Math.Max(0, tickCount);
            // Допустимы только 1..3; всё прочее пишется как «не задано».
            snapshot.TaxLevel = taxLevel >= 1 && taxLevel <= 3 ? taxLevel : 0;
            AddZones(snapshot, zones);
            Dictionary<BuildingState, int> indexByHome = AddBuildings(snapshot, homes);
            AddHouseholds(snapshot, households, indexByHome);
            AddRoads(snapshot, roads);
            return snapshot;
        }

        // Кодирование уровня в слот: низкий → 1, обычный → 2, высокий → 3.
        public static int EncodeTaxLevel(TaxPolicy.Level level)
        {
            return (int)level + 1;
        }

        // Чтение уровня из слота: 0 (слот v1) и неизвестное → обычный.
        public static TaxPolicy.Level DecodeTaxLevel(int taxLevel)
        {
            if (taxLevel == 1) return TaxPolicy.Level.Low;
            if (taxLevel == 3) return TaxPolicy.Level.High;
            return TaxPolicy.Level.Normal;
        }

        private static void AddZones(CitySnapshot snapshot, List<KeyValuePair<GridPosition, ZoneType>> zones)
        {
            if (zones == null)
            {
                return;
            }
            for (int i = 0; i < zones.Count; i++)
            {
                CitySnapshot.ZoneEntry entry = new CitySnapshot.ZoneEntry();
                entry.X = zones[i].Key.X;
                entry.Y = zones[i].Key.Y;
                entry.Type = (int)zones[i].Value;
                snapshot.Zones.Add(entry);
            }
        }

        private static Dictionary<BuildingState, int> AddBuildings(CitySnapshot snapshot, List<BuildingState> homes)
        {
            Dictionary<BuildingState, int> indexByHome = new Dictionary<BuildingState, int>();
            if (homes == null)
            {
                return indexByHome;
            }
            for (int i = 0; i < homes.Count; i++)
            {
                BuildingState home = homes[i];
                if (home == null || home.Definition == null || indexByHome.ContainsKey(home))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(home.Definition.Id))
                {
                    continue;
                }
                CitySnapshot.BuildingEntry entry = new CitySnapshot.BuildingEntry();
                entry.DefId = home.Definition.Id;
                entry.X = home.Origin.X;
                entry.Y = home.Origin.Y;
                indexByHome.Add(home, snapshot.Buildings.Count);
                snapshot.Buildings.Add(entry);
            }
            return indexByHome;
        }

        private static void AddHouseholds(CitySnapshot snapshot, List<HouseholdState> households, Dictionary<BuildingState, int> indexByHome)
        {
            if (households == null || indexByHome == null)
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
                snapshot.Households.Add(entry);
            }
        }

        private static void AddRoads(CitySnapshot snapshot, List<RoadState> roads)
        {
            if (roads == null)
            {
                return;
            }
            for (int i = 0; i < roads.Count; i++)
            {
                if (roads[i] == null)
                {
                    continue;
                }
                CitySnapshot.RoadEntry entry = new CitySnapshot.RoadEntry();
                entry.X = roads[i].Cell.X;
                entry.Y = roads[i].Cell.Y;
                snapshot.Roads.Add(entry);
            }
        }
    }
}
