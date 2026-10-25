using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Автовырастание домов на жилых зонах: счётчик тиков часов, first-fit. Только старый Input.
    // Снос выросших домов — в SettlementDemolitionController на том же объекте.
    // Тики даёт SettlementGrowthTicker на том же объекте. Runner обязателен, по умолчанию выключен.
    public class SettlementGrowthController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязателен: даёт Grid и клетку под курсором.")] public GridVisualizer GridVisualizer;
        [Tooltip("Кисть зон. Обязательна: даёт ZoneSystem с жилыми клетками.")] public ZonePaintingController ZonePainter;
        [Tooltip("Вид выросших домов. Пусто = создастся на этом же объекте.")] public SettlementView View;
        [Tooltip("Казна для оплаты роста. Пусто = рост бесплатный.")] public TreasuryView Treasury;
        [Tooltip("Драйвер часов. Обязателен: даёт тики часов для роста.")] public SimulationRunner Runner;
        [Header("Transport")]
        [Tooltip("Пробки. Пусто = рост без пробок.")] public CongestionController Congestion;
        [Header("Growth")]
        [Tooltip("Типы под зоны. Пусто = рост не работает.")] public List<BuildingDefinition> Definitions = new List<BuildingDefinition>();
        [Tooltip("Тиков часов между домами. Минимум 1.")] public int TicksPerGrowth = 5;
        [Tooltip("Потолок выросших домов. Минимум 1.")] public int MaxSettlements = 30;
        [Tooltip("Режим автовырастания. Выключите для ручной стройки.")] public bool placementActive = true;
        private BuildingSystem buildings;
        private SettlementSystem settlement;
        private readonly List<BuildingState> states = new List<BuildingState>();
        private const int MaxRequiredTicks = 1000;
        private int tickCount;
        private ZoneType lastGrownZone = ZoneType.None;
        // Копия выросших домов для заселения. Отдаём копию, а не сам список.
        public List<BuildingState> Homes
        {
            get
            {
                return new List<BuildingState>(states);
            }
        }
        // Доступ к системе поселений для занятости. Может быть null до EnsureReady.
        public SettlementSystem Settlement
        {
            get
            {
                return settlement;
            }
        }
        private void Start()
        {
            ClampLimits();
            EnsureReady();
        }
        // Вход тиков от SettlementGrowthTicker. Параметр времени не нужен: сам факт тика = один игровой час.
        public void OnClockTick(SimulationTime time)
        {
            if (!placementActive) return;
            if (!EnsureReady() || settlement == null) return;
            if (settlement.SettledCount >= MaxSettlements) return;
            tickCount++;
            int neededTicks = RequiredTicks(TicksPerGrowth, Congestion != null ? Congestion.GrowthTickMultiplier : 1);
            if (tickCount < neededTicks) return;
            tickCount = 0;
            TryGrow();
        }
        private void OnValidate() { ClampLimits(); }
        // Проверяет ссылки и чинит логику после перезагрузки домена. Выросшее при reload теряется (не V1).
        private bool EnsureReady()
        {
            if (GridVisualizer == null || ZonePainter == null || Runner == null)
            {
                Debug.LogError("SettlementGrowthController: задайте GridVisualizer, ZonePainter и Runner.");
                enabled = false;
                return false;
            }
            if (Definitions == null) Definitions = new List<BuildingDefinition>();
            for (int i = Definitions.Count - 1; i >= 0; i--) if (Definitions[i] == null) Definitions.RemoveAt(i);
            if (Definitions.Count == 0)
            {
                Debug.LogError("SettlementGrowthController: задайте Definitions.");
                enabled = false;
                return false;
            }
            if (View == null) View = GetComponent<SettlementView>();
            if (View == null) View = gameObject.AddComponent<SettlementView>();
            if (buildings == null) buildings = new BuildingSystem(GridVisualizer.Grid);
            if (settlement == null)
            {
                ZoneSystem zones = ZonePainter.Zones;
                if (zones == null) return false;
                settlement = new SettlementSystem(GridVisualizer.Grid, zones, buildings);
            }
            return true;
        }
        private void TryGrow()
        {
            BuildingDefinition def;
            GridPosition origin;
            if (!FindFreeOrigin(out def, out origin)) return;
            if (Treasury != null && Treasury.Treasury != null)
            {
                var spent = Treasury.Treasury.TrySpend(def.ConstructionCost, def.Id);
                if (!spent.Success) return;
            }
            BuildingState state = settlement.Settle(def, origin);
            if (state == null) return;
            states.Add(state);
            lastGrownZone = ZonePainter.Zones.GetZone(origin);
            View.AddBuilding(state, def, GridVisualizer.Grid.CellSize);
            GridVisualizer.RefreshOccupied();
        }
        // Первая зонная клетка и первый подходящий тип под её зону.
        // Чередование: сначала чужие зоны (не как прошлый рост), потом все.
        private bool FindFreeOrigin(out BuildingDefinition def, out GridPosition origin)
        {
            List<KeyValuePair<GridPosition, ZoneType>> zoned = ZonePainter.Zones.GetZonedCells();
            for (int pass = 0; pass < 2; pass++)
            {
            for (int i = 0; i < zoned.Count; i++)
            {
                GridPosition cell = zoned[i].Key;
                ZoneType zone = zoned[i].Value;
                if (pass == 0 && zone == lastGrownZone) continue;
                for (int d = 0; d < Definitions.Count; d++)
                {
                    BuildingDefinition candidate = Definitions[d];
                    if (candidate == null) continue;
                    if (!MatchesZone(candidate, zone)) continue;
                    if (settlement.CanSettle(candidate, cell))
                    {
                        def = candidate;
                        origin = cell;
                        return true;
                    }
                }
            }
            }
            def = null;
            origin = default(GridPosition);
            return false;
        }
        private bool MatchesZone(BuildingDefinition def, ZoneType zone)
        {
            return def.AllowedZone == ZoneType.None || def.AllowedZone == zone;
        }
        // Восстановить выросший дом из сохранения: без оплаты, вид и занятость обновляются.
        public BuildingState RestoreGrown(string defId, GridPosition origin)
        {
            if (!EnsureReady() || settlement == null) return null;
            BuildingDefinition def = null;
            for (int i = 0; i < Definitions.Count; i++) if (Definitions[i] != null && Definitions[i].Id == defId) def = Definitions[i];
            if (def == null) return null;
            BuildingState state = settlement.Settle(def, origin);
            if (state == null) return null;
            states.Add(state);
            View.AddBuilding(state, def, GridVisualizer.Grid.CellSize);
            GridVisualizer.RefreshOccupied();
            return state;
        }

        // Дом по клетке для сноса. Нет дома — null.
        public BuildingState FindStateAt(GridPosition cell)
        {
            for (int i = 0; i < states.Count; i++)
            {
                List<GridPosition> cells = states[i].Cells;
                for (int c = 0; c < cells.Count; c++)
                {
                    if (cells[c] == cell) return states[i];
                }
            }
            return null;
        }
        // Снос одного выросшего дома: занятость, вид, список, сетка. Выселение — в Demolition.
        public void DemolishState(BuildingState state)
        {
            if (state == null || settlement == null) return;
            settlement.Demolish(state);
            View.RemoveBuilding(state);
            states.Remove(state);
            GridVisualizer.RefreshOccupied();
        }
        private void ClampLimits()
        {
            if (TicksPerGrowth < 1) TicksPerGrowth = 1;
            if (MaxSettlements < 1) MaxSettlements = 1;
        }

        // Тиков на дом с учётом пробок. Чистая функция: клампы снизу 1, сверху 1000.
        public static int RequiredTicks(int baseTicks, int multiplier)
        {
            if (baseTicks <= 0) return 1;
            if (multiplier <= 0) multiplier = 1;
            long ticks = (long)baseTicks * multiplier;
            return ticks > MaxRequiredTicks ? MaxRequiredTicks : (int)ticks;
        }
    }
}
