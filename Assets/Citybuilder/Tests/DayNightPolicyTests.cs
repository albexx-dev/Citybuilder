using NUnit.Framework;

namespace Citybuilder
{
    // Фазовая математика дня и ночи (EPIC-58): точки 6/12/18/24 детерминированы,
    // интерполяция между точками линейна, параметры в границах, гард NaN.
    public class DayNightPolicyTests
    {
        private const float Delta = 0.001f;

        [Test]
        public void DawnPhase_GivesWarmRisingSun()
        {
            DayNightSample sample = DayNightPolicy.Sample(6f);
            Assert.AreEqual(12f, sample.SunPitch, Delta);
            Assert.AreEqual(0.55f, sample.SunIntensity, Delta);
            Assert.AreEqual(-90f, sample.SunAzimuth, Delta);
            Assert.AreEqual(0.10f, sample.MoonIntensity, Delta);
            Assert.Greater(sample.SunColorR, sample.SunColorB);
        }

        [Test]
        public void NoonPhase_GivesPeakWarmLight()
        {
            DayNightSample sample = DayNightPolicy.Sample(12f);
            Assert.AreEqual(58f, sample.SunPitch, Delta);
            Assert.AreEqual(DayNightPolicy.MaxSunIntensity, sample.SunIntensity, Delta);
            Assert.AreEqual(0f, sample.SunAzimuth, Delta);
            Assert.AreEqual(0f, sample.MoonIntensity, Delta);
            Assert.Greater(sample.AmbientR, sample.AmbientB);
        }

        [Test]
        public void SunsetPhase_GivesOrangeLowSun()
        {
            DayNightSample sample = DayNightPolicy.Sample(18f);
            Assert.AreEqual(14f, sample.SunPitch, Delta);
            Assert.AreEqual(0.50f, sample.SunIntensity, Delta);
            Assert.AreEqual(90f, sample.SunAzimuth, Delta);
            Assert.AreEqual(0.05f, sample.MoonIntensity, Delta);
            Assert.Greater(sample.SunColorR, sample.SunColorB);
        }

        [Test]
        public void MidnightAt24_MatchesHourZeroAndIsColdDark()
        {
            DayNightSample at24 = DayNightPolicy.Sample(24f);
            DayNightSample at0 = DayNightPolicy.Sample(0f);
            AssertSameSample(at0, at24);
            Assert.AreEqual(DayNightPolicy.MinSunIntensity, at0.SunIntensity, Delta);
            Assert.AreEqual(0.30f, at0.MoonIntensity, Delta);
            Assert.AreEqual(-180f, at0.SunAzimuth, Delta);
            Assert.Greater(at0.AmbientB, at0.AmbientR);
            Assert.Less(at0.AmbientB, DayNightPolicy.Sample(12f).AmbientB);
        }

        [Test]
        public void MidwayBetweenDawnAndMorning_InterpolatesLinearly()
        {
            DayNightSample sample = DayNightPolicy.Sample(7.5f);
            Assert.AreEqual(22f, sample.SunPitch, Delta);
            Assert.AreEqual(0.75f, sample.SunIntensity, Delta);
            Assert.AreEqual(-67.5f, sample.SunAzimuth, Delta);
            Assert.AreEqual(0.05f, sample.MoonIntensity, Delta);
        }

        [Test]
        public void WholeDay_KeepsParametersInBounds()
        {
            for (float hour = 0f; hour < DayNightPolicy.HoursPerDay; hour += 0.25f)
            {
                DayNightSample sample = DayNightPolicy.Sample(hour);
                Assert.GreaterOrEqual(sample.SunPitch, 0f);
                Assert.Less(sample.SunPitch, 90f);
                Assert.GreaterOrEqual(sample.SunIntensity, DayNightPolicy.MinSunIntensity);
                Assert.LessOrEqual(sample.SunIntensity, DayNightPolicy.MaxSunIntensity);
                Assert.GreaterOrEqual(sample.MoonIntensity, 0f);
                Assert.LessOrEqual(sample.MoonIntensity, DayNightPolicy.MaxMoonIntensity);
                AssertColorInBounds(sample.SunColorR, sample.SunColorG, sample.SunColorB);
                AssertColorInBounds(sample.AmbientR, sample.AmbientG, sample.AmbientB);
            }
        }

        [Test]
        public void NanAndInfinity_FallBackToNoon()
        {
            Assert.AreEqual(DayNightPolicy.NoonHour, DayNightPolicy.NormalizeHour(float.NaN), Delta);
            Assert.AreEqual(DayNightPolicy.NoonHour, DayNightPolicy.NormalizeHour(float.PositiveInfinity), Delta);
            Assert.AreEqual(DayNightPolicy.NoonHour, DayNightPolicy.NormalizeHour(float.NegativeInfinity), Delta);
            AssertSameSample(DayNightPolicy.Noon(), DayNightPolicy.Sample(float.NaN));
        }

        [Test]
        public void NormalizeHour_WrapsNegativeAndOverflow()
        {
            Assert.AreEqual(22f, DayNightPolicy.NormalizeHour(-2f), Delta);
            Assert.AreEqual(2f, DayNightPolicy.NormalizeHour(26f), Delta);
            Assert.AreEqual(0f, DayNightPolicy.NormalizeHour(24f), Delta);
            Assert.AreEqual(0f, DayNightPolicy.NormalizeHour(48f), Delta);
        }

        [Test]
        public void SameHour_IsDeterministic()
        {
            DayNightSample first = DayNightPolicy.Sample(15f);
            DayNightSample second = DayNightPolicy.Sample(15f);
            AssertSameSample(first, second);
        }

        private static void AssertColorInBounds(float r, float g, float b)
        {
            Assert.GreaterOrEqual(r, 0f);
            Assert.LessOrEqual(r, 1f);
            Assert.GreaterOrEqual(g, 0f);
            Assert.LessOrEqual(g, 1f);
            Assert.GreaterOrEqual(b, 0f);
            Assert.LessOrEqual(b, 1f);
        }

        private static void AssertSameSample(DayNightSample expected, DayNightSample actual)
        {
            Assert.AreEqual(expected.SunPitch, actual.SunPitch, Delta);
            Assert.AreEqual(expected.SunAzimuth, actual.SunAzimuth, Delta);
            Assert.AreEqual(expected.SunIntensity, actual.SunIntensity, Delta);
            Assert.AreEqual(expected.MoonIntensity, actual.MoonIntensity, Delta);
            Assert.AreEqual(expected.SunColorR, actual.SunColorR, Delta);
            Assert.AreEqual(expected.SunColorG, actual.SunColorG, Delta);
            Assert.AreEqual(expected.SunColorB, actual.SunColorB, Delta);
            Assert.AreEqual(expected.AmbientR, actual.AmbientR, Delta);
            Assert.AreEqual(expected.AmbientG, actual.AmbientG, Delta);
            Assert.AreEqual(expected.AmbientB, actual.AmbientB, Delta);
        }
    }
}
