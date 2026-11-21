using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Стиль вида «обесточено» (EPIC-50): приглушённые материалы у куба или префаба здания.
    // Общие ассеты не трогаем: у каждого рендерера создаётся копия материала.
    public static class BlackoutViewStyle
    {
        public const float DarkenFactor = 0.45f;

        // Затемнить все материалы вида; bucket — учёт копий для последующего Destroy.
        public static void Apply(GameObject view, List<Material> bucket = null)
        {
            if (view == null) return;
            MeshRenderer[] renderers = view.GetComponentsInChildren<MeshRenderer>();
            if (bucket != null) DestroyTracked(bucket);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material mat = renderers[i].material;
                if (mat == null) continue;
                mat.color = Darken(mat.color);
                if (bucket != null) bucket.Add(mat);
            }
        }

        private static void DestroyTracked(List<Material> bucket)
        {
            for (int i = 0; i < bucket.Count; i++)
            {
                Material mat = bucket[i];
                if (mat == null) continue;
                if (Application.isPlaying) Object.Destroy(mat);
                else Object.DestroyImmediate(mat);
            }
            bucket.Clear();
        }

        // Приглушённый цвет для материалов, создающихся при сборке вида.
        public static Color Darken(Color color)
        {
            return new Color(color.r * DarkenFactor, color.g * DarkenFactor, color.b * DarkenFactor);
        }
    }
}
