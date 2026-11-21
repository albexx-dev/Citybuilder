using NUnit.Framework;
using UnityEditor;

namespace Citybuilder
{
    // Числовой сценарий ребаланса (задача 171): город из 2 домов и станций энергии/воды/мусора
    // держит казну не ниже −100 за 48 часов и не течёт в минус по итогу часа.
    // Расчёт чистый (EconomyMath, без MonoBehaviour), числа читаются из реальных ассетов.
    public class EconomyRebalanceTests
    {
        private const string BuildingsFolder = "Assets/Citybuilder/Data/Buildings/";
        private const int StartingBalance = 1500;
        private LanguageTestScope languageScope;

        [SetUp]
        public void InjectLanguageScope()
        {
            languageScope = new LanguageTestScope();
        }

        [TearDown]
        public void RestoreLanguageScope()
        {
            languageScope.Dispose();
        }

        // Старт 1500, стройка 2 домов и трёх станций, 48 часов по обычному налогу новой политики.
        [Test]
        public void BaseCity_TwoHousesThreeStations_48h_StaysAboveMinus100()
        {
            int tax = new TaxPolicy().PerWorker;
            int residents = 2 * Load("House").ResidentCapacity;
            int jobs = SumJobs("PowerStation", "WaterTower", "WasteDepot");
            int upkeep = SumUpkeep("PowerStation", "WaterTower", "WasteDepot");
            int start = StartingBalance - BuildCost("House", "House", "PowerStation", "WaterTower", "WasteDepot");

            int balance24 = EconomyMath.BalanceAfterHours(start, residents, jobs, tax, upkeep, 24);
            int balance48 = EconomyMath.BalanceAfterHours(start, residents, jobs, tax, upkeep, 48);

            Assert.GreaterOrEqual(balance24, -100, "balance at 24h");
            Assert.GreaterOrEqual(balance48, -100, "balance at 48h");
        }

        // Антиспираль: итог часа базового города не отрицательный (старые числа давали −9 $/ч).
        [Test]
        public void BaseCity_NetPerHour_NotBleeding()
        {
            int tax = new TaxPolicy().PerWorker;
            int residents = 2 * Load("House").ResidentCapacity;
            int jobs = SumJobs("PowerStation", "WaterTower", "WasteDepot");
            int upkeep = SumUpkeep("PowerStation", "WaterTower", "WasteDepot");
            Assert.GreaterOrEqual(EconomyMath.NetPerHour(residents, jobs, tax, upkeep), 0);
        }

        // Все шесть служб дают рабочие места 2–4: их содержание окупается занятостью.
        [TestCase("PowerStation")]
        [TestCase("WaterTower")]
        [TestCase("WasteDepot")]
        [TestCase("FireStation")]
        [TestCase("PoliceStation")]
        [TestCase("Hospital")]
        public void Services_ProvideTwoToFourJobs(string name)
        {
            Assert.GreaterOrEqual(Load(name).JobsProvided, 2, name);
            Assert.LessOrEqual(Load(name).JobsProvided, 4, name);
        }

        // Правила EconomyMath повторяют EmploymentSystem и TaxSystem: занятость и нулевая ставка.
        [Test]
        public void EconomyMath_FollowsEmploymentAndTaxRules()
        {
            Assert.AreEqual(5, EconomyMath.Employed(8, 5));
            Assert.AreEqual(8, EconomyMath.Employed(8, 12));
            Assert.AreEqual(0, EconomyMath.IncomePerHour(6, 0));
            Assert.AreEqual(24, EconomyMath.IncomePerHour(6, 4));
            Assert.AreEqual(-10, EconomyMath.NetPerHour(8, 5, 2, 20));
        }

        // Строка бюджета: формат и признак дефицита по итогу часа.
        [Test]
        public void BudgetLine_ShowsIncomeUpkeepAndNet()
        {
            Assert.AreEqual("Доход: +24 $/ч - Расход: 10 $/ч = Итог: +14 $/ч",
                BudgetSummaryPolicy.Line(24, 10));
            Assert.AreEqual("Доход: +4 $/ч - Расход: 10 $/ч = Итог: -6 $/ч",
                BudgetSummaryPolicy.Line(4, 10));
            Assert.IsFalse(BudgetSummaryPolicy.IsNetDeficit(24, 10));
            Assert.IsTrue(BudgetSummaryPolicy.IsNetDeficit(4, 10));
        }

        private static int BuildCost(params string[] names)
        {
            int total = 0;
            for (int i = 0; i < names.Length; i++) total += Load(names[i]).ConstructionCost;
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

        private static BuildingDefinition Load(string name)
        {
            string path = BuildingsFolder + name + ".asset";
            BuildingDefinition definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
            Assert.IsNotNull(definition, "Missing asset: " + path);
            return definition;
        }
    }
}
