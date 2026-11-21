using UnityEngine;

namespace Citybuilder
{
    // Стиль уровня дома (issue #94): чистая логика для кубов и Kenney-префабов.
    // Цвета крыш группы 1..3 сохраняются; маркер красится своим материалом, без emissive.
    public static class HouseLevelStyle
    {
        public const int MarkerLevel = 2;

        // Масштаб вида: высота куба ×1.5/×2, габариты префаба ×1.25/×1.5.
        public static float LevelScale(int level, bool height)
        {
            if (level >= 3) return height ? 2f : 1.5f;
            if (level >= 2) return height ? 1.5f : 1.25f;
            return 1f;
        }

        // Цвет крыши: 1 — коричневый с оттенком варианта, 2 — терракота, 3 — синяя черепица.
        public static Color RoofColor(int level, int variant)
        {
            float tint = variant * 0.08f;
            if (level >= 3) return new Color(0.2f, 0.35f, 0.6f);
            if (level >= 2) return new Color(0.75f, 0.3f + tint * 0.5f, 0.2f);
            return new Color(0.45f + tint, 0.25f + tint * 0.5f, 0.1f);
        }

        // Маркер виден с уровня 2: дом вырос — это читается и без сравнения размеров.
        public static bool HasMarker(int level)
        {
            return level >= MarkerLevel;
        }

        // Цвет маркера отделен от цвета крыши: жёлтый для 2, голубой для 3.
        public static Color MarkerColor(int level)
        {
            return level >= 3 ? new Color(0.35f, 0.75f, 1f) : new Color(1f, 0.85f, 0.25f);
        }
    }
}
