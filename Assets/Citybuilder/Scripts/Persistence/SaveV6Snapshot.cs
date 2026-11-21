using System.Collections.Generic;

namespace Citybuilder
{
    // Опциональные поля слота v6 (ADR-014): запись и чтение без обязательного присутствия в JSON.
    // Старые слоты v1–v5 остаются валидными: каждое отсутствующее поле читается как пусто/0.
    public static class SaveV6Snapshot
    {
        public const int Version = 6;

        // Запись доступных систем. Отсутствующая система даёт пустое значение — как слот v5.
        public static void Write(CitySnapshot snapshot, GarbageSystem garbage, CrimeSystem crimes,
            List<BuildingState> homes, List<CitySnapshot.ConstructionProgressEntry> sites)
        {
            if (snapshot == null) return;
            snapshot.GarbageQueue = new List<CitySnapshot.GarbageQueueEntry>();
            snapshot.ComfortStreak = new List<CitySnapshot.ComfortStreakEntry>();
            snapshot.ConstructionProgress = new List<CitySnapshot.ConstructionProgressEntry>();
            if (garbage != null) garbage.CollectSnapshot(snapshot.GarbageQueue);
            snapshot.TheftCounter = crimes == null ? 0 : crimes.TotalThefts;
            WriteComfort(snapshot, homes);
            if (sites != null) snapshot.ConstructionProgress.AddRange(sites);
        }

        // Серия комфорта дома привязана к координатам: порядок списка зданий при загрузке плывёт.
        private static void WriteComfort(CitySnapshot snapshot, List<BuildingState> homes)
        {
            if (homes == null) return;
            for (int i = 0; i < homes.Count; i++)
            {
                BuildingState home = homes[i];
                if (home == null) continue;
                CitySnapshot.ComfortStreakEntry entry = new CitySnapshot.ComfortStreakEntry();
                entry.X = home.Origin.X;
                entry.Y = home.Origin.Y;
                entry.Streak = home.ComfortStreak;
                snapshot.ComfortStreak.Add(entry);
            }
        }

        public const int MapCells = 50;
        public const int MaxConstructionTicks = 10000;
        public const int MaxStreakMagnitude = 10000;

        // Нормализация только полей v6 перед применением слота; v1–v5 не отклоняются и не меняются.
        public static void Normalize(CitySnapshot snapshot)
        {
            if (snapshot == null) return;
            snapshot.GarbageQueue = NormalizeGarbage(snapshot.GarbageQueue);
            snapshot.ConstructionProgress = NormalizeSites(snapshot.ConstructionProgress);
            snapshot.ComfortStreak = NormalizeStreaks(snapshot.ComfortStreak);
            if (snapshot.TheftCounter < 0) snapshot.TheftCounter = 0;
        }
        private static List<CitySnapshot.GarbageQueueEntry> NormalizeGarbage(
            List<CitySnapshot.GarbageQueueEntry> source)
        {
            List<CitySnapshot.GarbageQueueEntry> result = new List<CitySnapshot.GarbageQueueEntry>();
            Dictionary<long, int> indexByCell = new Dictionary<long, int>();
            if (source == null) return result;
            int limit = MapCells * MapCells;
            for (int i = 0; i < source.Count; i++)
            {
                CitySnapshot.GarbageQueueEntry entry = source[i];
                if (entry == null || !InMap(entry.X, entry.Y) || entry.Accumulated < 0) continue;
                ReplaceOrAdd(result, indexByCell, entry, entry.X, entry.Y, limit);
            }
            return result;
        }

        private static List<CitySnapshot.ConstructionProgressEntry> NormalizeSites(
            List<CitySnapshot.ConstructionProgressEntry> source)
        {
            List<CitySnapshot.ConstructionProgressEntry> result = new List<CitySnapshot.ConstructionProgressEntry>();
            Dictionary<long, int> indexByCell = new Dictionary<long, int>();
            if (source == null) return result;
            int limit = MapCells * MapCells;
            for (int i = 0; i < source.Count; i++)
            {
                CitySnapshot.ConstructionProgressEntry entry = source[i];
                if (entry == null || !InMap(entry.X, entry.Y) || string.IsNullOrEmpty(entry.DefId)) continue;
                if (entry.TicksElapsed < 0 || entry.TicksElapsed > MaxConstructionTicks) continue;
                ReplaceOrAdd(result, indexByCell, entry, entry.X, entry.Y, limit);
            }
            return result;
        }

        private static List<CitySnapshot.ComfortStreakEntry> NormalizeStreaks(
            List<CitySnapshot.ComfortStreakEntry> source)
        {
            List<CitySnapshot.ComfortStreakEntry> result = new List<CitySnapshot.ComfortStreakEntry>();
            Dictionary<long, int> indexByCell = new Dictionary<long, int>();
            if (source == null) return result;
            int limit = MapCells * MapCells;
            for (int i = 0; i < source.Count; i++)
            {
                CitySnapshot.ComfortStreakEntry entry = source[i];
                if (entry == null || !InMap(entry.X, entry.Y) || entry.Streak < -MaxStreakMagnitude
                    || entry.Streak > MaxStreakMagnitude) continue;
                ReplaceOrAdd(result, indexByCell, entry, entry.X, entry.Y, limit);
            }
            return result;
        }
        // Повтор по клетке заменяет прежнюю запись: в списке остаётся последняя.
        private static void ReplaceOrAdd<T>(List<T> result, Dictionary<long, int> indexByCell,
            T entry, int x, int y, int limit)
        {
            long key = (long)x * MapCells + y;
            int index;
            if (indexByCell.TryGetValue(key, out index)) { result[index] = entry; return; }
            if (result.Count >= limit) return;
            indexByCell.Add(key, result.Count);
            result.Add(entry);
        }
        private static bool InMap(int x, int y)
        {
            return x >= 0 && y >= 0 && x < MapCells && y < MapCells;
        }
        // Чтение: null-списки старых слотов заменяются пустыми, счётчик не уходит в минус.
        public static List<CitySnapshot.GarbageQueueEntry> ReadGarbage(CitySnapshot snapshot)
        {
            return snapshot == null || snapshot.GarbageQueue == null
                ? new List<CitySnapshot.GarbageQueueEntry>() : snapshot.GarbageQueue;
        }

        public static List<CitySnapshot.ConstructionProgressEntry> ReadSites(CitySnapshot snapshot)
        {
            return snapshot == null || snapshot.ConstructionProgress == null
                ? new List<CitySnapshot.ConstructionProgressEntry>() : snapshot.ConstructionProgress;
        }

        public static int ReadThefts(CitySnapshot snapshot)
        {
            if (snapshot == null || snapshot.TheftCounter < 0) return 0;
            return snapshot.TheftCounter;
        }

        // Серия дома из v6; для слота v5 и чужой клетки честный fallback — 0.
        public static int ReadStreak(CitySnapshot snapshot, int x, int y)
        {
            if (snapshot == null || snapshot.ComfortStreak == null) return 0;
            List<CitySnapshot.ComfortStreakEntry> entries = snapshot.ComfortStreak;
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                CitySnapshot.ComfortStreakEntry entry = entries[i];
                if (entry != null && entry.X == x && entry.Y == y) return entry.Streak;
            }
            return 0;
        }
    }
}
