using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Решения игрока (ADR-020, issue #167): две записи в ленте, казна, тушение и дедуп.
    public class DecisionControllerTests
    {
        private CityEventLog log;
        private Treasury treasury;
        private List<int> extinguished;
        private DecisionController controller;

        [SetUp]
        public void SetUp()
        {
            log = new CityEventLog();
            treasury = new Treasury(100);
            extinguished = new List<int>();
            controller = new DecisionController(log, treasury, extinguished.Add);
        }

        [Test]
        public void ExpectedResolveSuppress_TwoEntriesTreasurySpentAndExtinguished()
        {
            Assert.IsTrue(controller.Expected(7, 10));
            Assert.IsTrue(controller.Resolve(7, true, 10));
            Assert.AreEqual(2, log.Count);
            CityEvent offered = log.Snapshot()[0];
            Assert.AreEqual(EventMessageCatalog.DecisionOffered, offered.MessageKey);
            Assert.AreEqual(EventSubtype.DecisionOffered, offered.Subtype);
            Assert.AreEqual(7, offered.Args[0]);
            CityEvent resolved = log.Snapshot()[1];
            Assert.AreEqual(EventMessageCatalog.DecisionSuppressed, resolved.MessageKey);
            Assert.AreEqual(EventSubtype.DecisionSuppressed, resolved.Subtype);
            Assert.AreEqual(11, resolved.Tick);
            Assert.AreEqual(75, treasury.Balance);
            Assert.AreEqual(7, extinguished[0]);
        }

        [Test]
        public void ResolveIgnore_NoSpendNoExtinguish_SecondKeyDiffers()
        {
            controller.Expected(9, 4);
            Assert.IsTrue(controller.Resolve(9, false, 4));
            Assert.AreEqual(100, treasury.Balance);
            Assert.IsEmpty(extinguished);
            Assert.AreEqual(EventMessageCatalog.DecisionIgnored, log.Snapshot()[1].MessageKey);
            Assert.AreNotEqual(log.Snapshot()[0].MessageKey, log.Snapshot()[1].MessageKey);
        }

        [Test]
        public void TwoAdds_SameTickDifferentKeysAndArgs_DedupKeepsBoth()
        {
            bool first = log.Add(EventKind.Fire, EventMessageCatalog.DecisionOffered, new object[] { 7, 25 }, 5);
            bool second = log.Add(EventKind.Fire, EventMessageCatalog.DecisionSuppressed, new object[] { 7, 25, 1 }, 5);
            Assert.IsTrue(first && second);
            Assert.AreEqual(2, log.Count);
        }

        [Test]
        public void ExpectedTwiceForSameHome_OnlyOneEntry_AndNoneAfterResolve()
        {
            Assert.IsTrue(controller.Expected(7, 1));
            Assert.IsFalse(controller.Expected(7, 2));
            Assert.IsTrue(controller.Resolve(7, false, 2));
            Assert.IsFalse(controller.Expected(7, 20));
            Assert.AreEqual(2, log.Count);
        }

        [Test]
        public void ResolveWithoutFunds_KeepsDecisionPending()
        {
            DecisionController poor = new DecisionController(log, new Treasury(0), extinguished.Add);
            Assert.IsTrue(poor.Expected(3, 6));
            Assert.IsFalse(poor.Resolve(3, true, 6));
            Assert.AreEqual(1, poor.PendingCount);
            Assert.IsTrue(poor.Resolve(3, false, 6));
            Assert.AreEqual(2, log.Count);
        }

        [Test]
        public void DecisionMessages_RegisteredInBothLanguages()
        {
            Localization.SetLanguage(Language.Ru);
            string[] keys = { EventMessageCatalog.DecisionOffered, EventMessageCatalog.DecisionSuppressed,
                EventMessageCatalog.DecisionIgnored, EventMessageCatalog.DecisionTitle,
                EventMessageCatalog.DecisionSuppressButton, EventMessageCatalog.DecisionIgnoreButton };
            foreach (string key in keys) Assert.IsTrue(Localization.Has(key), key);
            Assert.AreEqual("Тушить за $25", Localization.T(EventMessageCatalog.DecisionSuppressButton, 25));
        }
    }
}
