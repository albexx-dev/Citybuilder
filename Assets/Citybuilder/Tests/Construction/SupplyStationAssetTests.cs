using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Citybuilder
{
    // Ассеты-источники (энергия, вода, мусор): работают при нуле снабжения, без потребления.
    public class SupplyStationAssetTests
    {
        private const string PowerStationPath = "Assets/Citybuilder/Data/Buildings/PowerStation.asset";
        private const string WaterTowerPath = "Assets/Citybuilder/Data/Buildings/WaterTower.asset";
        private const string WasteDepotPath = "Assets/Citybuilder/Data/Buildings/WasteDepot.asset";

        [Test]
        public void PowerStation_ProducesPowerWithoutDemand()
        {
            BuildingDefinition definition = Load(PowerStationPath);

            Assert.AreEqual("power_station", definition.Id);
            Assert.AreEqual("Power Station", definition.DisplayName);
            AssertCommon(definition);
            Assert.AreEqual(12, definition.PowerOutput);
            Assert.AreEqual(0, definition.PowerDemand);
            Assert.AreEqual(0, definition.WaterDemand);
            Assert.AreEqual(0, definition.WaterOutput);
            Assert.AreEqual(0, definition.WasteCapacity);
        }

        [Test]
        public void WaterTower_ProducesWaterWithoutDemand()
        {
            BuildingDefinition definition = Load(WaterTowerPath);

            Assert.AreEqual("water_tower", definition.Id);
            Assert.AreEqual("Water Tower", definition.DisplayName);
            AssertCommon(definition);
            Assert.AreEqual(12, definition.WaterOutput);
            Assert.AreEqual(0, definition.WaterDemand);
            Assert.AreEqual(0, definition.PowerDemand);
            Assert.AreEqual(0, definition.PowerOutput);
            Assert.AreEqual(0, definition.WasteCapacity);
        }

        [Test]
        public void WasteDepot_ProvidesCapacityWithoutOutput()
        {
            BuildingDefinition definition = Load(WasteDepotPath);

            Assert.AreEqual("waste_depot", definition.Id);
            Assert.AreEqual("Waste Depot", definition.DisplayName);
            AssertCommon(definition);
            Assert.AreEqual(12, definition.WasteCapacity);
            Assert.AreEqual(0, definition.WasteOutput);
            Assert.AreEqual(0, definition.PowerDemand);
            Assert.AreEqual(0, definition.WaterDemand);
        }

        [Test]
        public void Ids_AreUnique()
        {
            string power = Load(PowerStationPath).Id;
            string water = Load(WaterTowerPath).Id;
            string waste = Load(WasteDepotPath).Id;

            CollectionAssert.AllItemsAreUnique(new string[] { power, water, waste });
        }

        private static BuildingDefinition Load(string path)
        {
            BuildingDefinition definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
            Assert.IsNotNull(definition, "Missing asset: " + path);
            return definition;
        }

        // Общие требования к любому ассету-источнику.
        private static void AssertCommon(BuildingDefinition definition)
        {
            Assert.Greater(definition.ConstructionCost, 0, "cost");
            Assert.AreEqual(0, definition.UnlockBuildings, "unlock");
            Assert.AreEqual(0, definition.ResidentCapacity, "capacity");
            Assert.Greater(definition.JobsProvided, 0, "jobs");
            Assert.AreEqual(0, definition.ServiceRadius, "radius");
            Assert.AreEqual(ServiceKind.None, definition.ServiceKind, "kind");
            Assert.AreEqual(new Vector2Int(2, 2), definition.Footprint, "footprint");
            Assert.AreEqual(ZoneType.Industrial, definition.AllowedZone, "zone");
            Assert.IsNull(definition.Prefab, "prefab");
        }
    }
}
