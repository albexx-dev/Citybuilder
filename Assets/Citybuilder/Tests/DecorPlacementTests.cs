using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Тесты декора (EPIC-56): чистая уборка по занятости/зонам и поведение контроллера в редакторе.
    public class DecorPlacementTests
    {
        private static List<GridPosition> Cells(params int[] xy)
        {
            List<GridPosition> cells = new List<GridPosition>();
            for (int i = 0; i + 1 < xy.Length; i += 2) cells.Add(new GridPosition(xy[i], xy[i + 1]));
            return cells;
        }

        private static DecorPlacementController NewDecor(GameObject go, DecorDefinition tree)
        {
            GridVisualizer viz = go.GetComponent<GridVisualizer>();
            if (viz == null) viz = go.AddComponent<GridVisualizer>();
            DecorPlacementController decor = go.AddComponent<DecorPlacementController>();
            decor.GridVisualizer = viz;
            decor.Definitions = new List<DecorDefinition> { tree };
            decor.SetSelected(tree);
            return decor;
        }

        private static DecorDefinition MakeTree(string id)
        {
            DecorDefinition tree = ScriptableObject.CreateInstance<DecorDefinition>();
            tree.Id = id;
            tree.DisplayName = id;
            return tree;
        }

        [Test]
        public void CellsToRemove_BuildingOccupiedCell_RemovesDecorFromIt()
        {
            List<GridPosition> removed = DecorCleanupPolicy.CellsToRemove(Cells(0, 0, 1, 0, 2, 0), Cells(1, 0), null);
            Assert.AreEqual(1, removed.Count);
            Assert.AreEqual(new GridPosition(1, 0), removed[0]);
        }

        [Test]
        public void CellsToRemove_ZonedCell_AlsoRemoved()
        {
            List<GridPosition> removed = DecorCleanupPolicy.CellsToRemove(Cells(3, 3), null, Cells(3, 3));
            Assert.AreEqual(1, removed.Count);
            Assert.AreEqual(new GridPosition(3, 3), removed[0]);
        }

        [Test]
        public void CellsToRemove_FreeCells_StayUntouched()
        {
            List<GridPosition> removed = DecorCleanupPolicy.CellsToRemove(Cells(0, 0, 5, 5), Cells(9, 9), Cells(8, 8));
            Assert.AreEqual(0, removed.Count);
        }

        [Test]
        public void CellsToRemove_NullInputs_ReturnEmptyWithoutThrow()
        {
            Assert.AreEqual(0, DecorCleanupPolicy.CellsToRemove(null, Cells(1, 1), null).Count);
        }

        [Test]
        public void Placement_PlaceReplaceAndRemove_KeepsOneDecorPerCell()
        {
            GameObject go = new GameObject("DecorPlaceTest");
            DecorDefinition tree = MakeTree("tree_pine");
            try
            {
                DecorPlacementController decor = NewDecor(go, tree);
                Assert.IsTrue(decor.CanPlaceAt(new GridPosition(2, 2)));
                Assert.IsTrue(decor.TryPlaceAt(new GridPosition(2, 2)));
                Assert.AreEqual(1, decor.PlacedCount);
                Assert.IsTrue(decor.TryPlaceAt(new GridPosition(2, 2)));
                Assert.AreEqual(1, decor.PlacedCount);
                Assert.IsTrue(decor.TryRemoveAt(new GridPosition(2, 2)));
                Assert.AreEqual(0, decor.PlacedCount);
            }
            finally
            {
                ScriptableObject.DestroyImmediate(tree);
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void SweepBlocked_CellOccupiedByBuilding_RemovesDecor()
        {
            GameObject go = new GameObject("DecorSweepBuildingTest");
            DecorDefinition tree = MakeTree("tree_oak");
            try
            {
                DecorPlacementController decor = NewDecor(go, tree);
                Assert.IsTrue(decor.TryPlaceAt(new GridPosition(4, 4)));
                Assert.AreEqual(1, decor.PlacedCount);
                // Здание заняло клетку: декор не занимает клетки и убирается.
                go.GetComponent<GridVisualizer>().Grid.Occupy(new GridPosition(4, 4), EntityId.New());
                decor.SweepBlocked();
                Assert.AreEqual(0, decor.PlacedCount);
            }
            finally
            {
                ScriptableObject.DestroyImmediate(tree);
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void SweepBlocked_CellZoned_RemovesDecor()
        {
            GameObject go = new GameObject("DecorSweepZoneTest");
            DecorDefinition tree = MakeTree("tree_bush");
            try
            {
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                DecorPlacementController decor = NewDecor(go, tree);
                Assert.IsTrue(decor.TryPlaceAt(new GridPosition(6, 6)));
                ZonePaintingController painter = go.AddComponent<ZonePaintingController>();
                painter.GridVisualizer = viz;
                decor.ZonePainter = painter;
                Assert.IsTrue(painter.Zones.SetZone(new GridPosition(6, 6), ZoneType.Residential));
                decor.SweepBlocked();
                Assert.AreEqual(0, decor.PlacedCount);
            }
            finally
            {
                ScriptableObject.DestroyImmediate(tree);
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CanPlaceAt_OccupiedZonedOrOutside_Rejected()
        {
            GameObject go = new GameObject("DecorCanPlaceTest");
            DecorDefinition tree = MakeTree("tree_fir");
            try
            {
                GridVisualizer viz = go.AddComponent<GridVisualizer>();
                DecorPlacementController decor = NewDecor(go, tree);
                viz.Grid.Occupy(new GridPosition(1, 1), EntityId.New());
                Assert.IsFalse(decor.CanPlaceAt(new GridPosition(1, 1)));
                ZonePaintingController painter = go.AddComponent<ZonePaintingController>();
                painter.GridVisualizer = viz;
                decor.ZonePainter = painter;
                Assert.IsTrue(painter.Zones.SetZone(new GridPosition(2, 2), ZoneType.Industrial));
                Assert.IsFalse(decor.CanPlaceAt(new GridPosition(2, 2)));
                Assert.IsFalse(decor.CanPlaceAt(new GridPosition(99, 99)));
            }
            finally
            {
                ScriptableObject.DestroyImmediate(tree);
                Object.DestroyImmediate(go);
            }
        }
    }
}
