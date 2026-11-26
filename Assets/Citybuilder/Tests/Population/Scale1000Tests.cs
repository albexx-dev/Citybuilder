using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Масштаб issue #160: 1300 жителей на 325 домах и вехи после 200 без финала.
    // Награды 50/100/200 и слот 0..3 не меняются; новые цели считаются от населения.
    public class Scale1000Tests
    {
        private const int HouseholdSize = 4;
        private const int Capacity = 4;
        private const int Homes = 325;
        private const int GoalStage = GoalStages.StageCount;

        private static BuildingDefinition HouseDefinition()
        {
            BuildingDefinition definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.Id = "house";
            definition.Footprint = new Vector2Int(2, 2);
            definition.ResidentCapacity = Capacity;
            return definition;
        }

        // 325 домов 2×2 укладываются в 25×25 слотов карты 50×50 — координаты валидны для сейва.
        private static List<BuildingState> MakeHomes()
        {
            BuildingDefinition definition = HouseDefinition();
            List<BuildingState> homes = new List<BuildingState>();
            for (int i = 0; i < Homes; i++)
            {
                GridPosition origin = new GridPosition((i % 25) * 2, (i / 25) * 2);
                List<GridPosition> cells = new List<GridPosition>
                {
                    origin,
                    new GridPosition(origin.X + 1, origin.Y),
                    new GridPosition(origin.X, origin.Y + 1),
                    new GridPosition(origin.X + 1, origin.Y + 1)
                };
                homes.Add(new BuildingState(EntityId.New(), definition, origin, cells));
            }
            return homes;
        }

        [Test]
        public void EndlessMilestones_After200_NoFinale()
        {
            GoalStages stages = new GoalStages();
            Assert.AreEqual(50, stages.TargetFor(0));
            Assert.AreEqual(100, stages.TargetFor(51));
            Assert.AreEqual(200, stages.TargetFor(101));
            Assert.AreEqual(250, stages.TargetFor(200));
            Assert.AreEqual(300, stages.TargetFor(250));
            Assert.AreEqual(1350, stages.TargetFor(1300));
            Assert.AreEqual(1400, stages.TargetFor(1350));
            Assert.AreEqual(7000, stages.Claim(200));
            Assert.AreEqual(GoalStage, stages.Stage);
        }

        [Test]
        public void Homes325_Reach1300Residents()
        {
            List<BuildingState> homes = MakeHomes();
            PopulationSystem population = new PopulationSystem();
            for (int i = 0; i < homes.Count; i++)
            {
                Assert.IsNotNull(population.MoveIn(homes[i], HouseholdSize));
            }
            Assert.AreEqual(1300, population.TotalResidents);
            Assert.AreEqual(Homes, population.HouseholdCount);
            for (int i = 0; i < homes.Count; i++)
            {
                Assert.AreEqual(Capacity, population.ResidentsIn(homes[i]));
            }
        }

        [Test]
        public void MoveInPace_1300Residents_UnderTimeBudget()
        {
            float interval = PopulationController.EffectiveInterval(3f, 1.5f);
            Assert.AreEqual(2f, interval, 1e-4f);
            Assert.LessOrEqual(Homes * interval, 1200f);
        }

        [Test]
        public void SaveRoundtrip_1300Residents_KeepsCounters()
        {
            List<BuildingState> homes = MakeHomes();
            PopulationSystem population = new PopulationSystem();
            for (int i = 0; i < homes.Count; i++) population.MoveIn(homes[i], HouseholdSize);

            CitySnapshot snapshot = SaveSystem.BuildSnapshot(homes, population.Households, null,
                10000, 1625, null, 2, null, GoalStage);
            Assert.AreEqual(GoalStage, snapshot.GoalStage);
            Assert.AreEqual(Homes, snapshot.Buildings.Count);
            Assert.AreEqual(Homes, snapshot.Households.Count);

            CitySnapshot restored = SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(snapshot));
            Assert.IsNotNull(restored);
            Assert.IsTrue(SaveSlotStorage.Validate(restored));
            Assert.AreEqual(GoalStage, restored.GoalStage);
            Assert.AreEqual(1300, SumResidents(restored));

            GoalStages stages = new GoalStages();
            stages.Restore(restored.GoalStage);
            Assert.AreEqual(0, stages.Claim(1300));
            Assert.AreEqual(1350, stages.TargetFor(1300));
        }

        private static int SumResidents(CitySnapshot snapshot)
        {
            int total = 0;
            for (int i = 0; i < snapshot.Households.Count; i++) total += snapshot.Households[i].Size;
            return total;
        }
    }
}
