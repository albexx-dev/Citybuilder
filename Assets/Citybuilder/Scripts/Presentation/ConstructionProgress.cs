namespace Citybuilder
{
    // Чистая математика фазы стройки (EPIC-61): прогресс и готовность без Unity.
    // Прогресс = прошедшие тики / ConstructionTicks из BuildingDefinition; 0 тиков — мгновенно.
    public static class ConstructionProgress
    {
        // Доля готовности 0..1: нулевая длительность — здание готово сразу.
        public static float Progress(int elapsedTicks, int totalTicks)
        {
            if (totalTicks <= 0) return 1f;
            if (elapsedTicks <= 0) return 0f;
            if (elapsedTicks >= totalTicks) return 1f;
            return (float)elapsedTicks / totalTicks;
        }

        // Здание достроено: длительность исчерпана (или была нулевой).
        public static bool IsDone(int elapsedTicks, int totalTicks)
        {
            return totalTicks <= 0 || elapsedTicks >= totalTicks;
        }

        // Длительность не бывает отрицательной: -N превращается в 0 (мгновенно).
        public static int ClampTicks(int ticks)
        {
            return ticks < 0 ? 0 : ticks;
        }
    }
}
