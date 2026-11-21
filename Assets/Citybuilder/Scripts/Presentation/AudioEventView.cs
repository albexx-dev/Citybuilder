using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Событийные звуки города (хвосты #71/#66): тревожный сигнал пожара, низкий «удар»
    // сгоревшего дома, короткий диссонанс кражи. Домен пишет в CityEventLog без Unity,
    // поэтому вью читает ленту опросом через EconomyTicker.EventLog (образец чтения
    // FireOverlayView: снимок раз в PollIntervalSec). Кулдаун на вид сигнала —
    // SoundCooldownPolicy (образец EventLogView.Record), сгоревший дом — тот же клип
    // пожара на низком питче BurnedPitch.
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
        private const float MinPollIntervalSec = 0.05f;

        private AudioSource source;
        private readonly SoundCooldownPolicy fireCooldown = new SoundCooldownPolicy();
        private readonly SoundCooldownPolicy crimeCooldown = new SoundCooldownPolicy();
        private string lastEventKey = "";
        private float nextPollTime;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
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
                PlaySound(AudioEventPolicy.Resolve(snapshot[i].Kind, snapshot[i].Message));
            }
        }

        private CityEventLog ResolveLog()
        {
            if (Economy == null) Economy = FindFirstObjectByType<EconomyTicker>();
            if (Economy == null || Economy.EventLog == null) return null;
            return Economy.EventLog.Log;
        }

        // Один звук на вид сигнала: кулдаун гасит серии, питч различает пожар и сгоревший дом.
        private void PlaySound(AudioEventSound sound)
        {
            if (sound == AudioEventSound.None || source == null) return;
            if (sound == AudioEventSound.Crime)
            {
                if (crimeCooldown.ShouldPlayDeficit(Time.unscaledTime)) PlayOneShotSafe(CrimeClip, CrimeVolumeScale, 1f);
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
