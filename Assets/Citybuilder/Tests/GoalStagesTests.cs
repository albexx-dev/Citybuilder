using NUnit.Framework;

namespace Citybuilder
{
    // Ступени цели: границы наград, переходы и прогресс внутри ступени.
    public class GoalStagesTests
    {
        private const float Delta = 1e-4f;

        [Test]
        public void Start_Stage0_Target50()
        {
            GoalStages stages = new GoalStages();
            Assert.AreEqual(0, stages.Stage);
            Assert.AreEqual(50, stages.Target);
            Assert.IsFalse(stages.AllDone);
            Assert.AreEqual(0f, stages.Progress01(0), Delta);
        }

        [Test]
        public void BelowFirst_NoClaim()
        {
            GoalStages stages = new GoalStages();
            Assert.AreEqual(0, stages.Claim(49));
            Assert.AreEqual(0, stages.Stage);
            Assert.AreEqual(50, stages.Target);
        }

        [Test]
        public void FirstClaim_Pays1000_Target100()
        {
            GoalStages stages = new GoalStages();
            Assert.AreEqual(1000, stages.Claim(50));
            Assert.AreEqual(1, stages.Stage);
            Assert.AreEqual(100, stages.Target);
        }

        [Test]
        public void RepeatedClaim_PaysNothing()
        {
            GoalStages stages = new GoalStages();
            Assert.AreEqual(1000, stages.Claim(50));
            Assert.AreEqual(0, stages.Claim(50));
            Assert.AreEqual(1, stages.Stage);
        }

        [Test]
        public void SecondClaim_Pays2000()
        {
            GoalStages stages = new GoalStages();
            stages.Claim(50);
            Assert.AreEqual(2000, stages.Claim(100));
            Assert.AreEqual(2, stages.Stage);
            Assert.AreEqual(200, stages.Target);
        }

        [Test]
        public void ThirdClaim_Pays4000_AllDone()
        {
            GoalStages stages = new GoalStages();
            stages.Claim(50);
            stages.Claim(100);
            Assert.AreEqual(4000, stages.Claim(200));
            Assert.IsTrue(stages.AllDone);
            Assert.AreEqual(200, stages.Target);
        }

        [Test]
        public void JumpTo250_PaysAllStages()
        {
            GoalStages stages = new GoalStages();
            Assert.AreEqual(7000, stages.Claim(250));
            Assert.AreEqual(3, stages.Stage);
            Assert.IsTrue(stages.AllDone);
        }

        [Test]
        public void AboveDone_NoDoubleReward()
        {
            GoalStages stages = new GoalStages();
            stages.Claim(200);
            Assert.AreEqual(0, stages.Claim(300));
            Assert.AreEqual(3, stages.Stage);
        }

        [Test]
        public void NegativeResidents_NoClaim()
        {
            GoalStages stages = new GoalStages();
            Assert.AreEqual(0, stages.Claim(-10));
            Assert.AreEqual(0, stages.Stage);
            Assert.AreEqual(0f, stages.Progress01(-10), Delta);
        }

        [Test]
        public void Progress_SecondStage_Midpoint()
        {
            GoalStages stages = new GoalStages();
            stages.Claim(50);
            stages.Claim(100);
            Assert.AreEqual(0.5f, stages.Progress01(150), Delta);
            Assert.AreEqual(1f, stages.Progress01(250), Delta);
            Assert.AreEqual(0f, stages.Progress01(100), Delta);
        }

        [Test]
        public void Progress_FirstStage_Half()
        {
            GoalStages stages = new GoalStages();
            Assert.AreEqual(0.5f, stages.Progress01(25), Delta);
        }
    }
}
