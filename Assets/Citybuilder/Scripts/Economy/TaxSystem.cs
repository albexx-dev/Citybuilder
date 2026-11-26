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
        private readonly DistrictPolicy districts;
        private readonly Func<int> ratingProvider;

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

        // Районы и рейтинг мэра: override ставки поверх зоны и бонус к ставке. Без них — старый сбор.
        public TaxSystem(EmploymentSystem employment, Treasury treasury, Func<ZonalJobs> zonalJobsProvider,
            DistrictPolicy districts, Func<int> ratingProvider)
            : this(employment, treasury, zonalJobsProvider)
        {
            this.districts = districts;
            this.ratingProvider = ratingProvider;
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
            int rating = ratingProvider != null ? ratingProvider() : 0;
            return ZoneIncome(policy, ZoneType.Residential, employed.Residential, rating)
                + ZoneIncome(policy, ZoneType.Commercial, employed.Commercial, rating)
                + ZoneIncome(policy, ZoneType.Industrial, employed.Industrial, rating);
        }

        // Приоритет: ставка района (если назначен) > ставка зоны; бонус рейтинга поверх.
        private int ZoneIncome(TaxPolicy policy, ZoneType zone, int count, int rating)
        {
            if (count <= 0) return 0;
            if (districts != null)
            {
                int district = districts.DistrictOfZone(zone);
                if (district > 0) return districts.IncomeFor(policy, zone, district, count, rating);
            }
            return count * MayorRating.ModifyRate(policy.RateFor(zone), rating);
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
