using NUnit.Framework;

namespace Citybuilder
{
    // Деньги города: траты, доходы, границы. Без Unity API.
    public class TreasuryTests
    {
        [Test]
        public void Ctor_Negative_StartsAtZero()
        {
            Assert.AreEqual(0, new Treasury(-100).Balance);
        }

        [TestCase(100, 100, true)]
        [TestCase(100, 101, false)]
        [TestCase(100, 0, true)]
        [TestCase(100, -5, false)]
        public void CanAfford_ChecksBalance(int balance, int amount, bool expected)
        {
            Assert.AreEqual(expected, new Treasury(balance).CanAfford(amount));
        }

        [Test]
        public void TrySpend_Enough_SucceedsAndNotifies()
        {
            Treasury treasury = new Treasury(100);
            int notified = -1;
            treasury.BalanceChanged += (value) => { notified = value; };
            Treasury.TransactionResult result = treasury.TrySpend(40, "house");
            Assert.IsTrue(result.Success);
            Assert.AreEqual(60, treasury.Balance);
            Assert.AreEqual(60, notified);
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void TrySpend_ZeroOrNegative_Fails(int amount)
        {
            Treasury treasury = new Treasury(100);
            Treasury.TransactionResult result = treasury.TrySpend(amount, "house");
            Assert.IsFalse(result.Success);
            Assert.AreEqual(100, treasury.Balance);
        }

        [Test]
        public void TrySpend_Shortage_Fails()
        {
            Treasury treasury = new Treasury(30);
            Treasury.TransactionResult result = treasury.TrySpend(40, "house");
            Assert.IsFalse(result.Success);
            Assert.AreEqual(30, treasury.Balance);
        }

        [Test]
        public void Earn_Positive_GrowsBalanceAndNotifies()
        {
            Treasury treasury = new Treasury(100);
            int notified = -1;
            treasury.BalanceChanged += (value) => { notified = value; };
            Treasury.TransactionResult result = treasury.Earn(25);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(125, treasury.Balance);
            Assert.AreEqual(125, notified);
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void Earn_ZeroOrNegative_Fails(int amount)
        {
            Treasury treasury = new Treasury(100);
            Treasury.TransactionResult result = treasury.Earn(amount);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(100, treasury.Balance);
        }

        [Test]
        public void FailedOps_DoNotNotify()
        {
            Treasury treasury = new Treasury(30);
            int calls = 0;
            treasury.BalanceChanged += (value) => { calls++; };
            treasury.TrySpend(40, "house");
            treasury.TrySpend(0, "house");
            treasury.Earn(-5);
            Assert.AreEqual(0, calls);
            Assert.AreEqual(30, treasury.Balance);
        }
    }
}
