namespace Citybuilder
{
    // Рейтинг мэра (ADR-019): бесконечный производный счётчик из вех и состояния города.
    // Не хранится отдельно, не имеет финиша; последствия — лента и модификаторы политик.
    public class MayorRating
    {
        public const string ChangedEventKey = EventMessageCatalog.MayorRatingChanged;
        private const int MilestoneStep = 50;
        private const int PerQuest = 10;
        private const int PerMilestone = 4;

        // CompletedCount, вехи жителей (каждые +50) и баланс казны. Минус не вычитает.
        public static int Compute(int completed, int residents, int balance)
        {
            long value = 0;
            if (completed > 0) value += (long)completed * PerQuest;
            if (residents > 0) value += (long)(residents / MilestoneStep) * PerMilestone;
            if (balance > 0) value += balance / 1000;
            return Clamp(value);
        }

        // Уровень последствий 0..3 и выше: потолка и терминального состояния нет.
        public static int Tier(int rating)
        {
            if (rating >= 120) return 3;
            if (rating >= 60) return 2;
            if (rating >= 25) return 1;
            return 0;
        }

        // Модификатор ставки налога поверх базовой ставки зоны.
        public static int TaxBonus(int rating)
        {
            return Tier(rating);
        }

        public static int ModifyRate(int rate, int rating)
        {
            if (rate <= 0) return 0;
            return Clamp((long)rate + TaxBonus(rating));
        }

        // Событие в ленту только при смене уровня: без экрана и блокирующих режимов.
        public static bool Record(CityEventLog log, int tick, int rating, int previousRating)
        {
            if (log == null || Tier(rating) == Tier(previousRating)) return false;
            return log.Add(EventKind.Info, ChangedEventKey,
                new object[] { rating, Tier(rating), TaxBonus(rating) }, tick);
        }

        // Строка «Рейтинг мэра: 120 (уровень 3, +3 $/ч к налогам)» на текущем языке.
        public static string Label(int completed, int residents, int balance)
        {
            int rating = Compute(completed, residents, balance);
            return Localization.T("mayor_rating_line", rating, Tier(rating), TaxBonus(rating));
        }

        private static int Clamp(long value)
        {
            if (value <= 0) return 0;
            if (value > int.MaxValue) return int.MaxValue;
            return (int)value;
        }
    }
}
