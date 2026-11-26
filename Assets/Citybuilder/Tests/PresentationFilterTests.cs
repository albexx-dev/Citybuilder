using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // ADR-021 / issue #170: presentation-фильтр — чистая математика без Unity.
    // Проверяем детерминизм, границы и то, что доменные поля не меняются.
    public class PresentationFilterTests
    {
        private const float Delta = 0.001f;

        [Test]
        public void SeasonZero_KeepsNeutralTintAndBaseLight()
        {
            DayNightSample plain = DayNightPolicy.Sample(12f);
            DayNightSample tinted = DayNightPolicy.Sample(12f, 0);
            Assert.AreEqual(plain.SunPitch, tinted.SunPitch, Delta);
            Assert.AreEqual(plain.SunIntensity, tinted.SunIntensity, Delta);
            Assert.AreEqual(plain.AmbientB, tinted.AmbientB, Delta);
            Assert.AreEqual(1f, tinted.SeasonTintR, Delta);
            Assert.AreEqual(1f, tinted.SeasonTintG, Delta);
            Assert.AreEqual(1f, tinted.SeasonTintB, Delta);
        }

        [Test]
        public void SeasonIndex_WrapsAndIsDeterministic()
        {
            AssertSameFog(DayNightPolicy.Sample(9f, 3), DayNightPolicy.Sample(9f, -1));
            AssertSameFog(DayNightPolicy.Sample(9f, 0), DayNightPolicy.Sample(9f, 4));
            AssertSameFog(DayNightPolicy.Sample(9f, 2), DayNightPolicy.Sample(9f, 2));
        }

        [Test]
        public void FogDensity_IsDenserAtNightAndInWinter()
        {
            float night = DayNightPolicy.Sample(0f, 0).FogDensity;
            float noon = DayNightPolicy.Sample(12f, 0).FogDensity;
            Assert.Greater(night, noon);
            Assert.Greater(DayNightPolicy.Sample(12f, 3).FogDensity, DayNightPolicy.Sample(12f, 1).FogDensity);
        }

        [Test]
        public void FogColor_IsColdAtNightAndTintedBySeason()
        {
            DayNightSample night = DayNightPolicy.Sample(0f, 0);
            DayNightSample noon = DayNightPolicy.Sample(12f, 0);
            Assert.Greater(night.FogB, night.FogR);
            Assert.Greater(noon.FogR, night.FogR);
            Assert.Greater(DayNightPolicy.Sample(12f, 2).FogR, DayNightPolicy.Sample(12f, 3).FogR);
        }

        [Test]
        public void WholeCycleAndSeasons_KeepPresentationFieldsInBounds()
        {
            for (int season = 0; season < DayNightPolicy.SeasonCount; season++)
            {
                for (float hour = 0f; hour < DayNightPolicy.HoursPerDay; hour += 1f)
                {
                    DayNightSample sample = DayNightPolicy.Sample(hour, season);
                    Assert.GreaterOrEqual(sample.FogDensity, DayNightPolicy.MinFogDensity);
                    Assert.LessOrEqual(sample.FogDensity, DayNightPolicy.MaxFogDensity);
                    AssertColorInBounds(sample.FogR, sample.FogG, sample.FogB);
                    AssertTintInBounds(sample.SeasonTintR, sample.SeasonTintG, sample.SeasonTintB);
                }
            }
        }

        [Test]
        public void TimeColor_MarksNightDawnDayAndDusk()
        {
            Color night = HudText.TimeColor(2);
            Color dawn = HudText.TimeColor(7);
            Color day = HudText.TimeColor(12);
            Color dusk = HudText.TimeColor(19);
            Assert.Greater(night.b, night.r);
            Assert.Greater(dawn.r, dawn.b);
            Assert.AreEqual(1f, day.r, Delta);
            Assert.Greater(dusk.r, dusk.b);
            Assert.AreEqual(night.r, HudText.TimeColor(26).r, Delta);
        }

        [Test]
        public void PulseColor_IsBoundedAndDeterministic()
        {
            for (int i = 0; i < 20; i++)
            {
                float t = i * 0.37f;
                float gain = HudText.PulseGain(t);
                Assert.GreaterOrEqual(gain, 0f);
                Assert.LessOrEqual(gain, 1f);
                Assert.AreEqual(gain, HudText.PulseGain(t), Delta);
                Color color = HudText.PulseColor(t);
                Assert.GreaterOrEqual(color.g, 0.78f);
                Assert.LessOrEqual(color.g, 1f);
            }
            Assert.Greater(System.Math.Abs(HudText.PulseColor(0f).g - HudText.PulseColor(0.4f).g), 0.01f);
        }

        private static void AssertSameFog(DayNightSample expected, DayNightSample actual)
        {
            Assert.AreEqual(expected.FogDensity, actual.FogDensity, Delta);
            Assert.AreEqual(expected.FogR, actual.FogR, Delta);
            Assert.AreEqual(expected.FogG, actual.FogG, Delta);
            Assert.AreEqual(expected.FogB, actual.FogB, Delta);
            Assert.AreEqual(expected.SeasonTintR, actual.SeasonTintR, Delta);
        }

        private static void AssertTintInBounds(float r, float g, float b)
        {
            Assert.GreaterOrEqual(r, 0.7f);
            Assert.LessOrEqual(r, 1.2f);
            Assert.GreaterOrEqual(g, 0.7f);
            Assert.LessOrEqual(g, 1.2f);
            Assert.GreaterOrEqual(b, 0.7f);
            Assert.LessOrEqual(b, 1.2f);
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
    }
}
