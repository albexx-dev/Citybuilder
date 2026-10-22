using System;

namespace Citybuilder
{
    // Удовлетворённость жителей жильём, работой и энергией. Своего состояния нет, чистый C#.
    public class NeedSystem
    {
        private readonly PopulationSystem population;
        private readonly SettlementSystem settlement;
        private readonly Func<float> powerRate;

        // Старый конструктор: без провайдера энергии, среднее по трём потребностям.
        public NeedSystem(PopulationSystem population, SettlementSystem settlement)
            : this(population, settlement, null)
        {
        }

        // Провайдер энергии необязателен: null — энергия не участвует в среднем.
        public NeedSystem(PopulationSystem population, SettlementSystem settlement, Func<float> powerRate)
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
            this.powerRate = powerRate;
        }

        // Трудоустроено: жителей не больше, чем рабочих мест.
        private int Employed
        {
            get { return Math.Min(population.TotalResidents, settlement.TotalJobs); }
        }

        // Есть ли источник энергии: без провайдера энергия в среднее не входит.
        public bool HasPower
        {
            get { return powerRate != null; }
        }

        // Покрытие энергией 0..1. Провайдера нет — 1: энергии не бывает мало.
        public float PowerRate
        {
            get
            {
                if (powerRate == null)
                {
                    return 1f;
                }
                return Clamp01(powerRate());
            }
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

        // Среднее потребностей: 4 с энергией, 3 без неё (совместимость с 036).
        public float AverageSatisfaction
        {
            get
            {
                float sum = HousingRoomRate + JobsRoomRate + EmploymentRate;
                if (!HasPower)
                {
                    return sum / 3f;
                }
                return (sum + PowerRate) / 4f;
            }
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }
            if (value > 1f)
            {
                return 1f;
            }
            return value;
        }
    }
}
