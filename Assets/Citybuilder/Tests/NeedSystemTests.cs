using System;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Границы потребностей из 036 на настоящем API.
    // Unity нужен только фикстурам (CreateInstance), проверки — числа чистой логики.
    public class NeedSystemTests
    {
        private const float Delta = 1e-4f;

        private static void MakeWorld(out SettlementSystem settlement, out PopulationSystem population)
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            population = new PopulationSystem();
        }

        private static BuildingDefinition HouseDef(int capacity)
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.AllowedZone = ZoneType.None;
            definition.Footprint = new Vector2Int(1, 1);
            definition.ResidentCapacity = capacity;
            definition.JobsProvided = 0;
            return definition;
        }

        private static BuildingDefinition ShopDef(int jobs)
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.AllowedZone = ZoneType.None;
            definition.Footprint = new Vector2Int(1, 1);
            definition.ResidentCapacity = 0;
            definition.JobsProvided = jobs;
            return definition;
        }

        private static BuildingState MustSettle(SettlementSystem settlement, BuildingDefinition definition, int slot)
        {
            BuildingState state = settlement.Settle(definition, new GridPosition(slot * 2, 0));
            Assert.IsNotNull(state);
            return state;
        }

        private static void MustMoveIn(PopulationSystem population, BuildingState home, int size)
        {
            Assert.IsNotNull(population.MoveIn(home, size));
        }

        private static void AssertRates(NeedSystem needs, float housing, float jobs, float employment)
        {
            Assert.AreEqual(housing, needs.HousingRoomRate, Delta);
            Assert.AreEqual(jobs, needs.JobsRoomRate, Delta);
            Assert.AreEqual(employment, needs.EmploymentRate, Delta);
            Assert.AreEqual((housing + jobs + employment) / 3f, needs.AverageSatisfaction, Delta);
            Assert.IsTrue(housing >= 0f && housing <= 1f && jobs >= 0f && jobs <= 1f && employment >= 0f && employment <= 1f);
        }

        [Test]
        public void Empty_NoHouses()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out settlement, out population);
            AssertRates(new NeedSystem(population, settlement), 0f, 1f, 1f);
        }
        [Test]
        public void Empty_WithHouse()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out settlement, out population);
            MustSettle(settlement, HouseDef(4), 0);
            AssertRates(new NeedSystem(population, settlement), 1f, 1f, 1f);
        }
        [Test]
        public void NoJobs()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out settlement, out population);
            MustMoveIn(population, MustSettle(settlement, HouseDef(4), 0), 4);
            AssertRates(new NeedSystem(population, settlement), 0f, 0f, 0f);
        }
        [Test]
        public void AllFull()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out settlement, out population);
            MustMoveIn(population, MustSettle(settlement, HouseDef(4), 0), 4);
            MustSettle(settlement, ShopDef(4), 1);
            AssertRates(new NeedSystem(population, settlement), 0f, 0f, 1f);
        }
        [Test]
        public void AllFree()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out settlement, out population);
            MustSettle(settlement, HouseDef(4), 0);
            BuildingState home = MustSettle(settlement, HouseDef(4), 1);
            MustSettle(settlement, ShopDef(8), 2);
            MustMoveIn(population, home, 2);
            AssertRates(new NeedSystem(population, settlement), 0.75f, 0.75f, 1f);
        }
        [Test]
        public void Overcrowded()
        {
            // Перенаселение через снос без выселения: было 6 жителей в доме на 6, остался дом на 4.
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out settlement, out population);
            BuildingState big = MustSettle(settlement, HouseDef(6), 0);
            MustMoveIn(population, big, 6);
            Assert.IsTrue(settlement.Demolish(big));
            MustSettle(settlement, HouseDef(4), 1);
            MustSettle(settlement, ShopDef(2), 2);
            AssertRates(new NeedSystem(population, settlement), 0f, 0f, 2f / 6f);
        }
        [Test]
        public void MoreJobsThanResidents()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out settlement, out population);
            MustMoveIn(population, MustSettle(settlement, HouseDef(4), 0), 2);
            MustSettle(settlement, ShopDef(10), 1);
            AssertRates(new NeedSystem(population, settlement), 0.5f, 0.8f, 1f);
        }
        [Test]
        public void Ctor_NullPopulation_Throws()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out settlement, out population);
            Assert.Throws<ArgumentNullException>(() => new NeedSystem(null, settlement));
        }
        [Test]
        public void Ctor_NullSettlement_Throws()
        {
            SettlementSystem settlement;
            PopulationSystem population;
            MakeWorld(out settlement, out population);
            Assert.Throws<ArgumentNullException>(() => new NeedSystem(population, null));
        }
    }
}
