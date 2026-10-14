using System;

namespace Citybuilder
{
    // Удовлетворённость жителей жильём и работой. Своего состояния нет, чистый C#.
    public class NeedSystem
    {
        private readonly PopulationSystem population;
        private readonly SettlementSystem settlement;

        public NeedSystem(PopulationSystem population, SettlementSystem settlement)
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
        private int Employed
        {
            get { return Math.Min(population.TotalResidents, settlement.TotalJobs); }
        }

        // Доля свободного жилья. Домов нет — 0, пустой город — 1.
        public float HousingRoomRate
        {
            get
            {
                if (settlement.TotalCapacity == 0)
                {
                    return 0f;
                }
                if (population.TotalResidents == 0)
                {
                    return 1f;
                }
                float rate = 1f - population.TotalResidents / (float)settlement.TotalCapacity;
                return Math.Max(0f, rate);
            }
        }

        // Доля свободных рабочих мест. Пустой город — 1, мест нет — 0.
        public float JobsRoomRate
        {
            get
            {
                if (population.TotalResidents == 0)
                {
                    return 1f;
                }
                if (settlement.TotalJobs == 0)
                {
                    return 0f;
                }
                return 1f - Employed / (float)settlement.TotalJobs;
            }
        }

        // Доля трудоустроенных. Жителей нет — 1: некого обижать.
        public float EmploymentRate
        {
            get
            {
                if (population.TotalResidents == 0)
                {
                    return 1f;
                }
                return Employed / (float)population.TotalResidents;
            }
        }

        // Среднее трёх потребностей, всё в 0..1.
        public float AverageSatisfaction
        {
            get { return (HousingRoomRate + JobsRoomRate + EmploymentRate) / 3f; }
        }
    }
}
