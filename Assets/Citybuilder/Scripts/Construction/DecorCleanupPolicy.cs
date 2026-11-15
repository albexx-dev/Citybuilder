using System.Collections.Generic;

namespace Citybuilder
{
    // Чистая логика уборки декора (EPIC-56): декор уходит с клеток под зданиями и зонами.
    // Только данные: без UnityEngine, прогоняется через dotnet вне редактора.
    public static class DecorCleanupPolicy
    {
        // Клетки декора, перекрытые зданиями (occupied) или зонами (zoned). Null-списки = пропустить.
        public static List<GridPosition> CellsToRemove(List<GridPosition> decorCells, List<GridPosition> occupiedCells, List<GridPosition> zonedCells)
        {
            List<GridPosition> remove = new List<GridPosition>();
            if (decorCells == null) return remove;
            for (int i = 0; i < decorCells.Count; i++)
            {
                GridPosition cell = decorCells[i];
                if (Contains(occupiedCells, cell) || Contains(zonedCells, cell)) remove.Add(cell);
            }
            return remove;
        }

        private static bool Contains(List<GridPosition> cells, GridPosition cell)
        {
            if (cells == null) return false;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].Equals(cell)) return true;
            }
            return false;
        }
    }
}
