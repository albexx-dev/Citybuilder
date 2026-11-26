using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Районы (ADR-019): override ставки/множителя поверх ZoneType; без района — старое поведение. Чистая логика без Unity.
    public class DistrictPolicy
    {
        private readonly Dictionary<int, int> districtByCell = new Dictionary<int, int>();
        private readonly Dictionary<int, TaxPolicy.Level> rates = new Dictionary<int, TaxPolicy.Level>();
        private readonly Dictionary<int, int> multipliers = new Dictionary<int, int>();
        private readonly Dictionary<int, int> seats = new Dictionary<int, int>();

        // Клетка → район. Район 0 и ниже снимает привязку (базовая корзина).
        public void AssignCell(int cell, int district)
        {
            if (district <= 0) districtByCell.Remove(cell);
            else districtByCell[cell] = district;
        }

        public int DistrictOf(int cell) => districtByCell.ContainsKey(cell) ? districtByCell[cell] : 0;
        // Ставка района. Битый уровень снимает override — фолбэк на зону.
        public void SetRate(int district, TaxPolicy.Level level)
        {
            if (district <= 0) return;
            if (level == TaxPolicy.Level.Low || level == TaxPolicy.Level.Normal
                || level == TaxPolicy.Level.High) rates[district] = level;
            else rates.Remove(district);
        }

        public TaxPolicy.Level LevelFor(int district) => rates.ContainsKey(district) ? rates[district] : TaxPolicy.Level.Normal;

        // Рыночный множитель района: 1 по умолчанию, неположительный снимает override.
        public void SetMultiplier(int district, int multiplier)
        {
            if (district <= 0) return;
            if (multiplier < 1) multipliers.Remove(district);
            else multipliers[district] = multiplier;
        }

        public int MultiplierFor(int district) => multipliers.ContainsKey(district) ? multipliers[district] : 1;

        // Приоритет: override района > базовая зона. Нет района или None — как раньше.
        public int RateFor(TaxPolicy policy, ZoneType zone, int district)
        {
            if (policy == null || zone == ZoneType.None) return 0;
            if (district > 0 && rates.ContainsKey(district))
            {
                TaxPolicy.Level level = LevelFor(district);
                if (level == TaxPolicy.Level.Low) return 1;
                if (level == TaxPolicy.Level.High) return 4;
                return 2;
            }
            return policy.RateFor(zone);
        }

        public int RateForCell(TaxPolicy policy, ZoneType zone, int cell) => RateFor(policy, zone, DistrictOf(cell));

        public int IncomeFor(TaxPolicy policy, ZoneType zone, int district, int employed) =>
            employed > 0 ? employed * RateFor(policy, zone, district) : 0;

        // Рабочие места района: неизвестный район и неположительные значения пропускаются.
        public void AddSeats(int district, int count)
        {
            if (district <= 0 || count <= 0) return;
            seats[district] = (seats.ContainsKey(district) ? seats[district] : 0) + count;
        }

        // Идентификаторы районов по возрастанию — стабильный порядок строк интерфейса.
        public int[] DistrictIds()
        {
            int[] ids = new int[rates.Count];
            rates.Keys.CopyTo(ids, 0);
            Array.Sort(ids);
            return ids;
        }

        // Кумулятивные доли как в ZonalJobs: сумма ровно employed, нет мест — корзина 0.
        public Dictionary<int, int> DistributeEmployed(int employed)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            if (employed <= 0) return result;
            int total = 0;
            foreach (KeyValuePair<int, int> pair in seats) total += pair.Value;
            if (total <= 0) { result[0] = employed; return result; }
            int[] ids = new int[seats.Count];
            seats.Keys.CopyTo(ids, 0);
            Array.Sort(ids);
            int cumulative = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                int next = cumulative + employed * seats[ids[i]] / total;
                if (next > cumulative) result[ids[i]] = next - cumulative;
                cumulative = next;
            }
            if (cumulative < employed) result[0] = employed - cumulative;
            return result;
        }

        // Новая игра: привязки клеток и все override'ы снимаются.
        public void Clear()
        {
            districtByCell.Clear(); rates.Clear(); multipliers.Clear(); seats.Clear();
        }

        // Подпись строки района: «Район 2: высокий (4) +12 $/ч» (RU/EN из Localization).
        public string LabelFor(TaxPolicy policy, ZoneType zone, int district, int employed)
        {
            string name = district > 0 ? Localization.T("district_name", district) : ZoneName(zone);
            return Localization.T("zone_tax_label", name, LevelName(policy, zone, district),
                RateFor(policy, zone, district), IncomeFor(policy, zone, district, employed));
        }

        private static string ZoneName(ZoneType zone) => zone == ZoneType.Commercial
            ? Localization.T("zone_commercial")
            : zone == ZoneType.Industrial ? Localization.T("zone_industrial") : Localization.T("zone_residential");

        private string LevelName(TaxPolicy policy, ZoneType zone, int district)
        {
            TaxPolicy.Level level = rates.ContainsKey(district) ? rates[district]
                : policy != null ? policy.LevelFor(zone) : TaxPolicy.Level.Normal;
            if (level == TaxPolicy.Level.Low) return Localization.T("tax_level_low");
            if (level == TaxPolicy.Level.High) return Localization.T("tax_level_high");
            return Localization.T("tax_level_normal");
        }
    }
}
