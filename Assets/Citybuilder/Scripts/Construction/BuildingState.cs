using System.Collections.Generic;

namespace Citybuilder
{
    // Снимок построенного здания: кто, что и какие клетки занимает.
    public class BuildingState
    {
        public EntityId Id { get; private set; }
        public BuildingDefinition Definition { get; private set; }
        public GridPosition Origin { get; private set; }
        public List<GridPosition> Cells { get; private set; }

        public BuildingState(EntityId id, BuildingDefinition definition, GridPosition origin, List<GridPosition> cells)
        {
            Id = id;
            Definition = definition;
            Origin = origin;
            Cells = new List<GridPosition>(cells);
        }
    }
}
