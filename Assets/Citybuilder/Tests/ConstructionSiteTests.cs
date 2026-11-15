using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Стройплощадка на prefab-пути (EPIC-61): площадка вместо готового вида, замена по завершении,
    // мгновенное появление при 0 тиков. Домен при фазе стройки не меняется; снос — деньги не возвращаются.
    public class ConstructionSiteTests
    {
        [Test]
        public void PrefabPath_StartsAsSite_AndReplacedByPrefabAfterTicks()
        {
            GameObject host = new GameObject("TestSettlementView");
            GameObject prefab = new GameObject("FakePrefab");
            try
            {
                SettlementView view = host.AddComponent<SettlementView>();
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Footprint = new Vector2Int(2, 2);
                def.ConstructionTicks = 4;
                def.Prefab = prefab;
                BuildingState state = new BuildingState(
                    EntityId.New(), def, new GridPosition(0, 0),
                    new List<GridPosition> { new GridPosition(0, 0) });

                GameObject siteGo = view.AddBuilding(state, def, 1f, false);
                Assert.IsNotNull(siteGo);
                Assert.AreEqual("ConstructionSite", siteGo.name);
                ConstructionSiteView site = siteGo.GetComponent<ConstructionSiteView>();
                Assert.IsNotNull(site);
                Assert.IsTrue(site.IsConstructing);
                Assert.IsNull(host.transform.Find("FakePrefab(Clone)"));

                site.OnClockTick(new SimulationTime(1));
                site.OnClockTick(new SimulationTime(2));
                site.OnClockTick(new SimulationTime(3));
                Assert.IsTrue(site.IsConstructing);
                site.OnClockTick(new SimulationTime(4));

                // Замена без анимации: площадка уничтожена, готовый префаб на её месте.
                Assert.IsFalse(site.IsConstructing);
                Assert.IsNull(host.GetComponentInChildren<ConstructionSiteView>());
                Assert.IsNotNull(host.transform.Find("FakePrefab(Clone)"));
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void PrefabPath_ZeroTicks_AppearsInstantly()
        {
            GameObject host = new GameObject("TestSettlementView");
            GameObject prefab = new GameObject("FakePrefab");
            try
            {
                SettlementView view = host.AddComponent<SettlementView>();
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Footprint = new Vector2Int(2, 2);
                def.ConstructionTicks = 0;
                def.Prefab = prefab;
                BuildingState state = new BuildingState(
                    EntityId.New(), def, new GridPosition(0, 0),
                    new List<GridPosition> { new GridPosition(0, 0) });

                GameObject go = view.AddBuilding(state, def, 1f, false);
                Assert.IsNotNull(go);
                Assert.IsNull(go.GetComponent<ConstructionSiteView>());
                Assert.AreEqual("FakePrefab(Clone)", go.name);
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(prefab);
            }
        }

        // Фаза стройки — чистый вид: занятость клеток одинакова с фазой и без (паттерн EPIC-17).
        [Test]
        public void ConstructionPhase_DoesNotTouchDomainMetrics()
        {
            BuildingDefinition withPhase = ScriptableObject.CreateInstance<BuildingDefinition>();
            withPhase.Footprint = new Vector2Int(2, 2);
            withPhase.ConstructionTicks = 5;
            BuildingDefinition instant = ScriptableObject.CreateInstance<BuildingDefinition>();
            instant.Footprint = new Vector2Int(2, 2);
            instant.ConstructionTicks = 0;

            GridSystem gridA = new GridSystem(new GridSize(10, 10), 1f);
            GridSystem gridB = new GridSystem(new GridSize(10, 10), 1f);
            BuildingState a = new BuildingSystem(gridA).Place(withPhase, new GridPosition(2, 2));
            BuildingState b = new BuildingSystem(gridB).Place(instant, new GridPosition(2, 2));

            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    GridPosition cell = new GridPosition(x, y);
                    Assert.AreEqual(gridA.IsOccupied(cell), gridB.IsOccupied(cell), "cell " + x + "," + y);
                }
            }
            Assert.AreEqual(4, gridA.OccupiedCount);
            Assert.AreEqual(4, gridB.OccupiedCount);

            Assert.IsTrue(new BuildingSystem(gridA).Demolish(a));
            Assert.IsTrue(new BuildingSystem(gridB).Demolish(b));
            Assert.AreEqual(0, gridA.OccupiedCount);
            Assert.AreEqual(0, gridB.OccupiedCount);
        }

        // Правила v1.1: снос в стройке освобождает клетку, деньги не возвращаются.
        [Test]
        public void DemolishDuringConstruction_FreesCells_KeepsMoneySpent()
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Footprint = new Vector2Int(2, 2);
            def.ConstructionCost = 100;
            def.ConstructionTicks = 3;
            Treasury treasury = new Treasury(150);
            GridSystem grid = new GridSystem(new GridSize(10, 10), 1f);
            BuildingSystem system = new BuildingSystem(grid);

            BuildingState state = system.Place(def, new GridPosition(0, 0));
            Assert.IsNotNull(state);
            Assert.IsTrue(treasury.TrySpend(def.ConstructionCost, def.Id).Success);
            Assert.AreEqual(50, treasury.Balance);

            Assert.IsTrue(system.Demolish(state));
            Assert.IsFalse(grid.IsOccupied(new GridPosition(0, 0)));
            Assert.IsFalse(grid.IsOccupied(new GridPosition(1, 1)));
            Assert.AreEqual(50, treasury.Balance);
        }
    }
}
