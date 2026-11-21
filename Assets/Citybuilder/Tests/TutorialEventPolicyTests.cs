using NUnit.Framework;
using UnityEngine;

namespace Citybuilder.Tests
{
    // Матрица переходов и текстов подсказок v1.2 (эпик 97): пожар, награда,
    // вывоз мусора, ночь. Цепочка 1–7 из TutorialPolicyTests не задевается.
    public class TutorialEventPolicyTests
    {
        private static TutorialEventPolicy.State State()
        {
            return new TutorialEventPolicy.State();
        }

        [Test]
        public void NoEvents_NothingToShow()
        {
            TutorialEventPolicy.State s = State();
            Assert.AreEqual(TutorialEventPolicy.Kind.None, TutorialEventPolicy.Pick(s));
            Assert.AreEqual(string.Empty, TutorialEventPolicy.FormatText(TutorialEventPolicy.Kind.None));
        }

        [Test]
        public void Fire_ShownUntilStationBuilt()
        {
            TutorialEventPolicy.State s = State();
            s.FireHappened = true;
            Assert.AreEqual(TutorialEventPolicy.Kind.FirstFire, TutorialEventPolicy.Pick(s));
            s.HasFireStation = true;
            Assert.AreEqual(TutorialEventPolicy.Kind.None, TutorialEventPolicy.Pick(s));
        }

        [Test]
        public void Fire_HasPriorityOverOtherEvents()
        {
            TutorialEventPolicy.State s = State();
            s.FireHappened = true;
            s.QuestRewarded = true;
            s.WasteCollected = true;
            s.NightArrived = true;
            Assert.AreEqual(TutorialEventPolicy.Kind.FirstFire, TutorialEventPolicy.Pick(s));
        }

        [Test]
        public void EventTexts_NameTheMechanic()
        {
            Language previous = Localization.Language;
            try
            {
                Localization.SetLanguage(Language.Ru);
                Assert.AreEqual(Localization.T("tutorial_event_fire"), TutorialEventPolicy.FormatText(TutorialEventPolicy.Kind.FirstFire));
                Assert.AreEqual(Localization.T("tutorial_event_reward"), TutorialEventPolicy.FormatText(TutorialEventPolicy.Kind.QuestReward));
                Assert.AreEqual(Localization.T("tutorial_event_waste"), TutorialEventPolicy.FormatText(TutorialEventPolicy.Kind.FirstWaste));
                Assert.AreEqual(Localization.T("tutorial_event_night"), TutorialEventPolicy.FormatText(TutorialEventPolicy.Kind.FirstNight));
                Localization.SetLanguage(Language.En);
                Assert.IsTrue(TutorialEventPolicy.FormatText(TutorialEventPolicy.Kind.FirstFire).Contains("Fire!"));
                Assert.IsFalse(TutorialEventPolicy.FormatText(TutorialEventPolicy.Kind.FirstFire).Contains("Пожар"));
            }
            finally { Localization.SetLanguage(previous); }
        }

        [Test]
        public void OnlyFire_PointsToCatalogCard()
        {
            Assert.AreEqual("fire_station", TutorialEventPolicy.TargetBuildingId(TutorialEventPolicy.Kind.FirstFire));
            Assert.IsNull(TutorialEventPolicy.TargetBuildingId(TutorialEventPolicy.Kind.QuestReward));
            Assert.IsNull(TutorialEventPolicy.TargetBuildingId(TutorialEventPolicy.Kind.FirstWaste));
            Assert.IsNull(TutorialEventPolicy.TargetBuildingId(TutorialEventPolicy.Kind.FirstNight));
        }

        [Test]
        public void Acknowledge_HidesOnlyTheChosenKind()
        {
            TutorialEventPolicy.State s = State();
            s.QuestRewarded = true;
            s.WasteCollected = true;
            s = TutorialEventPolicy.Acknowledge(s, TutorialEventPolicy.Kind.QuestReward);
            Assert.IsTrue(TutorialEventPolicy.IsAcknowledged(s, TutorialEventPolicy.Kind.QuestReward));
            Assert.IsFalse(TutorialEventPolicy.IsAcknowledged(s, TutorialEventPolicy.Kind.FirstWaste));
            Assert.AreEqual(TutorialEventPolicy.Kind.FirstWaste, TutorialEventPolicy.Pick(s));
            s = TutorialEventPolicy.Acknowledge(s, TutorialEventPolicy.Kind.FirstWaste);
            Assert.AreEqual(TutorialEventPolicy.Kind.None, TutorialEventPolicy.Pick(s));
        }

        // Rect-тест стрелки на карточку каталога: математика зеркалит CatalogCards.Draw.
        [TestCase(1024f, 600f, 6)]
        [TestCase(1920f, 1080f, 6)]
        public void CatalogCardRect_MirrorsCatalogCardsStrip(float width, float height, int count)
        {
            Rect strip = HudLayout.CatalogCardsRect(width, height, count);
            for (int i = 0; i < count; i++)
            {
                Rect card = TutorialArrowView.CatalogCardRect(width, height, count, i);
                Assert.AreEqual(strip.x + i * (HudLayout.CatalogCardWidth + HudLayout.ButtonGap), card.x, 0.01f);
                Assert.AreEqual(strip.y, card.y, 0.01f);
                Assert.AreEqual(HudLayout.CatalogCardWidth, card.width, 0.01f);
                Assert.AreEqual(strip.height, card.height, 0.01f);
            }
            Assert.AreEqual(0f, TutorialArrowView.CatalogCardRect(width, height, count, -1).width);
        }
    }
}
