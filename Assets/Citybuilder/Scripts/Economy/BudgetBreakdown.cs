namespace Citybuilder
{
    // Разложение бюджета за час (issue #96): налоги − содержание − кражи + награды = итог.
    // Чистая политика: без Unity, без состояния; текст берётся из реестра локализации.
    public static class BudgetBreakdown
    {
        // Итог часа. Кражи и награды уже приведены к одному часу (1 тик = 1 час).
        public static int Net(int income, int upkeep, int thefts, int rewards)
        {
            return income - upkeep - thefts + rewards;
        }

        // Отрицательный итог — легальное состояние: строка HUD красится как дефицитная.
        public static bool IsDeficit(int income, int upkeep, int thefts, int rewards)
        {
            return Net(income, upkeep, thefts, rewards) < 0;
        }

        // Одна строка HUD: доход и награды со знаком, расход и кражи — величиной.
        public static string Line(int income, int upkeep, int thefts, int rewards)
        {
            int net = Net(income, upkeep, thefts, rewards);
            return Localization.T("budget_breakdown", Signed(income), upkeep.ToString(),
                thefts.ToString(), Signed(rewards), Signed(net));
        }

        // Новая строка с долгом (issue #168): процент тика уменьшает итог, тело долга — в хвосте.
        // Старая Line и её ключ budget_breakdown остаются без изменений.
        public static string LineWithCredit(int income, int upkeep, int thefts, int rewards, int debt, int interest)
        {
            int net = Net(income, upkeep, thefts, rewards) - interest;
            return Localization.T("budget_breakdown_credit", Signed(income), upkeep.ToString(),
                thefts.ToString(), Signed(rewards), interest.ToString(), Signed(net), debt.ToString());
        }

        private static string Signed(int value)
        {
            return value >= 0 ? "+" + value : value.ToString();
        }
    }
}
