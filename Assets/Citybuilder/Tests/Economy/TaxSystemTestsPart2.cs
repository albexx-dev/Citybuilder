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
    }
}
