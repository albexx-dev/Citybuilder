using System.Collections.Generic;

namespace Citybuilder
{
    // Снимок очереди вывоза v6 (ADR-014): запись и восстановление без событий и наград.
    public static class GarbageSnapshot
    {
        // Координаты дома и накопленный мусор; готовые очереди идут первыми.
        public static void Collect(GarbageSystem system, List<CitySnapshot.GarbageQueueEntry> entries)
        {
            if (system == null || entries == null) return;
            List<GarbageSystem.Pickup> ready = system.Ready;
            for (int i = 0; i < ready.Count; i++) AddEntry(entries, system, ready[i].Id, ready[i].At);
            foreach (KeyValuePair<EntityId, int> pair in system.Accumulated)
                AddEntry(entries, system, pair.Key, system.OriginOf(pair.Key));
        }

        private static void AddEntry(List<CitySnapshot.GarbageQueueEntry> entries, GarbageSystem system,
            EntityId id, GridPosition at)
        {
            int value = system.AccOf(id);
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].X != at.X || entries[i].Y != at.Y) continue;
                entries[i].Accumulated = value;
                return;
            }
            entries.Add(new CitySnapshot.GarbageQueueEntry { X = at.X, Y = at.Y, Accumulated = value });
        }

        // Восстановление по координатам: лента и штрафы не перепроигрываются.
        public static void Restore(GarbageSystem system, List<CitySnapshot.GarbageQueueEntry> entries,
            List<BuildingState> homes)
        {
            if (system == null || entries == null || homes == null) return;
            for (int i = 0; i < entries.Count; i++)
            {
                CitySnapshot.GarbageQueueEntry entry = entries[i];
                if (entry == null || entry.Accumulated < 0) continue;
                BuildingState home = FindHome(homes, entry.X, entry.Y);
                if (home == null || system.IsQueued(home.Id)) continue;
                system.SetAccumulated(home, entry.Accumulated);
                if (entry.Accumulated >= GarbageSystem.ReadyThreshold) system.Queue(home);
            }
        }

        private static BuildingState FindHome(List<BuildingState> homes, int x, int y)
        {
            for (int i = 0; i < homes.Count; i++)
            {
                BuildingState home = homes[i];
                if (home != null && home.Origin.X == x && home.Origin.Y == y) return home;
            }
            return null;
        }
    }
}
