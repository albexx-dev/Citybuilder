using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Событийные звуки (хвосты #71/#66): сопоставление «событие ленты -> сигнал» и
    // индекс новых событий снимка. Чистый C# без Unity — как SoundCooldownPolicyTests.
    public class AudioEventPolicyTests
    {
        private static CityEvent FireStart(int tick, int home)
        {
            return new CityEvent(EventKind.Fire, "Пожар в доме #" + home + "!", tick);
        }

        [Test]
        public void Resolve_FireIgnite_IsFireStart()
        {
            Assert.AreEqual(AudioEventSound.FireStart, AudioEventPolicy.Resolve(EventKind.Fire, "Пожар в доме #12!"));
        }

        [Test]
        public void Resolve_BurnedHome_IsBurned()
        {
            Assert.AreEqual(AudioEventSound.Burned, AudioEventPolicy.Resolve(EventKind.Fire, "Дом #7 сгорел дотла"));
        }

        [Test]
        public void Resolve_Extinguished_IsSilent()
        {
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(EventKind.Fire, "Пожар в доме #3 потушен"));
        }

        [Test]
        public void Resolve_Crime_IsCrime()
        {
            Assert.AreEqual(AudioEventSound.Crime, AudioEventPolicy.Resolve(EventKind.Crime, "Кража в доме #9 (День 4)"));
        }

        [Test]
        public void Resolve_Garbage_IsGarbage()
        {
            // Вывоз мусора — тихий сигнал, не монетка.
            Assert.AreEqual(AudioEventSound.Garbage, AudioEventPolicy.Resolve(EventKind.Garbage, "Мусор вывезен (День 3)"));
        }

        [Test]
        public void Resolve_Blackout_IsBlackout()
        {
            Assert.AreEqual(AudioEventSound.Blackout, AudioEventPolicy.Resolve(EventKind.Blackout, "Отключилась энергия (День 3)"));
        }

        [Test]
        public void Resolve_TreasuryAndIncome_AreSilent()
        {
            // Доктрина: coin.wav — только награда цели (GoalView.cs:64); лента дохода молчит.
            // ShouldPlayCoin остаётся мёртвым путём только в тестах SoundCooldownPolicyTests.
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(EventKind.Treasury, "Налоговый сбор +45 $/ч"));
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(EventKind.Treasury, "Казна +120 $"));
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(EventKind.Supply, "Энергия 46%"));
        }

        [Test]
        public void Resolve_OtherKinds_AndEmptyMessage_AreSilent()
        {
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(EventKind.Supply, "Энергия 46%"));
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(EventKind.Fire, ""));
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(EventKind.Treasury, "Дефицит казны: баланс $5"));
        }

        [Test]
        public void KeyOf_IsStable_ForSameEvent()
        {
            CityEvent first = FireStart(5, 12);
            CityEvent second = FireStart(5, 12);
            Assert.AreEqual(AudioEventPolicy.KeyOf(first), AudioEventPolicy.KeyOf(second));
            Assert.AreNotEqual(AudioEventPolicy.KeyOf(first), AudioEventPolicy.KeyOf(FireStart(6, 12)));
        }

        [Test]
        public void NewEventsStart_EmptyOrFreshLog_TakesOnlyLastEvent()
        {
            Assert.AreEqual(0, AudioEventPolicy.NewEventsStart(new List<CityEvent>(), ""));
            List<CityEvent> snapshot = new List<CityEvent> { FireStart(1, 1), FireStart(2, 2) };
            // Старт вью: историю ленты не переигрываем, берём только последнее событие.
            Assert.AreEqual(1, AudioEventPolicy.NewEventsStart(snapshot, ""));
        }

        [Test]
        public void NewEventsStart_AfterLastSeenKey_ReturnsTail()
        {
            List<CityEvent> snapshot = new List<CityEvent>
            {
                FireStart(1, 1), new CityEvent(EventKind.Crime, "Кража в доме #2 (День 0)", 2), FireStart(3, 3)
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
