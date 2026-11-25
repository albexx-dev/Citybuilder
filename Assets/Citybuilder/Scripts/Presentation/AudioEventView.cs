using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Событийные звуки города (хвосты #71/#66, задачи 104/124): тревожный сигнал пожара,
    // низкий «удар» сгоревшего дома, короткий диссонанс кражи; мусор, свет, снабжение,
    // пробки и занятость — тихие сигналы на уже подключённых клипах. Домен пишет в CityEventLog
    // без Unity, поэтому вью читает ленту опросом через EconomyTicker.EventLog (снимок раз
    // в PollIntervalSec). Кулдаун отдельный на вид сигнала — AudioEventCooldowns; сгоревший
    // дом — тот же клип пожара на низком питче BurnedPitch. Казна/доход молчат: coin —
    // только награда цели.
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
        private const float SupplyVolumeScale = 0.35f;
        private const float SupplyPitch = 0.8f;
        private const float CongestionVolumeScale = 0.35f;
        private const float CongestionPitch = 1.2f;
        private const float EmploymentVolumeScale = 0.3f;
        private const float EmploymentPitch = 1.3f;
        private const float MinPollIntervalSec = 0.05f;

        private AudioSource source;
        private readonly AudioEventCooldowns cooldowns = new AudioEventCooldowns();
        private string lastEventKey = "";
        private float nextPollTime;

        // Новая игра/загрузка (LoadController.ClearAll): кулдауны и ключ ленты — с чистого листа.
        public void Reset()
        {
            cooldowns.Reset();
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

        // Один звук на вид сигнала: кулдаун гасит серии, питч различает подтипы и источники.
        private void PlaySound(AudioEventSound sound)
        {
            if (sound == AudioEventSound.None || source == null) return;
            if (!cooldowns.ShouldPlay(sound, Time.unscaledTime)) return;
            PlaySoundNow(sound);
        }

        // Garbage/Supply/Congestion — тихие чирпы клипа кражи, Employment/Blackout — приглушённый
        // клип пожара; питч и громкость задают разный звук без новых ассетов и без coin.
        private void PlaySoundNow(AudioEventSound sound)
        {
            if (sound == AudioEventSound.Crime) { PlayOneShotSafe(CrimeClip, CrimeVolumeScale, 1f); return; }
            if (sound == AudioEventSound.Garbage) { PlayOneShotSafe(CrimeClip, GarbageVolumeScale, GarbagePitch); return; }
            if (sound == AudioEventSound.Supply) { PlayOneShotSafe(CrimeClip, SupplyVolumeScale, SupplyPitch); return; }
            if (sound == AudioEventSound.Congestion) { PlayOneShotSafe(CrimeClip, CongestionVolumeScale, CongestionPitch); return; }
            if (sound == AudioEventSound.Employment) { PlayOneShotSafe(FireClip, EmploymentVolumeScale, EmploymentPitch); return; }
            if (sound == AudioEventSound.Blackout) { PlayOneShotSafe(FireClip, BlackoutVolumeScale, BlackoutPitch); return; }
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
