using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Проверки мусора: чистый кламп, дефолты данных, выбор депо вывоза (issue #90)
    // и лимит слотов флота. Без сцены и Play.
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

        private static GarbageTrucks.DepotCandidate Depot(int x, int y, int capacity)
        {
            return new GarbageTrucks.DepotCandidate(new GridPosition(x, y), capacity);
        }

        [Test]
        public void SelectDepot_NullAndEmpty_ReturnNull()
        {
            Assert.IsFalse(GarbageTrucks.SelectDepot(null).HasValue, "нет списка — нет вывоза");
            Assert.IsFalse(GarbageTrucks.SelectDepot(new List<GarbageTrucks.DepotCandidate>()).HasValue);
        }

        [Test]
        public void SelectDepot_OneDepot_ReturnsItsOrigin()
        {
            List<GarbageTrucks.DepotCandidate> depots = new List<GarbageTrucks.DepotCandidate> { Depot(3, 4, 12) };
            GridPosition? depot = GarbageTrucks.SelectDepot(depots);
            Assert.IsTrue(depot.HasValue);
            Assert.AreEqual(new GridPosition(3, 4), depot.Value);
        }

        [Test]
        public void SelectDepot_TwoDepots_HigherCapacityWins()
        {
            List<GarbageTrucks.DepotCandidate> depots = new List<GarbageTrucks.DepotCandidate>
            {
                Depot(1, 1, 12), Depot(8, 9, 20)
            };
            Assert.AreEqual(new GridPosition(8, 9), GarbageTrucks.SelectDepot(depots).Value,
                "компостная станция ёмче — рейс от неё");
        }

        [Test]
        public void SelectDepot_TwoDepots_Tie_FirstWins()
        {
            List<GarbageTrucks.DepotCandidate> depots = new List<GarbageTrucks.DepotCandidate>
            {
                Depot(2, 2, 12), Depot(7, 7, 12)
            };
            Assert.AreEqual(new GridPosition(2, 2), GarbageTrucks.SelectDepot(depots).Value,
                "равная ёмкость — первый по списку");
        }

        [Test]
        public void SelectDepot_ThreeDepots_MaxCapacityWins()
        {
            List<GarbageTrucks.DepotCandidate> depots = new List<GarbageTrucks.DepotCandidate>
            {
                Depot(1, 1, 12), Depot(8, 9, 20), Depot(4, 4, 16)
            };
            Assert.AreEqual(new GridPosition(8, 9), GarbageTrucks.SelectDepot(depots).Value);
        }

        [Test]
        public void Dispatch_ThreeTimes_UsesAtMostTwoSlots()
        {
            GarbageTrucks fleet = new GarbageTrucks();
            for (int i = 0; i < 3; i++)
            {
                List<GarbageSystem.Pickup> ready = new List<GarbageSystem.Pickup>
                {
                    new GarbageSystem.Pickup { Id = EntityId.New(), At = new GridPosition(2 + i, 2) }
                };
                fleet.Dispatch(new GridPosition(0, 0), ready, null);
            }
            Assert.AreEqual(GarbageTrucks.MaxTrucks, fleet.ActiveCount, "слоты 0 и 1 заняты");
            Assert.IsFalse(fleet.At(2).Active, "третий слот свободен только после освобождения");
        }
    }
}
