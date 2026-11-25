using NUnit.Framework;

namespace Citybuilder
{
    // Тесты отсечения машин (issue #120): дистанция, пирамида камеры, счётчики замера.
    public class VehicleVisibilityPolicyTests
    {
        private static VehicleVisibilityPolicy MakePolicy(float distance = 40f, float margin = 0f)
        {
            VehicleVisibilityPolicy policy = new VehicleVisibilityPolicy();
            policy.Configure(60f, 16f / 9f, distance, margin);
            policy.SetCamera(0f, 0f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 1f, 0f);
            return policy;
        }

        [Test]
        public void Visible_InFrontInsideFrustum()
        {
            VehicleVisibilityPolicy policy = MakePolicy();
            Assert.IsTrue(policy.IsVisible(0f, 0f, 10f));
            Assert.IsTrue(policy.IsVisible(5f, 3f, 10f));
        }

        [Test]
        public void Hidden_BehindCamera()
        {
            Assert.IsFalse(MakePolicy().IsVisible(0f, 0f, -1f));
        }

        [Test]
        public void Hidden_BeyondDistance()
        {
            Assert.IsFalse(MakePolicy(40f).IsVisible(0f, 0f, 41f));
        }

        [Test]
        public void Hidden_OutsideHorizontalAndVertical()
        {
            VehicleVisibilityPolicy policy = MakePolicy();
            Assert.IsFalse(policy.IsVisible(30f, 0f, 10f));
            Assert.IsFalse(policy.IsVisible(0f, 30f, 10f));
        }

        [Test]
        public void Margin_ExtendsFrustumEdges()
        {
            Assert.IsFalse(MakePolicy(40f, 0f).IsVisible(11f, 0f, 10f));
            Assert.IsTrue(MakePolicy(40f, 3f).IsVisible(11f, 0f, 10f));
        }

        [Test]
        public void RotatedCamera_UsesItsBasis()
        {
            VehicleVisibilityPolicy policy = new VehicleVisibilityPolicy();
            policy.Configure(60f, 1f, 40f, 0f);
            // Камера смотрит по −X: forward=(-1,0,0), right=(0,0,1), up=(0,1,0).
            policy.SetCamera(0f, 0f, 0f, -1f, 0f, 0f, 0f, 0f, 1f, 0f, 1f, 0f);
            Assert.IsTrue(policy.IsVisible(-10f, 0f, 0f));
            Assert.IsFalse(policy.IsVisible(10f, 0f, 0f));
        }

        [Test]
        public void Counters_ResetOnConfigure()
        {
            VehicleVisibilityPolicy policy = MakePolicy();
            policy.IsVisible(0f, 0f, 10f);
            policy.IsVisible(0f, 0f, -10f);
            Assert.AreEqual(1, policy.VisibleCount);
            Assert.AreEqual(1, policy.HiddenCount);
            policy.Configure(60f, 1f, 40f, 0f);
            Assert.AreEqual(0, policy.VisibleCount);
            Assert.AreEqual(0, policy.HiddenCount);
        }

        [Test]
        public void InvalidParameters_AreClamped()
        {
            VehicleVisibilityPolicy policy = new VehicleVisibilityPolicy();
            policy.Configure(-10f, 0f, -5f, -2f);
            policy.SetCamera(0f, 0f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 1f, 0f);
            Assert.IsTrue(policy.IsVisible(0f, 0f, 0.5f));
            Assert.IsFalse(policy.IsVisible(0f, 0f, 2f));
        }
    }
}
