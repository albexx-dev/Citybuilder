using System.Collections.Generic;

namespace Citybuilder
{
    // Восстановление города из снапшота. Чистый C#, без Unity.
    // Порядок: зоны -> дороги -> дома -> жители. Баланс/часы — хуками снаружи.
    public static class LoadSystem
    {
        // Полный проход: возвращает восстановленные дома для вида и заселения.
        public static List<BuildingState> Apply(
            CitySnapshot snapshot,
            ZoneSystem zones,
            RoadSystem roads,
            SettlementSystem settlement,
            PopulationSystem population,
            List<BuildingDefinition> definitions)
        {
            List<BuildingState> homes = new List<BuildingState>();
            if (snapshot == null)
            {
                return homes;
            }
            ApplyZones(snapshot, zones);
            ApplyRoads(snapshot, roads);
            ApplyBuildings(snapshot, settlement, definitions, homes);
            ApplyHouseholds(snapshot, population, homes);
            return homes;
        }

        public static void ApplyZones(CitySnapshot snapshot, ZoneSystem zones)
        {
            if (snapshot == null || snapshot.Zones == null || zones == null)
            {
                return;
            }
            for (int i = 0; i < snapshot.Zones.Count; i++)
            {
                CitySnapshot.ZoneEntry entry = snapshot.Zones[i];
                if (entry == null)
                {
                    continue;
                }
                if (entry.Type < (int)ZoneType.Residential || entry.Type > (int)ZoneType.Industrial)
                {
                    continue;
                }
                zones.SetZone(new GridPosition(entry.X, entry.Y), (ZoneType)entry.Type);
            }
        }

        public static void ApplyRoads(CitySnapshot snapshot, RoadSystem roads)
        {
            if (snapshot == null || snapshot.Roads == null || roads == null)
            {
                return;
            }
            for (int i = 0; i < snapshot.Roads.Count; i++)
            {
                CitySnapshot.RoadEntry entry = snapshot.Roads[i];
                if (entry == null)
                {
                    continue;
                }
                roads.Place(new GridPosition(entry.X, entry.Y));
            }
        }

        public static void ApplyBuildings(
            CitySnapshot snapshot,
            SettlementSystem settlement,
            List<BuildingDefinition> definitions,
            List<BuildingState> homes)
        {
            if (snapshot == null || snapshot.Buildings == null || settlement == null || homes == null)
            {
                return;
            }
            for (int i = 0; i < snapshot.Buildings.Count; i++)
            {
                CitySnapshot.BuildingEntry entry = snapshot.Buildings[i];
                if (entry == null)
                {
                    continue;
                }
                BuildingState state = settlement.Settle(FindDefinition(definitions, entry.DefId), new GridPosition(entry.X, entry.Y));
                if (state != null)
                {
                    homes.Add(state);
                }
            }
        }

        public static void ApplyHouseholds(CitySnapshot snapshot, PopulationSystem population, List<BuildingState> homes)
        {
            HouseholdBinder.Apply(snapshot, population, homes);
        }

        private static BuildingDefinition FindDefinition(List<BuildingDefinition> definitions, string defId)
        {
            if (definitions == null || string.IsNullOrEmpty(defId))
            {
                return null;
            }
            for (int i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] != null && definitions[i].Id == defId)
                {
                    return definitions[i];
                }
            }
            return null;
        }
    }
}
