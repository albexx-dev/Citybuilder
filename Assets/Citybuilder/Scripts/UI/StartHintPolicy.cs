namespace Citybuilder
{
    // Первый шаг игрока: зоны → рост → первый дом. Чистая логика без Unity.
    public static class StartHintPolicy
    {
        public enum Step
        {
            PaintZones,
            EnableGrowth,
            WaitForHouse,
            Done
        }

        // Подсказка нужна, пока не вырос первый дом. Режим «Рост» без зон
        // всё равно возвращает PaintZones: без зон расти нечему.
        public static Step Pick(int residentialCells, int grownCount, bool growthMode)
        {
            if (grownCount > 0) return Step.Done;
            if (residentialCells <= 0) return Step.PaintZones;
            if (!growthMode) return Step.EnableGrowth;
            return Step.WaitForHouse;
        }
    }
}
