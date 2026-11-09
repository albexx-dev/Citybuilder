using NUnit.Framework;

namespace Citybuilder
{
    // Содержание через Charge: уход в минус, флаг дефицита и восстановление после Earn.
    // Отдельно проверяем, что TrySpend за стройку при нехватке по-прежнему отказывает (008).
    // Без Unity API: только Treasury.
    public class TreasuryUpkeepTests
    {
        [Test]
        public void Charge_BeyondBalance_GoesNegativeAndNotifies()
        {
            Treasury treasury = new Treasury(0);
            int notified = -1;
            treasury.BalanceChanged += (value) => { notified = value; };
            Treasury.TransactionResult result = treasury.Charge(6);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(-6, treasury.Balance);
            Assert.AreEqual(-6, notified);
        }

        [Test]
        public void Charge_AboveZero_LeavesDeficitFalse()
        {
            Treasury treasury = new Treasury(10);
            treasury.Charge(4);
            Assert.AreEqual(6, treasury.Balance);
            Assert.IsFalse(treasury.IsInDeficit);
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void Charge_ZeroOrNegative_FailsAndDoesNotNotify(int amount)
        {
            Treasury treasury = new Treasury(100);
            int calls = 0;
            treasury.BalanceChanged += (value) => { calls++; };
            Assert.IsFalse(treasury.Charge(amount).Success);
            Assert.AreEqual(0, calls);
            Assert.AreEqual(100, treasury.Balance);
        }

        [Test]
        public void Deficit_EarnAboveZero_LeavesDeficit()
        {
            Treasury treasury = new Treasury(0);
            treasury.Charge(6);
            Assert.IsTrue(treasury.IsInDeficit);
            treasury.Earn(4);
            Assert.IsTrue(treasury.IsInDeficit);
            treasury.Earn(2);
            Assert.AreEqual(0, treasury.Balance);
            Assert.IsFalse(treasury.IsInDeficit);
        }

        [Test]
        public void FreshTreasury_IsNotInDeficit()
        {
            Assert.IsFalse(new Treasury(0).IsInDeficit);
            Assert.IsFalse(new Treasury(500).IsInDeficit);
        }

        [Test]
        public void TrySpend_AtZeroBalance_StillFails()
        {
            Treasury treasury = new Treasury(0);
            Treasury.TransactionResult result = treasury.TrySpend(1, "house");
            Assert.IsFalse(result.Success);
            Assert.AreEqual(0, treasury.Balance);
        }

        [Test]
        public void TrySpend_InDeficit_StillFails()
        {
            Treasury treasury = new Treasury(0);
            treasury.Charge(5);
            Assert.IsFalse(treasury.TrySpend(1, "house").Success);
            Assert.IsFalse(treasury.CanAfford(1));
            Assert.AreEqual(-5, treasury.Balance);
        }

        [Test]
        public void NetHour_UnderIncome_StaysPositive()
        {
            Treasury treasury = new Treasury(100);
            treasury.Earn(30);
            treasury.Charge(16);
            Assert.AreEqual(114, treasury.Balance);
            Assert.IsFalse(treasury.IsInDeficit);
        }

        [Test]
        public void Restore_PreservesNegativeDeficitBalance()
        {
            Treasury treasury = Treasury.Restore(-45);
            Assert.AreEqual(-45, treasury.Balance);
            Assert.IsTrue(treasury.IsInDeficit);
        }
    }
}
