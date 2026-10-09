using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Чистая логика стройки поверх GridSystem. Без Unity.
    public class BuildingSystem
    {
        private readonly GridSystem grid;

        public BuildingSystem(GridSystem grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException("grid");
            }
            this.grid = grid;
        }

        // Все клетки прямоугольника внутри карты и свободны.
        public bool CanPlace(BuildingDefinition definition, GridPosition origin)
        {
            if (definition == null)
            {
                return false;
            }
            List<GridPosition> cells = definition.CoversCells(origin);
            for (int i = 0; i < cells.Count; i++)
            {
                if (grid.IsOccupied(cells[i]))
                {
                    return false;
                }
            }
            return true;
        }

        // Занимает клетки новым EntityId. Вне карты или поверх — null.
        public BuildingState Place(BuildingDefinition definition, GridPosition origin)
        {
            if (!CanPlace(definition, origin))
            {
                return null;
            }
            List<GridPosition> cells = definition.CoversCells(origin);
            EntityId id = EntityId.New();
            for (int i = 0; i < cells.Count; i++)
            {
                grid.Occupy(cells[i], id);
            }
            return new BuildingState(id, definition, origin, cells);
        }

        // Освобождает клетки. Повторный снос — false.
        public bool Demolish(BuildingState state)
        {
            if (state == null || state.Cells == null)
            {
                return false;
            }
            bool releasedAny = false;
            for (int i = 0; i < state.Cells.Count; i++)
            {
                if (grid.IsOccupied(state.Cells[i]))
                {
                    grid.Release(state.Cells[i]);
                    releasedAny = true;
                }
            }
            return releasedAny;
        }
    }
}
