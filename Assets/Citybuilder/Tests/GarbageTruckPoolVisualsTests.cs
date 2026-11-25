using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // issue #115: модель создаётся при выключенном тумблере и появляется после включения.
    public class GarbageTruckPoolVisualsTests
    {
        private GameObject host;

        [SetUp]
        public void SetUp() { host = new GameObject("GarbageTruckPoolHost"); }

        [TearDown]
        public void TearDown() { Object.DestroyImmediate(host); }

        private static GarbageSystem GarbageWithActiveTruck()
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.WasteOutput = 40;
            BuildingState home = new BuildingState(EntityId.New(), def,
                new GridPosition(2, 2), new List<GridPosition> { new GridPosition(2, 2) });
            GarbageSystem garbage = new GarbageSystem();
            garbage.Tick(1, new List<BuildingState> { home }, new GridPosition(0, 0), null);
            Object.DestroyImmediate(def);
            return garbage;
        }

        [Test]
        public void Pool_CreatedWithVisualsOff_ShowsModelAfterToggle()
        {
            GameObject model = new GameObject("FakeTruckModel");
            GarbageTruckPool pool = null;
            try
            {
                pool = new GarbageTruckPool(host.transform, new[] { model }, 1,
                    1f, 0f, 0f, 1f, false);
                Transform car = host.transform.GetChild(0);
                Assert.AreEqual(1, car.childCount, "модель создана даже при выключенном виде");
                GameObject instance = car.GetChild(0).gameObject;
                Assert.IsFalse(car.gameObject.activeSelf);
                pool.SetShowVisuals(true);
                GarbageSystem garbage = GarbageWithActiveTruck();
                pool.Sync(garbage, host.AddComponent<GridVisualizer>());
                Assert.Greater(garbage.ActiveTrucks, 0);
                Assert.IsTrue(car.gameObject.activeSelf);
                Assert.IsTrue(instance.activeInHierarchy);
                pool.SetShowVisuals(false);
                Assert.IsFalse(car.gameObject.activeSelf);
                Assert.IsFalse(instance.activeInHierarchy);
            }
            finally
            {
                if (pool != null) pool.DestroyAll();
                Object.DestroyImmediate(model);
            }
        }

        [Test]
        public void Pool_NoModels_StillKeepsSlotForSimulation()
        {
            GarbageTruckPool pool = new GarbageTruckPool(host.transform, null, 1,
                1f, 0f, 0f, 1f, false);
            try
            {
                Transform car = host.transform.GetChild(0);
                Assert.AreEqual(0, car.childCount);
                pool.SetShowVisuals(true);
                pool.Sync(GarbageWithActiveTruck(), host.AddComponent<GridVisualizer>());
                Assert.IsTrue(car.gameObject.activeSelf, "слот включается и без FBX-модели");
            }
            finally { pool.DestroyAll(); }
        }
    }
}
