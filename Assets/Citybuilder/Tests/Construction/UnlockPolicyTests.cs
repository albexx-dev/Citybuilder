using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Проверки порогов разблокировки: границы, отрицательное население, null, дефолт поля.
    public class UnlockPolicyTests
    {
        private static BuildingDefinition Definition(int unlockResidents)
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.UnlockResidents = unlockResidents;
            return definition;
        }

        [Test]
        public void ZeroThreshold_IsUnlockedWithoutResidents()
        {
            Assert.IsTrue(UnlockPolicy.IsUnlocked(Definition(0), 0));
        }

        [Test]
        public void ThresholdTen_LockedAtNine()
        {
            Assert.IsFalse(UnlockPolicy.IsUnlocked(Definition(10), 9));
        }

        [Test]
        public void ThresholdTen_UnlockedAtTen()
        {
            Assert.IsTrue(UnlockPolicy.IsUnlocked(Definition(10), 10));
        }

        [Test]
        public void ThresholdTen_UnlockedAbove()
        {
            Assert.IsTrue(UnlockPolicy.IsUnlocked(Definition(10), 11));
        }

        [Test]
        public void NegativeResidents_TreatedAsZero()
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
        public void Remaining_CountsMissingResidents()
        {
            BuildingDefinition definition = Definition(10);
            Assert.AreEqual(1, UnlockPolicy.Remaining(definition, 9));
            Assert.AreEqual(7, UnlockPolicy.Remaining(definition, 3));
            Assert.AreEqual(0, UnlockPolicy.Remaining(definition, 10));
        }

        [Test]
        public void NewDefinition_UnlockResidentsIsZero()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            Assert.AreEqual(0, definition.UnlockResidents);
        }
    }
}
