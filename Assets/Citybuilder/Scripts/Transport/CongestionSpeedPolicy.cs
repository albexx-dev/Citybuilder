namespace Citybuilder
{
    // Скорость машины на клетке от нагрузки (EPIC-45, agent-slowdown). Чистый C#, без Unity.
    // Паттерн 084: NaN на входе безопасен — полная скорость.
    public static class CongestionSpeedPolicy
    {
        // Потеря скорости при полной загрузке клетки: Load01 = 1 → 30% скорости.
        public const float Slowdown = 0.7f;

        // Множитель скорости клетки: 1 - 0.7 * Load01. NaN → 1, вне [0..1] — зажим.
        public static float SpeedForLoad01(float load01)
        {
            if (float.IsNaN(load01)) return 1f;
            if (load01 < 0f) load01 = 0f;
            if (load01 > 1f) load01 = 1f;
            return 1f - Slowdown * load01;
        }
    }
}
