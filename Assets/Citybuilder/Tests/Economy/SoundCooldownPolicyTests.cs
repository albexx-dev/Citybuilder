using NUnit.Framework;

namespace Citybuilder
{
    // Чистые тесты политики звуковых кулдаунов (задача 172): без Unity, время подаётся параметром.
    public class SoundCooldownPolicyTests
    {
        // Первый вызов монетки звучит всегда.
        [Test]
        public void Coin_FirstCall_Plays()
        {
            Assert.IsTrue(new SoundCooldownPolicy().ShouldPlayCoin(10.0, 100));
        }

        // Повтор в окне кулдауна гасится, даже если баланс вырос сильно.
        [Test]
        public void Coin_WithinCooldown_GrowsALot_Silent()
        {
            SoundCooldownPolicy policy = new SoundCooldownPolicy(2.5);
            Assert.IsTrue(policy.ShouldPlayCoin(0.0, 100));
            Assert.IsFalse(policy.ShouldPlayCoin(1.0, 500));
            Assert.IsFalse(policy.ShouldPlayCoin(2.4, 500));
        }

        // После кулдауна рост меньше 10 $ молчит и не двигает порог.
        [Test]
        public void Coin_AfterCooldown_GrowthBelowDollars_Silent()
        {
            SoundCooldownPolicy policy = new SoundCooldownPolicy(2.5);
            Assert.IsTrue(policy.ShouldPlayCoin(0.0, 100));
            Assert.IsFalse(policy.ShouldPlayCoin(3.0, 105));
            Assert.IsFalse(policy.ShouldPlayCoin(6.0, 109));
        }

        // Кулдаун истёк и баланс вырос ≥ 10 $ — монетка звучит; на границе кулдауна уже можно.
        [Test]
        public void Coin_AfterCooldown_GrowthReachesDollars_Plays()
        {
            SoundCooldownPolicy policy = new SoundCooldownPolicy(2.5);
            Assert.IsTrue(policy.ShouldPlayCoin(0.0, 100));
            Assert.IsTrue(policy.ShouldPlayCoin(2.5, 110));
            Assert.IsTrue(policy.ShouldPlayCoin(5.0, 130));
        }

        // Падение баланса монетку не включает: нужен рост от прошлого звука.
        [Test]
        public void Coin_AfterCooldown_BalanceDropped_Silent()
        {
            SoundCooldownPolicy policy = new SoundCooldownPolicy(2.5);
            Assert.IsTrue(policy.ShouldPlayCoin(0.0, 100));
            Assert.IsFalse(policy.ShouldPlayCoin(3.0, 90));
        }

        // Дефицит: первый репорт звучит, в окне гасится, по истечении кулдауна снова звучит.
        [Test]
        public void Deficit_FirstPlays_ThenCooldownGates_ThenPlays()
        {
            SoundCooldownPolicy policy = new SoundCooldownPolicy(2.5);
            Assert.IsTrue(policy.ShouldPlayDeficit(0.0));
            Assert.IsFalse(policy.ShouldPlayDeficit(1.0));
            Assert.IsFalse(policy.ShouldPlayDeficit(2.4));
            Assert.IsTrue(policy.ShouldPlayDeficit(2.5));
        }

        // Монетка и дефицит — раздельные состояния: история одного звука не занимает окно другого.
        [Test]
        public void CoinAndDeficit_StatesAreIsolated()
        {
            SoundCooldownPolicy policy = new SoundCooldownPolicy(2.5);
            Assert.IsTrue(policy.ShouldPlayCoin(0.0, 100));
            Assert.IsTrue(policy.ShouldPlayDeficit(5.0));
            Assert.IsTrue(policy.ShouldPlayCoin(5.0, 200));

            Assert.IsFalse(policy.ShouldPlayCoin(5.1, 300));
            Assert.IsFalse(policy.ShouldPlayDeficit(5.1));
        }

        // Отрицательный кулдаун не валиден — превращается в 0, звуки не блокируются.
        [Test]
        public void NegativeCooldown_ClampedToZero()
        {
            SoundCooldownPolicy policy = new SoundCooldownPolicy(-1.0);
            Assert.AreEqual(SoundCooldownPolicy.DefaultCooldownSeconds, new SoundCooldownPolicy().CooldownSeconds);
            Assert.AreEqual(0.0, policy.CooldownSeconds);
            Assert.IsTrue(policy.ShouldPlayCoin(5.0, 100));
            Assert.IsTrue(policy.ShouldPlayCoin(5.0, 200));
        }

        // Симуляция «Новой игры»: после Reset монетка звучит сразу при новом балансе, без догоняния старого.
        [Test]
        public void Reset_CoinPlaysAtOnceWithNewBalance()
        {
            SoundCooldownPolicy policy = new SoundCooldownPolicy(2.5);
            Assert.IsTrue(policy.ShouldPlayCoin(10.0, 5000));
            Assert.IsFalse(policy.ShouldPlayCoin(11.0, 6000));
            policy.Reset();
            Assert.IsTrue(policy.ShouldPlayCoin(11.5, 1500));
            Assert.IsFalse(policy.ShouldPlayCoin(11.6, 1600));
        }

        // Reset возвращает и дефицитный канал к состоянию «первого звука».
        [Test]
        public void Reset_DeficitPlaysAtOnce()
        {
            SoundCooldownPolicy policy = new SoundCooldownPolicy(2.5);
            Assert.IsTrue(policy.ShouldPlayDeficit(0.0));
            Assert.IsFalse(policy.ShouldPlayDeficit(0.1));
            policy.Reset();
            Assert.IsTrue(policy.ShouldPlayDeficit(0.2));
        }
    }
}
