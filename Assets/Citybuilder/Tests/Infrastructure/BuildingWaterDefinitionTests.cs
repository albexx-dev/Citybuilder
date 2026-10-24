using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Проверки водных полей BuildingDefinition: дефолты и запись значений.
    // Клампы OnValidate рефлексией не дёргаем — они проверяются в Inspector на приёмке.
    public class BuildingWaterDefinitionTests
    {
        [Test]
        public void NewDefinition_DemandIsOne_OutputIsZero()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            Assert.AreEqual(1, definition.WaterDemand);
            Assert.AreEqual(0, definition.WaterOutput);
        }

        [Test]
        public void AssignedValues_AreStored()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.WaterDemand = 3;
            definition.WaterOutput = 7;
            Assert.AreEqual(3, definition.WaterDemand);
            Assert.AreEqual(7, definition.WaterOutput);
        }

        [Test]
        public void HouseDefinition_WithoutOutput_StaysZero()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.WaterDemand = 0;
            Assert.AreEqual(0, definition.WaterDemand);
            Assert.AreEqual(0, definition.WaterOutput);
        }
    }
}
