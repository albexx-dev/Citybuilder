using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Домен drag-стройки дорог (EPIC-10 шаг 3): осевой отрезок, валидация клеток и цена.
    // Чистый C#, без Unity и без состояния.
    public static class RoadPlacementInput
    {
        // Клетки осевого отрезка от from к to: при |dx| >= |dy| идём по X, иначе по Y.
        public static List<GridPosition> GetLineCells(GridPosition from, GridPosition to)
        {
            List<GridPosition> cells = new List<GridPosition>();
            int dx = to.X - from.X;
            int dy = to.Y - from.Y;
            bool horizontal = Math.Abs(dx) >= Math.Abs(dy);
            int step = (horizontal ? dx : dy) >= 0 ? 1 : -1;
            int length = Math.Max(Math.Abs(dx), Math.Abs(dy));
            for (int i = 0; i <= length; i++)
            {
                cells.Add(horizontal
                    ? new GridPosition(from.X + i * step, from.Y)
                    : new GridPosition(from.X, from.Y + i * step));
            }
            return cells;
        }

        // Число клеток превью, которых ещё нет среди своих дорог.
        public static int CountNewCells(List<GridPosition> cells, Predicate<GridPosition> isOwnRoad)
        {
            if (cells == null) return 0;
            int count = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                if (isOwnRoad == null || !isOwnRoad(cells[i])) count++;
            }
            return count;
        }

        // Допустим ли отрезок: все клетки либо свободны для стройки, либо уже свои дороги, и казна тянет цену новых.
        public static bool CanCommit(List<GridPosition> cells, Predicate<GridPosition> canPlace,
            Predicate<GridPosition> isOwnRoad, int cellCost, Treasury treasury)
        {
            if (cells == null) return false;
            for (int i = 0; i < cells.Count; i++)
            {
                bool free = canPlace != null && canPlace(cells[i]);
                bool own = isOwnRoad != null && isOwnRoad(cells[i]);
                if (!free && !own) return false;
            }
            if (treasury != null)
            {
                int total = CountNewCells(cells, isOwnRoad) * cellCost;
                return treasury.CanAfford(total);
            }
            return true;
        }
    }
}
