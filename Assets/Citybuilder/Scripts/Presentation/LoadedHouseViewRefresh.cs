using System.Collections.Generic;

namespace Citybuilder
{
    // Пересборка вида выросших домов после загрузки слота (EPIC-64 шаг 3).
    // Вид дома создаётся в Growth.RestoreGrown, когда уровень ещё равен 1, а восстановленные
    // уровни проставляются позже — SnapshotLevels.ApplyAll. Без этого дом 2-3 уровня какое-то
    // время выглядит как уровень 1, пока HouseLevelController не пересоберёт вид по тику.
    // Путь тот же, что у HouseLevelController.RebuildView; дома 1 уровня не трогаем — их вид
    // уже соответствует состоянию, пересобирать его значит зря пересоздать кубы.
    public static class LoadedHouseViewRefresh
    {
        public static void Apply(SettlementGrowthController growth, List<BuildingState> homes)
        {
            if (growth == null || growth.View == null || growth.GridVisualizer == null || homes == null) return;
            float cellSize = growth.GridVisualizer.Grid.CellSize;
            for (int i = 0; i < homes.Count; i++)
            {
                BuildingState home = homes[i];
                if (home == null || home.Definition == null) continue;
                if (home.Level <= HouseLevelPolicy.MinLevel) continue;
                growth.View.AddBuilding(home, home.Definition, cellSize, true);
            }
        }
    }
}
