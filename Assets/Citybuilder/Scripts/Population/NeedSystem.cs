using System;

namespace Citybuilder
{
    // Удовлетворённость жителей: жильё, работа, занятость и подключённые провайдеры. Чистый C#.
    public class NeedSystem
    {
        private readonly PopulationSystem population;
        private readonly SettlementSystem settlement;
        private readonly NeedProviders providers;

        // Старый конструктор: ни энергии, ни воды, ни мусора — среднее по трём потребностям.
        public NeedSystem(PopulationSystem population, SettlementSystem settlement)
            : this(population, settlement, new NeedProviders()) { }

        // Только энергия: вода и мусор в среднем не участвуют.
        public NeedSystem(PopulationSystem population, SettlementSystem settlement, Func<float> powerRate)
            : this(population, settlement, new NeedProviders(powerRate, null)) { }

        public NeedSystem(PopulationSystem population, SettlementSystem settlement,
            Func<float> powerRate, Func<float> waterRate)
            : this(population, settlement, new NeedProviders(powerRate, waterRate)) { }

        // Провайдеры необязательны: null — показатель не входит в среднее.
        public NeedSystem(PopulationSystem population, SettlementSystem settlement,
            Func<float> powerRate, Func<float> waterRate, Func<float> wasteRate)
            : this(population, settlement, new NeedProviders(powerRate, waterRate, wasteRate)) { }

        private NeedSystem(PopulationSystem population, SettlementSystem settlement, NeedProviders providers)
        {
            if (population == null) throw new ArgumentNullException("population");
            if (settlement == null) throw new ArgumentNullException("settlement");
            this.population = population;
            this.settlement = settlement;
            this.providers = providers ?? new NeedProviders();
        }

        // Трудоустроено: жителей не больше, чем рабочих мест.
        private int Employed
        {
            get { return Math.Min(population.TotalResidents, settlement.TotalJobs); }
        }

        // Флаги и ставки проксируются в провайдеров: подключён ли показатель и сколько он даёт.
        public bool HasPower { get { return providers.HasPower; } }
        public bool HasWater { get { return providers.HasWater; } }
        public bool HasWaste { get { return providers.HasWaste; } }
        public float PowerRate { get { return providers.PowerRate; } }
        public float WaterRate { get { return providers.WaterRate; } }
        public float WasteRate { get { return providers.WasteRate; } }

        // Доля свободного жилья. Домов нет — 0, пустой город — 1.
        public float HousingRoomRate
        {
            get
            {
                if (settlement.TotalCapacity == 0) return 0f;
                if (population.TotalResidents == 0) return 1f;
                float rate = 1f - population.TotalResidents / (float)settlement.TotalCapacity;
                return Math.Max(0f, rate);
            }
        }

        // Доля свободных рабочих мест. Пустой город — 1, мест нет — 0.
        public float JobsRoomRate
        {
            get
            {
                if (population.TotalResidents == 0) return 1f;
                if (settlement.TotalJobs == 0) return 0f;
                return 1f - Employed / (float)settlement.TotalJobs;
            }
        }

        // Доля трудоустроенных. Жителей нет — 1: некого обижать.
        public float EmploymentRate
        {
            get
            {
                if (population.TotalResidents == 0) return 1f;
                return Employed / (float)population.TotalResidents;
            }
        }

        // Среднее по трём базовым потребностям плюс подключённые провайдеры (3..6 слагаемых).
        public float AverageSatisfaction
        {
            get
            {
                float sum = HousingRoomRate + JobsRoomRate + EmploymentRate + providers.Sum;
                return sum / (3 + providers.Count);
            }
        }
    }
}
