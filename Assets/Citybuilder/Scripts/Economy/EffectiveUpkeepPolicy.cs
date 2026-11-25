namespace Citybuilder
{
    // Медленное списание содержания: базовая сумма делится на Divisor без изменения ассетов.
    // Чистая политика без Unity и состояния; базовую сумму даёт UpkeepPolicy.
    public static class EffectiveUpkeepPolicy
    {
        // Во столько раз медленнее казна тратит содержание за час.
        public const int Divisor = 8;

        // Целочисленное деление C# детерминировано (усечение к нулю).
        // UpkeepPolicy всегда даёт неотрицательную сумму, поэтому это то же, что floor.
        public static int Apply(int baseUpkeep)
        {
            return baseUpkeep / Divisor;
        }
    }
}
