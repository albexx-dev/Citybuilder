using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Апгрейд станций (EPIC-51): гейты типа/уровня/казны, списание, рост уровня,
    // пересчёт покрытия по колбэку, цена кнопки. BuildingDefinition — ScriptableObject,
    // поэтому тесты требуют Unity (как UpkeepPolicyTests).
    public class UpgradeControllerTests
    {
        private BuildingDefinition station;
        private BuildingState state;
        private Treasury treasury;
        private int rebuilds;

        [SetUp]
        public void SetUp()
        {
            station = ScriptableObject.CreateInstance<BuildingDefinition>();
            station.Id = "fire_station";
            station.ConstructionCost = 100;
            station.ResidentCapacity = 0;
            station.ServiceKind = ServiceKind.Fire;
            station.ServiceRadius = 3;
            state = new BuildingState(EntityId.New(), station, new GridPosition(0, 0),
                new List<GridPosition> { new GridPosition(0, 0) });
            treasury = new Treasury(1000);
            rebuilds = 0;
        }

        [TearDown]
        public void TearDown()
        {
            ScriptableObject.DestroyImmediate(station);
        }

        private UpgradeController MakeController()
        {
            return new UpgradeController(delegate { rebuilds++; });
        }

        [Test]
        public void Success_SpendsPrice_RaisesLevel_RebuildsCoverage()
        {
            UpgradeController.Result result = MakeController().TryUpgrade(state, treasury);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.NewLevel);
            Assert.AreEqual(2, state.Level);
            Assert.AreEqual(1000 - 75, treasury.Balance);
            Assert.AreEqual(1, rebuilds);
        }

        [Test]
        public void Twice_SecondUpgradeCostsOneAndQuarter()
        {
            UpgradeController controller = MakeController();
            Assert.IsTrue(controller.TryUpgrade(state, treasury).Success);
            Assert.IsTrue(controller.TryUpgrade(state, treasury).Success);
            Assert.AreEqual(3, state.Level);
            Assert.AreEqual(1000 - 75 - 125, treasury.Balance);
            Assert.AreEqual(2, rebuilds);
        }

        [Test]
        public void MaxLevel_RefusedWithoutSpending()
        {
            state.ApplyUpgrade();
            state.ApplyUpgrade();
            UpgradeController.Result result = MakeController().TryUpgrade(state, treasury);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UpgradeController.Failure.MaxLevel, result.Reason);
            Assert.AreEqual(3, state.Level);
            Assert.AreEqual(1000, treasury.Balance);
            Assert.AreEqual(0, rebuilds);
        }

        [Test]
        public void NotEnoughFunds_Refused_LevelAndBalanceUnchanged()
        {
            treasury = new Treasury(10);
            UpgradeController.Result result = MakeController().TryUpgrade(state, treasury);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UpgradeController.Failure.NotEnoughFunds, result.Reason);
            Assert.AreEqual(1, state.Level);
            Assert.AreEqual(10, treasury.Balance);
            Assert.AreEqual(0, rebuilds);
        }

        [Test]
        public void ResidentialBuilding_NotUpgradeable()
        {
            station.ResidentCapacity = 4;
            UpgradeController.Result result = MakeController().TryUpgrade(state, treasury);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UpgradeController.Failure.NotUpgradeable, result.Reason);
        }

        [Test]
        public void NullStateAndBrokenState_Refused()
        {
            UpgradeController controller = MakeController();
            Assert.AreEqual(UpgradeController.Failure.NotUpgradeable, controller.TryUpgrade(null, treasury).Reason);
            BuildingState broken = new BuildingState(EntityId.New(), null, new GridPosition(0, 0),
                new List<GridPosition>());
            Assert.AreEqual(UpgradeController.Failure.NotUpgradeable,
                controller.TryUpgrade(broken, treasury).Reason);
        }

        [Test]
        public void PriceFor_LevelOneAndTwo_FollowsPolicy()
        {
            Assert.AreEqual(75, UpgradeController.PriceFor(station, 1));
            Assert.AreEqual(125, UpgradeController.PriceFor(station, 2));
        }

        [Test]
        public void PriceFor_MaxLevelOrNotUpgradeable_IsZero()
        {
            Assert.AreEqual(0, UpgradeController.PriceFor(station, 3));
            station.ResidentCapacity = 4;
            Assert.AreEqual(0, UpgradeController.PriceFor(station, 1));
        }
    }
}
