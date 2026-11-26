using NUnit.Framework;
using UnityEngine;

namespace Citybuilder
{
    // Регресс ревью w25: «Новая игра» не наследует долг, рынок и районы предыдущего слота v7.
    public class SaveV7ResetTests
    {
        [Test]
        public void NewGame_ResetsCreditMarketAndDistricts()
        {
            GameObject go = new GameObject("SaveV7Reset");
            LoadController load = go.AddComponent<LoadController>();
            load.Credit = new CreditLedger();
            load.Market = new MarketPricePolicy();
            load.Districts = new DistrictPolicy();
            // Состояние, пришедшее из загруженного слота v7.
            load.Credit.Restore(500, 5);
            load.Market.Restore(1.4);
            load.Districts.SetRate(2, TaxPolicy.Level.High);
            load.NewGame();
            Assert.AreEqual(0, load.Credit.Outstanding);
            Assert.AreEqual(0, load.Credit.InterestPercent);
            Assert.AreEqual(MarketPricePolicy.DefaultMultiplier, load.Market.Multiplier, 0.0001);
            Assert.AreEqual(0, load.Districts.DistrictIds().Length);
            Assert.AreEqual(0, load.RestoredMayorRating);
            Assert.AreEqual(0, load.RestoredMilestoneCounterV2);
            Object.DestroyImmediate(go);
        }
    }
}
