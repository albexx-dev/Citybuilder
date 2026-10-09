using System;

namespace Citybuilder
{
    // Чистая логика стройки дорог поверх GridSystem. Без Unity.
    public class RoadSystem
    {
        private readonly GridSystem grid;

        public RoadSystem(GridSystem grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException("grid");
            }
            this.grid = grid;
        }

        // Клетка внутри карты и свободна.
        public bool CanPlace(GridPosition cell)
        {
            return !grid.IsOccupied(cell);
        }

        // Занимает клетку новым EntityId. Вне карты или поверх — null.
        public RoadState Place(GridPosition cell)
        {
            if (!CanPlace(cell))
            {
                return null;
            }
            EntityId id = EntityId.New();
            grid.Occupy(cell, id);
            return new RoadState(id, cell);
        }

        // Освобождает клетку. Повторный снос — false.
        public bool Demolish(RoadState state)
        {
            if (state == null)
            {
                return false;
            }
            if (!grid.IsOccupied(state.Cell))
            {
                return false;
            }
            grid.Release(state.Cell);
            return true;
        }

        // Цена одной клетки. Null и отрицательный cost — 0.
        public int CellCost(RoadDefinition definition)
        {
            if (definition == null)
            {
                return 0;
            }
            return Math.Max(0, definition.CostPerCell);
        }
    }
}
