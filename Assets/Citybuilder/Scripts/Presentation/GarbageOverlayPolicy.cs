namespace Citybuilder
{
    // Чистое правило видимости оверлея переполнения (issue #94): презентация только
    // читает GarbageSystem.AccOf, не хранит своё состояние и не пишет в сейв.
    public static class GarbageOverlayPolicy
    {
        // Нет полигона — мусоровозы не ездят; оверлей висел бы вечно, поэтому не показываем.
        public static bool NeedsOverlay(int accumulated, bool depotExists)
        {
            return depotExists && accumulated >= GarbageSystem.ReadyThreshold;
        }

        // Заполненность бака 0..1: масштаб оверлея, а не доменная величина.
        // Верхняя граница не даёт баку вырасти выше крыши при большом переполнении.
        public static float Load01(int accumulated)
        {
            if (accumulated <= 0) return 0f;
            float load = accumulated / (float)GarbageSystem.ReadyThreshold;
            return load < 1f ? load : 1f;
        }
    }
}
