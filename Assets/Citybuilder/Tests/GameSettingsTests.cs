using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Настройки игрока (EPIC-69 шаг 1): дефолты, клампы, событие Changed, профиль PlayerPrefs.
    public class GameSettingsTests
    {
        private GameSettings settings;
        private int changes;

        [SetUp]
        public void SetUp()
        {
            ClearKeys();
            settings = new GameSettings();
            changes = 0;
            settings.Changed += delegate { changes++; };
        }

        // Чистим только свои ключи: чужие настройки (звук и др.) в прогоне не трогаем.
        private static void ClearKeys()
        {
            PlayerPrefs.DeleteKey(GameSettingsStore.VolumeKey); PlayerPrefs.DeleteKey(GameSettingsStore.MuteKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.AgentLimitKey); PlayerPrefs.DeleteKey(GameSettingsStore.DecorKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.DayNightKey); PlayerPrefs.DeleteKey(GameSettingsStore.AutosaveHoursKey);
        }

        [Test]
        public void Defaults_MatchCurrentGameConstants()
        {
            Assert.AreEqual(AudioController.DefaultVolume, settings.Volume, 0.001f); Assert.IsFalse(settings.Muted);
            Assert.AreEqual(GameSettings.DefaultAgentLimit, settings.AgentLimit); Assert.IsTrue(settings.DecorEnabled);
            Assert.IsTrue(settings.DayNightEnabled); Assert.AreEqual(AutosaveController.DefaultAutosaveIntervalTicks, settings.AutosaveHours);
            Assert.AreEqual(0, changes);
        }

        [Test]
        public void Volume_ClampsAndNaNBecomesZero()
        {
            settings.Volume = 2f;
            Assert.AreEqual(1f, settings.Volume, 0.001f); settings.Volume = -3f;
            Assert.AreEqual(0f, settings.Volume, 0.001f); settings.Volume = float.NaN; Assert.AreEqual(0f, settings.Volume, 0.001f);
        }

        [Test]
        public void AgentLimit_ClampsToAllowedRange()
        {
            settings.AgentLimit = 1;
            Assert.AreEqual(GameSettings.MinAgentLimit, settings.AgentLimit); settings.AgentLimit = 5000;
            Assert.AreEqual(GameSettings.MaxAgentLimit, settings.AgentLimit);
            settings.AgentLimit = 64;
            Assert.AreEqual(64, settings.AgentLimit);
        }

        [Test]
        public void AutosaveHours_ClampsToAllowedRange()
        {
            settings.AutosaveHours = 1;
            Assert.AreEqual(GameSettings.MinAutosaveHours, settings.AutosaveHours); settings.AutosaveHours = 999;
            Assert.AreEqual(GameSettings.MaxAutosaveHours, settings.AutosaveHours);
        }

        [Test]
        public void Changed_FiresOnlyOnRealChange()
        {
            settings.AgentLimit = 64;
            Assert.AreEqual(1, changes);
            settings.AgentLimit = 64;
            Assert.AreEqual(1, changes);
            settings.AgentLimit = 5000; // кламп до 120 — изменение
            Assert.AreEqual(2, changes);
            settings.Muted = false; // уже false
            Assert.AreEqual(2, changes);
            settings.Muted = true;
            Assert.AreEqual(3, changes);
        }

        [Test]
        public void ResetToDefaults_RestoresEverythingAndFiresOnce()
        {
            settings.Volume = 0.9f; settings.Muted = true; settings.AgentLimit = 100;
            settings.DecorEnabled = false; settings.DayNightEnabled = false; settings.AutosaveHours = 60;
            changes = 0;
            settings.ResetToDefaults();
            Assert.AreEqual(1, changes); Assert.AreEqual(GameSettings.DefaultVolume, settings.Volume, 0.001f);
            Assert.IsFalse(settings.Muted); Assert.IsTrue(settings.DecorEnabled); Assert.IsTrue(settings.DayNightEnabled);
            Assert.AreEqual(GameSettings.DefaultAgentLimit, settings.AgentLimit);
            Assert.AreEqual(GameSettings.DefaultAutosaveHours, settings.AutosaveHours);
            settings.ResetToDefaults();
            Assert.AreEqual(1, changes); // повторный сброс ничего не меняет
        }

        [Test]
        public void Store_RoundTrip_KeepsEveryValue()
        {
            settings.Volume = 0.25f;
            settings.Muted = true;
            settings.AgentLimit = 90;
            settings.DecorEnabled = false;
            settings.DayNightEnabled = false;
            settings.AutosaveHours = 30;
            GameSettingsStore.Save(settings);
            GameSettings loaded = new GameSettings();
            GameSettingsStore.Load(loaded);
            Assert.AreEqual(0.25f, loaded.Volume, 0.001f); Assert.IsTrue(loaded.Muted); Assert.AreEqual(90, loaded.AgentLimit);
            Assert.IsFalse(loaded.DecorEnabled); Assert.IsFalse(loaded.DayNightEnabled); Assert.AreEqual(30, loaded.AutosaveHours);
        }

        [Test]
        public void Store_EmptyPrefs_GiveDefaults()
        {
            GameSettingsStore.Load(settings);
            Assert.AreEqual(GameSettings.DefaultVolume, settings.Volume, 0.001f);
            Assert.AreEqual(GameSettings.DefaultAgentLimit, settings.AgentLimit);
            Assert.AreEqual(GameSettings.DefaultAutosaveHours, settings.AutosaveHours); Assert.IsTrue(settings.DecorEnabled);
        }

        [Test]
        public void Store_GarbagePrefs_AreClampedNotCrashing()
        {
            PlayerPrefs.SetFloat(GameSettingsStore.VolumeKey, 17f);
            PlayerPrefs.SetInt(GameSettingsStore.AgentLimitKey, -40);
            PlayerPrefs.SetInt(GameSettingsStore.AutosaveHoursKey, 10000);
            GameSettingsStore.Load(settings);
            Assert.AreEqual(1f, settings.Volume, 0.001f); Assert.AreEqual(GameSettings.MinAgentLimit, settings.AgentLimit);
            Assert.AreEqual(GameSettings.MaxAutosaveHours, settings.AutosaveHours);
        }

        [Test]
        public void Store_VolumeKeysMatchAudioController()
        {
            Assert.AreEqual(AudioController.VolumePrefKey, GameSettingsStore.VolumeKey);
            Assert.AreEqual(AudioController.MutePrefKey, GameSettingsStore.MuteKey);
            settings.Volume = 0.75f;
            GameSettingsStore.Save(settings);
            Assert.AreEqual(0.75f, PlayerPrefs.GetFloat(AudioController.VolumePrefKey, 0f), 0.001f);
        }

        [Test]
        public void Store_NullSettings_IsSafe()
        {
            GameSettingsStore.Save(null); GameSettingsStore.Load(null);
            Assert.IsFalse(PlayerPrefs.HasKey(GameSettingsStore.AgentLimitKey));
        }
    }
}