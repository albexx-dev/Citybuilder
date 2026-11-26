namespace Citybuilder
{
    // Рыночная цена (ADR-019): чистая политика без Unity и глобального состояния.
    // Применяется только в точках цены и строках HUD; дефолт ×1 не меняет базовые суммы.
    public class MarketPricePolicy
    {
        public const double DefaultMultiplier = 1.0;

        public double Multiplier { get; private set; }

        public bool IsDefault { get { return Multiplier == DefaultMultiplier; } }

        public MarketPricePolicy() : this(DefaultMultiplier) { }

        public MarketPricePolicy(double multiplier) { Restore(multiplier); }

        // Нечисло, бесконечность и неположительный множитель — нейтральный дефолт.
        public void Restore(double multiplier)
        {
            bool valid = !double.IsNaN(multiplier) && !double.IsInfinity(multiplier) && multiplier > 0.0;
            Multiplier = valid ? multiplier : DefaultMultiplier;
        }

        // Дефолт возвращает базу тождественно; иначе — усечение к неотрицательному целому.
        public int Apply(int basePrice)
        {
            if (IsDefault) return basePrice;
            double scaled = basePrice * Multiplier;
            if (scaled <= 0.0) return 0;
            if (scaled >= int.MaxValue) return int.MaxValue;
            return (int)scaled;
        }
    }
}
