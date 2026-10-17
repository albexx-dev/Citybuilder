using System.Collections.Generic;
using NUnit.Framework;
namespace Citybuilder
{
    public class RouteTests
    {
        private static GridSystem MakeGrid()
        {
            return new GridSystem(new GridSize(10, 8), 2f);
        }
        [Test]
        public void SameCell_ReturnsSingle()
        {
            GridSystem grid = MakeGrid();
            GridPosition cell = new GridPosition(2, 3);
            grid.Occupy(cell, EntityId.New());
            RouteSystem routes = new RouteSystem(grid);
            List<GridPosition> path = routes.FindPath(cell, cell);
            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(cell, path[0]);
        }
        [Test]
        public void StraightLine_FindsPath()
        {
            GridSystem grid = MakeGrid();
            for (int x = 0; x <= 3; x++)
            {
                grid.Occupy(new GridPosition(x, 0), EntityId.New());
            }
            RouteSystem routes = new RouteSystem(grid);
            List<GridPosition> path = routes.FindPath(new GridPosition(0, 0), new GridPosition(3, 0));
            Assert.IsNotNull(path);
            Assert.AreEqual(4, path.Count);
            Assert.AreEqual(new GridPosition(0, 0), path[0]);
            Assert.AreEqual(new GridPosition(3, 0), path[3]);
        }
        [Test]
        public void LShaped_FindsPath()
        {
            GridSystem grid = MakeGrid();
            for (int x = 0; x <= 2; x++)
            {
                grid.Occupy(new GridPosition(x, 0), EntityId.New());
            }
            for (int y = 1; y <= 2; y++)
            {
                grid.Occupy(new GridPosition(2, y), EntityId.New());
            }
            RouteSystem routes = new RouteSystem(grid);
            List<GridPosition> path = routes.FindPath(new GridPosition(0, 0), new GridPosition(2, 2));
            Assert.IsNotNull(path);
            Assert.AreEqual(5, path.Count);
            Assert.AreEqual(new GridPosition(0, 0), path[0]);
            Assert.AreEqual(new GridPosition(2, 2), path[path.Count - 1]);
        }
        [Test]
        public void Gap_NoPath()
        {
            GridSystem grid = MakeGrid();
            grid.Occupy(new GridPosition(0, 0), EntityId.New());
            grid.Occupy(new GridPosition(2, 0), EntityId.New());
            RouteSystem routes = new RouteSystem(grid);
            Assert.IsNull(routes.FindPath(new GridPosition(0, 0), new GridPosition(2, 0)));
        }
        [Test]
        public void StartNotRoad_Null()
        {
            GridSystem grid = MakeGrid();
            grid.Occupy(new GridPosition(1, 0), EntityId.New());
            RouteSystem routes = new RouteSystem(grid);
            Assert.IsNull(routes.FindPath(new GridPosition(0, 0), new GridPosition(1, 0)));
        }
        [Test]
        public void GoalOutside_Null()
        {
            GridSystem grid = MakeGrid();
            grid.Occupy(new GridPosition(0, 0), EntityId.New());
            RouteSystem routes = new RouteSystem(grid);
            Assert.IsNull(routes.FindPath(new GridPosition(0, 0), new GridPosition(10, 0)));
        }
        [Test]
        public void TrackLoad_LoadOf_Clear()
        {
            GridSystem grid = MakeGrid();
            GridPosition a = new GridPosition(0, 0);
            GridPosition b = new GridPosition(1, 0);
            grid.Occupy(a, EntityId.New());
            grid.Occupy(b, EntityId.New());
            RouteSystem routes = new RouteSystem(grid);
            List<GridPosition> path = new List<GridPosition> { a, b };
            routes.TrackLoad(null);
            routes.TrackLoad(new List<GridPosition>());
            Assert.IsFalse(routes.HasLoad(a));
            routes.TrackLoad(path);
            Assert.AreEqual(1, routes.LoadOf(a));
            Assert.AreEqual(1, routes.LoadOf(b));
            Assert.IsTrue(routes.HasLoad(a));
            Assert.AreEqual(0, routes.LoadOf(new GridPosition(5, 5)));
            routes.ClearLoads();
            Assert.AreEqual(0, routes.LoadOf(a));
            Assert.IsFalse(routes.HasLoad(a));
        }
    }
}
