using System;
using NUnit.Framework;

namespace Citybuilder
{
    // Проверки чистой логики сетки. Без Unity API, только NUnit.
    public class GridSystemTests
    {
        private static GridSystem MakeGrid()
        {
            return new GridSystem(new GridSize(10, 8), 2f);
        }

        [TestCase(0, 0, true)]
        [TestCase(9, 7, true)]
        [TestCase(10, 7, false)]
        [TestCase(9, 8, false)]
        [TestCase(-1, 0, false)]
        [TestCase(0, -1, false)]
        public void IsInside_RespectsBorders(int x, int y, bool expected)
        {
            Assert.AreEqual(expected, MakeGrid().IsInside(new GridPosition(x, y)));
        }

        [Test]
        public void Occupy_FreeCell_MarksOccupied()
        {
            GridSystem grid = MakeGrid();
            GridPosition cell = new GridPosition(3, 4);
            Assert.IsFalse(grid.IsOccupied(cell));
            Assert.IsTrue(grid.Occupy(cell, EntityId.New()));
            Assert.IsTrue(grid.IsOccupied(cell));
            Assert.AreEqual(1, grid.OccupiedCount);
        }

        [Test]
        public void Occupy_Twice_SecondFails()
        {
            GridSystem grid = MakeGrid();
            GridPosition cell = new GridPosition(3, 4);
            Assert.IsTrue(grid.Occupy(cell, EntityId.New()));
            Assert.IsFalse(grid.Occupy(cell, EntityId.New()));
            Assert.AreEqual(1, grid.OccupiedCount);
        }

        [Test]
        public void Release_FreesCell()
        {
            GridSystem grid = MakeGrid();
            GridPosition cell = new GridPosition(3, 4);
            grid.Occupy(cell, EntityId.New());
            grid.Release(cell);
            Assert.IsFalse(grid.IsOccupied(cell));
            Assert.AreEqual(0, grid.OccupiedCount);
        }

        [Test]
        public void IsOccupied_OutsideMap_ReturnsTrue()
        {
            // Вне карты стройка запрещена: клетка считается занятой.
            GridSystem grid = MakeGrid();
            Assert.IsTrue(grid.IsOccupied(new GridPosition(10, 0)));
            Assert.IsTrue(grid.IsOccupied(new GridPosition(-1, -1)));
        }

        [TestCase(3.9f, 5.1f, 1, 2)]
        [TestCase(0f, 0f, 0, 0)]
        [TestCase(4f, 6f, 2, 3)]
        [TestCase(-0.5f, -0.5f, -1, -1)]
        public void WorldToGrid_TruncatesFraction(float worldX, float worldZ, int x, int y)
        {
            Assert.AreEqual(new GridPosition(x, y), MakeGrid().WorldToGrid(worldX, worldZ));
        }

        [Test]
        public void GridToWorld_ReturnsCellCenter()
        {
            GridSystem grid = MakeGrid();
            float worldX;
            float worldZ;
            grid.GridToWorld(new GridPosition(1, 2), out worldX, out worldZ);
            Assert.AreEqual(3f, worldX, 1e-5f);
            Assert.AreEqual(5f, worldZ, 1e-5f);
        }

        [TestCase(0, 0)]
        [TestCase(5, 3)]
        [TestCase(9, 7)]
        public void WorldToGrid_GridToWorld_Roundtrip(int x, int y)
        {
            GridSystem grid = MakeGrid();
            GridPosition cell = new GridPosition(x, y);
            float worldX;
            float worldZ;
            grid.GridToWorld(cell, out worldX, out worldZ);
            Assert.AreEqual(cell, grid.WorldToGrid(worldX, worldZ));
        }

        [TestCase(0f)]
        [TestCase(-2f)]
        public void Ctor_NonPositiveCellSize_Throws(float cellSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridSystem(new GridSize(10, 8), cellSize));
        }
    }
}
