using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Чистая математика земли и рамки (EPIC-57, шаг ground): размеры от сетки,
    // рамка шире карты на 6 м с каждой стороны, детерминизм оттенков и пятен.
    public class GridGroundBuilderTests
    {
        [Test]
        public void SideLength_FiftyCellsOneMeter_GivesFifty()
        {
            Assert.AreEqual(50f, GridGroundBuilder.SideLength(50, 1f), 1e-4f);
            Assert.AreEqual(25f, GridGroundBuilder.SideLength(50, 0.5f), 1e-4f);
        }

        [Test]
        public void GroundBounds_MatchesGridAndGroundY()
        {
            Vector3 min, max;
            GridGroundBuilder.GroundBounds(50, 50, 1f, GridGroundBuilder.GroundY, out min, out max);
            Assert.AreEqual(new Vector3(0f, -0.02f, 0f), min);
            Assert.AreEqual(new Vector3(50f, -0.02f, 50f), max);
        }

        [Test]
        public void BorderBounds_IsSixMetersWiderPerSide()
        {
            Vector3 min, max;
            GridGroundBuilder.BorderBounds(50, 50, 1f, GridGroundBuilder.BorderY, out min, out max);
            Assert.AreEqual(new Vector3(-6f, -0.05f, -6f), min);
            Assert.AreEqual(new Vector3(56f, -0.05f, 56f), max);
        }

        [Test]
        public void BorderBounds_TotalSideIsSixtyTwo()
        {
            Vector3 min, max;
            GridGroundBuilder.BorderBounds(50, 50, 1f, 0f, out min, out max);
            Assert.AreEqual(62f, max.x - min.x, 1e-4f);
            Assert.AreEqual(62f, max.z - min.z, 1e-4f);
        }

        [Test]
        public void PatchRect_StaysInsideGround()
        {
            for (int i = 0; i < GridGroundBuilder.PatchCount; i++)
            {
                Vector3 min, max;
                GridGroundBuilder.PatchRect(i, 50f, out min, out max);
                Assert.GreaterOrEqual(min.x, 0f);
                Assert.GreaterOrEqual(min.z, 0f);
                Assert.LessOrEqual(max.x, 50f);
                Assert.LessOrEqual(max.z, 50f);
                Assert.Less(max.x - min.x, 50f);
                Assert.AreEqual(GridGroundBuilder.PatchY, min.y, 1e-6f);
            }
        }

        [Test]
        public void PatchRect_IsDeterministic()
        {
            Vector3 minA, maxA, minB, maxB;
            GridGroundBuilder.PatchRect(2, 50f, out minA, out maxA);
            GridGroundBuilder.PatchRect(2, 50f, out minB, out maxB);
            Assert.AreEqual(minA, minB);
            Assert.AreEqual(maxA, maxB);
        }

        [Test]
        public void ShadeIndex_IsDeterministicAndInRange()
        {
            for (int i = 0; i < 32; i++)
            {
                int first = GridGroundBuilder.ShadeIndex(i, GridGroundBuilder.ShadeCount);
                Assert.AreEqual(first, GridGroundBuilder.ShadeIndex(i, GridGroundBuilder.ShadeCount));
                Assert.GreaterOrEqual(first, 0);
                Assert.Less(first, GridGroundBuilder.ShadeCount);
            }
        }
    }
}
