using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Проверки порогов разблокировки по выросшим зданиям: границы, отрицательное число, null, дефолт.
    public class UnlockPolicyTests
    {
        private static BuildingDefinition Definition(int unlockBuildings)
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.UnlockBuildings = unlockBuildings;
            return definition;
        }

        [Test]
        public void ZeroThreshold_IsUnlockedWithoutBuildings()
        {
            Assert.IsTrue(UnlockPolicy.IsUnlocked(Definition(0), 0));
        }

        [Test]
        public void ThresholdFour_LockedAtThree()
        {
            Assert.IsFalse(UnlockPolicy.IsUnlocked(Definition(4), 3));
        }

        [Test]
        public void ThresholdFour_UnlockedAtFour()
        {
            Assert.IsTrue(UnlockPolicy.IsUnlocked(Definition(4), 4));
        }

        [Test]
        public void ThresholdFour_UnlockedAbove()
        {
            Assert.IsTrue(UnlockPolicy.IsUnlocked(Definition(4), 5));
        }

        [Test]
        public void ThresholdEight_LockedAtSevenUnlockedAtEight()
        {
            Assert.IsFalse(UnlockPolicy.IsUnlocked(Definition(8), 7));
            Assert.IsTrue(UnlockPolicy.IsUnlocked(Definition(8), 8));
        }

        [Test]
        public void NegativeBuildings_TreatedAsZero()
        {
            Assert.IsTrue(UnlockPolicy.IsUnlocked(Definition(0), -5));
            Assert.IsFalse(UnlockPolicy.IsUnlocked(Definition(1), -5));
        }

        [Test]
        public void NullDefinition_IsLocked()
        {
            Assert.IsFalse(UnlockPolicy.IsUnlocked(null, 100));
            Assert.AreEqual(0, UnlockPolicy.Remaining(null, 100));
        }

        [Test]
        public void Remaining_CountsMissingBuildings()
        {
            BuildingDefinition definition = Definition(4);
            Assert.AreEqual(1, UnlockPolicy.Remaining(definition, 3));
            Assert.AreEqual(3, UnlockPolicy.Remaining(definition, 1));
            Assert.AreEqual(0, UnlockPolicy.Remaining(definition, 4));
        }

        [Test]
        public void NewDefinition_UnlockBuildingsIsZero()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            Assert.AreEqual(0, definition.UnlockBuildings);
        }
    }
}
