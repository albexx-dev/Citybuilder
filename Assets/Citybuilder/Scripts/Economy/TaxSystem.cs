using System;

namespace Citybuilder
{
    // Налоги с работающих (эпик #49): зональный сбор по ставкам TaxPolicy. Без Unity и без состояния.
    // Связи работник→здание нет (EmploymentSystem хранит только агрегаты), поэтому занятые
    // распределяются по зонам пропорционально долям рабочих мест (ZonalJobs.DistributeEmployed).
    public class TaxSystem
    {
        private readonly EmploymentSystem employment;
        private readonly Treasury treasury;
        private readonly Func<ZonalJobs> zonalJobsProvider;

        public TaxSystem(EmploymentSystem employment, Treasury treasury)
            : this(employment, treasury, null)
        {
        }

        // Поставщик зональных рабочих мест. Пусто — вся занятость считается жилой.
        public TaxSystem(EmploymentSystem employment, Treasury treasury, Func<ZonalJobs> zonalJobsProvider)
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
            this.zonalJobsProvider = zonalJobsProvider;
        }

        // Сколько работающих платит. Для строки дохода в HUD.
        public int Employed
        {
            get { return employment.Employed; }
        }

        // Казна, в которую зачисляется налог.
        public Treasury Treasury
        {
            get { return treasury; }
        }

        // Занятые по зонам: пропорционально долям рабочих мест. Мест нет — вся занятость жилой.
        public ZonalJobs EmployedPerZone()
        {
            ZonalJobs jobs = zonalJobsProvider != null ? zonalJobsProvider() : new ZonalJobs();
            return jobs.DistributeEmployed(employment.Employed);
        }

        // Налог за тик по ставкам политики: сумма по зонам занятых × ставка зоны.
        public int ComputeTax(TaxPolicy policy)
        {
            if (policy == null)
            {
                return 0;
            }
            ZonalJobs employed = EmployedPerZone();
            return policy.IncomeForZone(ZoneType.Residential, employed.Residential)
                + policy.IncomeForZone(ZoneType.Commercial, employed.Commercial)
                + policy.IncomeForZone(ZoneType.Industrial, employed.Industrial);
        }

        // Зачисляет зональный налог в казну. Суммы нет — Fail, Earn не дёргаем.
        public Treasury.TransactionResult Collect(TaxPolicy policy)
        {
            int total = ComputeTax(policy);
            if (total <= 0)
            {
                return Treasury.TransactionResult.Fail("No taxes to collect.");
            }
            return treasury.Earn(total);
        }

        // Налог за тик одной ставкой (совместимость): Employed × ставка. Ставка 0 и ниже — сбор выключен.
        public int ComputeTax(int perWorker)
        {
            if (perWorker <= 0)
            {
                return 0;
            }
            return employment.Employed * perWorker;
        }

        // Зачисляет налог одной ставкой в казну (совместимость). Суммы нет — Fail, Earn не дёргаем.
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
