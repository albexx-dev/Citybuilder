using NUnit.Framework;

namespace Citybuilder
{
    // Оставшиеся kinds ленты (issue #124): звук по типу, а не по тексту; доход/казня молчат,
    // у каждого вида сигнала свой кулдаун (не переиспользуем coin для дохода).
    public class AudioEventSoundTests
    {
        private static CityEvent Event(EventKind kind, string key)
        {
            return new CityEvent(kind, key, new object[] { 42 }, 5);
        }

        [Test]
        public void Resolve_SupplyCongestionEmployment_ByKind()
        {
            Assert.AreEqual(AudioEventSound.Supply, AudioEventPolicy.Resolve(
                Event(EventKind.Supply, EventMessageCatalog.SupplyPowerDeficit)));
            Assert.AreEqual(AudioEventSound.Supply, AudioEventPolicy.Resolve(
                Event(EventKind.Supply, EventMessageCatalog.SupplyRestored)));
            Assert.AreEqual(AudioEventSound.Congestion, AudioEventPolicy.Resolve(
                Event(EventKind.Congestion, EventMessageCatalog.CongestionSlowdown)));
            Assert.AreEqual(AudioEventSound.Employment, AudioEventPolicy.Resolve(
                Event(EventKind.Employment, EventMessageCatalog.UnemploymentHigh)));
        }

        [Test]
        public void Resolve_IncomeAndQuestReward_AreSilent()
        {
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(
                Event(EventKind.Treasury, EventMessageCatalog.TreasuryDeficit)));
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(
                Event(EventKind.Treasury, EventMessageCatalog.QuestCompleted)));
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(
                Event(EventKind.Info, string.Empty)));
        }

        [Test]
        public void Resolve_TextDoesNotChangeSound()
        {
            CityEvent deficit = Event(EventKind.Supply, EventMessageCatalog.SupplyPowerDeficit);
            CityEvent otherText = new CityEvent(EventKind.Supply, "Энергия восстановлена", null, 6);
            Assert.AreEqual(AudioEventPolicy.Resolve(deficit), AudioEventPolicy.Resolve(otherText));
        }

        [Test]
        public void Cooldown_IsPerSoundKind()
        {
            AudioEventCooldowns cooldowns = new AudioEventCooldowns(2.5);
            Assert.IsTrue(cooldowns.ShouldPlay(AudioEventSound.Supply, 0.0));
            Assert.IsFalse(cooldowns.ShouldPlay(AudioEventSound.Supply, 1.0));
            Assert.IsTrue(cooldowns.ShouldPlay(AudioEventSound.Congestion, 1.0));
            Assert.IsTrue(cooldowns.ShouldPlay(AudioEventSound.Employment, 1.0));
            Assert.IsTrue(cooldowns.ShouldPlay(AudioEventSound.Supply, 2.5));
            Assert.IsFalse(cooldowns.ShouldPlay(AudioEventSound.None, 3.0));
        }

        // Дефолт событийных звуков ≥10 с: повтор Supply в пределах окна не звучит.
        [Test]
        public void DefaultCooldown_AtLeastTenSeconds_SuppressesRepeat()
        {
            AudioEventCooldowns cooldowns = new AudioEventCooldowns();
            Assert.IsTrue(cooldowns.CooldownSeconds >= 10.0);
            Assert.IsTrue(cooldowns.ShouldPlay(AudioEventSound.Supply, 0.0));
            Assert.IsFalse(cooldowns.ShouldPlay(AudioEventSound.Supply, 2.5));
            Assert.IsFalse(cooldowns.ShouldPlay(AudioEventSound.Supply, 9.9));
            Assert.IsTrue(cooldowns.ShouldPlay(AudioEventSound.Supply, 10.0));
        }

        [Test]
        public void Resolve_TreasuryIncome_IsSilent()
        {
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(
                Event(EventKind.Treasury, EventMessageCatalog.TreasuryDeficit)));
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(
                Event(EventKind.Treasury, EventMessageCatalog.QuestCompleted)));
        }

        [Test]
        public void Cooldown_Reset_RestoresFirstSound()
        {
            AudioEventCooldowns cooldowns = new AudioEventCooldowns(2.5);
            Assert.IsTrue(cooldowns.ShouldPlay(AudioEventSound.Congestion, 0.0));
            Assert.IsFalse(cooldowns.ShouldPlay(AudioEventSound.Congestion, 0.5));
            cooldowns.Reset();
            Assert.IsTrue(cooldowns.ShouldPlay(AudioEventSound.Congestion, 0.6));
        }
    }
}
