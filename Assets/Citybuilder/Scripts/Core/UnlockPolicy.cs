namespace Citybuilder
{
    // Правило разблокировки типов зданий по числу жителей. Чистый C#, без Unity.
    // Порог берётся из BuildingDefinition.UnlockResidents: 0 — доступно сразу.
    public static class UnlockPolicy
    {
        // Открыт ли тип. Нет описания — закрыт; отрицательное население считаем нулём.
        public static bool IsUnlocked(BuildingDefinition definition, int residents)
        {
            if (definition == null) return false;
            return Normalize(residents) >= definition.UnlockResidents;
        }

        // Сколько жителей не хватает до открытия. Открыто или нет описания — 0.
        public static int Remaining(BuildingDefinition definition, int residents)
        {
            if (definition == null) return 0;
            int missing = definition.UnlockResidents - Normalize(residents);
            return missing > 0 ? missing : 0;
        }

        private static int Normalize(int residents)
        {
            return residents < 0 ? 0 : residents;
        }
    }
}
