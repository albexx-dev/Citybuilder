namespace Citybuilder
{
    // Строка бюджета для HUD: доход, содержание и итог за час. Чистая логика без Unity.
    public static class BudgetSummaryPolicy
    {
        // «Доход: +24 $/ч - Расход: 10 $/ч = Итог: +14 $/ч». Доход и итог всегда со знаком.
        public static string Line(int incomePerHour, int upkeepPerHour)
        {
            int net = NetPerHour(incomePerHour, upkeepPerHour);
            return "Доход: +" + incomePerHour + " $/ч - Расход: " + upkeepPerHour
                + " $/ч = Итог: " + Signed(net) + " $/ч";
        }

        // Итог часа: доход минус содержание.
        public static int NetPerHour(int incomePerHour, int upkeepPerHour)
        {
            return incomePerHour - upkeepPerHour;
        }

        // Итог ниже нуля — строка бюджета красится как дефицитная.
        public static bool IsNetDeficit(int incomePerHour, int upkeepPerHour)
        {
            return NetPerHour(incomePerHour, upkeepPerHour) < 0;
        }

        private static string Signed(int value)
        {
            return value >= 0 ? "+" + value : value.ToString();
        }
    }
}
