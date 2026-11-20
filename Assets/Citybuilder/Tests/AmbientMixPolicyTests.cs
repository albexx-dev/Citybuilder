using NUnit.Framework;

namespace Citybuilder.Tests
{
    // Микс эмбиента (EPIC-71): края 6/9/18/22, линейность, границы громкостей,
    // масштаб от активности города и гарды часа — по образцу DayNightPolicyTests.
    public class AmbientMixPolicyTests
    {
        private const float Delta = 0.001f;

        [Test]
        public void NoonAtFullActivity_PlaysDayOnly()
        {
            AmbientMixSample mix = AmbientMixPolicy.Sample(12f, 1f);
            Assert.AreEqual(AmbientMixPolicy.DayBaseVolume, mix.DayVolume, Delta);
            Assert.AreEqual(0f, mix.NightVolume, Delta);
        }

        [Test]
        public void MidnightAtFullActivity_PlaysNightOnly()
        {
            AmbientMixSample mix = AmbientMixPolicy.Sample(0f, 1f);
            Assert.AreEqual(0f, mix.DayVolume, Delta);
            Assert.AreEqual(AmbientMixPolicy.NightBaseVolume, mix.NightVolume, Delta);
        }

        [Test]
        public void KeyEdges_MatchDayNightKeyPoints()
        {
            Assert.AreEqual(0f, AmbientMixPolicy.DayMix(6f), Delta);
            Assert.AreEqual(1f, AmbientMixPolicy.DayMix(9f), Delta);
            Assert.AreEqual(1f, AmbientMixPolicy.DayMix(18f), Delta);
            Assert.AreEqual(0f, AmbientMixPolicy.DayMix(22f), Delta);
        }

        [Test]
        public void DawnAndDuskMiddle_InterpolateLinearly()
        {
            Assert.AreEqual(0.5f, AmbientMixPolicy.DayMix(7.5f), Delta);
            Assert.AreEqual(0.75f, AmbientMixPolicy.DayMix(8.25f), Delta);
            Assert.AreEqual(0.5f, AmbientMixPolicy.DayMix(20f), Delta);
        }

        [Test]
        public void WholeDay_KeepsVolumesInUnitRange()
        {
            for (float hour = 0f; hour < AmbientMixPolicy.HoursPerDay; hour += 0.25f)
            {
                AmbientMixSample mix = AmbientMixPolicy.Sample(hour, 1f);
                Assert.GreaterOrEqual(mix.DayVolume, 0f);
                Assert.LessOrEqual(mix.DayVolume, 1f);
                Assert.GreaterOrEqual(mix.NightVolume, 0f);
                Assert.LessOrEqual(mix.NightVolume, 1f);
            }
        }

        [Test]
        public void ZeroActivity_ScalesBothVolumesDown()
        {
            AmbientMixSample noon = AmbientMixPolicy.Sample(12f, 0f);
            Assert.AreEqual(AmbientMixPolicy.DayBaseVolume * AmbientMixPolicy.MinActivityScale, noon.DayVolume, Delta);
            AmbientMixSample midnight = AmbientMixPolicy.Sample(0f, 0f);
            Assert.AreEqual(AmbientMixPolicy.NightBaseVolume * AmbientMixPolicy.MinActivityScale, midnight.NightVolume, Delta);
        }

        [Test]
        public void Activity_IsClampedToUnitRange()
        {
            AmbientMixSample high = AmbientMixPolicy.Sample(12f, 2.5f);
            Assert.AreEqual(AmbientMixPolicy.DayBaseVolume, high.DayVolume, Delta);
            AmbientMixSample negative = AmbientMixPolicy.Sample(0f, -1f);
            Assert.AreEqual(AmbientMixPolicy.NightBaseVolume * AmbientMixPolicy.MinActivityScale, negative.NightVolume, Delta);
        }

        [Test]
        public void NormalizeHour_WrapsNegativeAndOverflow()
        {
            Assert.AreEqual(22f, AmbientMixPolicy.NormalizeHour(-2f), Delta);
            Assert.AreEqual(2f, AmbientMixPolicy.NormalizeHour(26f), Delta);
            Assert.AreEqual(0f, AmbientMixPolicy.NormalizeHour(24f), Delta);
            Assert.AreEqual(AmbientMixPolicy.NoonHour, AmbientMixPolicy.NormalizeHour(float.NaN), Delta);
            Assert.AreEqual(AmbientMixPolicy.NoonHour, AmbientMixPolicy.NormalizeHour(float.PositiveInfinity), Delta);
        }

        [Test]
        public void SameHourAndActivity_IsDeterministic()
        {
            AmbientMixSample first = AmbientMixPolicy.Sample(15f, 0.6f);
            AmbientMixSample second = AmbientMixPolicy.Sample(15f, 0.6f);
            Assert.AreEqual(first.DayVolume, second.DayVolume, Delta);
            Assert.AreEqual(first.NightVolume, second.NightVolume, Delta);
        }
    }
}
