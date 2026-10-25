using System;

namespace Citybuilder
{
    // Удовлетворённость жителей жильём, работой, энергией и водой. Своего состояния нет, чистый C#.
    public class NeedSystem
    {
        private readonly PopulationSystem population;
        private readonly SettlementSystem settlement;
        private readonly Func<float> powerRate;
        private readonly Func<float> waterRate;

        // Старый конструктор: ни энергии, ни воды, среднее по трём потребностям.
        public NeedSystem(PopulationSystem population, SettlementSystem settlement)
            : this(population, settlement, null, null) { }

        // Только энергия: вода в среднем не участвует.
        public NeedSystem(PopulationSystem population, SettlementSystem settlement, Func<float> powerRate)
            : this(population, settlement, powerRate, null) { }

        // Провайдеры необязательны: null — показатель не входит в среднее.
        public NeedSystem(PopulationSystem population, SettlementSystem settlement,
            Func<float> powerRate, Func<float> waterRate)
        {
            if (population == null) throw new ArgumentNullException("population");
            if (settlement == null) throw new ArgumentNullException("settlement");
            this.population = population;
            this.settlement = settlement;
            this.powerRate = powerRate;
            this.waterRate = waterRate;
        }

        // Трудоустроено: жителей не больше, чем рабочих мест.
        private int Employed
        {
            get { return Math.Min(population.TotalResidents, settlement.TotalJobs); }
        }

        // Флаг провайдера: не подключён — показателя нет в среднем.
        public bool HasPower
        {
            get { return powerRate != null; }
        }
        public bool HasWater
        {
            get { return waterRate != null; }
        }
        public float PowerRate
        {
            get { return Rate01(powerRate); }
        }
        public float WaterRate
        {
            get { return Rate01(waterRate); }
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

        // Среднее по 3 базовым потребностям плюс подключённые энергия и вода (3..5 слагаемых).
        public float AverageSatisfaction
        {
            get
            {
                float sum = HousingRoomRate + JobsRoomRate + EmploymentRate;
                int count = 3;
                if (HasPower)
                {
                    sum += PowerRate;
                    count++;
                }
                if (HasWater)
                {
                    sum += WaterRate;
                    count++;
                }
                return sum / count;
            }
        }

        // Ставка провайдера 0..1. Провайдера нет — 1: потребность не ограничивает.
        private static float Rate01(Func<float> provider)
        {
            if (provider == null)
            {
                return 1f;
            }
            return Clamp01(provider());
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
