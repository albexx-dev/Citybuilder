using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты детерминированного выбора вариантов, фазы стройки 3 тика и сноса в стройке (EPIC-17).
    public class GrowthStageTests
    {
        [Test]
        public void SelectVariant_SameEntityId_ReturnsDeterministicIndex()
        {
            EntityId id = EntityId.New();
            int expected = BuildingDefinition.SelectVariant(id, 3);
            Assert.GreaterOrEqual(expected, 0);
            Assert.Less(expected, 3);
            Assert.AreEqual(expected, BuildingDefinition.SelectVariant(id, 3));
            Assert.AreEqual(0, BuildingDefinition.SelectVariant(id, 1));
            Assert.AreEqual(0, BuildingDefinition.SelectVariant(id, 0));
        }

        [Test]
        public void ConstructionPhase_FinishesAfterThreeTicks_AndDemolishDuringPhaseCleansUp()
        {
            GameObject host = new GameObject("TestSettlementView");
            try
            {
                SettlementView view = host.AddComponent<SettlementView>();
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Footprint = new Vector2Int(2, 2);
                def.VariantCount = 3;
                BuildingState state = new BuildingState(
                    EntityId.New(), def, new GridPosition(0, 0),
                    new List<GridPosition> { new GridPosition(0, 0) });

                GameObject go = view.AddBuilding(state, def, 1f, false);
                Assert.IsNotNull(go);
                GrowthStageView stage = go.GetComponent<GrowthStageView>();
                Assert.IsNotNull(stage);
                Assert.IsTrue(stage.IsConstructing);
                Assert.AreEqual(3, stage.TicksRemaining);

                stage.OnClockTick(new SimulationTime(1));
                stage.OnClockTick(new SimulationTime(2));
                Assert.IsTrue(stage.IsConstructing);
                stage.OnClockTick(new SimulationTime(3));
                Assert.IsFalse(stage.IsConstructing);

                // Восстановление из сейва пропускает фазу стройки.
                BuildingState restored = new BuildingState(
                    EntityId.New(), def, new GridPosition(4, 4),
                    new List<GridPosition> { new GridPosition(4, 4) });
                GameObject restoredGo = view.AddBuilding(restored, def, 1f, true);
                Assert.IsNull(restoredGo.GetComponent<GrowthStageView>());

                // Снос в фазе стройки не бросает исключений.
                BuildingState midBuild = new BuildingState(
                    EntityId.New(), def, new GridPosition(8, 8),
                    new List<GridPosition> { new GridPosition(8, 8) });
                view.AddBuilding(midBuild, def, 1f, false);
                Assert.DoesNotThrow(() => view.RemoveBuilding(midBuild));
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }
    }
}
