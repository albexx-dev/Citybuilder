using System;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса EconomyTicker (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class EconomyTicker : MonoBehaviour
    {
        private DistrictPolicy districts;

        // Районы зон: три корзины без override ставок. Один экземпляр для налога и инфоменю.
        public DistrictPolicy Districts
        {
            get
            {
                if (districts == null)
                {
                    districts = new DistrictPolicy();
                    MapZones(districts);
                }
                return districts;
            }
        }

        // Рейтинг мэра из QuestSystem: модификатор ставки налога, 0 в начале партии.
        public int MayorRatingValue;

        private void ResetDistricts()
        {
            DistrictPolicy policy = Districts;
            policy.Clear();
            MapZones(policy);
            MayorRatingValue = 0;
        }

        private static void MapZones(DistrictPolicy policy)
        {
            policy.AssignZone(ZoneType.Residential, 1);
            policy.AssignZone(ZoneType.Commercial, 2);
            policy.AssignZone(ZoneType.Industrial, 3);
        }

        private void ReportCongestion(CityEventLog log, int tick)
        {
            if (Growth == null || Growth.Congestion == null || Growth.Congestion.GrowthTickMultiplier <= 1) return;
            if (!eventThrottle.Allow(EventKind.Congestion.ToString(), tick)) return;
            log.Add(EventKind.Congestion, EventMessageCatalog.CongestionSlowdown,
                new object[] { Growth.Congestion.GrowthTickMultiplier }, tick);
        }
        private void ReportDeficit(CityEventLog log, int tick)
        {
            if (Treasury == null || Treasury.Treasury == null || !Treasury.Treasury.IsInDeficit) return;
            if (!eventThrottle.Allow(EventKind.Treasury.ToString(), tick)) return;
            log.Add(EventKind.Treasury, EventMessageCatalog.TreasuryDeficit,
                new object[] { Treasury.Treasury.Balance }, tick);
        }
        private void ReportEmployment(CityEventLog log, int tick)
        {
            if (employment == null || Population.Population.TotalResidents <= 0) return;
            if (employment.EmploymentRate >= Population.MinEmployment) return;
            if (!eventThrottle.Allow(EventKind.Employment.ToString(), tick)) return;
            int pct = (int)Math.Round(employment.EmploymentRate * 100f);
            log.Add(EventKind.Employment, EventMessageCatalog.UnemploymentHigh, new object[] { pct }, tick);
        }
        // Пожары (EPIC-43, ADR-009): тик FireSystem по домам, разрушение и лента.
        private void TickFires(int tick)
        {
            if (Growth == null) return;
            fireTicker.Tick(tick, Growth.Homes, Coverage != null ? Coverage.System : null,
                EventLog != null ? EventLog.Log : null,
                Population != null ? Population.Population : null, Growth.DemolishState);
        }
        // Кражи (EPIC-47): дома без покрытия полиции воруют — штраф казне и событие в ленту.
        private void TickCrimes(int tick)
        {
            crimeTicker.Tick(tick, Growth != null ? Growth.Homes : null,
                Coverage != null ? Coverage.System : null, EventLog != null ? EventLog.Log : null,
                Treasury.Treasury);
        }
    }
}
