using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Опция «Мусоровоз» (issue #115): дефолт выкл, тумблер применяется на лету.
    public class SettingsGarbageTruckApplyTests
    {
        private GameSettings settings;
        private GameObject host;

        [SetUp]
        public void SetUp()
        {
            ClearKeys();
            settings = new GameSettings();
            host = new GameObject("GarbageTruckSettingsHost");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
            ClearKeys();
        }

        private static void ClearKeys()
        {
            PlayerPrefs.DeleteKey(GameSettingsStore.VolumeKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.AmbientVolumeKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.MuteKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.AgentLimitKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.DecorKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.DayNightKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.AutosaveHoursKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.ServiceCoverageOverlayKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.CongestionOverlayKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.GridOverlayKey);
            PlayerPrefs.DeleteKey(GameSettingsStore.GarbageTrucksKey);
        }

        [Test]
        public void Defaults_GarbageTrucksOff_AndResetRestoresOff()
        {
            Assert.IsFalse(settings.ShowGarbageTrucks);
            int changes = 0;
            settings.Changed += delegate { changes++; };
            settings.ShowGarbageTrucks = false;
            Assert.AreEqual(0, changes);
            settings.ShowGarbageTrucks = true;
            Assert.AreEqual(1, changes);
            settings.ResetToDefaults();
            Assert.IsFalse(settings.ShowGarbageTrucks);
            Assert.AreEqual(2, changes);
        }

        [Test]
        public void ApplyGarbageTrucks_TogglesView_WithoutRestartingComponent()
        {
            GarbageTruckView view = host.AddComponent<GarbageTruckView>();
            view.ShowTruckVisuals = true;
            settings.ShowGarbageTrucks = false;
            SettingsAppliers.ApplyGarbageTrucks(settings, view);
            Assert.IsFalse(view.ShowTruckVisuals);
            Assert.IsTrue(view.enabled);
            settings.ShowGarbageTrucks = true;
            SettingsAppliers.ApplyGarbageTrucks(settings, view);
            Assert.IsTrue(view.ShowTruckVisuals);
            Assert.IsTrue(view.enabled);
            SettingsAppliers.ApplyGarbageTrucks(null, view);
            Assert.IsTrue(view.ShowTruckVisuals);
            SettingsAppliers.ApplyGarbageTrucks(settings, null);
        }

        [Test]
        public void Store_RoundTrip_KeepsGarbageTrucksFlag()
        {
            settings.ShowGarbageTrucks = true;
            GameSettingsStore.Save(settings);
            GameSettings loaded = new GameSettings();
            GameSettingsStore.Load(loaded);
            Assert.IsTrue(loaded.ShowGarbageTrucks);
            PlayerPrefs.DeleteKey(GameSettingsStore.GarbageTrucksKey);
            loaded = new GameSettings();
            GameSettingsStore.Load(loaded);
            Assert.IsFalse(loaded.ShowGarbageTrucks);
        }

        [Test]
        public void SetShowTruckVisuals_KeepsServiceTextAndComponent()
        {
            GarbageTruckView view = host.AddComponent<GarbageTruckView>();
            string before = view.ServiceText;
            view.SetShowTruckVisuals(true);
            Assert.IsTrue(view.ShowTruckVisuals);
            Assert.AreEqual(before, view.ServiceText);
            Assert.IsTrue(view.enabled);
            view.SetShowTruckVisuals(false);
            Assert.IsFalse(view.ShowTruckVisuals);
            Assert.AreEqual(before, view.ServiceText);
            Assert.IsTrue(view.enabled);
        }

        [Test]
        public void GarbageTrucksToggle_HasRuAndEnStrings()
        {
            Language saved = Localization.Language;
            try
            {
                Localization.SetLanguage(Language.Ru);
                Assert.AreEqual("Мусоровоз", Localization.T("settings_garbage_trucks"));
                Localization.SetLanguage(Language.En);
                Assert.AreEqual("Garbage truck", Localization.T("settings_garbage_trucks"));
            }
            finally { Localization.SetLanguage(saved); }
        }
    }
}
