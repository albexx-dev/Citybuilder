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

        // Уровень дома 1..3 (EPIC-59). ВАЖНО: в снапшоте сейва v4 НЕ сериализуется
        // (v5 готовит #64) — после загрузки дома снова уровня 1.
        public int Level { get; private set; }
        // Серия тиков комфорта для гистерезиса HouseLevelPolicy: >0 хороших подряд, <0 плохих.
        public int ComfortStreak { get; private set; }

        public BuildingState(EntityId id, BuildingDefinition definition, GridPosition origin, List<GridPosition> cells)
        {
            Id = id;
            Definition = definition;
            Origin = origin;
            Cells = new List<GridPosition>(cells);
            Level = HouseLevelPolicy.MinLevel;
        }

        // Обновление уровня от HouseLevelPolicy по тику: уровень клампится 1..3, серия берётся как есть.
        public void ApplyHouseTick(int level, int comfortStreak)
        {
            if (level < HouseLevelPolicy.MinLevel) level = HouseLevelPolicy.MinLevel;
            if (level > HouseLevelPolicy.MaxLevel) level = HouseLevelPolicy.MaxLevel;
            Level = level;
            ComfortStreak = comfortStreak;
        }

        // Апгрейд станции или службы на следующий уровень (EPIC-51): только вручную, потолок общий 1..3.
        public void ApplyUpgrade()
        {
            Level = HouseLevelPolicy.ClampLevel(Level + 1);
        }
    }
}
