using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Проверки мусора: чистый кламп и дефолты данных. Без сцены и Play.
    public class WasteTests
    {
        [Test]
        public void ClampNonNegative_Negative_BecomesZero()
        {
            Assert.AreEqual(0, BuildingDefinition.ClampNonNegative(-3));
        }

        [Test]
        public void ClampNonNegative_IntMinValue_BecomesZero()
        {
            Assert.AreEqual(0, BuildingDefinition.ClampNonNegative(int.MinValue));
        }

        [Test]
        public void ClampNonNegative_Zero_StaysZero()
        {
            Assert.AreEqual(0, BuildingDefinition.ClampNonNegative(0));
        }

        [Test]
        public void ClampNonNegative_Positive_StaysSame()
        {
            Assert.AreEqual(7, BuildingDefinition.ClampNonNegative(7));
        }

        [Test]
        public void Defaults_WasteOutputOneAndCapacityZero()
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            Assert.AreEqual(1, def.WasteOutput);
            Assert.AreEqual(0, def.WasteCapacity);
        }
    }
}
