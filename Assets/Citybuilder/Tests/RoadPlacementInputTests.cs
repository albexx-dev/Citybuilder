using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Домен drag-стройки дорог (EPIC-10 шаг 3): осевой отрезок, подсчёт новых клеток и валидация цены.
    public class RoadPlacementInputTests
    {
        [Test]
        public void GetLineCells_HorizontalTie_WalksAlongX()
        {
            List<GridPosition> cells = RoadPlacementInput.GetLineCells(new GridPosition(2, 3), new GridPosition(5, 4));
            Assert.AreEqual(4, cells.Count);
            Assert.AreEqual(new GridPosition(2, 3), cells[0]);
            Assert.AreEqual(new GridPosition(5, 3), cells[3]);
        }

        [Test]
        public void GetLineCells_VerticalDominantNegative_WalksAlongYBackwards()
        {
            List<GridPosition> cells = RoadPlacementInput.GetLineCells(new GridPosition(4, 6), new GridPosition(5, 3));
            Assert.AreEqual(4, cells.Count);
            Assert.AreEqual(new GridPosition(4, 6), cells[0]);
            Assert.AreEqual(new GridPosition(4, 3), cells[3]);
        }

        [Test]
        public void CountNewCells_SkipsOwnRoadCells()
        {
            List<GridPosition> cells = RoadPlacementInput.GetLineCells(new GridPosition(0, 0), new GridPosition(2, 0));
            int count = RoadPlacementInput.CountNewCells(cells, cell => cell.X == 1);
            Assert.AreEqual(2, count);
        }

        [Test]
        public void CanCommit_BlockedNonOwnCell_ReturnsFalse()
        {
            List<GridPosition> cells = RoadPlacementInput.GetLineCells(new GridPosition(0, 0), new GridPosition(2, 0));
            bool ok = RoadPlacementInput.CanCommit(cells, cell => cell.X != 1, cell => false, 10, new Treasury(100));
            Assert.IsFalse(ok);
        }

        [Test]
        public void CanCommit_OwnCellsFree_ChecksTreasuryForNewOnly()
        {
            List<GridPosition> cells = RoadPlacementInput.GetLineCells(new GridPosition(0, 0), new GridPosition(2, 0));
            bool enough = RoadPlacementInput.CanCommit(cells, cell => cell.X != 1, cell => cell.X == 1, 10, new Treasury(20));
            bool poor = RoadPlacementInput.CanCommit(cells, cell => cell.X != 1, cell => cell.X == 1, 10, new Treasury(19));
            Assert.IsTrue(enough);
            Assert.IsFalse(poor);
        }
    }
}
