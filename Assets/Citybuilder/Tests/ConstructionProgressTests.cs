using NUnit.Framework;

namespace Citybuilder
{
    // Чистая математика прогресса стройки (EPIC-61): дублирует dotnet-прогон ConstructionProgress.
    public class ConstructionProgressTests
    {
        [Test]
        public void Progress_ZeroTotalMeansInstant()
        {
            Assert.AreEqual(1f, ConstructionProgress.Progress(0, 0));
            Assert.AreEqual(1f, ConstructionProgress.Progress(0, -3));
            Assert.IsTrue(ConstructionProgress.IsDone(0, 0));
        }

        [Test]
        public void Progress_ZeroElapsedIsZero()
        {
            Assert.AreEqual(0f, ConstructionProgress.Progress(0, 3));
            Assert.AreEqual(0f, ConstructionProgress.Progress(-2, 3));
            Assert.IsFalse(ConstructionProgress.IsDone(0, 3));
        }

        [Test]
        public void Progress_PartialTicksAreFractionOfTotal()
        {
            Assert.AreEqual(1f / 3f, ConstructionProgress.Progress(1, 3));
            Assert.AreEqual(2f / 3f, ConstructionProgress.Progress(2, 3));
            Assert.AreEqual(0.25f, ConstructionProgress.Progress(1, 4));
            Assert.IsFalse(ConstructionProgress.IsDone(2, 3));
        }

        [Test]
        public void Progress_ClampsAtTotal()
        {
            Assert.AreEqual(1f, ConstructionProgress.Progress(3, 3));
            Assert.AreEqual(1f, ConstructionProgress.Progress(10, 3));
            Assert.IsTrue(ConstructionProgress.IsDone(3, 3));
            Assert.IsTrue(ConstructionProgress.IsDone(10, 3));
        }

        [Test]
        public void ClampTicks_NegativeBecomesInstant()
        {
            Assert.AreEqual(0, ConstructionProgress.ClampTicks(-1));
            Assert.AreEqual(0, ConstructionProgress.ClampTicks(-99));
            Assert.AreEqual(0, ConstructionProgress.ClampTicks(0));
            Assert.AreEqual(5, ConstructionProgress.ClampTicks(5));
        }
    }
}
