using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // ADR-021 / issue #170: копии emissive и их очистка идемпотентны, источники не меняются.
    public class EmissiveWindowTests
    {
        private const float Delta = 0.001f;

        [Test]
        public void IsNight_UsesMoonThreshold()
        {
            Assert.IsTrue(BlackoutViewStyle.IsNight(DayNightPolicy.Sample(0f)));
            Assert.IsFalse(BlackoutViewStyle.IsNight(DayNightPolicy.Noon()));
        }

        [Test]
        public void ApplyEmissive_CreatesSingleCopyAndLeavesSourceUntouched()
        {
            GameObject view = CreateView(out Material source);
            Color original = source.color;
            Dictionary<Material, Material> cache = new Dictionary<Material, Material>();
            BlackoutViewStyle.ApplyEmissive(view, BlackoutViewStyle.WarmWindows, cache);
            Assert.AreEqual(1, cache.Count);
            Material copy = cache[source];
            Assert.AreNotSame(source, copy);
            Assert.AreSame(copy, view.GetComponent<MeshRenderer>().sharedMaterial);
            BlackoutViewStyle.ApplyEmissive(view, BlackoutViewStyle.WarmWindows, cache);
            Assert.AreEqual(1, cache.Count);
            Assert.AreSame(copy, view.GetComponent<MeshRenderer>().sharedMaterial);
            Assert.AreEqual(original.r, source.color.r, Delta);
            Object.DestroyImmediate(view);
            Object.DestroyImmediate(source);
        }

        [Test]
        public void ClearEmissive_IsIdempotentAndNeverMutatesSource()
        {
            GameObject view = CreateView(out Material source);
            Color original = source.color;
            Dictionary<Material, Material> cache = new Dictionary<Material, Material>();
            BlackoutViewStyle.ApplyEmissive(view, BlackoutViewStyle.WarmWindows, cache);
            BlackoutViewStyle.ClearEmissive(cache);
            BlackoutViewStyle.ClearEmissive(cache);
            Assert.AreEqual(0, cache.Count);
            Assert.AreEqual(original.r, source.color.r, Delta);
            Assert.AreEqual(original.g, source.color.g, Delta);
            Object.DestroyImmediate(view);
            Object.DestroyImmediate(source);
        }

        [Test]
        public void Publish_NotifiesAndTracksNight()
        {
            int calls = 0;
            DayNightSample seen = DayNightPolicy.Noon();
            System.Action<DayNightSample> handler = delegate(DayNightSample sample) { calls++; seen = sample; };
            BlackoutViewStyle.SampleChanged += handler;
            BlackoutViewStyle.Publish(DayNightPolicy.Sample(0f));
            BlackoutViewStyle.Publish(DayNightPolicy.Sample(0f));
            Assert.AreEqual(2, calls);
            Assert.IsTrue(BlackoutViewStyle.IsNightNow);
            Assert.IsTrue(BlackoutViewStyle.IsNight(seen));
            BlackoutViewStyle.Publish(DayNightPolicy.Noon());
            Assert.IsFalse(BlackoutViewStyle.IsNightNow);
            BlackoutViewStyle.SampleChanged -= handler;
            BlackoutViewStyle.Publish(DayNightPolicy.Sample(0f));
            Assert.AreEqual(3, calls);
            BlackoutViewStyle.Publish(DayNightPolicy.Noon());
        }

        private static GameObject CreateView(out Material source)
        {
            GameObject view = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MeshRenderer renderer = view.GetComponent<MeshRenderer>();
            source = new Material(Shader.Find("Standard"));
            renderer.sharedMaterial = source;
            return view;
        }
    }
}
