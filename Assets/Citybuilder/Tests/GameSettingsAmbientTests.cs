using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Слайдер «Эмбиент» (хвост #71): кламп 0..1, дефолт 1, событие Changed, ключ
    // citybuilder_ambient в GameSettingsStore и применение к AmbientAudioView.
    public class GameSettingsAmbientTests
    {
        private GameSettings settings;
        private int changes;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(GameSettingsStore.AmbientVolumeKey);
            settings = new GameSettings();
            changes = 0;
            settings.Changed += delegate { changes++; };
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(GameSettingsStore.AmbientVolumeKey);
        }

        [Test]
        public void AmbientVolume_DefaultIsOne()
        {
            Assert.AreEqual(GameSettings.DefaultAmbientVolume, settings.AmbientVolume, 0.001f);
            Assert.AreEqual(0, changes);
        }

        [Test]
        public void AmbientVolume_ClampsToZeroOne()
        {
            settings.AmbientVolume = 2f;
            Assert.AreEqual(1f, settings.AmbientVolume, 0.001f);
            settings.AmbientVolume = -3f;
            Assert.AreEqual(0f, settings.AmbientVolume, 0.001f);
            settings.AmbientVolume = float.NaN;
            Assert.AreEqual(0f, settings.AmbientVolume, 0.001f);
            settings.AmbientVolume = 0.45f;
            Assert.AreEqual(0.45f, settings.AmbientVolume, 0.001f);
        }

        [Test]
        public void AmbientVolume_ChangedFiresOnlyOnRealChange()
        {
            settings.AmbientVolume = 0.3f;
            Assert.AreEqual(1, changes);
            settings.AmbientVolume = 0.3f;
            Assert.AreEqual(1, changes);
            settings.AmbientVolume = 5f; // кламп до 1 — реальное изменение
            Assert.AreEqual(2, changes);
        }

        [Test]
        public void AmbientVolume_ResetToDefaults_RestoresAndFiresOnce()
        {
            settings.AmbientVolume = 0.2f;
            changes = 0;
            settings.ResetToDefaults();
            Assert.AreEqual(1, changes);
            Assert.AreEqual(GameSettings.DefaultAmbientVolume, settings.AmbientVolume, 0.001f);
            settings.ResetToDefaults();
            Assert.AreEqual(1, changes);
        }

        [Test]
        public void Store_AmbientKey_IsCitybuilderAmbient_AndRoundTrips()
        {
            Assert.AreEqual("citybuilder_ambient", GameSettingsStore.AmbientVolumeKey);
            settings.AmbientVolume = 0.35f;
            GameSettingsStore.Save(settings);
            GameSettings loaded = new GameSettings();
            GameSettingsStore.Load(loaded);
            Assert.AreEqual(0.35f, loaded.AmbientVolume, 0.001f);
        }

        [Test]
        public void Store_EmptyAmbientKey_GivesDefault()
        {
            GameSettingsStore.Load(settings);
            Assert.AreEqual(GameSettings.DefaultAmbientVolume, settings.AmbientVolume, 0.001f);
        }

        [Test]
        public void Store_GarbageAmbientValue_IsClamped()
        {
            PlayerPrefs.SetFloat(GameSettingsStore.AmbientVolumeKey, 17f);
            GameSettingsStore.Load(settings);
            Assert.AreEqual(1f, settings.AmbientVolume, 0.001f);
        }

        [Test]
        public void ApplyAmbient_PutsProfileValueIntoView()
        {
            GameObject host = new GameObject("AmbientTest");
            try
            {
                AmbientAudioView ambient = host.AddComponent<AmbientAudioView>();
                settings.AmbientVolume = 0.25f;
                SettingsAppliers.ApplyAmbient(settings, ambient);
                Assert.AreEqual(0.25f, ambient.AmbientVolume, 0.001f);
                SettingsAppliers.ApplyAmbient(null, ambient);
                Assert.AreEqual(0.25f, ambient.AmbientVolume, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }
    }
}
