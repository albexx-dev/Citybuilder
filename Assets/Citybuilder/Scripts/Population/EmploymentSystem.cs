using System;

namespace Citybuilder
{
    // Агрегаты занятости поверх готовых счётчиков. Своего состояния нет, чистый C#.
    public class EmploymentSystem
    {
        private readonly PopulationSystem population;
        private readonly SettlementSystem settlement;

        public EmploymentSystem(PopulationSystem population, SettlementSystem settlement)
        {
            if (population == null)
            {
                throw new ArgumentNullException("population");
            }
            if (settlement == null)
            {
                throw new ArgumentNullException("settlement");
            }
            this.population = population;
            this.settlement = settlement;
        }

        // Трудоустроено: жителей не больше, чем рабочих мест.
        public int Employed
        {
            get { return Math.Min(population.TotalResidents, settlement.TotalJobs); }
        }

        // Безработные: остаток жителей, минимум 0.
        public int Unemployed
        {
            get { return Math.Max(0, population.TotalResidents - Employed); }
        }

        // Доля трудоустроенных от всех жителей. Жителей нет — 0.
        public float EmploymentRate
        {
            get
            {
                if (population.TotalResidents == 0)
                {
                    return 0f;
                }
                return Employed / (float)population.TotalResidents;
            }
        }

        // Доля занятых рабочих мест. Мест нет — 0.
        public float JobsFillRate
        {
            get
            {
                if (settlement.TotalJobs == 0)
                {
                    return 0f;
                }
                return Employed / (float)settlement.TotalJobs;
            }
        }
    }
}
