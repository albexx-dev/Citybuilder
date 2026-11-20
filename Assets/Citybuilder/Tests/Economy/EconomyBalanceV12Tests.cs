using NUnit.Framework;
using UnityEditor;

namespace Citybuilder
{
    // Сценарии баланса v1.2 (эпик #53): зрелый город при трёх налоговых режимах.
    // Цели эпика: обычный налог — профицит меньше половины дохода (не «бесконечные деньги»),
    // высокие налоги — час профицита покрывает апгрейд больницы, низкие — полный обвес службами
    // даёт дефицит (выбор «налоги или терпеть»). Числа читаются из реальных ассетов,
    // расчёт — EconomyMath (правила EmploymentSystem и TaxSystem). Требует Unity (AssetDatabase).
    public class EconomyBalanceV12Tests
    {
        private const string BuildingsFolder = "Assets/Citybuilder/Data/Buildings/";
        private const int StartingBalance = 1500;

        // Зрелый город у потолка роста: 50 домов (200 жителей), 12 магазинов, 8 заводов,
        // сети 7/7/8 (спрос ~80 зданий), 3 пожарные, 3 полицейские, 2 парка, больница.
        private static readonly string[] CityNames =
        {
            "House", "Shop", "Factory", "PowerStation", "WaterTower", "WasteDepot",
            "FireStation", "PoliceStation", "Park", "Hospital"
        };

        private static readonly int[] CityCounts = { 50, 12, 8, 7, 7, 8, 3, 3, 2, 1 };

        [Test]
        public void FullCity_NormalTax_SurplusStaysUnderHalfOfIncome()
        {
            int residents = Count("House") * Load("House").ResidentCapacity;
            int jobs = TotalJobs();
            int upkeep = TotalUpkeep();
            int rate = new TaxPolicy().PerWorker; // обычный налог v1.2: 2

            int income = EconomyMath.IncomePerHour(EconomyMath.Employed(residents, jobs), rate);
            int net = EconomyMath.NetPerHour(residents, jobs, rate, upkeep);

            Assert.AreEqual(2, rate, "normal rate");
            Assert.GreaterOrEqual(net, 0, "net per hour");
            Assert.LessOrEqual(net, income / 2, "surplus must stay under half of income");
        }

        [Test]
        public void FullCity_HighTax_HourOfSurplusCoversHospitalUpgrade()
        {
            int residents = Count("House") * Load("House").ResidentCapacity;
            int rate = 4; // высокий налог v1.2
            int upgradeCost = BuildingLevelPolicy.UpgradeCost(Load("Hospital").ConstructionCost, 1);

            int net = EconomyMath.NetPerHour(residents, TotalJobs(), rate, TotalUpkeep());

            Assert.GreaterOrEqual(net, upgradeCost, "high taxes must fund a hospital upgrade per hour");
        }

        [Test]
        public void FullCity_LowTax_FullServicesBleed()
        {
            int residents = Count("House") * Load("House").ResidentCapacity;
            int rate = 1; // низкий налог v1.2

            int net = EconomyMath.NetPerHour(residents, TotalJobs(), rate, TotalUpkeep());

            Assert.Less(net, 0, "low taxes with full coverage must force lean services or deficit");
        }

        // Базовый город с первой службой: пожарная часть достижима без структурного дефицита.
        [Test]
        public void BaseCity_WithFireStation_NetPerHour_StaysPositive()
        {
            int residents = 2 * Load("House").ResidentCapacity;
            int jobs = SumJobs("PowerStation", "WaterTower", "WasteDepot", "FireStation");
            int upkeep = SumUpkeep("PowerStation", "WaterTower", "WasteDepot", "FireStation");
            int rate = new TaxPolicy().PerWorker;
            int start = StartingBalance - SumCosts("House", "House", "PowerStation", "WaterTower", "WasteDepot", "FireStation");

            int net = EconomyMath.NetPerHour(residents, jobs, rate, upkeep);
            int balance24 = EconomyMath.BalanceAfterHours(start, residents, jobs, rate, upkeep, 24);

            Assert.GreaterOrEqual(net, 0, "net per hour");
            Assert.GreaterOrEqual(balance24, 0, "balance at 24h");
        }

        private static int Count(string name)
        {
            int index = System.Array.IndexOf(CityNames, name);
            return CityCounts[index];
        }

        private static int TotalJobs()
        {
            int total = 0;
            for (int i = 0; i < CityNames.Length; i++) total += CityCounts[i] * Load(CityNames[i]).JobsProvided;
            return total;
        }

        private static int TotalUpkeep()
        {
            int total = 0;
            for (int i = 0; i < CityNames.Length; i++) total += CityCounts[i] * Load(CityNames[i]).UpkeepPerHour;
            return total;
        }

        private static int SumJobs(params string[] names)
        {
            int total = 0;
            for (int i = 0; i < names.Length; i++) total += Load(names[i]).JobsProvided;
            return total;
        }

        private static int SumUpkeep(params string[] names)
        {
            int total = 0;
            for (int i = 0; i < names.Length; i++) total += Load(names[i]).UpkeepPerHour;
            return total;
        }

        private static int SumCosts(params string[] names)
        {
            int total = 0;
            for (int i = 0; i < names.Length; i++) total += Load(names[i]).ConstructionCost;
            return total;
        }

        private static BuildingDefinition Load(string name)
        {
            string path = BuildingsFolder + name + ".asset";
            BuildingDefinition definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
            Assert.IsNotNull(definition, "Missing asset: " + path);
            return definition;
        }
    }
}
