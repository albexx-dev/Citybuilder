using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Снимок горящих и пул эффектов (issue #116): слоты переиспользуются, погасшие
    // вытесняются без аллокаций, фаза идёт по кругу; звук FireStart не дублируется.
    public class FireFxSlotsTests
    {
        [Test]
        public void SnapshotBurning_TwoHomes_KeepIdsAndPositions()
        {
            FireFxSlots slots = new FireFxSlots();
            EntityId first = EntityId.New();
            EntityId second = EntityId.New();
            slots.BeginSync();
            Assert.IsTrue(slots.Touch(first, 1f, 2f));
            Assert.IsTrue(slots.Touch(second, 3f, 4f));
            slots.EndSync();
            Assert.AreEqual(2, slots.Count);
            Assert.AreEqual(first, slots.IdAt(0));
            Assert.AreEqual(1f, slots.XAt(0));
            Assert.AreEqual(second, slots.IdAt(1));
            Assert.AreEqual(4f, slots.ZAt(1));
        }

        [Test]
        public void Refresh_ExtinguishedHome_ReusesSlotAndKeepsAge()
        {
            FireFxSlots slots = new FireFxSlots();
            EntityId gone = EntityId.New();
            EntityId alive = EntityId.New();
            slots.BeginSync();
            slots.Touch(gone, 0f, 0f);
            slots.Touch(alive, 1f, 1f);
            slots.EndSync();
            slots.Advance(0.5f);
            slots.BeginSync();
            Assert.IsFalse(slots.Touch(alive, 9f, 9f));
            slots.EndSync();
            Assert.AreEqual(1, slots.Count);
            Assert.AreEqual(alive, slots.IdAt(0));
            Assert.AreEqual(9f, slots.XAt(0));
            Assert.AreEqual(0.5f, slots.AgeAt(0), 0.0001f);
        }

        [Test]
        public void Refresh_NewHomeAfterBurnout_UsesFreedSlot()
        {
            FireFxSlots slots = new FireFxSlots();
            EntityId old = EntityId.New();
            EntityId kept = EntityId.New();
            EntityId fresh = EntityId.New();
            slots.BeginSync();
            slots.Touch(old, 0f, 0f);
            slots.Touch(kept, 1f, 1f);
            slots.EndSync();
            slots.BeginSync();
            slots.Touch(kept, 2f, 2f);
            Assert.IsTrue(slots.Touch(fresh, 3f, 3f));
            slots.EndSync();
            Assert.AreEqual(2, slots.Count);
            Assert.AreEqual(kept, slots.IdAt(0));
            Assert.AreEqual(fresh, slots.IdAt(1));
        }

        [Test]
        public void Touch_OverCapacity_DropsExtraHomes()
        {
            FireFxSlots slots = new FireFxSlots();
            slots.BeginSync();
            for (int i = 0; i < FireFxSlots.Capacity + 3; i++) slots.Touch(EntityId.New(), i, i);
            slots.EndSync();
            Assert.AreEqual(FireFxSlots.Capacity, slots.Count);
        }

        [Test]
        public void Advance_WrapsPhase_WithoutDrift()
        {
            FireFxSlots slots = new FireFxSlots();
            slots.BeginSync();
            slots.Touch(EntityId.New(), 0f, 0f);
            slots.EndSync();
            slots.Advance(FireFxSlots.CycleSec * 2f + 0.25f);
            Assert.AreEqual(0.25f, slots.AgeAt(0), 0.0001f);
        }

        [Test]
        public void FireStart_FromSystem_UsesExistingAudioPolicyOnly()
        {
            BuildingState home = new BuildingState(EntityId.New(), null, default(GridPosition),
                new List<GridPosition>());
            FireSystem fires = new FireSystem();
            List<CityEvent> events = new List<CityEvent>();
            fires.Fired += delegate (CityEvent cityEvent) { events.Add(cityEvent); };
            fires.ForceIgnite(home, 10);
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(AudioEventSound.FireStart, AudioEventPolicy.Resolve(events[0]));
            fires.Tick(11, new List<BuildingState> { home }, delegate { return 1f; }, null);
            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(AudioEventSound.None, AudioEventPolicy.Resolve(events[1]));
        }
    }
}
