namespace Citybuilder
{
    // Почему город растёт или стоит. Чистая логика без Unity.
    public static class GrowthStatusPolicy
    {
        public enum Kind
        {
            None,
            GrowthOff,
            NoZones,
            AtCapacity,
            NoMoney,
            Growing
        }

        // None — подсказку роста не рисуем: до первого дома её заменяет стартовая.
        // Порядок важен: сначала то, что игрок чинит одной кнопкой.
        public static Kind Pick(int grown, int residentialCells, bool growthMode,
            int maxSettlements, int cheapestCost, int balance)
        {
            if (grown <= 0) return Kind.None;
            if (!growthMode) return Kind.GrowthOff;
            if (residentialCells <= 0) return Kind.NoZones;
            if (grown >= maxSettlements) return Kind.AtCapacity;
            if (balance < cheapestCost) return Kind.NoMoney;
            return Kind.Growing;
        }

        // Русское склонение: 1 дом, 2 дома, 5 домов, 11 домов, 21 дом.
        public static string HousesWord(int count)
        {
            int mod100 = count % 100;
            if (mod100 >= 11 && mod100 <= 14) return "домов";
            int mod10 = count % 10;
            if (mod10 == 1) return "дом";
            if (mod10 >= 2 && mod10 <= 4) return "дома";
            return "домов";
        }
    }
}
