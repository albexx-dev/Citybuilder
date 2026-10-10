using System;

namespace Citybuilder
{
    // Налоги с работающих. Чистая симуляция без Unity и без состояния.
    public class TaxSystem
    {
        private readonly EmploymentSystem employment;
        private readonly Treasury treasury;

        public TaxSystem(EmploymentSystem employment, Treasury treasury)
        {
            if (employment == null)
            {
                throw new ArgumentNullException("employment");
            }
            if (treasury == null)
            {
                throw new ArgumentNullException("treasury");
            }
            this.employment = employment;
            this.treasury = treasury;
        }

        // Налог за тик: Employed × ставка. Ставка 0 и ниже — сбор выключен.
        public int ComputeTax(int perWorker)
        {
            if (perWorker <= 0)
            {
                return 0;
            }
            return employment.Employed * perWorker;
        }

        // Зачисляет налог в казну. Суммы нет — Fail, Earn не дёргаем.
        public Treasury.TransactionResult Collect(int perWorker)
        {
            int total = ComputeTax(perWorker);
            if (total <= 0)
            {
                return Treasury.TransactionResult.Fail("No taxes to collect.");
            }
            return treasury.Earn(total);
        }
    }
}
