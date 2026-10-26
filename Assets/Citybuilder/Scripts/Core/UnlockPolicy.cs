using System.Collections.Generic;

namespace Citybuilder
{
    // Правило разблокировки типов зданий по числу выросших зданий. Чистый C#, без Unity.
    // Порог берётся из BuildingDefinition.UnlockBuildings: 0 — доступно сразу.
    public static class UnlockPolicy
    {
        // Открыт ли тип. Нет описания — закрыт; отрицательное число зданий считаем нулём.
        public static bool IsUnlocked(BuildingDefinition definition, int buildings)
        {
            if (definition == null) return false;
            return Normalize(buildings) >= definition.UnlockBuildings;
        }

        // Сколько зданий не хватает до открытия. Открыто или нет описания — 0.
        public static int Remaining(BuildingDefinition definition, int buildings)
        {
            if (definition == null) return 0;
            int missing = definition.UnlockBuildings - Normalize(buildings);
            return missing > 0 ? missing : 0;
        }

        private static int Normalize(int buildings)
        {
            return buildings < 0 ? 0 : buildings;
        }

        // Ближайший закрытый тип: минимальный порог, при равенстве — первый по списку.
        // Пусто, все открыты или только null-элементы — null. Без LINQ.
        public static BuildingDefinition NextLocked(List<BuildingDefinition> definitions, int buildings)
        {
            if (definitions == null) return null;
            int grown = Normalize(buildings);
            BuildingDefinition next = null;
            for (int i = 0; i < definitions.Count; i++)
            {
                BuildingDefinition candidate = definitions[i];
                if (candidate == null) continue;
                if (candidate.UnlockBuildings <= grown) continue;
                if (next == null || candidate.UnlockBuildings < next.UnlockBuildings) next = candidate;
            }
            return next;
        }
    }
}
