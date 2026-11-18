using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Регрессия EPIC-51 шага 3: успешный апгрейд из инфопанели делает сохранение «грязным»,
    // любой отказ (казна / максимум уровня / не апгрейдится) — оставляет флаг чистым.
    public class SaveDirtyUpgradeTests
    {
        private GameObject go;
        private SaveController save;
        private SettlementGrowthController growth;
        private BuildingPlacementController placement;
        private TreasuryView treasury;
        private BuildingDefinition station;
        private BuildingDefinition house;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestUpgradeDirty");
            save = go.AddComponent<SaveController>();
            GridVisualizer viz = go.AddComponent<GridVisualizer>();
            treasury = go.AddComponent<TreasuryView>(); treasury.StartingBalance = 1000;
            growth = go.AddComponent<SettlementGrowthController>(); growth.Treasury = treasury;
            station = MakeDefinition("fire_station_upgrade", 0, ServiceKind.Fire, 3);
            house = MakeDefinition("house_upgrade", 4, ServiceKind.None, 0);
            placement = go.AddComponent<BuildingPlacementController>();
            placement.GridVisualizer = viz; placement.Save = save; // стройка бесплатна: казну тратит только апгрейд
        }

        [TearDown]
        public void TearDown()
        {
            ScriptableObject.DestroyImmediate(station);
            ScriptableObject.DestroyImmediate(house);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Upgrade_Success_MarksSaveDirty()
        {
            Place(station, new GridPosition(3, 3));
            Assert.IsTrue(Upgrade(new GridPosition(3, 3)).Success);
            Assert.AreEqual(2, placement.Placed[0].Level);
            Assert.AreEqual(1000 - 75, treasury.Treasury.Balance);
            Assert.IsTrue(save.IsDirty);
        }

        [Test]
        public void Upgrade_NotEnoughFunds_DoesNotMarkSaveDirty()
        {
            Place(station, new GridPosition(4, 4));
            treasury.StartingBalance = 10;
            UpgradeController.Result result = Upgrade(new GridPosition(4, 4));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UpgradeController.Failure.NotEnoughFunds, result.Reason);
            Assert.AreEqual(1, placement.Placed[0].Level);
            Assert.IsFalse(save.IsDirty);
        }

        [Test]
        public void Upgrade_MaxLevel_DoesNotMarkSaveDirty()
        {
            Place(station, new GridPosition(5, 5));
            Assert.IsTrue(Upgrade(new GridPosition(5, 5)).Success);
            Assert.IsTrue(Upgrade(new GridPosition(5, 5)).Success);
            save.ClearDirty();
            UpgradeController.Result result = Upgrade(new GridPosition(5, 5));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UpgradeController.Failure.MaxLevel, result.Reason);
            Assert.IsFalse(save.IsDirty);
        }

        [Test]
        public void Upgrade_NotUpgradeableBuilding_DoesNotMarkSaveDirty()
        {
            Place(house, new GridPosition(6, 6));
            UpgradeController.Result result = Upgrade(new GridPosition(6, 6));
            Assert.IsFalse(result.Success);
            Assert.AreEqual(UpgradeController.Failure.NotUpgradeable, result.Reason);
            Assert.IsFalse(save.IsDirty);
        }

        private static BuildingDefinition MakeDefinition(
            string id, int residentCapacity, ServiceKind kind, int radius)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Id = id; def.Footprint = new Vector2Int(1, 1); def.ConstructionCost = 100;
            def.ResidentCapacity = residentCapacity; def.ServiceKind = kind; def.ServiceRadius = radius;
            return def;
        }

        // Ставит здание и гасит флаг стройки: дальше видна только реакция на апгрейд.
        private void Place(BuildingDefinition def, GridPosition cell)
        {
            placement.Definition = def;
            Assert.IsTrue(placement.TryPlaceAt(cell));
            save.ClearDirty();
        }

        private UpgradeController.Result Upgrade(GridPosition cell)
        {
            BuildingInfo current = new BuildingInfo();
            current.Origin = cell;
            return InfoPanelUpgrade.TryUpgradeCurrent(current, growth, placement, null);
        }
    }
}
