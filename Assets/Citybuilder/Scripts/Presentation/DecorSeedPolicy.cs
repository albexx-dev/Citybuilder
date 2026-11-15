namespace Citybuilder
{
    // Детерминированная политика декора (EPIC-56): хеш клетки решает,
    // есть ли декор, какой это вариант и как он повёрнут. Чистые функции
    // без Unity: одинаковый результат между запусками и в тестах вне редактора.
    public static class DecorSeedPolicy
    {
        // Шанс декора на свободной клетке по умолчанию.
        public const float DefaultChance = 0.15f;

        // Есть ли декор на клетке при шансе 0..1: всегда одинаково для (x, z).
        public static bool HasDecor(int x, int z, float chance)
        {
            if (chance <= 0f) return false;
            if (chance >= 1f) return true;
            return Normalized(x, z) < chance;
        }

        // Индекс варианта в границах [0, variantCount); пустой список даёт 0.
        public static int VariantIndex(int x, int z, int variantCount)
        {
            if (variantCount <= 1) return 0;
            return (int)((Hash(x, z) >> 16) % (uint)variantCount);
        }

        // Шаг поворота в границах [0, steps): разнообразие без случайности.
        public static int YawStep(int x, int z, int steps)
        {
            if (steps <= 1) return 0;
            return (int)((Hash(x, z) >> 24) % (uint)steps);
        }

        // Хеш клетки, приведённый к диапазону [0, 1).
        public static float Normalized(int x, int z)
        {
            return (Hash(x, z) & 0x00FFFFFF) / 16777216f;
        }

        // FNV-1a по частям координат с финальным перемешиванием битов.
        public static uint Hash(int x, int z)
        {
            unchecked
            {
                uint hash = 2166136261u;
                hash = Mix(hash, (uint)x);
                hash = Mix(hash, (uint)(x >> 16));
                hash = Mix(hash, (uint)z);
                hash = Mix(hash, (uint)(z >> 16));
                hash ^= hash >> 13;
                hash *= 16777619u;
                return hash;
            }
        }

        private static uint Mix(uint hash, uint value)
        {
            return (hash ^ value) * 16777619u;
        }
    }
}
