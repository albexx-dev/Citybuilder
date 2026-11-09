namespace Citybuilder
{
    // Строка расхода в HUD: её текст и вид. Чистая логика без Unity, как GrowthStatusPolicy.
    public static class UpkeepStatusPolicy
    {
        // Normal — обычный белый текст, Deficit — казна в минусе, строка красная.
        public enum Look
        {
            Normal,
            Deficit
        }

        // N берётся из UpkeepPolicy без правок: при нулевом содержании строка честно показывает 0.
        public static string Text(int upkeepPerHour)
        {
            return "Расход: " + upkeepPerHour + " $/ч";
        }

        // Источник признака — Treasury.IsInDeficit (баланс < 0): он и меняет вид строки.
        public static Look Pick(bool inDeficit)
        {
            return inDeficit ? Look.Deficit : Look.Normal;
        }
    }
}
