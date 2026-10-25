using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Проверки поля ServiceRadius: дефолт, запись и чистый кламп.
    public class ServiceRadiusDefinitionTests
    {
        [Test]
        public void NewDefinition_ServiceRadiusIsZero()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            Assert.AreEqual(0, definition.ServiceRadius);
        }

        [Test]
        public void AssignedRadius_IsStored()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.ServiceRadius = 6;
            Assert.AreEqual(6, definition.ServiceRadius);
        }

        [Test]
        public void ClampNonNegative_NegativeBecomesZero()
        {
            Assert.AreEqual(0, BuildingDefinition.ClampNonNegative(-3));
        }
    }
}
