namespace Citybuilder
{
    // Снимок построенного сегмента дороги: кто, какую клетку занимает и какого типа.
    public class RoadState
    {
        public EntityId Id { get; private set; }
        public GridPosition Cell { get; private set; }
        // v7 (ADR-018): Id типа дороги. Пусто и старые записи читаются как asphalt.
        public string TypeId { get; private set; }
        // v7 (ADR-018/022): уровень полотна, земля 1 / мост 2; диапазон 1..3.
        public int Level { get; private set; }

        public RoadState(EntityId id, GridPosition cell, string typeId = "asphalt", int level = 1)
        {
            Id = id;
            Cell = cell;
            TypeId = string.IsNullOrEmpty(typeId) ? "asphalt" : typeId;
            Level = level < 1 ? 1 : level > 3 ? 3 : level;
        }
    }
}
