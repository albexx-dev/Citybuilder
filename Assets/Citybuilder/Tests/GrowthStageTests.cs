using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты детерминированного выбора вариантов и стройфазы куба (EPIC-17 → EPIC-61).
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
        public void ConstructionPhase_GrowsCubeByTicks_AndDemolishDuringPhaseCleansUp()
        {
            GameObject host = new GameObject("TestSettlementView");
            try
            {
                SettlementView view = host.AddComponent<SettlementView>();
                BuildingState state = MakeCubeState();
                GameObject go = view.AddBuilding(state, state.Definition, 1f, false);
                Assert.IsNotNull(go);
                ConstructionSiteView site = go.GetComponent<ConstructionSiteView>();
                Assert.IsNotNull(site);
                Assert.IsTrue(site.IsConstructing);
                Assert.AreEqual(0, site.TicksElapsed);
                Assert.Less(go.transform.localScale.y, 0.01f);
                Assert.IsFalse(go.transform.GetChild(0).gameObject.activeSelf);
                AssertGrowthTicks(go, site);
                AssertRestoreAndDemolish(view, state.Definition);
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }

        // Состояние куба с тремя тиками стройки (issue #127).
        private static BuildingState MakeCubeState()
        {
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Footprint = new Vector2Int(2, 2);
                def.VariantCount = 3;
                def.ConstructionTicks = 3;
                BuildingState state = new BuildingState(
                    EntityId.New(), def, new GridPosition(0, 0),
                    new List<GridPosition> { new GridPosition(0, 0) });
            return state;
        }

        // Рост по тикам (issue #127): проверки сохранены.
        private static void AssertGrowthTicks(GameObject go, ConstructionSiteView site)
        {
                site.OnClockTick(new SimulationTime(1));
                Assert.Greater(go.transform.localScale.y, 0.01f);
                site.OnClockTick(new SimulationTime(2));
                Assert.IsTrue(site.IsConstructing);
                site.OnClockTick(new SimulationTime(3));
                Assert.IsFalse(site.IsConstructing);
                Assert.Greater(go.transform.localScale.y, 0.9f);
                Assert.IsTrue(go.transform.GetChild(0).gameObject.activeSelf);
        }

        // Восстановление и снос в фазе стройки (issue #127).
        private static void AssertRestoreAndDemolish(SettlementView view, BuildingDefinition def)
        {
                // Восстановление из сейва пропускает фазу стройки.
                BuildingState restored = new BuildingState(
                    EntityId.New(), def, new GridPosition(4, 4),
                    new List<GridPosition> { new GridPosition(4, 4) });
                GameObject restoredGo = view.AddBuilding(restored, def, 1f, true);
                Assert.IsNull(restoredGo.GetComponent<ConstructionSiteView>());

                // Снос в фазе стройки не бросает исключений.
                BuildingState midBuild = new BuildingState(
                    EntityId.New(), def, new GridPosition(8, 8),
                    new List<GridPosition> { new GridPosition(8, 8) });
                view.AddBuilding(midBuild, def, 1f, false);
                Assert.DoesNotThrow(() => view.RemoveBuilding(midBuild));
        }
    }
}
