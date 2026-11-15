using System.Collections.Generic;
using NUnit.Framework;

namespace Citybuilder
{
    // Блэкаут (EPIC-50): детерминированное отключение при дефиците. Чистый C#, без Unity API.
    public class SupplyBlackoutTests
    {
        private const float Tolerance = 0.0001f;

        private static SupplyBalance MakeUnitCity(int homes, float supply, List<EntityId> ids)
        {
            SupplyBalance balance = new SupplyBalance();
            balance.SetSupply(EntityId.New(), supply);
            for (int i = 0; i < homes; i++)
            {
                EntityId id = EntityId.New();
                ids.Add(id);
                balance.SetDemand(id, 1f, SupplyBalance.TierResidential);
            }
            return balance;
        }

        private static int CountPowered(SupplyBalance balance, List<EntityId> ids)
        {
            int powered = 0;
            for (int i = 0; i < ids.Count; i++)
            {
                if (balance.IsPowered(ids[i])) powered++;
            }
            return powered;
        }

        [Test]
        public void NoShortage_AllPowered()
        {
            List<EntityId> ids = new List<EntityId>();
            SupplyBalance balance = MakeUnitCity(10, 10f, ids);
            Assert.AreEqual(0, balance.UnpoweredCount);
            Assert.AreEqual(10, CountPowered(balance, ids));
        }

        [Test]
        public void Deficit2Of10_CutsExactlyTwo()
        {
            List<EntityId> ids = new List<EntityId>();
            SupplyBalance balance = MakeUnitCity(10, 8f, ids);
            Assert.AreEqual(2, balance.UnpoweredCount);
            Assert.AreEqual(8, CountPowered(balance, ids));
        }

        [Test]
        public void SameState_TwoRuns_SameCutoffSet()
        {
            List<EntityId> ids = new List<EntityId>();
            SupplyBalance first = MakeUnitCity(10, 8f, ids);
            List<EntityId> expected = first.UnpoweredIds();

            SupplyBalance second = new SupplyBalance();
            second.SetSupply(EntityId.New(), 8f);
            for (int i = 0; i < ids.Count; i++) second.SetDemand(ids[i], 1f, SupplyBalance.TierResidential);
            List<EntityId> actual = second.UnpoweredIds();

            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++) Assert.AreEqual(expected[i], actual[i]);
        }

        [Test]
        public void Cutoff_IsMinimalPrefix()
        {
            List<EntityId> ids = new List<EntityId>();
            SupplyBalance balance = MakeUnitCity(10, 8f, ids);
            List<EntityId> cutoff = balance.UnpoweredIds();
            float shortage = balance.TotalDemand - balance.TotalSupply;
            float cutSum = 0f;
            for (int i = 0; i < cutoff.Count; i++) cutSum += 1f;
            // Набор закрывает нехватку, а без последнего элемента — нет: минимальный префикс.
            Assert.GreaterOrEqual(cutSum, shortage);
            Assert.Less(cutSum - 1f, shortage);
        }

        [Test]
        public void SmallShortage_OnlyServiceCut()
        {
            SupplyBalance balance = new SupplyBalance();
            balance.SetSupply(EntityId.New(), 6f);
            EntityId station = EntityId.New();
            balance.SetDemand(station, 1f, SupplyBalance.TierService);
            List<EntityId> homes = new List<EntityId>();
            for (int i = 0; i < 6; i++)
            {
                EntityId home = EntityId.New();
                homes.Add(home);
                balance.SetDemand(home, 1f, SupplyBalance.TierResidential);
            }
            List<EntityId> cutoff = balance.UnpoweredIds();
            Assert.AreEqual(1, cutoff.Count);
            Assert.AreEqual(station, cutoff[0]);
            Assert.AreEqual(6, CountPowered(balance, homes));
        }

        [Test]
        public void Services_CutBeforeResidential()
        {
            SupplyBalance balance = new SupplyBalance();
            balance.SetSupply(EntityId.New(), 5f);
            EntityId station = EntityId.New();
            balance.SetDemand(station, 1f, SupplyBalance.TierService);
            List<EntityId> homes = new List<EntityId>();
            for (int i = 0; i < 6; i++)
            {
                EntityId home = EntityId.New();
                homes.Add(home);
                balance.SetDemand(home, 1f, SupplyBalance.TierResidential);
            }
            List<EntityId> cutoff = balance.UnpoweredIds();
            Assert.AreEqual(2, cutoff.Count);
            Assert.AreEqual(station, cutoff[0]); // служба всегда раньше жилых
            Assert.AreEqual(5, CountPowered(balance, homes));
        }

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
