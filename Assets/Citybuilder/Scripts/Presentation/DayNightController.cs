using UnityEngine;
using UnityEngine.Rendering;

namespace Citybuilder
{
    // Цикл дня и ночи (EPIC-58, view-слой): домен о сутках не знает — часы уже есть.
    // Подписка на TickCompleted как у EconomyTicker; фаза из HourOfDay.
    // Свет меняется кодом: солнце ищется в сцене, его тени не трогаются.
    // Ночью солнце падает до MinSunIntensity и включается холодная «луна» —
    // город читается силуэтами. Emissive-окна сознательно отложены (шаг #69):
    // у видов общие материалы, безопасного свечения инстансов не сделать.
    public class DayNightController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Драйвер часов: даёт тики для фазы суток. Пусто = ищется в сцене.")]
        public SimulationRunner Runner;

        [Header("Cycle")]
        [Tooltip("Цикл включён. Выключено — фиксированный полдень (настройка #69).")]
        public bool DayNightEnabled = true;
        [Tooltip("Высота «луны» над горизонтом, градусы.")]
        public float MoonPitch = 48f;
        [Tooltip("Азимут «луны», градусы.")]
        public float MoonAzimuth = 30f;
        [Tooltip("Холодный цвет лунного света.")]
        public Color MoonColor = new Color(0.58f, 0.68f, 1f);

        private const float MoonEnableThreshold = 0.01f;

        private Light sun;
        private Light moon;

        private void OnEnable()
        {
            if (!EnsureReady()) return;
            Runner.Clock.TickCompleted += OnClockTick;
            Apply(ClockHour());
        }

        private void OnDisable()
        {
            if (Runner != null && Runner.Clock != null) Runner.Clock.TickCompleted -= OnClockTick;
        }

        private void OnValidate()
        {
            if (Application.isPlaying && sun != null && Runner != null) Apply(ClockHour());
        }

        // --- Подготовка источников света ---

        private bool EnsureReady()
        {
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Runner == null)
            {
                Debug.LogError("DayNightController: задайте Runner в Inspector.");
                enabled = false;
                return false;
            }
            EnsureSun();
            EnsureMoon();
            RenderSettings.ambientMode = AmbientMode.Flat;
            return true;
        }

        // Солнце: первый DirectionalLight сцены (основой шага #57). Нет — создаём.
        private void EnsureSun()
        {
            if (sun != null) return;
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].type == LightType.Directional)
                {
                    sun = lights[i];
                    return;
                }
            }
            sun = new GameObject("SunLight").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            Debug.LogWarning("DayNightController: DirectionalLight не найден, создан SunLight.");
        }

        // Луна создаётся кодом: в сцене её нет, тени не считает (перф).
        private void EnsureMoon()
        {
            if (moon != null) return;
            moon = new GameObject("MoonLight").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.shadows = LightShadows.None;
            moon.color = MoonColor;
            moon.intensity = 0f;
            moon.enabled = false;
            moon.transform.rotation = Quaternion.Euler(MoonPitch, MoonAzimuth, 0f);
        }

        // --- Применение фазы ---

        private void OnClockTick(SimulationTime time)
        {
            Apply(time.HourOfDay);
        }

        // Единственная точка применения: Policy отдаёт параметры, здесь только Unity.
        private void Apply(int hour)
        {
            DayNightSample sample = DayNightEnabled ? DayNightPolicy.Sample(hour) : DayNightPolicy.Noon();
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(sample.SunPitch, sample.SunAzimuth, 0f);
                sun.intensity = sample.SunIntensity;
                sun.color = new Color(sample.SunColorR, sample.SunColorG, sample.SunColorB);
            }
            if (moon != null)
            {
                moon.intensity = sample.MoonIntensity;
                moon.enabled = sample.MoonIntensity > MoonEnableThreshold;
            }
            RenderSettings.ambientLight = new Color(sample.AmbientR, sample.AmbientG, sample.AmbientB);
        }

        private int ClockHour()
        {
            return Runner.Clock.CurrentTime.HourOfDay;
        }
    }
}
