using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Проверки типа службы в описании здания.
    public class ServiceKindTests
    {
        [Test]
        public void NewDefinition_ServiceKindIsNone()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            Assert.AreEqual(ServiceKind.None, definition.ServiceKind);
        }

        [Test]
        public void AssignedPolice_IsStored()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.ServiceKind = ServiceKind.Police;
            Assert.AreEqual(ServiceKind.Police, definition.ServiceKind);
        }

        [Test]
        public void UnknownValue_DoesNotBreakOtherFields()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.ServiceRadius = 5;
            definition.UnlockBuildings = 7;
            definition.ServiceKind = (ServiceKind)99;
            Assert.AreEqual(5, definition.ServiceRadius);
            Assert.AreEqual(7, definition.UnlockBuildings);
        }

        [Test]
        public void Sanitize_UnknownBecomesNone()
        {
            Assert.AreEqual(ServiceKind.None, BuildingDefinition.SanitizeServiceKind((ServiceKind)99));
            Assert.AreEqual(ServiceKind.Fire, BuildingDefinition.SanitizeServiceKind(ServiceKind.Fire));
            Assert.AreEqual(ServiceKind.Police, BuildingDefinition.SanitizeServiceKind(ServiceKind.Police));
        }
    }
}
