using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Сумма содержания за час: размещённые здания, границы и мусор в списках.
    // Требует Unity: BuildingDefinition — ScriptableObject, вне редактора не собрать.
    // Клампы OnValidate рефлексией не дёргаем: кламп поля проверяется в Inspector.
    public class UpkeepPolicyTests
    {
        [Test]
        public void NewDefinition_UpkeepDefaultsToZero()
        {
            Assert.AreEqual(0, ScriptableObject.CreateInstance<BuildingDefinition>().UpkeepPerHour);
        }

        [Test]
        public void TwoStationsAndThreeHomes_SumsSixteen()
        {
            List<BuildingDefinition> placed = new List<BuildingDefinition>();
            placed.Add(MakeDefinition(8));
            placed.Add(MakeDefinition(8));
            placed.Add(MakeDefinition(0));
            placed.Add(MakeDefinition(0));
            placed.Add(MakeDefinition(0));
            Assert.AreEqual(16, UpkeepPolicy.UpkeepPerHour(placed));
        }

        [Test]
        public void EmptyPlacement_IsZero()
        {
            Assert.AreEqual(0, UpkeepPolicy.UpkeepPerHour(new List<BuildingDefinition>()));
        }

        [Test]
        public void NullList_IsZero()
        {
            Assert.AreEqual(0, UpkeepPolicy.UpkeepPerHour((IEnumerable<BuildingDefinition>)null));
        }

        [Test]
        public void NullEntries_AreSkipped()
        {
            List<BuildingDefinition> placed = new List<BuildingDefinition>();
            placed.Add(null);
            placed.Add(MakeDefinition(6));
            Assert.AreEqual(6, UpkeepPolicy.UpkeepPerHour(placed));
        }

        [TestCase(-5)]
        [TestCase(int.MinValue)]
        public void NegativeUpkeep_NeverReducesTotal(int upkeep)
        {
            List<BuildingDefinition> placed = new List<BuildingDefinition>();
            placed.Add(MakeDefinition(upkeep));
            placed.Add(MakeDefinition(3));
            Assert.AreEqual(3, UpkeepPolicy.UpkeepPerHour(placed));
        }

        [Test]
        public void GrownAndPlaced_SumBothLists()
        {
            List<BuildingState> grown = new List<BuildingState>();
            grown.Add(MakeState(1));
            grown.Add(MakeState(1));
            List<BuildingState> placed = new List<BuildingState>();
            placed.Add(MakeState(8));
            Assert.AreEqual(10, UpkeepPolicy.UpkeepPerHour(grown, placed));
        }

        [Test]
        public void GrownAndPlaced_NullOrBroken_IsZero()
        {
            List<BuildingState> placed = new List<BuildingState>();
            placed.Add(null);
            placed.Add(new BuildingState(EntityId.New(), null, new GridPosition(0, 0),
                new List<GridPosition>()));
            Assert.AreEqual(0, UpkeepPolicy.UpkeepPerHour(null, placed));
        }

        private static BuildingDefinition MakeDefinition(int upkeep)
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.UpkeepPerHour = upkeep;
            return definition;
        }

        private static BuildingState MakeState(int upkeep)
        {
            return new BuildingState(EntityId.New(), MakeDefinition(upkeep), new GridPosition(0, 0),
                new List<GridPosition>());
        }
    }
}
