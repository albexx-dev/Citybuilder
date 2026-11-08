using System.Collections.Generic;

namespace Citybuilder
{
    // Суммарное содержание за час по размещённым зданиям. Чистый C#, без Unity и без состояния.
    public static class UpkeepPolicy
    {
        // По одному элементу на каждое размещённое здание. Отрицательное и int.MinValue — как 0.
        public static int UpkeepPerHour(IEnumerable<BuildingDefinition> placed)
        {
            if (placed == null)
            {
                return 0;
            }
            int total = 0;
            foreach (BuildingDefinition definition in placed)
            {
                if (definition == null)
                {
                    continue;
                }
                total += BuildingDefinition.ClampNonNegative(definition.UpkeepPerHour);
            }
            return total;
        }

        // Выросшие дома и ручная стройка: те же списки, по которым считают спрос контроллеры.
        public static int UpkeepPerHour(List<BuildingState> grown, List<BuildingState> placed)
        {
            return SumStates(grown) + SumStates(placed);
        }

        // Один проход по списку: null-список и null-здания пропускаются.
        private static int SumStates(List<BuildingState> states)
        {
            if (states == null)
            {
                return 0;
            }
            int total = 0;
            for (int i = 0; i < states.Count; i++)
            {
                BuildingState state = states[i];
                if (state == null || state.Definition == null)
                {
                    continue;
                }
                total += BuildingDefinition.ClampNonNegative(state.Definition.UpkeepPerHour);
            }
            return total;
        }
    }
}
