using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса PopulationMoveInTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class PopulationMoveInTests
    {
        [Test]
        public void FindMoveInHome_NullElementInHomes_HandledGracefully()
        {
            List<BuildingState> listWithNull = new List<BuildingState> { null, homes[0], null };
            Assert.AreSame(homes[0], PopulationController.FindMoveInHome(listWithNull, population, 2, h => true));
            Assert.IsNull(PopulationController.FindMoveInHome(listWithNull, population, 0, null));
            Assert.IsNull(PopulationController.FindMoveInHome(listWithNull, population, -1, null));
        }

        [Test]
        public void SimulationSpeed_ScalesTickAccumulation()
        {
            GameObject go = new GameObject("PopSpeedTest");
            try
            {
                SimulationRunner runner = go.AddComponent<SimulationRunner>();
                PopulationController pop = go.AddComponent<PopulationController>();
                pop.Runner = runner;
                // Проверяем только масштабирование скорости: длинный интервал исключает въезд и сброс таймера.
                pop.MoveInIntervalSec = 100f;
                runner.SetSpeed(SimulationSpeed.Paused);
                pop.Timer = 0f; pop.Tick(1f);
                Assert.AreEqual(0f, pop.Timer);
                runner.SetSpeed(SimulationSpeed.Playing);
                pop.Tick(1f);
                Assert.AreEqual(1f, pop.Timer);
                runner.SetSpeed(SimulationSpeed.FastForward);
                pop.Tick(1f);
                Assert.AreEqual(5f, pop.Timer);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void BuildingViewFactory_CreateGhost_DestroySafeInEditMode()
        {
            GameObject root = new GameObject("GhostTestRoot");
            GameObject ghost = null;
            try
            {
                GridVisualizer viz = root.AddComponent<GridVisualizer>();
                ghost = BuildingViewFactory.CreateGhost(HouseDef(2), viz, root.transform);
                Assert.IsNotNull(ghost);
                Assert.IsNull(ghost.GetComponent<Collider>());
                Assert.DoesNotThrow(() => BuildingViewFactory.DestroySafe(null));
            }
            finally
            {
                BuildingViewFactory.DestroySafe(ghost);
                BuildingViewFactory.DestroySafe(root);
            }
        }
    }
}
