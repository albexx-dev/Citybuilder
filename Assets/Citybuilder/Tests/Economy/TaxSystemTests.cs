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
