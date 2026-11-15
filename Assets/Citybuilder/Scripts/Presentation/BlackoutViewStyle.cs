using UnityEngine;

namespace Citybuilder
{
    // Стиль вида «обесточено» (EPIC-50): приглушённые материалы у куба или префаба здания.
    // Общие ассеты не трогаем: у каждого рендерера создаётся копия материала.
    public static class BlackoutViewStyle
    {
        public const float DarkenFactor = 0.45f;

        // Затемнить все материалы вида: и префабы, и кубы с крышами.
        public static void Apply(GameObject view)
        {
            if (view == null) return;
            MeshRenderer[] renderers = view.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Color color = renderers[i].material.color;
                renderers[i].material.color = Darken(color);
            }
        }

        // Приглушённый цвет для материалов, создающихся при сборке вида.
        public static Color Darken(Color color)
        {
            return new Color(color.r * DarkenFactor, color.g * DarkenFactor, color.b * DarkenFactor);
        }
    }
}
