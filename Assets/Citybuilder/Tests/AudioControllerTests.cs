using NUnit.Framework;
using UnityEngine;

namespace Citybuilder.Tests
{
    public class AudioControllerTests
    {
        [Test]
        public void ClampVolume_ClampsToZeroOneRange()
        {
            Assert.AreEqual(0f, AudioController.ClampVolume(-0.5f), 0.0001f);
            Assert.AreEqual(0.5f, AudioController.ClampVolume(0.5f), 0.0001f);
            Assert.AreEqual(1f, AudioController.ClampVolume(2.5f), 0.0001f);
        }

        [Test]
        public void PlayMethods_WithoutAudioSourceOrClips_DoNotThrow()
        {
            GameObject go = new GameObject("TestAudio");
            try
            {
                AudioController ctrl = go.AddComponent<AudioController>();
                Assert.DoesNotThrow(() =>
                {
                    ctrl.PlayClick();
                    ctrl.PlayCoin();
                    ctrl.PlayPop();
                    ctrl.PlaySwoosh();
                });
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void SetVolumeAndMute_UpdatesStateAndClamps()
        {
            GameObject go = new GameObject("TestAudioPrefs");
            try
            {
                AudioController ctrl = go.AddComponent<AudioController>();
                ctrl.SetVolume(1.5f);
                Assert.AreEqual(1f, ctrl.Volume, 0.0001f);
                ctrl.SetVolume(-0.2f);
                Assert.AreEqual(0f, ctrl.Volume, 0.0001f);
                ctrl.SetVolume(0.5f);
                ctrl.SetMuted(true);
                Assert.IsTrue(ctrl.IsMuted);
                ctrl.SetMuted(false);
                Assert.IsFalse(ctrl.IsMuted);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
