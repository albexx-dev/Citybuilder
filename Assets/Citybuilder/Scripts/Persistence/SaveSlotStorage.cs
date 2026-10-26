using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Citybuilder
{
    // Слот на диске: JSON через JsonUtility, проверка, чтение и запись файла.
    public static class SaveSlotStorage
    {
        public const string SlotFileName = "slot0.json";

        public static string ToJson(CitySnapshot snapshot)
        {
            if (snapshot == null)
            {
                return string.Empty;
            }
            return JsonUtility.ToJson(snapshot);
        }
        public static CitySnapshot FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            try
            {
                return JsonUtility.FromJson<CitySnapshot>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static bool Validate(CitySnapshot snapshot)
        {
            if (snapshot == null || !IsSupportedVersion(snapshot.Version))
            {
                return false;
            }
            if (snapshot.TickCount < 0 || snapshot.Balance < 0)
            {
                return false;
            }
            if (snapshot.Zones == null || snapshot.Buildings == null)
            {
                return false;
            }
            if (snapshot.Households == null || snapshot.Roads == null)
            {
                return false;
            }
            // Ручные здания (v3): поле может отсутствовать в слотах v1/v2 — это не ошибка.
            if (snapshot.ManualBuildings != null && !ValidateBuildings(snapshot.ManualBuildings)) return false;
            return ValidateZones(snapshot.Zones) && ValidateBuildings(snapshot.Buildings)
                && ValidateHouseholds(snapshot.Households, snapshot.Buildings.Count)
                && ValidateRoads(snapshot.Roads);
        }
        // Версии 1..3: v1 без TaxLevel, v2 с TaxLevel, v3 с ручными зданиями.
        private static bool IsSupportedVersion(int version)
        {
            return version >= 1 && version <= SaveSystem.CurrentVersion;
        }
        public static bool WriteSlot(string path, CitySnapshot snapshot)
        {
            if (string.IsNullOrEmpty(path) || snapshot == null)
            {
                return false;
            }
            try
            {
                File.WriteAllText(path, ToJson(snapshot));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static CitySnapshot ReadSlot(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }
            try
            {
                return FromJson(File.ReadAllText(path));
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static bool ValidateZones(List<CitySnapshot.ZoneEntry> zones)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i] == null)
                {
                    return false;
                }
                if (zones[i].Type < (int)ZoneType.Residential || zones[i].Type > (int)ZoneType.Industrial)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool ValidateBuildings(List<CitySnapshot.BuildingEntry> buildings)
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i] == null || string.IsNullOrEmpty(buildings[i].DefId))
                {
                    return false;
                }
            }
            return true;
        }
        private static bool ValidateHouseholds(List<CitySnapshot.HouseholdEntry> households, int buildingCount)
        {
            for (int i = 0; i < households.Count; i++)
            {
                if (households[i] == null || households[i].Size <= 0)
                {
                    return false;
                }
                if (households[i].BuildingIndex < 0 || households[i].BuildingIndex >= buildingCount)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool ValidateRoads(List<CitySnapshot.RoadEntry> roads)
        {
            for (int i = 0; i < roads.Count; i++)
            {
                if (roads[i] == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
