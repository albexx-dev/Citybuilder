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

        // Казна в минусе: содержание превысило доход. Само решение о штрафе — не здесь.
        public bool IsInDeficit
        {
            get { return Balance < 0; }
        }

        public Treasury(int startingBalance)
        {
            Balance = startingBalance < 0 ? 0 : startingBalance;
        }

        // Казна из сохранения: минус допустим, иначе загрузка «прощала» бы дефицит содержания.
        public static Treasury Restore(int balance)
        {
            Treasury treasury = new Treasury(0);
            treasury.Balance = balance;
            return treasury;
        }

        // Сброс баланса без пересоздания объекта: сохраняет подписчиков и внешние ссылки.
        public void Reset(int balance)
        {
            Balance = balance;
            Notify();
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

        // Регулярное списание (содержание): проходит всегда, баланс уходит в минус.
        // TrySpend за стройку при нехватке по-прежнему отказывает — правило 008.
        public TransactionResult Charge(int amount)
        {
            if (amount <= 0)
            {
                return TransactionResult.Fail("Amount must be positive.");
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
