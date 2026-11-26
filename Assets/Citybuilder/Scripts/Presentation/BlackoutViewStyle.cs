using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Стиль вида «обесточено» (EPIC-50): приглушённые материалы у куба или префаба здания.
    // Общие ассеты не трогаем: у каждого рендерера создаётся копия материала.
    // ADR-021: тот же паттерн используется для emissive-окен — копии кешируются и удаляются владельцем.
    public static class BlackoutViewStyle
    {
        public const float DarkenFactor = 0.45f;
        public const string EmissionProperty = "_EmissionColor";
        public const float NightMoonThreshold = 0.05f;
        public static readonly Color WarmWindows = new Color(1f, 0.78f, 0.42f);

        // Шов presentation (ADR-021): контроллер публикует снимок, владельцы видов подписываются.
        public static event Action<DayNightSample> SampleChanged;
        public static bool IsNightNow { get; private set; }

        public static bool IsNight(DayNightSample sample)
        {
            return sample.MoonIntensity > NightMoonThreshold;
        }

        // Одна публикация на Apply: повтор снимка слушатель отфильтрует сам.
        public static void Publish(DayNightSample sample)
        {
            IsNightNow = IsNight(sample);
            Action<DayNightSample> handler = SampleChanged;
            if (handler != null) handler(sample);
        }

        // Затемнить все материалы вида; копии добавляются в bucket к исходникам, владелец уничтожит все сразу.
        public static void Apply(GameObject view, List<Material> bucket = null)
        {
            if (view == null) return;
            MeshRenderer[] renderers = view.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Material mat = renderers[i].material;
                if (mat == null) continue;
                mat.color = Darken(mat.color);
                if (bucket != null) bucket.Add(mat);
            }
        }

        // Приглушённый цвет для материалов, создающихся при сборке вида.
        public static Color Darken(Color color)
        {
            return new Color(color.r * DarkenFactor, color.g * DarkenFactor, color.b * DarkenFactor);
        }

        // ADR-021: окна загораются только в копиях; одна копия на исходный материал (кеш).
        public static void ApplyEmissive(GameObject view, Color emission, Dictionary<Material, Material> cache)
        {
            if (view == null || cache == null) return;
            MeshRenderer[] renderers = view.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Material source = renderers[i].sharedMaterial;
                if (source == null || !source.HasProperty(EmissionProperty)) continue;
                // Повторный вызов видит уже свою копию: меняем цвет, но не плодим материалы.
                if (cache.ContainsValue(source))
                {
                    source.SetColor(EmissionProperty, emission);
                    continue;
                }
                Material copy;
                if (!cache.TryGetValue(source, out copy))
                {
                    copy = new Material(source);
                    copy.EnableKeyword("_EMISSION");
                    cache.Add(source, copy);
                }
                copy.SetColor(EmissionProperty, emission);
                renderers[i].sharedMaterial = copy;
            }
        }

        // Все копии возвращаются владельцем вида одновременно; общие ассеты не трогаются.
        public static void ClearEmissive(Dictionary<Material, Material> cache)
        {
            if (cache == null) return;
            foreach (KeyValuePair<Material, Material> pair in cache)
            {
                if (pair.Value != null) UnityEngine.Object.Destroy(pair.Value);
            }
            cache.Clear();
        }
    }
}
