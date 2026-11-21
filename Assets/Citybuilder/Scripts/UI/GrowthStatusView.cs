using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Строка HUD «как идёт рост»: тики до следующего дома или причина остановки.
    // Вешать на Grid: рост и меню ищутся на этом же объекте.
    public class GrowthStatusView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Автовырастание домов. Пусто = ищется на этом же объекте.")]
        public SettlementGrowthController Growth;
        [Tooltip("Меню режимов. Пусто = ищется на этом же объекте.")]
        public BuildModeMenu Menu;
        [Tooltip("Население города (для привлекательности). Пусто = ищется в сцене.")]
        public PopulationController Population;

        // Для E2E-моста.
        public GrowthStatusPolicy.Kind CurrentKind
        {
            get { return PickKind(); }
        }

        public string CurrentText
        {
            get { return BuildText(); }
        }

        private void OnGUI()
        {
            string text = CurrentText;
            if (string.IsNullOrEmpty(text)) return;
            if (!HudLayout.IsHudRowVisible(454f, 0, Screen.height)) return;
            // y=454: под стартовой подсказкой (430); на низких окнах строка поджимается к низу.
            HudText.Draw(10, HudLayout.FixedOrBottom(454f, 0), text);
        }

        private GrowthStatusPolicy.Kind PickKind()
        {
            ResolveRefs();
            if (Growth == null) return GrowthStatusPolicy.Kind.None;
            bool growthMode = Menu != null && Menu.CurrentMode == BuildModeMenu.Mode.Growth;
            return GrowthStatusPolicy.Pick(Growth.GrownCount, CountResidential(), growthMode,
                Growth.MaxSettlements, CheapestCost(), Balance());
        }

        private void ResolveRefs()
        {
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Menu == null) Menu = GetComponent<BuildModeMenu>();
            if (Population == null) Population = FindFirstObjectByType<PopulationController>();
        }

        private int CountResidential()
        {
            ZoneSystem zones = Growth != null && Growth.ZonePainter != null ? Growth.ZonePainter.Zones : null;
            if (zones == null) return 0;
            List<KeyValuePair<GridPosition, ZoneType>> cells = zones.GetZonedCells();
            int count = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].Value == ZoneType.Residential) count++;
            }
            return count;
        }

        // Дешёвый доступный тип: по нему рост проверяет деньги.
        private int CheapestCost()
        {
            if (Growth == null || Growth.Definitions == null) return 0;
            int cheapest = int.MaxValue;
            int grown = Growth.GrownCount;
            for (int i = 0; i < Growth.Definitions.Count; i++)
            {
                BuildingDefinition def = Growth.Definitions[i];
                if (def == null || !UnlockPolicy.IsUnlocked(def, grown)) continue;
                if (def.ConstructionCost < cheapest) cheapest = def.ConstructionCost;
            }
            return cheapest == int.MaxValue ? 0 : cheapest;
        }

        // Казна не подключена — денежного гейта нет.
        private int Balance()
        {
            if (Growth == null || Growth.Treasury == null || Growth.Treasury.Treasury == null) return int.MaxValue;
            return Growth.Treasury.Treasury.Balance;
        }

        private string BuildText()
        {
            GrowthStatusPolicy.Kind kind = PickKind();
            if (kind == GrowthStatusPolicy.Kind.None) return string.Empty;
            string head = Localization.T("growth_head", Growth.GrownCount, GrowthStatusPolicy.HousesWord(Growth.GrownCount));
            switch (kind)
            {
                case GrowthStatusPolicy.Kind.GrowthOff:
                    return head + Localization.T("growth_off");
                case GrowthStatusPolicy.Kind.NoZones:
                    return head + Localization.T("growth_no_zones");
                case GrowthStatusPolicy.Kind.AtCapacity:
                    return head + Localization.T("growth_capacity", Growth.MaxSettlements);
                case GrowthStatusPolicy.Kind.NoMoney:
                    return head + Localization.T("growth_no_money", CheapestCost());
                case GrowthStatusPolicy.Kind.Deficit:
                    return head + Localization.T("growth_deficit");
                default:
                    float attr = Population != null ? Population.CurrentAttractiveness : 1f;
                    return head + Localization.T("growth_next", Growth.TicksToNextGrowth)
                        + SlowSuffix() + GrowthStatusPolicy.LowAttractivenessSuffix(attr);
            }
        }

        // Замедление называем причиной, а не только множителем.
        private string SlowSuffix()
        {
            int supply = Growth != null && Growth.SupplyGrowth != null ? Growth.SupplyGrowth.GrowthTickMultiplier : 1;
            int jams = Growth != null && Growth.Congestion != null ? Growth.Congestion.GrowthTickMultiplier : 1;
            int worst = Mathf.Max(supply, jams);
            if (worst <= 1) return string.Empty;
            string reason = supply > 1 && jams > 1 ? Localization.T("growth_slow_both")
                : supply > 1 ? Localization.T("growth_slow_supply") : Localization.T("growth_slow_jams");
            return Localization.T("growth_slow", worst, reason);
        }
    }
}
