using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static Citybuilder.GrowthEligibilityPolicy;

namespace Citybuilder
{
    // Гейт дефицита казны в росте (EPIC-04 шаг 2): стоп темпа и отдельная причина запрета.
    // Unity нужен только фикстуре описания здания; остальное — чистый C#.
    public class GrowthDeficitTests
    {
        private const int HouseCost = 100;
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
            house.ConstructionCost = HouseCost;
            definitions.Add(house);
            zoneSystem.SetZone(new GridPosition(0, 0), ZoneType.Residential);
        }

#if CB_DOTNET
        // Вне Unity CreateInstance недоступен: хватает голых полей описания.
        private static BuildingDefinition NewDefinition() { return new BuildingDefinition(); }
#else
        private static BuildingDefinition NewDefinition() { return ScriptableObject.CreateInstance<BuildingDefinition>(); }
#endif

        // Казна в минусе от содержания: 10 − 16 = −6.
        private static Treasury InDeficit()
        {
            Treasury treasury = new Treasury(10);
            treasury.Charge(16);
            return treasury;
        }

        private Verdict Check(Treasury treasury)
        {
            return policy.Evaluate(true, settlement, 30, Zoned(), ZoneType.None, 0, definitions, treasury);
        }

        private List<KeyValuePair<GridPosition, ZoneType>> Zoned() { return zoneSystem.GetZonedCells(); }

        [Test]
        public void Pace_Deficit_RequiresCeilingTicks()
        {
            Assert.AreEqual(GrowthPace.MaxRequiredTicks, GrowthPace.RequiredTicks(5, 1, 1, true));
            Assert.AreEqual(GrowthPace.MaxRequiredTicks, GrowthPace.RequiredTicks(1, 1, 1, true));
        }

        [Test]
        public void Pace_Deficit_IgnoresWorstMultiplier()
        {
            Assert.AreEqual(GrowthPace.MaxRequiredTicks, GrowthPace.RequiredTicks(5, 3, 2, true));
        }

        [Test]
        public void Pace_NoDeficit_KeepsWorstMultiplier()
        {
            Assert.AreEqual(15, GrowthPace.RequiredTicks(5, 3, 2, false));
            Assert.AreEqual(5, GrowthPace.RequiredTicks(5, 1, 1, false));
        }

        [Test]
        public void Eligibility_Deficit_BlockedByDeficit()
        {
            Verdict verdict = Check(InDeficit());
            Assert.IsFalse(verdict.Allowed);
            Assert.AreEqual(Reason.Deficit, verdict.BlockedBy);
        }

        [Test]
        public void Eligibility_EarnOutOfDeficit_GrowthResumes()
        {
            Treasury treasury = InDeficit();
            Assert.AreEqual(Reason.Deficit, Check(treasury).BlockedBy);
            treasury.Earn(HouseCost + 6);
            Verdict verdict = Check(treasury);
            Assert.IsTrue(verdict.Allowed);
            Assert.AreEqual(Reason.None, verdict.BlockedBy);
        }

        [Test]
        public void Eligibility_PositiveButShort_NoMoneyNotDeficit()
        {
            Assert.AreEqual(Reason.NoMoney, Check(new Treasury(50)).BlockedBy);
            Assert.AreEqual(Reason.NoMoney, Check(new Treasury(0)).BlockedBy);
        }

        [Test]
        public void Eligibility_Deficit_DoesNotOverrideGrowthOff()
        {
            Verdict verdict = policy.Evaluate(false, settlement, 30, Zoned(),
                ZoneType.None, 0, definitions, InDeficit());
            Assert.AreEqual(Reason.GrowthOff, verdict.BlockedBy);
        }

        [Test]
        public void Eligibility_Deficit_DoesNotOverrideCeiling()
        {
            settlement.Settle(house, new GridPosition(0, 0));
            Verdict verdict = policy.Evaluate(true, settlement, 1, Zoned(),
                ZoneType.None, 1, definitions, InDeficit());
            Assert.AreEqual(Reason.Ceiling, verdict.BlockedBy);
        }

        [Test]
        public void Eligibility_Deficit_DoesNotOverrideNoSpot()
        {
            List<KeyValuePair<GridPosition, ZoneType>> none = new List<KeyValuePair<GridPosition, ZoneType>>();
            Verdict verdict = policy.Evaluate(true, settlement, 30, none,
                ZoneType.None, 0, definitions, InDeficit());
            Assert.AreEqual(Reason.NoSpot, verdict.BlockedBy);
        }
    }
}
