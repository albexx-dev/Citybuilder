using NUnit.Framework;

namespace Citybuilder
{
    // Гистерезис уровней дома (EPIC-59): Raise ровно после 20 хороших тиков, не раньше;
    // Lower после 40 плохих; границы 1..3; середина шкалы сбрасывает серию; NaN — Hold без изменений.
    public class HouseLevelPolicyTests
    {
        private HouseLevelPolicy policy;

        [SetUp]
        public void SetUp()
        {
            policy = new HouseLevelPolicy();
        }

        // Прогнать ticks одинаковых тиков комфорта от стартового состояния.
        private HouseLevelPolicy.Verdict Run(int level, int startStreak, float comfort, int ticks)
        {
            HouseLevelPolicy.Verdict verdict = new HouseLevelPolicy.Verdict { Level = level, Streak = startStreak };
            for (int i = 0; i < ticks; i++) verdict = policy.Evaluate(verdict.Level, verdict.Streak, comfort);
            return verdict;
        }

        [Test]
        public void Evaluate_RaiseExactlyAfterTwentyGoodTicks()
        {
            HouseLevelPolicy.Verdict nineteen = Run(1, 0, 0.85f, HouseLevelPolicy.RaiseTicks - 1);
            Assert.AreEqual(HouseLevelPolicy.LevelAction.Hold, nineteen.Action);
            Assert.AreEqual(HouseLevelPolicy.RaiseTicks - 1, nineteen.Streak);
            HouseLevelPolicy.Verdict twenty = policy.Evaluate(nineteen.Level, nineteen.Streak, 0.85f);
            Assert.AreEqual(HouseLevelPolicy.LevelAction.Raise, twenty.Action);
            Assert.AreEqual(2, twenty.Level);
            Assert.AreEqual(0, twenty.Streak);
        }

        [Test]
        public void Evaluate_LowerExactlyAfterFortyBadTicks()
        {
            HouseLevelPolicy.Verdict before = Run(2, 0, 0.3f, HouseLevelPolicy.LowerTicks - 1);
            Assert.AreEqual(HouseLevelPolicy.LevelAction.Hold, before.Action);
            Assert.AreEqual(-(HouseLevelPolicy.LowerTicks - 1), before.Streak);
            HouseLevelPolicy.Verdict after = policy.Evaluate(before.Level, before.Streak, 0.3f);
            Assert.AreEqual(HouseLevelPolicy.LevelAction.Lower, after.Action);
            Assert.AreEqual(1, after.Level);
            Assert.AreEqual(0, after.Streak);
        }

        [Test]
        public void Evaluate_LevelThreeNeverRaises()
        {
            HouseLevelPolicy.Verdict verdict = Run(3, 0, 1f, HouseLevelPolicy.RaiseTicks * 2);
            Assert.AreEqual(HouseLevelPolicy.LevelAction.Hold, verdict.Action);
            Assert.AreEqual(3, verdict.Level);
            Assert.AreEqual(0, verdict.Streak);
        }

        [Test]
        public void Evaluate_LevelOneNeverLowers()
        {
            HouseLevelPolicy.Verdict verdict = Run(1, 0, 0f, HouseLevelPolicy.LowerTicks * 2);
            Assert.AreEqual(HouseLevelPolicy.LevelAction.Hold, verdict.Action);
            Assert.AreEqual(1, verdict.Level);
        }

        [Test]
        public void Evaluate_NaNComfortHoldsAndKeepsStreak()
        {
            HouseLevelPolicy.Verdict verdict = policy.Evaluate(2, 7, float.NaN);
            Assert.AreEqual(HouseLevelPolicy.LevelAction.Hold, verdict.Action);
            Assert.AreEqual(2, verdict.Level);
            Assert.AreEqual(7, verdict.Streak);
        }

        [Test]
        public void Evaluate_NeutralComfortResetsStreak()
        {
            HouseLevelPolicy.Verdict good = Run(1, 0, 0.9f, 15);
            Assert.AreEqual(15, good.Streak);
            HouseLevelPolicy.Verdict neutral = policy.Evaluate(good.Level, good.Streak, 0.6f);
            Assert.AreEqual(HouseLevelPolicy.LevelAction.Hold, neutral.Action);
            Assert.AreEqual(0, neutral.Streak);
        }

        [Test]
        public void Evaluate_AlternatingComfortNeverRaises()
        {
            HouseLevelPolicy.Verdict verdict = new HouseLevelPolicy.Verdict { Level = 1, Streak = 0 };
            for (int i = 0; i < 60; i++)
            {
                float comfort = i % 2 == 0 ? 0.9f : 0.1f;
                verdict = policy.Evaluate(verdict.Level, verdict.Streak, comfort);
                Assert.AreEqual(HouseLevelPolicy.LevelAction.Hold, verdict.Action);
            }
            Assert.AreEqual(1, verdict.Level);
        }

        [Test]
        public void Evaluate_ClampsStrangeLevelInput()
        {
            Assert.AreEqual(3, policy.Evaluate(9, 0, 0.5f).Level);
            Assert.AreEqual(1, policy.Evaluate(-2, 0, 0.5f).Level);
        }
    }
}
