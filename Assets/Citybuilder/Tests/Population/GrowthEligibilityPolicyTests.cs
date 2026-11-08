using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static Citybuilder.GrowthEligibilityPolicy;

namespace Citybuilder
{
    // Матрица правила «может ли вырасти следующий дом» (EPIC-10 шаг 1): режим, зоны,
    // деньги, потолок, разрешённый рост. Unity нужен только фикстуре; вне Unity — new.
    public class GrowthEligibilityPolicyTests
    {
        private GrowthEligibilityPolicy policy;
        private SettlementSystem settlement;
        private ZoneSystem zoneSystem;
        private readonly List<BuildingDefinition> definitions = new List<BuildingDefinition>();
        private BuildingDefinition house;

        [SetUp]
        public void SetUp()
        {
            policy = new GrowthEligibilityPolicy();
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            zoneSystem = new ZoneSystem(grid);
            settlement = new SettlementSystem(grid, zoneSystem, new BuildingSystem(grid));
            house = NewDefinition();
            house.Id = "house";
            house.AllowedZone = ZoneType.Residential;
            house.Footprint = new Vector2Int(1, 1);
            house.ConstructionCost = 100;
            definitions.Add(house);
        }

#if CB_DOTNET
        // Вне Unity CreateInstance недоступен: для матрицы хватает голых полей описания.
        private static BuildingDefinition NewDefinition() { return new BuildingDefinition(); }
#else
        private static BuildingDefinition NewDefinition() { return ScriptableObject.CreateInstance<BuildingDefinition>(); }
#endif

        // count жилых клеток подряд от (0,0); снимок зон для policy.
        private List<KeyValuePair<GridPosition, ZoneType>> ResidentialCells(int count)
        {
            for (int i = 0; i < count; i++) zoneSystem.SetZone(new GridPosition(i, 0), ZoneType.Residential);
            return zoneSystem.GetZonedCells();
        }

        // Матрица запретов: режим off / нет зон / нет денег / потолок.
        [TestCase(false, 1, 30, 0, 1000, Reason.GrowthOff)]
        [TestCase(true, 0, 30, 0, 1000, Reason.NoSpot)]
        [TestCase(true, 1, 30, 0, 50, Reason.NoMoney)]
        [TestCase(true, 1, 1, 1, 1000, Reason.Ceiling)]
        public void Evaluate_BlockedCases(bool active, int cells, int max, int settled, int balance, Reason expected)
        {
            List<KeyValuePair<GridPosition, ZoneType>> zoned = ResidentialCells(cells);
            for (int i = 0; i < settled; i++) settlement.Settle(house, new GridPosition(i, 0));
            Verdict verdict = policy.Evaluate(active, settlement, max, zoned,
                ZoneType.None, settled, definitions, new Treasury(balance));
            Assert.IsFalse(verdict.Allowed);
            Assert.AreEqual(expected, verdict.BlockedBy);
        }

        [Test]
        public void Evaluate_AllOk_AllowedOnFirstZone()
        {
            Verdict verdict = policy.Evaluate(true, settlement, 30, ResidentialCells(1),
                ZoneType.None, 0, definitions, new Treasury(1000));
            Assert.IsTrue(verdict.Allowed);
            Assert.AreSame(house, verdict.Definition);
            Assert.AreEqual(new GridPosition(0, 0), verdict.Origin);
        }
    }
}
