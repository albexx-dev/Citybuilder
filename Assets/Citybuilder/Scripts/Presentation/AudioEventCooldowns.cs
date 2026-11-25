using System.Collections.Generic;

namespace Citybuilder
{
    // Независимые кулдауны по видам сигналов ленты (issue #124): у каждого звука свой таймер.
    // Чистый C# без Unity — время задаётся вызывающим, как в SoundCooldownPolicy.
    public class AudioEventCooldowns
    {
        public readonly double CooldownSeconds;

        private readonly Dictionary<AudioEventSound, SoundCooldownPolicy> policies =
            new Dictionary<AudioEventSound, SoundCooldownPolicy>();

        public AudioEventCooldowns() : this(SoundCooldownPolicy.DefaultCooldownSeconds) { }

        public AudioEventCooldowns(double cooldownSeconds)
        {
            CooldownSeconds = cooldownSeconds < 0 ? 0 : cooldownSeconds;
        }

        // Первый сигнал вида звучит сразу; повтор — не раньше кулдауна этого же вида.
        public bool ShouldPlay(AudioEventSound sound, double nowSeconds)
        {
            SoundCooldownPolicy policy = PolicyFor(sound);
            return policy != null && policy.ShouldPlayDeficit(nowSeconds);
        }

        // «Новая игра»/загрузка: все виды снова звучат сразу.
        public void Reset()
        {
            foreach (KeyValuePair<AudioEventSound, SoundCooldownPolicy> pair in policies)
            {
                pair.Value.Reset();
            }
        }

        private SoundCooldownPolicy PolicyFor(AudioEventSound sound)
        {
            if (sound == AudioEventSound.None) return null;
            SoundCooldownPolicy policy;
            if (!policies.TryGetValue(sound, out policy))
            {
                policy = new SoundCooldownPolicy(CooldownSeconds);
                policies[sound] = policy;
            }
            return policy;
        }
    }
}
