using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Сейв v7 (issue #165, ADR-022): кирпичная дорога и мост переживают сохранение и загрузку.
    public class SaveV7RoundTripTests
    {
        [Test]
        public void PlayerAction_BrickAndBridge_RoundTripWithDebtAndRating()
        {
            List<RoadState> roads = new List<RoadState>
            {
                new RoadState(EntityId.New(), new GridPosition(1, 2), "brick", 1),
                new RoadState(EntityId.New(), new GridPosition(3, 4), "wood", 2)
            };
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(null, null, null, 100, 7, roads);
            snapshot.Debt = 500f;
            snapshot.DebtInterestPercent = 5f;
            snapshot.MayorRating = 42;
            CitySnapshot restored = SaveSlotStorage.FromJson(SaveSlotStorage.ToJson(snapshot));
            Assert.IsTrue(SaveSlotStorage.Validate(restored));
            Assert.AreEqual("brick", restored.Roads[0].Type);
            Assert.AreEqual(1, restored.Roads[0].Level);
            Assert.AreEqual("wood", restored.Roads[1].Type);
            Assert.AreEqual(2, restored.Roads[1].Level);
            Assert.AreEqual(500f, restored.Debt, 0.001f);
            Assert.AreEqual(42, restored.MayorRating);
        }

        [Test]
        public void LegacyV6Slot_WithoutV7Fields_ReadsWithDefaults()
        {
            CitySnapshot slot = SaveSlotStorage.FromJson("{\"Version\":6,\"Roads\":[{\"X\":1,\"Y\":2}]}");
            Assert.IsTrue(SaveSlotStorage.Validate(slot));
            SaveV6Snapshot.Normalize(slot);
            Assert.AreEqual("asphalt", SaveV7Snapshot.ReadRoadType(slot.Roads[0]));
            Assert.AreEqual(1, SaveV7Snapshot.ReadRoadLevel(slot.Roads[0]));
            Assert.AreEqual(0f, slot.Debt, 0.001f);
            Assert.AreEqual(1f, slot.MarketMultiplier, 0.001f);
        }

        [Test]
        public void FutureVersionEight_RejectedByValidation()
        {
            CitySnapshot slot = SaveSystem.BuildSnapshot(null, null, null, 0, 0, null);
            slot.Version = 8;
            Assert.IsFalse(SaveSlotStorage.Validate(slot));
        }

        [Test]
        public void SaveController_WritesV7FieldsFromInjectedSystems()
        {
            GameObject go = new GameObject("SaveV7RoundTrip");
            SaveController save = go.AddComponent<SaveController>();
            save.Credit = new CreditLedger(500, 5);
            save.Market = new MarketPricePolicy(1.4);
            save.Districts = new DistrictPolicy();
            save.Districts.SetRate(2, TaxPolicy.Level.High);
            save.MilestoneCounterV2 = 6;
            CitySnapshot snapshot = save.BuildCurrentSnapshot();
            Assert.AreEqual(500f, snapshot.Debt, 0.001f);
            Assert.AreEqual(5f, snapshot.DebtInterestPercent, 0.001f);
            Assert.AreEqual(1.4f, snapshot.MarketMultiplier, 0.001f);
            Assert.AreEqual("2", snapshot.DistrictTaxRates[0].DistrictId);
            Assert.AreEqual(3, snapshot.DistrictTaxRates[0].Rate);
            Assert.AreEqual(6, snapshot.MilestoneCounterV2);
            Object.DestroyImmediate(go);
        }
    }
}
