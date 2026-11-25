using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса ConstructionSiteTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class ConstructionSiteTests
    {
        // Правила v1.1: снос в стройке освобождает клетку, деньги не возвращаются.
        [Test]
        public void DemolishDuringConstruction_FreesCells_KeepsMoneySpent()
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Footprint = new Vector2Int(2, 2);
            def.ConstructionCost = 100;
            def.ConstructionTicks = 3;
            Treasury treasury = new Treasury(150);
            GridSystem grid = new GridSystem(new GridSize(10, 10), 1f);
            BuildingSystem system = new BuildingSystem(grid);

            BuildingState state = system.Place(def, new GridPosition(0, 0));
            Assert.IsNotNull(state);
            Assert.IsTrue(treasury.TrySpend(def.ConstructionCost, def.Id).Success);
            Assert.AreEqual(50, treasury.Balance);

            Assert.IsTrue(system.Demolish(state));
            Assert.IsFalse(grid.IsOccupied(new GridPosition(0, 0)));
            Assert.IsFalse(grid.IsOccupied(new GridPosition(1, 1)));
            Assert.AreEqual(50, treasury.Balance);
        }
    }
}
