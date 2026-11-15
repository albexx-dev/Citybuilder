using NUnit.Framework;
using System.Collections.Generic;

namespace Citybuilder
{
    // Троттлинг однотипных событий (issue: поп/лента спамили каждый тик,
    // когда в сообщении менялись числа): один вид — не чаще cooldownTicks.
    public class EventThrottlePolicyTests
    {
        [Test]
        public void FirstCall_Allows()
        {
            EventThrottlePolicy policy = new EventThrottlePolicy(10);
            Assert.IsTrue(policy.Allow("Supply", 0));
        }

        [Test]
        public void SameKind_WithinCooldown_Blocked()
        {
            EventThrottlePolicy policy = new EventThrottlePolicy(10);
            Assert.IsTrue(policy.Allow("Supply", 0));
            for (int tick = 1; tick <= 9; tick++)
            {
                Assert.IsFalse(policy.Allow("Supply", tick), "tick " + tick);
            }
        }

        [Test]
        public void SameKind_AfterCooldown_AllowsAgain()
        {
            EventThrottlePolicy policy = new EventThrottlePolicy(10);
            policy.Allow("Supply", 0);
            Assert.IsTrue(policy.Allow("Supply", 10));
        }

        [Test]
        public void DifferentKinds_AreIndependent()
        {
            EventThrottlePolicy policy = new EventThrottlePolicy(10);
            Assert.IsTrue(policy.Allow("Supply", 0));
            Assert.IsTrue(policy.Allow("Congestion", 0));
            Assert.IsTrue(policy.Allow("Employment", 0));
        }

        [Test]
        public void Reset_ClearsAllKinds()
        {
            EventThrottlePolicy policy = new EventThrottlePolicy(10);
            policy.Allow("Supply", 0);
            policy.Allow("Congestion", 0);
            policy.Reset();
            Assert.IsTrue(policy.Allow("Supply", 1));
            Assert.IsTrue(policy.Allow("Congestion", 1));
        }

        [Test]
        public void Cooldown_ClampedToAtLeastOne()
        {
            EventThrottlePolicy policy = new EventThrottlePolicy(0);
            Assert.IsTrue(policy.Allow("Supply", 0));
            Assert.IsTrue(policy.Allow("Supply", 1));
            Assert.IsTrue(policy.Allow("Supply", 2));
        }

        [Test]
        public void BlockedCall_DoesNotExtendCooldown()
        {
            EventThrottlePolicy policy = new EventThrottlePolicy(10);
            Assert.IsTrue(policy.Allow("Supply", 0));
            Assert.IsFalse(policy.Allow("Supply", 5));
            Assert.IsTrue(policy.Allow("Supply", 10));
        }
    }
}
