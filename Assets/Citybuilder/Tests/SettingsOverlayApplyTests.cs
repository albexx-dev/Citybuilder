using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Оверлеи (issue #114): дефолты и appliers читают профиль, а адаптеры не хардкодят видимость.
    public class SettingsOverlayApplyTests
    {
        private GameSettings settings;
        private GameObject host;

        [SetUp]
        public void SetUp()
        {
            ClearKeys();
            settings = new GameSettings();
            host = new GameObject("OverlaySettingsHost");
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
        }

        private static int MeshVertexCount(GameObject host, string child)
        {
            Transform node = host.transform.Find(child);
            if (node == null) return 0;
            MeshFilter filter = node.GetComponent<MeshFilter>();
            return filter == null || filter.sharedMesh == null ? 0 : filter.sharedMesh.vertexCount;
        }

        [Test]
        public void Defaults_KeepOverlaysVisible_AndResetRestoresThem()
        {
            Assert.IsTrue(settings.ServiceCoverageOverlayEnabled);
            Assert.IsTrue(settings.CongestionOverlayEnabled);
            Assert.IsTrue(settings.GridOverlayEnabled);
            settings.ServiceCoverageOverlayEnabled = false;
            settings.CongestionOverlayEnabled = false;
            settings.GridOverlayEnabled = false;
            settings.ResetToDefaults();
            Assert.IsTrue(settings.ServiceCoverageOverlayEnabled);
            Assert.IsTrue(settings.CongestionOverlayEnabled);
            Assert.IsTrue(settings.GridOverlayEnabled);
        }

        [Test]
        public void ApplyOverlays_CopiesAllThreeFlagsBothWays()
        {
            GridVisualizer grid = host.AddComponent<GridVisualizer>();
            RoadView roads = host.AddComponent<RoadView>();
            settings.ServiceCoverageOverlayEnabled = false;
            settings.CongestionOverlayEnabled = false;
            settings.GridOverlayEnabled = false;
            SettingsAppliers.ApplyOverlays(settings, grid, roads);
            Assert.IsFalse(grid.ServiceCoverageOverlayEnabled);
            Assert.IsFalse(grid.GridOverlayEnabled);
            Assert.IsFalse(roads.CongestionOverlayEnabled);
            settings.ServiceCoverageOverlayEnabled = true;
            settings.CongestionOverlayEnabled = true;
            settings.GridOverlayEnabled = true;
            SettingsAppliers.ApplyOverlays(settings, grid, roads);
            Assert.IsTrue(grid.ServiceCoverageOverlayEnabled);
            Assert.IsTrue(grid.GridOverlayEnabled);
            Assert.IsTrue(roads.CongestionOverlayEnabled);
        }

        [Test]
        public void ApplyOverlays_NullProfile_KeepsAdaptersUntouched()
        {
            RoadView roads = host.AddComponent<RoadView>();
            settings.CongestionOverlayEnabled = false;
            SettingsAppliers.ApplyOverlays(settings, null, roads);
            Assert.IsFalse(roads.CongestionOverlayEnabled);
            SettingsAppliers.ApplyOverlays(null, null, roads);
            Assert.IsFalse(roads.CongestionOverlayEnabled);
        }

        [Test]
        public void SetCongestionOverlay_OffThenOn_SameTick_RestoresMesh()
        {
            RoadView roads = host.AddComponent<RoadView>();
            List<GridPosition> cells = new List<GridPosition> { new GridPosition(1, 1) };
            Func<GridPosition, float> load = cell => 1f;
            roads.RefreshCongestionOncePerTick(7, cells, 1f, load);
            Assert.AreEqual(4, MeshVertexCount(host, "RoadsRed"));
            roads.SetCongestionOverlayEnabled(false);
            Assert.AreEqual(0, MeshVertexCount(host, "RoadsRed"));
            roads.SetCongestionOverlayEnabled(true);
            Assert.AreEqual(4, MeshVertexCount(host, "RoadsRed"));
        }

        [Test]
        public void ApplyOverlays_CachedCoverageRadius_SurvivesOffThenOn()
        {
            GridVisualizer grid = host.AddComponent<GridVisualizer>();
            grid.ShowRadiusOverlay(new GridPosition(4, 4), 2, ServiceKind.Fire);
            Assert.Greater(MeshVertexCount(host, "RadiusOverlay"), 0);
            settings.ServiceCoverageOverlayEnabled = false;
            SettingsAppliers.ApplyOverlays(settings, grid, null);
            Assert.AreEqual(0, MeshVertexCount(host, "RadiusOverlay"));
            settings.ServiceCoverageOverlayEnabled = true;
            SettingsAppliers.ApplyOverlays(settings, grid, null);
            Assert.Greater(MeshVertexCount(host, "RadiusOverlay"), 0);
        }

        [Test]
        public void Store_RoundTrip_KeepsOverlayFlags()
        {
            settings.ServiceCoverageOverlayEnabled = false;
            settings.CongestionOverlayEnabled = false;
            settings.GridOverlayEnabled = false;
            GameSettingsStore.Save(settings);
            GameSettings loaded = new GameSettings();
            GameSettingsStore.Load(loaded);
            Assert.IsFalse(loaded.ServiceCoverageOverlayEnabled);
            Assert.IsFalse(loaded.CongestionOverlayEnabled);
            Assert.IsFalse(loaded.GridOverlayEnabled);
        }
    }
}
