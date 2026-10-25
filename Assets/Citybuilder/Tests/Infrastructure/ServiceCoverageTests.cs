using NUnit.Framework;

namespace Citybuilder
{
    // Проверки покрытия служб: источники с радиусом и точки спроса. Чистый C#, без Unity API.
    public class ServiceCoverageTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Empty_DefaultsToCoveredNothing()
        {
            ServiceCoverage coverage = new ServiceCoverage();
            Assert.AreEqual(0, coverage.SourceCount);
            Assert.AreEqual(0, coverage.PointCount);
            Assert.AreEqual(0, coverage.CoveredPoints);
            Assert.AreEqual(1f, coverage.Coverage01, Tolerance);
            Assert.AreEqual(-1, coverage.NearestDistance(new GridPosition(0, 0)));
            Assert.AreEqual(0f, coverage.RateOf(EntityId.New()), Tolerance);
            Assert.IsFalse(coverage.IsCovered(new GridPosition(0, 0)));
        }

        [Test]
        public void RadiusZero_CoversOnlyOwnCell()
        {
            ServiceCoverage coverage = new ServiceCoverage();
            coverage.SetSource(EntityId.New(), new GridPosition(2, 3), 0);
            Assert.IsTrue(coverage.IsCovered(new GridPosition(2, 3)));
            Assert.IsFalse(coverage.IsCovered(new GridPosition(3, 3)));
            Assert.IsFalse(coverage.IsCovered(new GridPosition(2, 4)));

            // Отрицательный радиус — тоже 0: своя клетка накрыта, соседняя нет.
            coverage.SetSource(EntityId.New(), new GridPosition(10, 10), -5);
            Assert.IsTrue(coverage.IsCovered(new GridPosition(10, 10)));
            Assert.IsFalse(coverage.IsCovered(new GridPosition(11, 10)));
        }

        [Test]
        public void RadiusBoundary_EdgeInsideNextStepOutside()
        {
            ServiceCoverage horizontal = new ServiceCoverage();
            horizontal.SetSource(EntityId.New(), new GridPosition(0, 0), 3);
            Assert.IsTrue(horizontal.IsCovered(new GridPosition(3, 0)));
            Assert.IsFalse(horizontal.IsCovered(new GridPosition(4, 0)));

            ServiceCoverage diagonal = new ServiceCoverage();
            diagonal.SetSource(EntityId.New(), new GridPosition(0, 0), 5);
            Assert.IsTrue(diagonal.IsCovered(new GridPosition(3, 4)));
            Assert.IsFalse(diagonal.IsCovered(new GridPosition(4, 4)));
        }

        [Test]
        public void TwoSources_UnionWithoutDoubleCount()
        {
            ServiceCoverage coverage = new ServiceCoverage();
            coverage.SetSource(EntityId.New(), new GridPosition(0, 0), 2);
            coverage.SetSource(EntityId.New(), new GridPosition(2, 0), 2);
            coverage.SetPoint(EntityId.New(), new GridPosition(1, 0));
            coverage.SetPoint(EntityId.New(), new GridPosition(-2, 0));
            coverage.SetPoint(EntityId.New(), new GridPosition(10, 0));
            Assert.AreEqual(2, coverage.CoveredPoints);
            Assert.AreEqual(2f / 3f, coverage.Coverage01, Tolerance);
        }

        [Test]
        public void RemoveSource_AndClearSources_DropCoverageButKeepPoints()
        {
            EntityId station = EntityId.New();
            ServiceCoverage coverage = new ServiceCoverage();
            coverage.SetSource(station, new GridPosition(0, 0), 1);
            coverage.SetPoint(EntityId.New(), new GridPosition(0, 0));
            coverage.SetPoint(EntityId.New(), new GridPosition(5, 5));

            coverage.RemoveSource(station);
            Assert.AreEqual(0, coverage.SourceCount);
            Assert.AreEqual(2, coverage.PointCount);
            Assert.AreEqual(0, coverage.CoveredPoints);

            coverage.SetSource(station, new GridPosition(0, 0), 1);
            coverage.SetSource(EntityId.New(), new GridPosition(5, 5), 1);
            Assert.AreEqual(2, coverage.CoveredPoints);

            coverage.ClearSources();
            Assert.AreEqual(0, coverage.SourceCount);
            Assert.AreEqual(2, coverage.PointCount);
            Assert.AreEqual(0f, coverage.Coverage01, Tolerance);
        }

        [Test]
        public void Coverage01_ZeroHalfAndFull()
        {
            ServiceCoverage none = new ServiceCoverage();
            none.SetSource(EntityId.New(), new GridPosition(100, 100), 1);
            none.SetPoint(EntityId.New(), new GridPosition(0, 0));
            none.SetPoint(EntityId.New(), new GridPosition(1, 0));
            none.SetPoint(EntityId.New(), new GridPosition(2, 0));
            Assert.AreEqual(0f, none.Coverage01, Tolerance);

            ServiceCoverage half = new ServiceCoverage();
            half.SetSource(EntityId.New(), new GridPosition(0, 0), 1);
            half.SetPoint(EntityId.New(), new GridPosition(0, 0));
            half.SetPoint(EntityId.New(), new GridPosition(1, 0));
            half.SetPoint(EntityId.New(), new GridPosition(2, 0));
            half.SetPoint(EntityId.New(), new GridPosition(3, 0));
            Assert.AreEqual(0.5f, half.Coverage01, Tolerance);
            Assert.AreEqual(2, half.CoveredPoints);

            ServiceCoverage full = new ServiceCoverage();
            full.SetSource(EntityId.New(), new GridPosition(0, 0), 10);
            full.SetPoint(EntityId.New(), new GridPosition(0, 0));
            full.SetPoint(EntityId.New(), new GridPosition(3, 4));
            full.SetPoint(EntityId.New(), new GridPosition(-6, 8));
            Assert.AreEqual(1f, full.Coverage01, Tolerance);
        }

        [Test]
        public void RemovePoint_UpdatesCountsAndCoverage()
        {
            EntityId home = EntityId.New();
            EntityId shop = EntityId.New();
            ServiceCoverage coverage = new ServiceCoverage();
            coverage.SetSource(EntityId.New(), new GridPosition(0, 0), 1);
            coverage.SetPoint(home, new GridPosition(0, 0));
            coverage.SetPoint(shop, new GridPosition(5, 5));
            Assert.AreEqual(0.5f, coverage.Coverage01, Tolerance);

            coverage.RemovePoint(shop);
            Assert.AreEqual(1, coverage.PointCount);
            Assert.AreEqual(1f, coverage.Coverage01, Tolerance);
            Assert.AreEqual(1f, coverage.RateOf(home), Tolerance);
            Assert.AreEqual(0f, coverage.RateOf(shop), Tolerance);

            coverage.Clear();
            Assert.AreEqual(0, coverage.PointCount);
            Assert.AreEqual(1f, coverage.Coverage01, Tolerance);
        }

        [Test]
        public void NearestDistance_FloorsAndPicksMinimum()
        {
            ServiceCoverage coverage = new ServiceCoverage();
            coverage.SetSource(EntityId.New(), new GridPosition(0, 0), 1);
            coverage.SetSource(EntityId.New(), new GridPosition(10, 0), 1);
            Assert.AreEqual(5, coverage.NearestDistance(new GridPosition(3, 4)));
            Assert.AreEqual(0, coverage.NearestDistance(new GridPosition(10, 0)));
            Assert.AreEqual(1, coverage.NearestDistance(new GridPosition(1, 1)));
        }
    }
}
