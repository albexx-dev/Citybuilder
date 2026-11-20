using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Налог через реальный TaxSystem: Employed × ставка политики; эпик #49 — зональный разнос.
    // Требует Unity: BuildingDefinition — ScriptableObject, вне редактора не собрать.
    public class TaxSystemTests
    {
        [Test]
        public void ComputeTax_HighPolicy_UsesHighRate()
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
            // Высокая ставка после баланса v1.2 (эпик #53): 4 работающих × 4 = 16.
            Assert.AreEqual(16, tax.ComputeTax(policy.PerWorker));
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
            return MakeDefinition(residents, jobs, ZoneType.None);
        }

        private static BuildingDefinition MakeDefinition(int residents, int jobs, ZoneType zone)
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.AllowedZone = zone;
            definition.Footprint = new Vector2Int(1, 1);
            definition.ResidentCapacity = residents;
            definition.JobsProvided = jobs;
            return definition;
        }

        // Город эпика #49: дом на 8 жителей, магазин (6 мест, коммерция), завод (8 мест, промзона).
        // Возвращает таксистему с поставщиком зональных мест по списку выросших зданий.
        private static TaxSystem MakeZonalCity(PopulationSystem population, List<BuildingState> grown)
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            ZoneSystem zones = new ZoneSystem(grid);
            SettlementSystem settlement = new SettlementSystem(grid, zones, new BuildingSystem(grid));
            zones.SetZone(new GridPosition(0, 0), ZoneType.Residential);
            zones.SetZone(new GridPosition(1, 0), ZoneType.Commercial);
            zones.SetZone(new GridPosition(2, 0), ZoneType.Industrial);
            grown.Add(settlement.Settle(MakeDefinition(8, 0, ZoneType.Residential), new GridPosition(0, 0)));
            grown.Add(settlement.Settle(MakeDefinition(0, 6, ZoneType.Commercial), new GridPosition(1, 0)));
            grown.Add(settlement.Settle(MakeDefinition(0, 8, ZoneType.Industrial), new GridPosition(2, 0)));
            population.MoveIn(grown[0], 8);
            EmploymentSystem employment = new EmploymentSystem(population, settlement);
            return new TaxSystem(employment, new Treasury(0), () => ZonalJobSource.Count(grown, null));
        }

        // Разнос по зонам: связей работник→здание нет, занятые (8) распределяются по долям
        // мест зон (0/6/8 из 14) → 0/3/5. Обычные ставки 2/2/2 дают 16, коммерция высокая — 22 (v1.2).
        [Test]
        public void ComputeTax_ZonalPolicy_PaysByZoneJobs()
        {
            PopulationSystem population = new PopulationSystem();
            List<BuildingState> grown = new List<BuildingState>();
            TaxSystem tax = MakeZonalCity(population, grown);
            TaxPolicy policy = new TaxPolicy();

            Assert.AreEqual(16, tax.ComputeTax(policy));
            Assert.AreEqual(0, tax.EmployedPerZone().Residential);
            Assert.AreEqual(3, tax.EmployedPerZone().Commercial);
            Assert.AreEqual(5, tax.EmployedPerZone().Industrial);

            policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.High);
            Assert.AreEqual(22, tax.ComputeTax(policy));
            policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.Normal);
            policy.SetZone(ZoneType.Industrial, TaxPolicy.Level.High);
            Assert.AreEqual(26, tax.ComputeTax(policy));
        }

        // Зональный Collect зачисляет сумму разом в казну и возвращает Ok.
        [Test]
        public void Collect_ZonalPolicy_EarnsTreasury()
        {
            PopulationSystem population = new PopulationSystem();
            List<BuildingState> grown = new List<BuildingState>();
            TaxSystem tax = MakeZonalCity(population, grown);

            Treasury.TransactionResult result = tax.Collect(new TaxPolicy());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(16, tax.Treasury.Balance);
        }

        // Нет поставщика зональных мест: вся занятость считается жилой (совместимость).
        [Test]
        public void EmployedPerZone_WithoutProvider_AllToResidential()
        {
            GridSystem grid = new GridSystem(new GridSize(10, 10), 2f);
            SettlementSystem settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            PopulationSystem population = new PopulationSystem();
            BuildingState home = settlement.Settle(MakeDefinition(10, 0), new GridPosition(0, 0));
            settlement.Settle(MakeDefinition(0, 10), new GridPosition(4, 0));
            population.MoveIn(home, 4);
            EmploymentSystem employment = new EmploymentSystem(population, settlement);
            TaxSystem tax = new TaxSystem(employment, new Treasury(0));
            TaxPolicy policy = new TaxPolicy();
            policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.High);

            Assert.AreEqual(4, tax.EmployedPerZone().Residential);
            Assert.AreEqual(0, tax.EmployedPerZone().Commercial);
            // Высокая коммерция не действует: мест по зонам неизвестно, работает жилая ставка.
            Assert.AreEqual(8, tax.ComputeTax(policy));
        }
    }
}
