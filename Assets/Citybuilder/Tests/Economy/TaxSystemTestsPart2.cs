using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса TaxSystemTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class TaxSystemTests
    {
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

        // Приоритет района над зоной, кумулятивные доли и неизменная лестница 1/2/4.
        [Test]
        public void DistrictPolicy_PriorityFallbackAndEmployedSum_KeepBaseLadder()
        {
            TaxPolicy policy = new TaxPolicy();
            policy.SetZone(ZoneType.Residential, TaxPolicy.Level.Low);
            policy.SetZone(ZoneType.Commercial, TaxPolicy.Level.Normal);
            policy.SetZone(ZoneType.Industrial, TaxPolicy.Level.High);
            Assert.AreEqual(1, policy.RateFor(ZoneType.Residential));
            Assert.AreEqual(2, policy.RateFor(ZoneType.Commercial));
            Assert.AreEqual(4, policy.RateFor(ZoneType.Industrial));

            DistrictPolicy districts = new DistrictPolicy();
            Assert.AreEqual(1, districts.RateFor(policy, ZoneType.Residential, 0));
            districts.AssignZone(ZoneType.Residential, 7);
            Assert.AreEqual(1, districts.RateFor(policy, ZoneType.Residential, 7));
            districts.SetRate(7, TaxPolicy.Level.High);
            Assert.AreEqual(4, districts.RateFor(policy, ZoneType.Residential, 7));
            districts.AssignCell(11, 7);
            Assert.AreEqual(4, districts.RateForCell(policy, ZoneType.Residential, 11));

            districts.AddSeats(7, 3);
            districts.AddSeats(8, 1);
            Dictionary<int, int> shares = districts.DistributeEmployed(9);
            int sum = 0;
            foreach (KeyValuePair<int, int> pair in shares) sum += pair.Value;
            Assert.AreEqual(9, sum);
        }

        // Район меняет сбор казны; бонус рейтинга прибавляется к ставке района.
        [Test]
        public void ComputeTax_DistrictOverrideWins_AndRatingBonusApplies()
        {
            DistrictPolicy districts = new DistrictPolicy();
            districts.AssignZone(ZoneType.Commercial, 2);
            TaxSystem tax = MakeDistrictCity(districts, () => 0);
            TaxPolicy policy = new TaxPolicy();
            Assert.AreEqual(12, tax.ComputeTax(policy));

            districts.SetRate(2, TaxPolicy.Level.High);
            Assert.AreEqual(24, tax.ComputeTax(policy));

            TaxSystem rated = MakeDistrictCity(districts, () => 25);
            Assert.AreEqual(30, rated.ComputeTax(policy));
            Assert.AreEqual(6, rated.EmployedPerZone().Total);
        }

        private static TaxSystem MakeDistrictCity(DistrictPolicy districts, Func<int> rating)
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            ZoneSystem zones = new ZoneSystem(grid);
            SettlementSystem settlement = new SettlementSystem(grid, zones, new BuildingSystem(grid));
            PopulationSystem population = new PopulationSystem();
            List<BuildingState> grown = new List<BuildingState>();
            zones.SetZone(new GridPosition(0, 0), ZoneType.Residential);
            zones.SetZone(new GridPosition(1, 0), ZoneType.Commercial);
            grown.Add(settlement.Settle(MakeDefinition(8, 0, ZoneType.Residential), new GridPosition(0, 0)));
            grown.Add(settlement.Settle(MakeDefinition(0, 6, ZoneType.Commercial), new GridPosition(1, 0)));
            population.MoveIn(grown[0], 8);
            EmploymentSystem employment = new EmploymentSystem(population, settlement);
            return new TaxSystem(employment, new Treasury(0),
                () => ZonalJobSource.Count(grown, null), districts, rating);
        }
    }
}
