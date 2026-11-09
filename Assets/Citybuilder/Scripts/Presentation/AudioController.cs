using UnityEngine;

namespace Citybuilder
{
    // Тонкий контроллер UI-звуков (EPIC-19): 1 AudioSource, громкость и мут через PlayerPrefs.
    public class AudioController : MonoBehaviour
    {
        public const string VolumePrefKey = "citybuilder_volume";
        public const string MutePrefKey = "citybuilder_muted";
        public const float DefaultVolume = 0.5f;

        [Header("References")]
        [Tooltip("Источник звука для UI. Если пусто — создаётся автоматически.")] public AudioSource Source;
        [Header("Clips")]
        [Tooltip("Короткий тик по кнопке.")] public AudioClip ClickClip;
        [Tooltip("Мягкий звон при сборе налога.")] public AudioClip CoinClip;
        [Tooltip("Тихий поп события в ленте.")] public AudioClip PopClip;
        [Tooltip("Лёгкий свуш открытия/закрытия меню.")] public AudioClip SwooshClip;

        private static AudioController instance;
        private float volume = DefaultVolume;
        private bool muted;

        public float Volume { get { return volume; } }
        public bool IsMuted { get { return muted; } }

        public static float ClampVolume(float value)
        {
            return Mathf.Clamp01(value);
        }

        public static void PlayClickGlobal()
        {
            if (TryGetInstance(out AudioController ctrl)) ctrl.PlayClick();
        }

        public static void PlayCoinGlobal()
        {
            if (TryGetInstance(out AudioController ctrl)) ctrl.PlayCoin();
        }

        public static void PlayPopGlobal()
        {
            if (TryGetInstance(out AudioController ctrl)) ctrl.PlayPop();
        }

        public static void PlaySwooshGlobal()
        {
            if (TryGetInstance(out AudioController ctrl)) ctrl.PlaySwoosh();
        }

        private void Awake()
        {
            instance = this;
            if (Source == null) Source = GetComponent<AudioSource>();
            if (Source == null) Source = gameObject.AddComponent<AudioSource>();
            Source.playOnAwake = false;
            LoadPrefs();
        }

        public void LoadPrefs()
        {
            volume = ClampVolume(PlayerPrefs.GetFloat(VolumePrefKey, DefaultVolume));
            muted = PlayerPrefs.GetInt(MutePrefKey, 0) != 0;
            ApplyListenerVolume();
        }

        public void SetVolume(float newVolume)
        {
            volume = ClampVolume(newVolume);
            PlayerPrefs.SetFloat(VolumePrefKey, volume);
            ApplyListenerVolume();
        }

        public void SetMuted(bool isMuted)
        {
            muted = isMuted;
            PlayerPrefs.SetInt(MutePrefKey, muted ? 1 : 0);
            ApplyListenerVolume();
        }

        public void PlayClick() { PlayOneShotSafe(ClickClip, 0.6f); }
        public void PlayCoin() { PlayOneShotSafe(CoinClip, 0.85f); }
        public void PlayPop() { PlayOneShotSafe(PopClip, 0.75f); }
        public void PlaySwoosh() { PlayOneShotSafe(SwooshClip, 0.7f); }

        private void PlayOneShotSafe(AudioClip clip, float scale)
        {
            if (muted || volume <= 0f || Source == null || clip == null) return;
            Source.PlayOneShot(clip, scale);
        }

        private void ApplyListenerVolume()
        {
            AudioListener.volume = muted ? 0f : volume;
        }

        private static bool TryGetInstance(out AudioController ctrl)
        {
            if (instance == null) instance = FindFirstObjectByType<AudioController>();
            ctrl = instance;
            return ctrl != null;
        }
    }
}
