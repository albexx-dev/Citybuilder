namespace Citybuilder
{
    // Снимок построенного сегмента дороги: кто и какую клетку занимает.
    public class RoadState
    {
        public EntityId Id { get; private set; }
        public GridPosition Cell { get; private set; }

        public RoadState(EntityId id, GridPosition cell)
        {
            Id = id;
            Cell = cell;
        }
    }
}
