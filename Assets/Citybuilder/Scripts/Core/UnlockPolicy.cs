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
    }
}
