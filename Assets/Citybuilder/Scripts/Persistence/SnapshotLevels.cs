using System.Collections.Generic;

namespace Citybuilder
{
    // Восстановление уровней зданий из слота v5 (EPIC-64 шаг 2). Чистый C#, без Unity.
    // Привязка по координатам — как у домохозяйств: порядок списка плывёт, если здание не восстановилось.
    // Level = 0 (слот v4) и битые значения клампятся HouseLevelPolicy в 1..3.
    public static class SnapshotLevels
    {
        // Полный проход после загрузки одним вызовом — им пользуется LoadController: выросшие дома
        // там восстанавливает Growth.RestoreGrown, а не LoadSystem, и оба списка нужны сразу.
        public static void ApplyAll(CitySnapshot snapshot, List<BuildingState> homes, List<BuildingState> placed)
        {
            ApplyHomes(snapshot, homes);
            ApplyManual(snapshot, placed);
        }

        // Выросшие дома: тот же список, что вернули LoadSystem.ApplyBuildings или Growth.RestoreGrown.
        public static void ApplyHomes(CitySnapshot snapshot, List<BuildingState> homes)
        {
            Apply(snapshot, snapshot == null ? null : snapshot.Buildings, homes);
        }

        // Ручные здания игрока: улучшенные станции и службы (BuildingPlacementController.Placed).
        public static void ApplyManual(CitySnapshot snapshot, List<BuildingState> placed)
        {
            Apply(snapshot, snapshot == null ? null : snapshot.ManualBuildings, placed);
        }

        private static void Apply(CitySnapshot snapshot, List<CitySnapshot.BuildingEntry> entries,
            List<BuildingState> states)
        {
            if (entries == null || states == null) return;
            for (int i = 0; i < entries.Count; i++)
            {
                CitySnapshot.BuildingEntry entry = entries[i];
                if (entry == null) continue;
                BuildingState state = FindByOrigin(states, entry.X, entry.Y);
                if (state == null) continue;
                // Уровень и серию ставит ApplyHouseTick. Серия из v6 (ADR-014) по координатам;
                // для слота v1–v5 она равна 0 — поведение как раньше.
                state.ApplyHouseTick(HouseLevelPolicy.ClampLevel(entry.Level),
                    SaveV6Snapshot.ReadStreak(snapshot, entry.X, entry.Y));
            }
        }

        private static BuildingState FindByOrigin(List<BuildingState> states, int x, int y)
        {
            for (int i = 0; i < states.Count; i++)
            {
                BuildingState state = states[i];
                if (state != null && state.Origin.X == x && state.Origin.Y == y) return state;
            }
            return null;
        }
    }
}
