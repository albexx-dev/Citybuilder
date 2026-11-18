using NUnit.Framework;

namespace Citybuilder
{
    // Математика графиков (EPIC-68 шаг 1): прореживание с сохранением экстремумов, оси, нормализация.
    public class GraphMathTests
    {
        private readonly float[] output = new float[GraphMath.DefaultMaxPoints];

        private static float[] Ramp(int count)
        {
            float[] values = new float[count];
            for (int i = 0; i < count; i++) values[i] = i;
            return values;
        }

        [Test]
        public void Decimate_192Points_FitsLimitAndKeepsExtremes()
        {
            float[] values = Ramp(192);
            values[97] = -500f; // всплеск вниз между выбранными точками
            values[150] = 900f;
            int written = GraphMath.Decimate(values, values.Length, output, GraphMath.DefaultMaxPoints);
            Assert.LessOrEqual(written, GraphMath.DefaultMaxPoints);
            float min, max;
            GraphMath.Axes(output, written, out min, out max);
            Assert.AreEqual(-500f, min, 0.001f);
            Assert.AreEqual(900f, max, 0.001f);
        }

        [Test]
        public void Decimate_SeriesShorterThanLimit_KeepsEveryPoint()
        {
            float[] values = { 3f, 1f, 4f, 1f, 5f };
            int written = GraphMath.Decimate(values, values.Length, output, GraphMath.DefaultMaxPoints);
            Assert.AreEqual(5, written);
            for (int i = 0; i < values.Length; i++) Assert.AreEqual(values[i], output[i], 0.001f);
        }

        [Test]
        public void Decimate_EmptyOrNullSeries_WritesNothing()
        {
            Assert.AreEqual(0, GraphMath.Decimate(null, 0, output, 64));
            Assert.AreEqual(0, GraphMath.Decimate(new float[0], 0, output, 64));
            Assert.AreEqual(0, GraphMath.Decimate(Ramp(4), 0, output, 64));
            Assert.AreEqual(0, GraphMath.Decimate(Ramp(4), 4, null, 64));
        }

        [Test]
        public void Decimate_CountBeyondArray_ClampsToLength()
        {
            float[] values = Ramp(8);
            int written = GraphMath.Decimate(values, 999, output, 64);
            Assert.AreEqual(8, written);
            Assert.AreEqual(7f, output[7], 0.001f);
        }

        [Test]
        public void Decimate_IsDeterministic_AndReusesBuffer()
        {
            float[] values = Ramp(192);
            int first = GraphMath.Decimate(values, values.Length, output, 64);
            float[] copy = new float[first];
            for (int i = 0; i < first; i++) copy[i] = output[i];
            int second = GraphMath.Decimate(values, values.Length, output, 64);
            Assert.AreEqual(first, second);
            for (int i = 0; i < first; i++) Assert.AreEqual(copy[i], output[i], 0.001f);
        }

        [Test]
        public void Decimate_BufferShorterThanLimit_DoesNotOverflow()
        {
            float[] small = new float[4];
            int written = GraphMath.Decimate(Ramp(192), 192, small, 64);
            Assert.AreEqual(4, written);
        }

        [Test]
        public void Axes_NegativeBalance_SpansBelowZero()
        {
            float[] values = { -120f, -40f, 30f };
            float min, max;
            GraphMath.Axes(values, values.Length, out min, out max);
            Assert.AreEqual(-120f, min, 0.001f);
            Assert.AreEqual(30f, max, 0.001f);
        }

        [Test]
        public void Axes_FlatLine_DoesNotCollapseSpan()
        {
            float[] values = { 7f, 7f, 7f };
            float min, max;
            GraphMath.Axes(values, values.Length, out min, out max);
            Assert.Greater(max - min, 0f);
            float normalized = GraphMath.Normalize(7f, min, max);
            Assert.IsFalse(float.IsNaN(normalized));
            Assert.AreEqual(0.5f, normalized, 0.001f);
        }

        [Test]
        public void Axes_EmptyOrAllNaN_AreSafeZeroes()
        {
            float min, max;
            GraphMath.Axes(null, 0, out min, out max);
            Assert.AreEqual(0f, min, 0.001f); Assert.AreEqual(0f, max, 0.001f);
            float[] nan = { float.NaN, float.NaN };
            GraphMath.Axes(nan, nan.Length, out min, out max);
            Assert.AreEqual(0f, min, 0.001f);
            Assert.AreEqual(0f, max, 0.001f);
        }

        [Test]
        public void Axes_SkipsNaN_AndKeepsRealValues()
        {
            float[] values = { float.NaN, 12f, 4f };
            float min, max;
            GraphMath.Axes(values, values.Length, out min, out max);
            Assert.AreEqual(4f, min, 0.001f);
            Assert.AreEqual(12f, max, 0.001f);
        }

        [Test]
        public void Normalize_EdgesGiveZeroAndOne_OutsideClamps()
        {
            Assert.AreEqual(0f, GraphMath.Normalize(10f, 10f, 20f), 0.001f);
            Assert.AreEqual(1f, GraphMath.Normalize(20f, 10f, 20f), 0.001f);
            Assert.AreEqual(0.5f, GraphMath.Normalize(15f, 10f, 20f), 0.001f);
            Assert.AreEqual(0f, GraphMath.Normalize(-5f, 10f, 20f), 0.001f); Assert.AreEqual(1f, GraphMath.Normalize(99f, 10f, 20f), 0.001f);
        }

        [Test]
        public void Normalize_DegenerateOrNaNInput_IsFinite()
        {
            Assert.AreEqual(0.5f, GraphMath.Normalize(3f, 3f, 3f), 0.001f);
            Assert.AreEqual(0f, GraphMath.Normalize(float.NaN, 0f, 10f), 0.001f);
            Assert.AreEqual(0.5f, GraphMath.Normalize(3f, float.NaN, 10f), 0.001f);
        }

        [Test]
        public void Decimate_NaNInSeries_BecomesZeroWithoutBreakingExtremes()
        {
            float[] values = { 5f, float.NaN, 40f };
            int written = GraphMath.Decimate(values, values.Length, output, 64);
            Assert.AreEqual(3, written);
            Assert.AreEqual(0f, output[1], 0.001f);
            Assert.AreEqual(40f, output[2], 0.001f);
        }
    }
}