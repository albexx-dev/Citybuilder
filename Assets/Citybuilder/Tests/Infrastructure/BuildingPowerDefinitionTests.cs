using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Проверки энергополей BuildingDefinition: дефолты и запись значений.
    // Клампы OnValidate рефлексией не дёргаем — они проверяются в Inspector на приёмке.
    public class BuildingPowerDefinitionTests
    {
        [Test]
        public void NewDefinition_DemandIsOne_OutputIsZero()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            Assert.AreEqual(1, definition.PowerDemand);
            Assert.AreEqual(0, definition.PowerOutput);
        }

        [Test]
        public void AssignedValues_AreStored()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.PowerDemand = 3;
            definition.PowerOutput = 7;
            Assert.AreEqual(3, definition.PowerDemand);
            Assert.AreEqual(7, definition.PowerOutput);
        }

        [Test]
        public void HouseDefinition_WithoutOutput_StaysZero()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.PowerDemand = 0;
            Assert.AreEqual(0, definition.PowerDemand);
            Assert.AreEqual(0, definition.PowerOutput);
        }
    }
}
