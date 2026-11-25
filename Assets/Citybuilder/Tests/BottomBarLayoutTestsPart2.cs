using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса BottomBarLayoutTests (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class BottomBarLayoutTests
    {
        [TestCase(90f, 0f, -90f, 90f)]
        [TestCase(90f, 180f, 90f, 90f)]
        [TestCase(90f, 45f, 0f, 45f)]
        [TestCase(45f, 60f, 10f, 60f)]
        public void ReadYaw_HandlesDegenerateEulerAngles(float pitch, float rawY, float rawZ, float expected)
        {
            float yaw = FreeCameraController.ReadYaw(new Vector3(pitch, rawY, rawZ));
            Assert.AreEqual(expected, yaw, 0.01f);
        }

        [Test]
        public void Pan_TopDown90Pitch_EastFacing_PansAlongXAxis()
        {
            GameObject go = new GameObject("CameraTest");
            try
            {
                Camera cam = go.AddComponent<Camera>();
                FreeCameraController ctrl = go.AddComponent<FreeCameraController>();
                ctrl.Target = cam;
                ctrl.MapWidth = 200f;
                ctrl.MapHeight = 200f;
                cam.transform.position = new Vector3(50f, 20f, 50f);
                cam.transform.rotation = Quaternion.Euler(90f, 90f, 0f);

                ctrl.Pan(0f, 10f);
                Assert.Greater(cam.transform.position.x, 55f);
                Assert.AreEqual(50f, cam.transform.position.z, 0.05f);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void TutorialView_OnDisable_ClearsClickGuard()
        {
            GameObject go = new GameObject("TutTest");
            try
            {
                TutorialView tut = go.AddComponent<TutorialView>();
                UiClickGuard.Set("tutorial_dismiss", new Rect(460f, 100f, 90f, 24f));
                Vector2 pt = new Vector2(480f, Screen.height - 110f);
                Assert.IsTrue(UiClickGuard.IsOverMenu(pt));

                tut.OnDisable();
                Assert.IsFalse(UiClickGuard.IsOverMenu(pt));
            }
            finally
            {
                UiClickGuard.Set("tutorial_dismiss", new Rect());
                Object.DestroyImmediate(go);
            }
        }
    }
}
