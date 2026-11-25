using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Стройплощадка на prefab-пути (EPIC-61): площадка вместо готового вида, замена по завершении,
    // мгновенное появление при 0 тиков. Домен при фазе стройки не меняется; снос — деньги не возвращаются.
    public partial class ConstructionSiteTests
    {
        [Test]
        public void PrefabPath_StartsAsSite_AndReplacedByPrefabAfterTicks()
        {
            GameObject host = new GameObject("TestSettlementView");
            GameObject prefab = new GameObject("FakePrefab");
            try
            {
                SettlementView view = host.AddComponent<SettlementView>();
                BuildingState state = MakeSiteState(prefab);
                GameObject siteGo = view.AddBuilding(state, state.Definition, 1f, false);
                Assert.IsNotNull(siteGo);
                Assert.AreEqual("ConstructionSite", siteGo.name);
                ConstructionSiteView site = siteGo.GetComponent<ConstructionSiteView>();
                Assert.IsNotNull(site);
                Assert.IsTrue(site.IsConstructing);
                Assert.IsNull(host.transform.Find("FakePrefab(Clone)"));
                AssertPrefabReplacesSite(host, site);
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(prefab);
            }
        }

        // Состояние здания с фазой стройки (issue #127).
        private static BuildingState MakeSiteState(GameObject prefab)
        {
                BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Footprint = new Vector2Int(2, 2);
                def.ConstructionTicks = 4;
                def.Prefab = prefab;
                BuildingState state = new BuildingState(
                    EntityId.New(), def, new GridPosition(0, 0),
                    new List<GridPosition> { new GridPosition(0, 0) });
            return state;
        }

        // Прогон тиков и проверка замены площадки префабом (issue #127).
        private static void AssertPrefabReplacesSite(GameObject host, ConstructionSiteView site)
        {
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
            GridSystem gridA, gridB;
            BuildingState a, b;
            PlacePhasedAndInstant(out gridA, out gridB, out a, out b);

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

        // Две сетки: фаза стройки и мгновенная постройка (issue #127).
        private static void PlacePhasedAndInstant(out GridSystem gridA, out GridSystem gridB,
            out BuildingState a, out BuildingState b)
        {
            BuildingDefinition withPhase = ScriptableObject.CreateInstance<BuildingDefinition>();
            withPhase.Footprint = new Vector2Int(2, 2);
            withPhase.ConstructionTicks = 5;
            BuildingDefinition instant = ScriptableObject.CreateInstance<BuildingDefinition>();
            instant.Footprint = new Vector2Int(2, 2);
            instant.ConstructionTicks = 0;

            gridA = new GridSystem(new GridSize(10, 10), 1f);
            gridB = new GridSystem(new GridSize(10, 10), 1f);
            a = new BuildingSystem(gridA).Place(withPhase, new GridPosition(2, 2));
            b = new BuildingSystem(gridB).Place(instant, new GridPosition(2, 2));
        }


    }
}
