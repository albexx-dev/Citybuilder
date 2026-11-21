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
