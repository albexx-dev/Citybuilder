namespace Citybuilder
{
    // Кредит партии (ADR-019): тело и процент в одной записи, чистое состояние без Unity.
    // Выдача тела идёт через Earn, погашение и процент — через Charge, а не TrySpend.
    public class CreditLedger
    {
        public int Principal { get; private set; }
        public int Outstanding { get; private set; }
        public int InterestPercent { get; private set; }

        public CreditLedger() : this(0, 0) { }

        public CreditLedger(int principal, int interestPercent)
        {
            Principal = principal < 0 ? 0 : principal;
            Outstanding = Principal;
            InterestPercent = interestPercent < 0 ? 0 : interestPercent > 100 ? 100 : interestPercent;
        }

        // Восстановление из слота v7 (ADR-022): долг — текущий остаток, тело приравнивается к нему.
        public void Restore(int outstanding, int interestPercent)
        {
            Outstanding = outstanding < 0 ? 0 : outstanding;
            Principal = Outstanding;
            InterestPercent = interestPercent < 0 ? 0 : interestPercent > 100 ? 100 : interestPercent;
        }

        // Тело кредита пополняет казну; неположительная сумма игнорируется.
        public bool Borrow(Treasury treasury, int amount)
        {
            if (treasury == null || amount <= 0 || !treasury.Earn(amount).Success) return false;
            Principal += amount;
            Outstanding += amount;
            return true;
        }

        // Добровольное погашение тела: Charge проходит даже при пустой казне.
        public bool Repay(Treasury treasury, int amount)
        {
            if (treasury == null || amount <= 0 || amount > Outstanding) return false;
            if (!treasury.Charge(amount).Success) return false;
            Outstanding -= amount;
            return true;
        }

        // Остаток × ставка / 100 целочисленно, усечение к нулю; long страхует переполнение.
        public int InterestForTick()
        {
            if (Outstanding <= 0 || InterestPercent <= 0) return 0;
            return (int)((long)Outstanding * InterestPercent / 100L);
        }

        // Процент на границе тика: минус казны — легальный дефицит, не ошибка и не поражение.
        public int TickInterest(Treasury treasury)
        {
            int interest = InterestForTick();
            if (treasury == null || interest <= 0 || !treasury.Charge(interest).Success) return 0;
            return interest;
        }
    }
}
