using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса SaveController (v7, ADR-022): запись опциональных полей слота.
    // Системы опциональны: в старой сцене пишутся нейтральные дефолты v6.
    public partial class SaveController : MonoBehaviour
    {
        [Tooltip("Кредит v7 (ADR-022). Пусто = долг и процент 0.")] public CreditLedger Credit;
        [Tooltip("Рынок v7. Пусто = множитель ×1.")] public MarketPricePolicy Market;
        [Tooltip("Районы v7. Пусто = без ставок районов.")] public DistrictPolicy Districts;
        [Tooltip("Счётчик вех v2. 0 = не задан.")] public int MilestoneCounterV2;

        internal void WriteV7(CitySnapshot snapshot)
        {
            if (snapshot == null) return;
            snapshot.Debt = Credit == null ? 0f : Credit.Outstanding;
            snapshot.DebtInterestPercent = Credit == null ? 0f : Credit.InterestPercent;
            snapshot.MarketMultiplier = Market == null ? 1f : (float)Market.Multiplier;
            snapshot.MayorRating = ReadMayorRating();
            snapshot.MilestoneCounterV2 = MilestoneCounterV2;
            WriteDistricts(snapshot);
            SaveV7Snapshot.Write(snapshot);
        }

        private int ReadMayorRating()
        {
            QuestLogView quests = QuestLog != null ? QuestLog : FindFirstObjectByType<QuestLogView>();
            int completed = quests == null || quests.Quests == null ? 0 : quests.Quests.CompletedCount;
            int residents = Population == null || Population.Population == null ? 0 : Population.Population.TotalResidents;
            return MayorRating.Compute(completed, residents, ReadBalance());
        }

        private void WriteDistricts(CitySnapshot snapshot)
        {
            snapshot.DistrictTaxRates = new List<CitySnapshot.DistrictTaxEntry>();
            if (Districts == null) return;
            int[] ids = Districts.DistrictIds();
            for (int i = 0; i < ids.Length; i++)
            {
                CitySnapshot.DistrictTaxEntry entry = new CitySnapshot.DistrictTaxEntry();
                entry.DistrictId = ids[i].ToString();
                entry.Rate = SaveSystem.EncodeTaxLevel(Districts.LevelFor(ids[i]));
                snapshot.DistrictTaxRates.Add(entry);
            }
        }
    }
}
