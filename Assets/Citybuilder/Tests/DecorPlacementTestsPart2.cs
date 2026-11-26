using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса DecorPlacementTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class DecorPlacementTests
    {
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
                Assert.IsFalse(decor.CanPlaceAt(new GridPosition(101, 101)));
            }
            finally
            {
                ScriptableObject.DestroyImmediate(tree);
                Object.DestroyImmediate(go);
            }
        }
    }
}
