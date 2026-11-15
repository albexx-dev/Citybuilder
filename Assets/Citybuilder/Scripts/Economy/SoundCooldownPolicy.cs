namespace Citybuilder
{
    // Решает, играть ли звук, чтобы один звук не звучал чаще раза в кулдаун реального времени (задача 172).
    // Чистый C# без Unity: реальное время (Time.unscaledTime) вызывающий передаёт параметром nowSeconds.
    public class SoundCooldownPolicy
    {
        // Монетка слышна, только если с прошлого звука казна выросла минимум на эту сумму.
        public const int CoinGrowthDollars = 10;
        public const double DefaultCooldownSeconds = 2.5;

        public readonly double CooldownSeconds;

        private bool coinPlayedOnce;
        private double coinLastPlaySeconds;
        private int coinLastBalance;
        private bool deficitPlayedOnce;
        private double deficitLastPlaySeconds;

        public SoundCooldownPolicy() : this(DefaultCooldownSeconds) { }

        public SoundCooldownPolicy(double cooldownSeconds)
        {
            CooldownSeconds = cooldownSeconds < 0 ? 0 : cooldownSeconds;
        }

        // Монетка: первый сбор звучит всегда; дальше — кулдаун истёк И баланс вырос ≥ CoinGrowthDollars.
        public bool ShouldPlayCoin(double nowSeconds, int balance)
        {
            if (!coinPlayedOnce)
            {
                RememberCoin(nowSeconds, balance);
                return true;
            }
            if (nowSeconds - coinLastPlaySeconds < CooldownSeconds)
            {
                return false;
            }
            if (balance - coinLastBalance < CoinGrowthDollars)
            {
                return false;
            }
            RememberCoin(nowSeconds, balance);
            return true;
        }

        // Поп и дефицит ленты: первый раз звучит, дальше не чаще кулдауна.
        public bool ShouldPlayDeficit(double nowSeconds)
        {
            if (!deficitPlayedOnce || nowSeconds - deficitLastPlaySeconds >= CooldownSeconds)
            {
                deficitPlayedOnce = true;
                deficitLastPlaySeconds = nowSeconds;
                return true;
            }
            return false;
        }

        private void RememberCoin(double nowSeconds, int balance)
        {
            coinPlayedOnce = true;
            coinLastPlaySeconds = nowSeconds;
            coinLastBalance = balance;
        }

        // Сброс к состоянию «первого звука»: после «Новой игры» и загрузки монетка и дефицит звучат сразу.
        public void Reset()
        {
            coinPlayedOnce = false;
            coinLastPlaySeconds = 0;
            coinLastBalance = 0;
            deficitPlayedOnce = false;
            deficitLastPlaySeconds = 0;
        }
    }
}
