namespace Citybuilder
{
    // Деньги города. Чистая симуляция без Unity.
    public class Treasury
    {
        public struct TransactionResult
        {
            public bool Success;
            public string Message;

            public static TransactionResult Ok()
            {
                return new TransactionResult { Success = true, Message = string.Empty };
            }

            public static TransactionResult Fail(string message)
            {
                return new TransactionResult { Success = false, Message = message };
            }
        }

        public event System.Action<int> BalanceChanged;

        public int Balance { get; private set; }

        public Treasury(int startingBalance)
        {
            Balance = startingBalance < 0 ? 0 : startingBalance;
        }

        public bool CanAfford(int amount)
        {
            if (amount < 0)
            {
                return false;
            }
            return Balance >= amount;
        }

        public TransactionResult TrySpend(int amount, string reason)
        {
            if (amount <= 0)
            {
                return TransactionResult.Fail("Amount must be positive.");
            }
            if (Balance < amount)
            {
                return TransactionResult.Fail("Not enough funds.");
            }
            Balance -= amount;
            Notify();
            return TransactionResult.Ok();
        }

        public TransactionResult Earn(int amount)
        {
            if (amount <= 0)
            {
                return TransactionResult.Fail("Amount must be positive.");
            }
            Balance += amount;
            Notify();
            return TransactionResult.Ok();
        }

        private void Notify()
        {
            if (BalanceChanged != null)
            {
                BalanceChanged(Balance);
            }
        }
    }
}
