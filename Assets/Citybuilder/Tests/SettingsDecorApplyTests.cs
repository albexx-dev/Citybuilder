using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Применение настроек шага 3 (EPIC-69): DecorEnabled прячет/показывает декор DecorManager
    // и гасит режим стройки декора, AutosaveHours задаёт интервал AutosaveController.
    public class SettingsDecorApplyTests
    {
        private GameObject host;
        private GameObject prefab;
        private DecorManager decor;
        private GameSettings settings;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("SettingsDecorApplyTest");
            GridVisualizer viz = host.AddComponent<GridVisualizer>();
            viz.Width = 4; viz.Height = 2; // сетка до первого доступа к Grid: 8 клеток
            prefab = new GameObject("TreePrefab");
            decor = host.AddComponent<DecorManager>();
            decor.Visualizer = viz;
            decor.DecorPrefabs = new[] { prefab };
            decor.Chance = 1f; // декор на каждой свободной клетке
            settings = new GameSettings();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(prefab);
        }

        private static int ActiveChildren(Transform root)
        {
            int count = 0;
            foreach (Transform child in root) if (child.gameObject.activeInHierarchy) count++;
            return count;
        }

        private Transform SpawnedDecorRoot()
        {
            Transform root = decor.transform.Find("DecorRoot");
            Assert.IsNotNull(root, "корень декора не создан");
            return root;
        }

        [Test]
        public void SetDecorVisible_FalseThenTrue_HidesAndShowsSpawnedDecor()
        {
            decor.SetDecorVisible(false); // до первого спавна: только флаг, объектов ещё нет
            Assert.IsFalse(decor.DecorVisible);
            decor.SetDecorVisible(true); // включение: спавн 8 клеток при Chance = 1
            Transform root = SpawnedDecorRoot();
            Assert.AreEqual(8, ActiveChildren(root));

            decor.SetDecorVisible(false);
            Assert.IsFalse(root.gameObject.activeInHierarchy);
            Assert.AreEqual(0, ActiveChildren(root));
            Assert.IsFalse(decor.DecorVisible);

            decor.SetDecorVisible(true);
            Assert.IsTrue(root.gameObject.activeInHierarchy);
            Assert.AreEqual(8, ActiveChildren(root));
            Assert.IsTrue(decor.DecorVisible);
        }

        [Test]
        public void ApplyDecor_FollowsSettings_AndGatesDecorBuildMode()
        {
            BuildModeMenu menu = host.AddComponent<BuildModeMenu>();
            decor.SetDecorVisible(false);
            decor.SetDecorVisible(true); // спавн: 8 клеток при Chance = 1
            menu.SetMode(BuildModeMenu.Mode.Decor);

            settings.DecorEnabled = false;
            SettingsAppliers.ApplyDecor(settings, decor, menu);
            Assert.IsFalse(decor.DecorVisible);
            Assert.AreEqual(0, ActiveChildren(SpawnedDecorRoot()));
            Assert.IsFalse(menu.DecorBuildEnabled);
            Assert.AreEqual(BuildModeMenu.Mode.Off, menu.CurrentMode); // активный декор-режим погас
            menu.SetMode(BuildModeMenu.Mode.Decor); // вход в декор при выключенной настройке запрещён
            Assert.AreEqual(BuildModeMenu.Mode.Off, menu.CurrentMode);

            settings.DecorEnabled = true;
            SettingsAppliers.ApplyDecor(settings, decor, menu);
            Assert.IsTrue(decor.DecorVisible);
            Assert.AreEqual(8, ActiveChildren(SpawnedDecorRoot()));
            Assert.IsTrue(menu.DecorBuildEnabled);
            menu.SetMode(BuildModeMenu.Mode.Decor); // после включения декора режим снова доступен
            Assert.AreEqual(BuildModeMenu.Mode.Decor, menu.CurrentMode);
        }

        [Test]
        public void ApplyAutosave_SettingsHours_BecomeControllerInterval()
        {
            AutosaveController autosave = host.AddComponent<AutosaveController>();
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
