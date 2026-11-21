using UnityEngine;

namespace Citybuilder
{
    // Эмбиент день/ночь (EPIC-71, шаг ambient): два лупа на отдельных AudioSource,
    // кроссфейд по HourOfDay (края 6/9/18/22 в AmbientMixPolicy) и громкость от
    // активности города (доля населения, clamp). Ссылки ищутся сами, как в QuestLogView;
    // без DayNightController (или при выключенном цикле) звучит всегда дневной эмбиент.
    public class AmbientAudioView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Цикл суток: даёт Runner с часами. Пусто = ищется в сцене; без него — всегда день.")]
        public DayNightController DayNight;
        [Tooltip("Население: даёт активность города (доля жителей от референса). Пусто = ищется в сцене.")]
        public PopulationController Population;

        [Header("Clips")]
        [Tooltip("Дневной луп: птицы и городской гул. Пусто = днём тишина, без краша.")]
        public AudioClip DayClip;
        [Tooltip("Ночной луп: сверчки. Пусто = ночью тишина, без краша.")]
        public AudioClip NightClip;

        [Header("Mix")]
        [Tooltip("Сглаживание громкости в реальном времени, сек. Сутки игры = 24 сек (1 тик = 1 час), большие значения размывают смену день/ночь.")]
        public float CrossfadeSeconds = 2f;
        [Tooltip("Население полностью активного города (цель 200 из HUD).")]
        public int ReferencePopulation = 200;
        [Tooltip("Множитель итоговой громкости эмбиента 0..1 (слайдер «Эмбиент» в настройках).")]
        public float AmbientVolume = 1f;

        private const float DayHumDepth = 0.08f;
        private const float DayHumRate = 0.45f;
        private const float NightHumDepth = 0.05f;
        private const float NightHumRate = 0.31f;

        private AudioSource daySource;
        private AudioSource nightSource;
        private float dayTarget;
        private float nightTarget;
        private float dayCurrent;
        private float nightCurrent;

        private void Awake()
        {
            daySource = EnsureSource("AmbientDay");
            nightSource = EnsureSource("AmbientNight");
            StartLoop(daySource, DayClip);
            StartLoop(nightSource, NightClip);
        }

        private void OnDisable()
        {
            // Пауза эмбиента при выключении компонента; Update вернёт громкость при включении.
            if (daySource != null) daySource.volume = 0f;
            if (nightSource != null) nightSource.volume = 0f;
        }

        private void Update()
        {
            ResolveRefs();
            UpdateTargets();
            SmoothVolumes();
        }

        // Активность города: доля населения от референса, clamp 0..1 (бриф #71).
        public float ComputeActivity()
        {
            if (Population == null || Population.Population == null) return 0f;
            int reference = ReferencePopulation < 1 ? 1 : ReferencePopulation;
            return Mathf.Clamp01(Population.Population.TotalResidents / (float)reference);
        }

        // Час из часов Runner (как DayNightController); нет часов или цикл выключен — полдень.
        private float ReadHour()
        {
            if (DayNight == null || !DayNight.DayNightEnabled) return AmbientMixPolicy.NoonHour;
            SimulationRunner runner = DayNight.Runner;
            if (runner == null || runner.Clock == null) return AmbientMixPolicy.NoonHour;
            return runner.Clock.CurrentTime.HourOfDay;
        }

        // Цели микса из Policy + лёгкий «случайный» гул города (медленные волны громкости).
        private void UpdateTargets()
        {
            AmbientMixSample mix = AmbientMixPolicy.Sample(ReadHour(), ComputeActivity());
            dayTarget = Mathf.Clamp01(mix.DayVolume * (1f + DayHumDepth * Mathf.Sin(Time.unscaledTime * DayHumRate)));
            nightTarget = Mathf.Clamp01(mix.NightVolume * (1f + NightHumDepth * Mathf.Sin(Time.unscaledTime * NightHumRate)));
        }

        // Плавный кроссфейд: экспоненциальное приближение к целям за CrossfadeSeconds,
        // итоговая громкость умножается на AmbientVolume из настроек (хвост #71).
        private void SmoothVolumes()
        {
            float step = 1f - Mathf.Exp(-Time.unscaledDeltaTime / Mathf.Max(0.01f, CrossfadeSeconds));
            dayCurrent = Mathf.Lerp(dayCurrent, dayTarget, step);
            nightCurrent = Mathf.Lerp(nightCurrent, nightTarget, step);
            if (daySource != null) daySource.volume = dayCurrent * AmbientVolume;
            if (nightSource != null) nightSource.volume = nightCurrent * AmbientVolume;
        }

        // Источник эмбиента создаётся кодом на ДОЧЕРНЕМ объекте: AudioController.cs:55
        // берёт GetComponent<AudioSource>() со своего хоста — общий GO столкнул бы их.
        private AudioSource EnsureSource(string name)
        {
            GameObject host = new GameObject(name);
            host.transform.SetParent(transform, false);
            AudioSource source = host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0f;
            return source;
        }

        private static void StartLoop(AudioSource source, AudioClip clip)
        {
            if (source == null || clip == null) return;
            source.clip = clip;
            source.Play();
        }

        private void ResolveRefs()
        {
            if (DayNight == null) DayNight = FindFirstObjectByType<DayNightController>();
            if (Population == null) Population = FindFirstObjectByType<PopulationController>();
        }
    }
}
