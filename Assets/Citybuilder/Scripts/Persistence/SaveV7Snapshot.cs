using System.Collections.Generic;

namespace Citybuilder
{
    // Опциональные поля слота v7 (ADR-022): запись, чтение и нормализация без исключений.
    // Отсутствующие поля v1–v6 читаются JsonUtility и получают нейтральные дефолты.
    public static class SaveV7Snapshot
    {
        public const int Version = 7;
        public const int MapCells = 50;
        public const int MaxRoadType = 3;
        public const int MaxRoadLevel = 3;
        // Запись нейтральных значений, если доменные системы ещё не наполнили поля.
        public static void Write(CitySnapshot snapshot)
        {
            if (snapshot == null) return;
            if (snapshot.DistrictTaxRates == null)
                snapshot.DistrictTaxRates = new List<CitySnapshot.DistrictTaxEntry>();
            Normalize(snapshot);
        }
        // Нормализация только полей v7; слоты v1–v6 не отклоняются и не меняются иначе.
        public static void Normalize(CitySnapshot snapshot)
        {
            if (snapshot == null) return;
            NormalizeRoads(snapshot.Roads);
            if (snapshot.Debt < 0f || !IsFinite(snapshot.Debt)) snapshot.Debt = 0f;
            if (!IsFinite(snapshot.DebtInterestPercent) || snapshot.DebtInterestPercent < 0f
                || snapshot.DebtInterestPercent > 100f) snapshot.DebtInterestPercent = 0f;
            if (!IsFinite(snapshot.MarketMultiplier) || snapshot.MarketMultiplier <= 0f) snapshot.MarketMultiplier = 1f;
            if (snapshot.MayorRating < 0 || snapshot.MayorRating > 100) snapshot.MayorRating = 0;
            if (snapshot.MilestoneCounterV2 < 0) snapshot.MilestoneCounterV2 = 0;
            snapshot.DistrictTaxRates = NormalizeDistricts(snapshot.DistrictTaxRates);
        }
        // Некорректный тип/уровень → дефолт, дорога не удаляется; дубли по (X,Y,Level) — последняя.
        private static void NormalizeRoads(List<CitySnapshot.RoadEntry> roads)
        {
            if (roads == null) return;
            List<CitySnapshot.RoadEntry> result = new List<CitySnapshot.RoadEntry>();
            Dictionary<long, int> indexByCell = new Dictionary<long, int>();
            for (int i = 0; i < roads.Count; i++)
            {
                CitySnapshot.RoadEntry road = roads[i];
                if (road == null) continue;
                if (road.Type < 0 || road.Type > MaxRoadType) road.Type = 0;
                if (road.Level == 0) road.Level = 1;
                if (road.Level < 1 || road.Level > MaxRoadLevel) road.Level = 1;
                long key = ((long)road.X * MapCells + road.Y) * 4 + road.Level;
                int index;
                if (indexByCell.TryGetValue(key, out index)) { result[index] = road; continue; }
                indexByCell.Add(key, result.Count);
                result.Add(road);
            }
            roads.Clear();
            roads.AddRange(result);
        }

        // Пустой id и ставка вне 1..3 отбрасываются; дубли по id оставляют последнюю запись.
        private static List<CitySnapshot.DistrictTaxEntry> NormalizeDistricts(
            List<CitySnapshot.DistrictTaxEntry> source)
        {
            List<CitySnapshot.DistrictTaxEntry> result = new List<CitySnapshot.DistrictTaxEntry>();
            if (source == null) return result;
            Dictionary<string, int> indexById = new Dictionary<string, int>();
            for (int i = 0; i < source.Count; i++)
            {
                CitySnapshot.DistrictTaxEntry entry = source[i];
                if (entry == null || string.IsNullOrEmpty(entry.DistrictId)) continue;
                if (entry.Rate < 1 || entry.Rate > 3) continue;
                int index;
                if (indexById.TryGetValue(entry.DistrictId, out index)) { result[index] = entry; continue; }
                if (result.Count >= MapCells * MapCells) continue;
                indexById.Add(entry.DistrictId, result.Count);
                result.Add(entry);
            }
            return result;
        }
        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
        // Fallback для дорог: тип 0 и уровень 1, если поле отсутствует или бито.
        public static int ReadRoadType(CitySnapshot.RoadEntry road)
        {
            return road != null && road.Type >= 0 && road.Type <= MaxRoadType ? road.Type : 0;
        }
        public static int ReadRoadLevel(CitySnapshot.RoadEntry road)
        {
            return road != null && road.Level >= 1 && road.Level <= MaxRoadLevel ? road.Level : 1;
        }
    }
}
