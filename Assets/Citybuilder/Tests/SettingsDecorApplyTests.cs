using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Применение настроек (issue #95): тумблер гасит корень ручного декора,
    // эмбиент применяется одним путём — из SettingsView.ApplyToListeners.
    public class SettingsDecorApplyTests
    {
        private GameObject decorHost;
        private GameObject manualHost;
        private GameObject manualRoot;
        private DecorManager decor;
        private BuildModeMenu menu;
        private GameSettings settings;

        [SetUp]
        public void SetUp()
        {
            decorHost = new GameObject("DecorManagerHost");
            decor = decorHost.AddComponent<DecorManager>();
            menu = decorHost.AddComponent<BuildModeMenu>();

            manualHost = new GameObject("ManualDecorHost");
            DecorPlacementController placer = manualHost.AddComponent<DecorPlacementController>();
            manualRoot = new GameObject("DecorRoot");
            manualRoot.transform.SetParent(placer.transform, false);
            AddDecor("Tree1");
            AddDecor("Tree2");
            settings = new GameSettings();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(manualHost);
            Object.DestroyImmediate(decorHost);
        }

        private void AddDecor(string name)
        {
            GameObject tree = new GameObject(name);
            tree.transform.SetParent(manualRoot.transform, false);
        }

        private int ActiveCount()
        {
            int count = 0;
            foreach (Transform child in manualRoot.transform) if (child.gameObject.activeInHierarchy) count++;
            return count;
        }

        [Test]
        public void ManualDecor_SetAndRemove_ChangesChildren()
        {
            Assert.AreEqual(2, ActiveCount());
            AddDecor("Tree3");
            Assert.AreEqual(3, ActiveCount());
            Object.DestroyImmediate(manualRoot.transform.GetChild(0).gameObject);
            Assert.AreEqual(2, ActiveCount());
        }

        [Test]
        public void ApplyDecor_DisabledHidesManualDecor_AndGatesBuildMode()
        {
            menu.SetMode(BuildModeMenu.Mode.Decor);
            settings.DecorEnabled = false;
            SettingsAppliers.ApplyDecor(settings, decor, menu);
            Assert.IsFalse(decor.DecorVisible);
            Assert.IsFalse(manualRoot.activeInHierarchy);
            Assert.AreEqual(0, ActiveCount());
            Assert.IsFalse(menu.DecorBuildEnabled);
            Assert.AreEqual(BuildModeMenu.Mode.Off, menu.CurrentMode);
            menu.SetMode(BuildModeMenu.Mode.Decor);
            Assert.AreEqual(BuildModeMenu.Mode.Off, menu.CurrentMode);

            settings.DecorEnabled = true;
            SettingsAppliers.ApplyDecor(settings, decor, menu);
            Assert.IsTrue(decor.DecorVisible);
            Assert.IsTrue(manualRoot.activeInHierarchy);
            Assert.AreEqual(2, ActiveCount());
            menu.SetMode(BuildModeMenu.Mode.Decor);
            Assert.AreEqual(BuildModeMenu.Mode.Decor, menu.CurrentMode);
        }

        [Test]
        public void ApplyDecor_NullSettings_KeepsManualDecor()
        {
            SettingsAppliers.ApplyDecor(null, decor, menu);
            Assert.IsTrue(decor.DecorVisible);
            Assert.IsTrue(manualRoot.activeInHierarchy);
            Assert.AreEqual(2, ActiveCount());
        }

        [Test]
        public void ApplyAmbient_SettingsVolume_ReachesView()
        {
            GameObject ambientHost = new GameObject("AmbientHost");
            try
            {
                AmbientAudioView ambient = ambientHost.AddComponent<AmbientAudioView>();
                settings.AmbientVolume = 0.35f;
                SettingsAppliers.ApplyAmbient(settings, ambient);
                Assert.AreEqual(0.35f, ambient.AmbientVolume, 0.0001f);
                SettingsAppliers.ApplyAmbient(null, ambient);
                Assert.AreEqual(0.35f, ambient.AmbientVolume, 0.0001f);
            }
            finally { Object.DestroyImmediate(ambientHost); }
        }

        [Test]
        public void ApplyAutosave_SettingsHours_BecomeControllerInterval()
        {
            AutosaveController autosave = decorHost.AddComponent<AutosaveController>();
            Assert.AreEqual(AutosaveController.DefaultAutosaveIntervalTicks, autosave.AutosaveIntervalTicks);
            settings.AutosaveHours = 60;
            SettingsAppliers.ApplyAutosave(settings, autosave);
            Assert.AreEqual(60, autosave.AutosaveIntervalTicks);

            // 59 тиков копят счётчик, 60-й сбрасывает его (Save пуст — без записи файла).
            for (int t = 1; t <= 59; t++) autosave.OnClockTick(new SimulationTime(t));
            Assert.AreEqual(59, autosave.TicksSinceSave);
            autosave.OnClockTick(new SimulationTime(60));
            Assert.AreEqual(0, autosave.TicksSinceSave);
        }
    }
}
