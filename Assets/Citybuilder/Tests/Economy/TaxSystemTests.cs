using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Налог через реальный TaxSystem: Employed × ставка политики.
    // Требует Unity: BuildingDefinition — ScriptableObject, вне редактора не собрать.
    public class TaxSystemTests
    {
        [Test]
        public void ComputeTax_HighPolicy_UsesTripleRate()
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            SettlementSystem settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            PopulationSystem population = new PopulationSystem();
            BuildingState home = settlement.Settle(MakeDefinition(10, 0), new GridPosition(0, 0));
            settlement.Settle(MakeDefinition(0, 10), new GridPosition(4, 0));
            population.MoveIn(home, 4);
            EmploymentSystem employment = new EmploymentSystem(population, settlement);
            TaxSystem tax = new TaxSystem(employment, new Treasury(0));

            TaxPolicy policy = new TaxPolicy();
            policy.Set(TaxPolicy.Level.High);

            Assert.AreEqual(4, employment.Employed);
            Assert.AreEqual(12, tax.ComputeTax(policy.PerWorker));
        }

        [Test]
        public void TaxSystem_Treasury_MatchesProvidedInstance()
        {
            GridSystem grid = new GridSystem(new GridSize(10, 10), 2f);
            SettlementSystem settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            PopulationSystem population = new PopulationSystem();
            EmploymentSystem employment = new EmploymentSystem(population, settlement);
            Treasury treasury = new Treasury(500);
            TaxSystem tax = new TaxSystem(employment, treasury);

            Assert.AreSame(treasury, tax.Treasury);
        }

        [Test]
        public void EmploymentSystem_WithManualJobsProvider_CalculatesTotalJobsAndEmployed()
        {
            GridSystem grid = new GridSystem(new GridSize(10, 10), 2f);
            SettlementSystem settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            PopulationSystem population = new PopulationSystem();
            BuildingState home = settlement.Settle(MakeDefinition(10, 0), new GridPosition(0, 0));
            settlement.Settle(MakeDefinition(0, 3), new GridPosition(2, 0));
            population.MoveIn(home, 8);

            int manualJobs = 4;
            EmploymentSystem employment = new EmploymentSystem(population, settlement, () => manualJobs);

            Assert.AreEqual(7, employment.TotalJobs);
            Assert.AreEqual(7, employment.Employed);
            Assert.AreEqual(1, employment.Unemployed);
            Assert.AreEqual(1f, employment.JobsFillRate, 0.001f);

            manualJobs = 10;
            Assert.AreEqual(13, employment.TotalJobs);
            Assert.AreEqual(8, employment.Employed);
            Assert.AreEqual(0, employment.Unemployed);
            Assert.AreEqual(8f / 13f, employment.JobsFillRate, 0.001f);
        }

        private static BuildingDefinition MakeDefinition(int residents, int jobs)
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.AllowedZone = ZoneType.None;
            definition.Footprint = new Vector2Int(1, 1);
            definition.ResidentCapacity = residents;
            definition.JobsProvided = jobs;
            return definition;
        }
    }
}
