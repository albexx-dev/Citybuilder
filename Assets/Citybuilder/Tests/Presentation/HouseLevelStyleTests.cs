using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // issue #94: масштабы и цвета крыш остаются прежними, маркер — отдельный стиль.
    public class HouseLevelStyleTests
    {
        [Test]
        public void LevelScale_KeepsCubeHeightAndPrefabFootprint()
        {
            Assert.AreEqual(1f, HouseLevelStyle.LevelScale(1, false), 0.0001f);
            Assert.AreEqual(1f, HouseLevelStyle.LevelScale(1, true), 0.0001f);
            Assert.AreEqual(1.25f, HouseLevelStyle.LevelScale(2, false), 0.0001f);
            Assert.AreEqual(1.5f, HouseLevelStyle.LevelScale(2, true), 0.0001f);
            Assert.AreEqual(1.5f, HouseLevelStyle.LevelScale(3, false), 0.0001f);
            Assert.AreEqual(2f, HouseLevelStyle.LevelScale(3, true), 0.0001f);
        }

        [Test]
        public void RoofColor_KeepsTerracottaAndBlueTiles()
        {
            Color baseColor = HouseLevelStyle.RoofColor(1, 0);
            Assert.AreEqual(0.45f, baseColor.r, 0.0001f);
            Assert.AreEqual(0.1f, baseColor.b, 0.0001f);

            Color terracotta = HouseLevelStyle.RoofColor(2, 0);
            Assert.IsTrue(terracotta.r > terracotta.g);
            Assert.AreEqual(0.2f, terracotta.b, 0.0001f);

            Color tiles = HouseLevelStyle.RoofColor(3, 0);
            Assert.IsTrue(tiles.b > tiles.r);
            Assert.AreEqual(0.2f, tiles.r, 0.0001f);
        }

        [Test]
        public void HasMarker_OnlyFromSecondLevel()
        {
            Assert.IsFalse(HouseLevelStyle.HasMarker(1));
            Assert.IsTrue(HouseLevelStyle.HasMarker(2));
            Assert.IsTrue(HouseLevelStyle.HasMarker(3));
        }

        [Test]
        public void MarkerColor_DiffersFromRoofColor()
        {
            for (int level = 2; level <= 3; level++)
            {
                Color marker = HouseLevelStyle.MarkerColor(level);
                Color roof = HouseLevelStyle.RoofColor(level, 0);
                Assert.IsTrue(marker.r != roof.r || marker.g != roof.g || marker.b != roof.b);
            }
        }
    }
}
