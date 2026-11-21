using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Событийные звуки города (хвосты #71/#66, задача 104): тревожный сигнал пожара,
    // низкий «удар» сгоревшего дома, короткий диссонанс кражи; вывоз мусора и отключение
    // света — тихие сигналы на уже подключённых клипах. Домен пишет в CityEventLog без Unity,
    // поэтому вью читает ленту опросом через EconomyTicker.EventLog (образец чтения
    // FireOverlayView: снимок раз в PollIntervalSec). Кулдаун на вид сигнала —
    // SoundCooldownPolicy (образец EventLogView.Record), сгоревший дом — тот же клип
    // пожара на низком питче BurnedPitch. Казна/доход молчат: coin — только награда цели.
    public class AudioEventView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Экономика: даёт ленту событий EventLog.Log. Пусто = ищется в сцене.")] public EconomyTicker Economy;

        [Header("Clips")]
        [Tooltip("Тревожный сигнал пожара. Пусто = пожары беззвучны, без краша.")] public AudioClip FireClip;
        [Tooltip("Короткий диссонанс кражи. Пусто = кражи беззвучны, без краша.")] public AudioClip CrimeClip;

        [Header("Mix")]
        [Tooltip("Период опроса ленты, сек. Большие значения запаздывают со звуком.")] public float PollIntervalSec = 0.25f;
        [Tooltip("Питч сигнала сгоревшего дома: тот же клип пожара, но низкий «удар».")] public float BurnedPitch = 0.5f;

        private const float FireVolumeScale = 0.9f;
        private const float CrimeVolumeScale = 0.8f;
        private const float GarbageVolumeScale = 0.5f;
        private const float GarbagePitch = 1.6f;
        private const float BlackoutVolumeScale = 0.5f;
        private const float BlackoutPitch = 0.5f;
        private const float MinPollIntervalSec = 0.05f;

        private AudioSource source;
        private readonly SoundCooldownPolicy fireCooldown = new SoundCooldownPolicy();
        private readonly SoundCooldownPolicy crimeCooldown = new SoundCooldownPolicy();
        private readonly SoundCooldownPolicy garbageCooldown = new SoundCooldownPolicy();
        private readonly SoundCooldownPolicy blackoutCooldown = new SoundCooldownPolicy();
        private string lastEventKey = "";
        private float nextPollTime;

        // Новая игра/загрузка (LoadController.ClearAll): кулдауны и ключ ленты — с чистого листа.
        public void Reset()
        {
            fireCooldown.Reset();
            crimeCooldown.Reset();
            garbageCooldown.Reset();
            blackoutCooldown.Reset();
            lastEventKey = "";
            nextPollTime = 0f;
        }

        private void Awake()
        {
            // Источник на ДОЧЕРНЕМ GO: AudioController.cs:55 берёт GetComponent<AudioSource>
            // со своего хоста — общий GameObject столкнул бы событийные звуки с UI-звуком.
            source = GetComponentInChildren<AudioSource>();
            if (source == null)
            {
                GameObject host = new GameObject("AudioEvents");
                host.transform.SetParent(transform, false);
                source = host.AddComponent<AudioSource>();
            }
            source.playOnAwake = false;
        }

        private void Update()
        {
            if (Time.unscaledTime < nextPollTime) return;
            nextPollTime = Time.unscaledTime + Mathf.Max(MinPollIntervalSec, PollIntervalSec);
            PollNow();
        }

        // Один опрос ленты: события после прошлого ключа — по звуку на вид (AudioEventPolicy).
        public void PollNow()
        {
            CityEventLog log = ResolveLog();
            if (log == null) return;
            IReadOnlyList<CityEvent> snapshot = log.Snapshot();
            int start = AudioEventPolicy.NewEventsStart(snapshot, lastEventKey);
            for (int i = start; i < snapshot.Count; i++)
            {
                lastEventKey = AudioEventPolicy.KeyOf(snapshot[i]);
                PlaySound(AudioEventPolicy.Resolve(snapshot[i]));
            }
        }

        private CityEventLog ResolveLog()
        {
            if (Economy == null) Economy = FindFirstObjectByType<EconomyTicker>();
            if (Economy == null || Economy.EventLog == null) return null;
            return Economy.EventLog.Log;
        }

        // Один звук на вид сигнала: кулдаун гасит серии, питч различает пожар и сгоревший дом.
        // Garbage/Blackout: тихий чирп на клипе кражи и глухой «удар» на клипе пожара.
        private void PlaySound(AudioEventSound sound)
        {
            if (sound == AudioEventSound.None || source == null) return;
            if (sound == AudioEventSound.Crime)
            {
                if (crimeCooldown.ShouldPlayDeficit(Time.unscaledTime)) PlayOneShotSafe(CrimeClip, CrimeVolumeScale, 1f);
                return;
            }
            if (sound == AudioEventSound.Garbage)
            {
                if (garbageCooldown.ShouldPlayDeficit(Time.unscaledTime)) PlayOneShotSafe(CrimeClip, GarbageVolumeScale, GarbagePitch);
                return;
            }
            if (sound == AudioEventSound.Blackout)
            {
                if (blackoutCooldown.ShouldPlayDeficit(Time.unscaledTime)) PlayOneShotSafe(FireClip, BlackoutVolumeScale, BlackoutPitch);
                return;
            }
            if (!fireCooldown.ShouldPlayDeficit(Time.unscaledTime)) return;
            float pitch = sound == AudioEventSound.Burned ? BurnedPitch : 1f;
            PlayOneShotSafe(FireClip, FireVolumeScale, pitch);
        }

        private void PlayOneShotSafe(AudioClip clip, float scale, float pitch)
        {
            if (clip == null) return;
            source.pitch = pitch;
            source.PlayOneShot(clip, scale);
            source.pitch = 1f;
        }
    }
}
