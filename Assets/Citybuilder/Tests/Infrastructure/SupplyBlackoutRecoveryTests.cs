using System.Collections.Generic;
using NUnit.Framework;
using static Citybuilder.SupplyBlackoutTestSupport;

namespace Citybuilder
{
    // Восстановление питания и граничные случаи блэкаута (EPIC-50, продолжение SupplyBlackoutTests).
    public class SupplyBlackoutRecoveryTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Recovery_NewSupply_AllPowered()
        {
            List<EntityId> ids = new List<EntityId>();
            SupplyBalance balance = MakeUnitCity(10, 8f, ids);
            Assert.AreEqual(2, balance.UnpoweredCount);
            balance.SetSupply(EntityId.New(), 2f);
            Assert.AreEqual(0, balance.UnpoweredCount);
            Assert.AreEqual(10, CountPowered(balance, ids));
        }

        [Test]
        public void Recovery_RemoveConsumer_AllPowered()
        {
            List<EntityId> ids = new List<EntityId>();
            SupplyBalance balance = MakeUnitCity(10, 8f, ids);
            Assert.AreEqual(2, balance.UnpoweredCount);
            balance.Remove(ids[0]);
            balance.Remove(ids[1]);
            Assert.AreEqual(0, balance.UnpoweredCount);
        }

        [Test]
        public void ZeroDemand_NeverCut()
        {
            SupplyBalance balance = new SupplyBalance();
            balance.SetSupply(EntityId.New(), 0f);
            EntityId idle = EntityId.New();
            balance.SetDemand(idle, 0f, SupplyBalance.TierResidential);
            EntityId consumer = EntityId.New();
            balance.SetDemand(consumer, 5f, SupplyBalance.TierResidential);
            Assert.AreEqual(1, balance.UnpoweredCount);
            Assert.AreEqual(consumer, balance.UnpoweredIds()[0]);
            Assert.IsTrue(balance.IsPowered(idle));
            Assert.IsFalse(balance.IsPowered(consumer));
        }

        [Test]
        public void NaNDemand_ClampedToZero_NoException()
        {
            SupplyBalance balance = new SupplyBalance();
            balance.SetSupply(EntityId.New(), 3f);
            EntityId broken = EntityId.New();
            balance.SetDemand(broken, float.NaN, SupplyBalance.TierResidential);
            Assert.AreEqual(0f, balance.TotalDemand, Tolerance);
            Assert.AreEqual(0, balance.UnpoweredCount);
            Assert.IsTrue(balance.IsPowered(broken));
        }

        [Test]
        public void UnknownId_IsPowered()
        {
            SupplyBalance balance = new SupplyBalance();
            balance.SetSupply(EntityId.New(), 0f);
            balance.SetDemand(EntityId.New(), 10f);
            Assert.IsTrue(balance.IsPowered(EntityId.New()));
        }

        [Test]
        public void Blackout_KeepsBrownoutRate01()
        {
            // Rate01 при блэкауте прежний: агентный фид (ADR-011) и рост читают долю покрытия.
            List<EntityId> ids = new List<EntityId>();
            SupplyBalance balance = MakeUnitCity(10, 8f, ids);
            Assert.AreEqual(0.8f, balance.Rate01, Tolerance);
            Assert.AreEqual(2, balance.UnpoweredCount);
        }

        [Test]
        public void DuplicateSetDemand_OverwritesTier()
        {
            SupplyBalance balance = new SupplyBalance();
            balance.SetSupply(EntityId.New(), 1f);
            EntityId a = EntityId.New();
            EntityId b = EntityId.New();
            balance.SetDemand(a, 1f, SupplyBalance.TierResidential);
            balance.SetDemand(b, 1f, SupplyBalance.TierService);
            Assert.AreEqual(b, balance.UnpoweredIds()[0]);
            balance.SetDemand(a, 1f, SupplyBalance.TierService);
            balance.SetDemand(b, 1f, SupplyBalance.TierResidential);
            Assert.AreEqual(a, balance.UnpoweredIds()[0]);
        }

        [Test]
        public void Clear_ResetsCutoff()
        {
            List<EntityId> ids = new List<EntityId>();
            SupplyBalance balance = MakeUnitCity(10, 8f, ids);
            Assert.AreEqual(2, balance.UnpoweredCount);
            balance.Clear();
            Assert.AreEqual(0, balance.UnpoweredCount);
            Assert.IsTrue(balance.IsPowered(ids[0]));
        }
    }
}
