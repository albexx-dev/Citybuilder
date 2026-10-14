using System;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Границы победы/поражения цели из 045 на настоящем API.
    public class GoalSystemTests
    {
        private const float Delta = 1e-4f;

        private static void MakeGoal(int residents, long ticks, out GoalSystem goals)
        {
            GridSystem grid = new GridSystem(new GridSize(20, 20), 2f);
            SettlementSystem settlement = new SettlementSystem(grid, new ZoneSystem(grid), new BuildingSystem(grid));
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.AllowedZone = ZoneType.None;
            definition.Footprint = new Vector2Int(1, 1);
            definition.ResidentCapacity = 100;
            definition.JobsProvided = 0;
            BuildingState home = settlement.Settle(definition, new GridPosition(0, 0));
            Assert.IsNotNull(home);
            PopulationSystem population = new PopulationSystem();
            if (residents > 0)
            {
                Assert.IsNotNull(population.MoveIn(home, residents));
            }
            SimulationClock clock = new SimulationClock();
            clock.RestoreTime(ticks);
            goals = new GoalSystem(population, clock);
        }

        [Test]
        public void Ctor_NullPopulation_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new GoalSystem(null, new SimulationClock()));
        }

        [Test]
        public void Ctor_NullClock_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new GoalSystem(new PopulationSystem(), null));
        }

        [Test]
        public void Won_BelowGoal_False()
        {
            GoalSystem goals;
            MakeGoal(49, 0, out goals);
            Assert.IsFalse(goals.Won);
        }

        [Test]
        public void Won_AtGoal_True()
        {
            GoalSystem goals;
            MakeGoal(50, 0, out goals);
            Assert.IsTrue(goals.Won);
        }

        [Test]
        public void Won_AboveGoal_True()
        {
            GoalSystem goals;
            MakeGoal(51, 0, out goals);
            Assert.IsTrue(goals.Won);
        }

        [Test]
        public void Lost_BeforeDeadline_Empty_False()
        {
            GoalSystem goals;
            MakeGoal(0, 0, out goals);
            Assert.IsFalse(goals.Lost);
        }

        [Test]
        public void Lost_AtDeadline_Empty_False()
        {
            GoalSystem goals;
            MakeGoal(0, 72, out goals);
            Assert.IsFalse(goals.Lost);
        }

        [Test]
        public void Lost_AfterDeadline_Empty_True()
        {
            GoalSystem goals;
            MakeGoal(0, 73, out goals);
            Assert.IsTrue(goals.Lost);
        }

        [Test]
        public void Lost_AfterDeadline_WithResidents_False()
        {
            GoalSystem goals;
            MakeGoal(1, 73, out goals);
            Assert.IsFalse(goals.Lost);
        }

        [Test]
        public void Progress_Empty_Zero()
        {
            GoalSystem goals;
            MakeGoal(0, 0, out goals);
            Assert.AreEqual(0f, goals.Progress01, Delta);
        }

        [Test]
        public void Progress_Half()
        {
            GoalSystem goals;
            MakeGoal(25, 0, out goals);
            Assert.AreEqual(0.5f, goals.Progress01, Delta);
        }

        [Test]
        public void Progress_AtGoal_Full()
        {
            GoalSystem goals;
            MakeGoal(50, 0, out goals);
            Assert.AreEqual(1f, goals.Progress01, Delta);
        }

        [Test]
        public void Progress_OverGoal_Clamped()
        {
            GoalSystem goals;
            MakeGoal(60, 0, out goals);
            Assert.AreEqual(1f, goals.Progress01, Delta);
            Assert.IsTrue(goals.Progress01 >= 0f && goals.Progress01 <= 1f);
        }
    }
}
