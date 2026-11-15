using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Симметрия счётчиков без сетки (задача 183): CongestedCellsCount учитывает recordedLoads,
    // как WorstMultiplier, а не возвращает ноль при grid == null.
    public class CongestionRecordedLoadsTests
    {
        private static CongestionSystem MakeSystemWithoutGrid()
        {
            return new CongestionSystem((Func<GridPosition, int>)null, null, 4, 0.25f, 3f);
        }

        [Test]
        public void CongestedCellsCount_WithoutGrid_CountsRecordedLoads()
        {
            CongestionSystem congestion = MakeSystemWithoutGrid();
            List<GridPosition> trip = new List<GridPosition> { new GridPosition(2, 2) };
            for (int i = 0; i < 4; i++) congestion.TrackTrip(trip);

            Assert.AreEqual(1, congestion.CongestedCellsCount());
            Assert.Greater(congestion.WorstMultiplier(), 1f);
        }

        [Test]
        public void CongestedCellsCount_WithoutGrid_SubThresholdLoad_CountsNone()
        {
            CongestionSystem congestion = MakeSystemWithoutGrid();
            List<GridPosition> trip = new List<GridPosition> { new GridPosition(1, 1) };
            congestion.TrackTrip(trip);

            Assert.AreEqual(0, congestion.CongestedCellsCount());
            Assert.AreEqual(1f, congestion.WorstMultiplier());
        }

        [Test]
        public void CongestedCellsCount_WithoutGrid_EmptyLoads_Zero()
        {
            CongestionSystem congestion = MakeSystemWithoutGrid();

            Assert.AreEqual(0, congestion.CongestedCellsCount());
            Assert.AreEqual(1f, congestion.WorstMultiplier());
        }
    }
}
