using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Событийные звуки (хвосты #71/#66, ADR-013): звук по типу и подтипу события,
    // ключ опроса — по (tick, kind, key, args), без разбора локализованного текста.
    public class AudioEventPolicyTests
    {
        private LanguageTestScope languageScope;

        private static CityEvent FireStart(int tick, int home)
        {
            return new CityEvent(EventKind.Fire, EventSubtype.FireStart,
                EventMessageCatalog.FireStart, new object[] { home }, tick);
        }

        private static CityEvent Burned(int tick, int home)
        {
            return new CityEvent(EventKind.Fire, EventSubtype.Burned,
                EventMessageCatalog.FireBurned, new object[] { home }, tick);
        }

        [SetUp]
        public void InjectLanguageScope()
        {
            languageScope = new LanguageTestScope();
        }

        [TearDown]
        public void RestoreLanguageScope()
        {
            languageScope.Dispose();
        }

        [Test]
        public void Resolve_FireIgnite_IsFireStart()
        {
            Assert.AreEqual(AudioEventSound.FireStart, AudioEventPolicy.Resolve(FireStart(5, 12)));
        }

        [Test]
        public void Resolve_BurnedHome_IsBurned()
        {
            Assert.AreEqual(AudioEventSound.Burned, AudioEventPolicy.Resolve(Burned(5, 7)));
        }

        [Test]
        public void Resolve_Extinguished_IsSilent()
        {
            CityEvent e = new CityEvent(EventKind.Fire, EventSubtype.Extinguished,
                EventMessageCatalog.FireExtinguished, new object[] { 3 }, 5);
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(e));
        }

        [Test]
        public void Resolve_CrimeGarbageBlackout_ByKind()
        {
            Assert.AreEqual(AudioEventSound.Crime, AudioEventPolicy.Resolve(
                new CityEvent(EventKind.Crime, EventMessageCatalog.CrimeTheft, new object[] { 9, 4 }, 5)));
            Assert.AreEqual(AudioEventSound.Garbage, AudioEventPolicy.Resolve(
                new CityEvent(EventKind.Garbage, EventMessageCatalog.GarbageCollected, new object[] { 2 }, 5)));
            Assert.AreEqual(AudioEventSound.Blackout, AudioEventPolicy.Resolve(
                new CityEvent(EventKind.Blackout, EventMessageCatalog.Blackout, new object[] { 3 }, 5)));
        }

        [Test]
        public void Resolve_TreasuryIncomeAndInfo_AreSilent()
        {
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(
                new CityEvent(EventKind.Treasury, EventMessageCatalog.TreasuryDeficit, new object[] { 45 }, 5)));
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(
                new CityEvent(EventKind.Treasury, EventMessageCatalog.QuestCompleted, new object[] { "Цель", 120 }, 5)));
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(default(CityEvent)));
        }

        [Test]
        public void Resolve_SameEventInRuAndEn_IsStable()
        {
            CityEvent fire = FireStart(5, 12);
            CityEvent burned = Burned(6, 7);
            Localization.SetLanguage(Language.Ru);
            AudioEventSound fireRu = AudioEventPolicy.Resolve(fire);
            AudioEventSound burnedRu = AudioEventPolicy.Resolve(burned);
            Localization.SetLanguage(Language.En);
            Assert.AreEqual(fireRu, AudioEventPolicy.Resolve(fire));
            Assert.AreEqual(burnedRu, AudioEventPolicy.Resolve(burned));
            Assert.AreEqual(AudioEventSound.FireStart, fireRu);
            Assert.AreEqual(AudioEventSound.Burned, burnedRu);
        }

        [Test]
        public void KeyOf_StableAcrossLanguage_ChangesWithKeyAndArgs()
        {
            CityEvent first = FireStart(5, 12);
            CityEvent same = FireStart(5, 12);
            string keyRu = AudioEventPolicy.KeyOf(first);
            Localization.SetLanguage(Language.En);
            Assert.AreEqual(keyRu, AudioEventPolicy.KeyOf(first));
            Assert.AreEqual(keyRu, AudioEventPolicy.KeyOf(same));
            Assert.AreNotEqual(keyRu, AudioEventPolicy.KeyOf(FireStart(6, 12)));
            Assert.AreNotEqual(keyRu, AudioEventPolicy.KeyOf(FireStart(5, 13)));
        }

        [Test]
        public void NewEventsStart_EmptyOrFreshLog_TakesOnlyLastEvent()
        {
            Assert.AreEqual(0, AudioEventPolicy.NewEventsStart(new List<CityEvent>(), ""));
            List<CityEvent> snapshot = new List<CityEvent> { FireStart(1, 1), FireStart(2, 2) };
            Assert.AreEqual(1, AudioEventPolicy.NewEventsStart(snapshot, ""));
        }

        [Test]
        public void NewEventsStart_AfterLastSeenKey_ReturnsTail()
        {
            List<CityEvent> snapshot = new List<CityEvent>
            {
                FireStart(1, 1), Burned(2, 2), FireStart(3, 3)
            };
            string lastKey = AudioEventPolicy.KeyOf(snapshot[0]);
            Assert.AreEqual(1, AudioEventPolicy.NewEventsStart(snapshot, lastKey));
        }

        [Test]
        public void NewEventsStart_AllProcessed_ReturnsEnd()
        {
            List<CityEvent> snapshot = new List<CityEvent> { FireStart(1, 1), FireStart(2, 2) };
            string lastKey = AudioEventPolicy.KeyOf(snapshot[1]);
            Assert.AreEqual(2, AudioEventPolicy.NewEventsStart(snapshot, lastKey));
        }

        [Test]
        public void NewEventsStart_LostKey_LogCleared_TakesOnlyLast()
        {
            List<CityEvent> snapshot = new List<CityEvent> { FireStart(1, 1), FireStart(2, 2) };
            Assert.AreEqual(1, AudioEventPolicy.NewEventsStart(snapshot, "9:4:чужое"));
        }
    }
}
