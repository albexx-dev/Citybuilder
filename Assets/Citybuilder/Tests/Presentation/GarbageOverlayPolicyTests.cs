using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // issue #94: порог оверлея, сброс после вывоза и запрет оверлея без полигона.
    // Логика не зависит от Unity-объектов (кроме создания BuildingDefinition в тесте).
    public class GarbageOverlayPolicyTests
    {
        private static BuildingState Home(int waste)
        {
            BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.WasteOutput = waste;
            return new BuildingState(EntityId.New(), def, new GridPosition(1, 1), new List<GridPosition>());
        }

        private static void Tick(GarbageSystem garbage, BuildingState home, GridPosition? depot, int ticks)
        {
            List<BuildingState> homes = new List<BuildingState> { home };
            for (int i = 0; i < ticks; i++) garbage.Tick(i, homes, depot, null);
        }

        [Test]
        public void NeedsOverlay_OnlyAtThresholdWithDepot()
        {
            Assert.IsFalse(GarbageOverlayPolicy.NeedsOverlay(GarbageSystem.ReadyThreshold - 1, true));
            Assert.IsTrue(GarbageOverlayPolicy.NeedsOverlay(GarbageSystem.ReadyThreshold, true));
            Assert.IsFalse(GarbageOverlayPolicy.NeedsOverlay(GarbageSystem.ReadyThreshold, false));
        }

        [Test]
        public void PickUp_ClearsAccumulationAndOverlay()
        {
            GarbageSystem garbage = new GarbageSystem();
            BuildingState home = Home(3);
            Tick(garbage, home, new GridPosition(0, 0), GarbageSystem.ReadyThreshold);
            Assert.IsTrue(GarbageOverlayPolicy.NeedsOverlay(garbage.AccOf(home.Id), true));

            garbage.Advance(1.5f);
            Assert.AreEqual(0, garbage.AccOf(home.Id));
            Assert.IsFalse(GarbageOverlayPolicy.NeedsOverlay(garbage.AccOf(home.Id), true));
        }

        [Test]
        public void NoDepot_GarbageAccumulatesButOverlayHidden()
        {
            GarbageSystem garbage = new GarbageSystem();
            BuildingState home = Home(3);
            Tick(garbage, home, null, GarbageSystem.ReadyThreshold);

            int acc = garbage.AccOf(home.Id);
            Assert.IsTrue(acc >= GarbageSystem.ReadyThreshold);
            Assert.AreEqual(0, garbage.ActiveTrucks);
            Assert.IsFalse(GarbageOverlayPolicy.NeedsOverlay(acc, false));
        }

        [Test]
        public void Load01_MapsAmountToBinScale()
        {
            Assert.AreEqual(0f, GarbageOverlayPolicy.Load01(-1), 0.0001f);
            Assert.AreEqual(0.5f, GarbageOverlayPolicy.Load01(GarbageSystem.ReadyThreshold / 2), 0.0001f);
            Assert.AreEqual(1f, GarbageOverlayPolicy.Load01(GarbageSystem.ReadyThreshold), 0.0001f);
        }

        [Test]
        public void Load01_ClampsAboveThreshold()
        {
            Assert.AreEqual(1f, GarbageOverlayPolicy.Load01(GarbageSystem.ReadyThreshold + 1), 0.0001f);
            Assert.AreEqual(1f, GarbageOverlayPolicy.Load01(200), 0.0001f);
        }
    }
}
