using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Проверки ближайшего закрытого типа: пустой список, все открыты, порядок порогов,
    // отрицательное число зданий, null-элементы и дубли порогов.
    public class NextUnlockTests
    {
        private static BuildingDefinition Definition(string name, int unlockBuildings)
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.DisplayName = name;
            definition.UnlockBuildings = unlockBuildings;
            return definition;
        }

        // Дом открыт сразу, Shop с 4, Factory с 8.
        private static List<BuildingDefinition> City()
        {
            return new List<BuildingDefinition> { Definition("House", 0), Definition("Shop", 4), Definition("Factory", 8) };
        }

        [Test]
        public void EmptyOrNullList_ReturnsNull()
        {
            Assert.IsNull(UnlockPolicy.NextLocked(new List<BuildingDefinition>(), 0));
            Assert.IsNull(UnlockPolicy.NextLocked(null, 0));
        }

        [Test]
        public void AllUnlocked_ReturnsNull()
        {
            Assert.IsNull(UnlockPolicy.NextLocked(City(), 8));
            Assert.IsNull(UnlockPolicy.NextLocked(City(), 50));
        }

        [Test]
        public void NoBuildings_ReturnsShop()
        {
            List<BuildingDefinition> definitions = City();
            Assert.AreSame(definitions[1], UnlockPolicy.NextLocked(definitions, 0));
        }

        [Test]
        public void FourBuildings_ReturnsFactory()
        {
            List<BuildingDefinition> definitions = City();
            Assert.AreSame(definitions[2], UnlockPolicy.NextLocked(definitions, 4));
        }

        [Test]
        public void NegativeBuildings_TreatedAsZero()
        {
            List<BuildingDefinition> definitions = City();
            Assert.AreSame(definitions[1], UnlockPolicy.NextLocked(definitions, -7));
        }

        [Test]
        public void NullItems_Skipped()
        {
            List<BuildingDefinition> definitions = City();
            definitions.Insert(0, null);
            definitions.Add(null);
            Assert.AreSame(definitions[2], UnlockPolicy.NextLocked(definitions, 0));
            Assert.IsNull(UnlockPolicy.NextLocked(new List<BuildingDefinition> { null, null }, 0));
        }

        [Test]
        public void EqualThresholds_FirstInListWins()
        {
            BuildingDefinition first = Definition("Shop", 4);
            BuildingDefinition second = Definition("Market", 4);
            List<BuildingDefinition> definitions = new List<BuildingDefinition> { Definition("House", 0), first, second };
            Assert.AreSame(first, UnlockPolicy.NextLocked(definitions, 0));
        }
    }
}
