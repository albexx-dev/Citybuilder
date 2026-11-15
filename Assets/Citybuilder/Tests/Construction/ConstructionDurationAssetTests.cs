using NUnit.Framework;
using UnityEditor;

namespace Citybuilder
{
    // Длительности стройки по типам из ассетов (EPIC-61): House/Park 3, коммерция и утилити 4, службы 5.
    public class ConstructionDurationAssetTests
    {
        private const string BuildingsFolder = "Assets/Citybuilder/Data/Buildings/";

        [TestCase("House", 3)]
        [TestCase("Park", 3)]
        [TestCase("Shop", 4)]
        [TestCase("Factory", 4)]
        [TestCase("PowerStation", 4)]
        [TestCase("WaterTower", 4)]
        [TestCase("WasteDepot", 4)]
        [TestCase("FireStation", 5)]
        [TestCase("PoliceStation", 5)]
        [TestCase("Hospital", 5)]
        public void Assets_HavePerTypeConstructionTicks(string assetName, int expectedTicks)
        {
            BuildingDefinition definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(BuildingsFolder + assetName + ".asset");
            Assert.IsNotNull(definition, "Missing asset: " + assetName);
            Assert.AreEqual(expectedTicks, definition.ConstructionTicks, assetName);
        }

        [Test]
        public void DefaultDefinition_IsThreeTicks()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            Assert.AreEqual(3, definition.ConstructionTicks);
        }

        [Test]
        public void NegativeTicks_AreClampedToInstant()
        {
            Assert.AreEqual(0, ConstructionProgress.ClampTicks(-7));
        }
    }
}
