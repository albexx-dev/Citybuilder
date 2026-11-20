using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Citybuilder
{
    // Применение настроек (EPIC-69 шаг 2): Changed persist-ит профиль в PlayerPrefs и
    // на лету двигает системы с готовыми точками (AudioController, DayNightController).
    public class SettingsApplyTests
    {
        [SetUp]
        public void SetUp()
        {
            ClearKeys();
        }

        // Чистим только свои ключи: чужие настройки в прогоне не трогаем (образец GameSettingsTests).
        private static void ClearKeys()
        {
            PlayerPrefs.DeleteKey(GameSettingsStore.VolumeKey); PlayerPrefs.DeleteKey(GameSettingsStore.MuteKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.AgentLimitKey); PlayerPrefs.DeleteKey(GameSettingsStore.DecorKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.DayNightKey); PlayerPrefs.DeleteKey(GameSettingsStore.AutosaveHoursKey);
        }

        [Test]
        public void ApplyAudio_SetsControllerVolumeMuteAndListener()
        {
            GameObject go = new GameObject("TestSettingsAudio");
            try
            {
                AudioController audio = go.AddComponent<AudioController>();
                GameSettings settings = new GameSettings();
                settings.Volume = 0.25f;
                settings.Muted = true;

                SettingsView.ApplyAudio(settings, audio);

                Assert.AreEqual(0.25f, audio.Volume, 0.001f);
                Assert.IsTrue(audio.IsMuted);
                Assert.AreEqual(0f, AudioListener.volume, 0.001f);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void ApplyDayNight_TogglesControllerFieldBothWays()
        {
            // Без Runner контроллер честно ругается при включении — ждём это сообщение.
            LogAssert.Expect(LogType.Error, "DayNightController: задайте Runner в Inspector.");
            GameObject go = new GameObject("TestSettingsDayNight");
            try
            {
                DayNightController dayNight = go.AddComponent<DayNightController>();
                GameSettings settings = new GameSettings();

                settings.DayNightEnabled = false;
                SettingsView.ApplyDayNight(settings, dayNight);
                Assert.IsFalse(dayNight.DayNightEnabled);
                settings.DayNightEnabled = true;
                SettingsView.ApplyDayNight(settings, dayNight);
                Assert.IsTrue(dayNight.DayNightEnabled);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void SettingsView_Changes_PersistToStoreAndReachAudio()
        {
            GameObject go = new GameObject("TestSettingsView");
            try
            {
                SettingsView view = go.AddComponent<SettingsView>();
                AudioController audio = go.AddComponent<AudioController>();
                view.Audio = audio;

                view.Settings.Volume = 0.3f;
                Assert.AreEqual(0.3f, PlayerPrefs.GetFloat(GameSettingsStore.VolumeKey, 0f), 0.001f);
                Assert.AreEqual(0.3f, audio.Volume, 0.001f);
                view.Settings.Muted = true;
                Assert.IsTrue(audio.IsMuted);
                view.Settings.AgentLimit = 64;
                Assert.AreEqual(64, PlayerPrefs.GetInt(GameSettingsStore.AgentLimitKey, 0));
                view.Settings.AutosaveHours = 60;
                Assert.AreEqual(60, PlayerPrefs.GetInt(GameSettingsStore.AutosaveHoursKey, 0));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void SettingsView_OpenClose_RestoresSpeedAndReturnsToPause()
        {
            GameObject go = new GameObject("TestSettingsMenu");
            try
            {
                SimulationRunner runner = go.AddComponent<SimulationRunner>();
                PauseMenuView pause = go.AddComponent<PauseMenuView>();
                SettingsView view = go.AddComponent<SettingsView>();
                view.Runner = runner; view.Pause = pause;
                runner.SetSpeed(SimulationSpeed.Playing);

                view.Open();
                Assert.IsTrue(view.IsOpen); Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);
                view.Close();
                Assert.IsFalse(view.IsOpen); Assert.AreEqual(SimulationSpeed.Playing, runner.Speed);
                view.Open();
                view.ReturnToPause();
                Assert.IsFalse(view.IsOpen); Assert.IsTrue(pause.IsOpen); Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void SettingsView_Open_ClosesSiblingPanels_ToggleClosesAgain()
        {
            GameObject go = new GameObject("TestSettingsSiblings");
            try
            {
                EventLogView log = go.AddComponent<EventLogView>();
                QuestLogView quests = go.AddComponent<QuestLogView>();
                SettingsView view = go.AddComponent<SettingsView>();
                SimulationRunner runner = go.AddComponent<SimulationRunner>();
                view.Runner = runner;
                runner.SetSpeed(SimulationSpeed.Playing);
                log.SetOpen(true);
                quests.SetOpen(true);

                view.Open();
                Assert.IsTrue(view.IsOpen);
                Assert.IsFalse(log.IsOpen);
                Assert.IsFalse(quests.IsOpen);
                Assert.AreEqual(SimulationSpeed.Paused, runner.Speed);

                view.Toggle(); // повторный клик по кнопке нижней панели закрывает окно
                Assert.IsFalse(view.IsOpen);
                Assert.AreEqual(SimulationSpeed.Playing, runner.Speed);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
