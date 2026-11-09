using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Автовырастание домов: тонкий адаптер тик → policy → действие, правило — в GrowthEligibilityPolicy.
    // Снос — в SettlementDemolitionController, тики — SettlementGrowthTicker. Runner обязателен, по умолчанию выключен.
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
        [Tooltip("Дефицит энергии/воды/мусора. Пусто = рост без штрафа.")] public SupplyGrowthController SupplyGrowth;
        [Header("Growth")]
        [Tooltip("Типы под зоны. Пусто = рост не работает.")] public List<BuildingDefinition> Definitions = new List<BuildingDefinition>();
        [Tooltip("Тиков часов между домами. Минимум 1.")] public int TicksPerGrowth = 5;
        [Tooltip("Потолок выросших домов. Минимум 1.")] public int MaxSettlements = 80;
        [Tooltip("Режим автовырастания. Выключите для ручной стройки.")] public bool placementActive = true;
        private BuildingSystem buildings;
        private SettlementSystem settlement;
        private readonly GrowthEligibilityPolicy eligibility = new GrowthEligibilityPolicy();
        private readonly List<BuildingState> states = new List<BuildingState>();
        private int tickCount;
        private ZoneType lastGrownZone = ZoneType.None;
        // Копия выросших домов для заселения. Отдаём копию, а не сам список.
        public List<BuildingState> Homes { get { return new List<BuildingState>(states); } }
        // Доступ к системе поселений для занятости. Может быть null до EnsureReady.
        public SettlementSystem Settlement { get { return settlement; } }
        // Сколько зданий выросло: метрика открытия новых типов.
        public int GrownCount { get { return states.Count; } }
        // Часов до следующего дома с учётом пробок и дефицита. Для HUD, не для логики.
        public int TicksToNextGrowth
        {
            get
            {
                int needed = GrowthPace.RequiredTicks(TicksPerGrowth,
                    Congestion != null ? Congestion.GrowthTickMultiplier : 1,
                    SupplyGrowth != null ? SupplyGrowth.GrowthTickMultiplier : 1, Treasury != null && Treasury.Treasury.IsInDeficit);
                return tickCount >= needed ? 0 : needed - tickCount;
            }
        }
        private void Start() { ClampLimits(); EnsureReady(); }
        // Вход тиков от SettlementGrowthTicker. Параметр времени не нужен: сам факт тика = один игровой час.
        public void OnClockTick(SimulationTime time)
        {
            if (!EnsureReady() || settlement == null) return;
            if (!eligibility.CanProgress(placementActive, settlement.SettledCount, MaxSettlements)) return;
            bool deficit = Treasury != null && Treasury.Treasury.IsInDeficit;
            if (!deficit) tickCount++;
            int neededTicks = GrowthPace.RequiredTicks(TicksPerGrowth,
                Congestion != null ? Congestion.GrowthTickMultiplier : 1,
                SupplyGrowth != null ? SupplyGrowth.GrowthTickMultiplier : 1, deficit);
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
        // Рост по тику: policy решает, контроллер заселяет и платит.
        private void TryGrow()
        {
            Treasury treasury = Treasury != null ? Treasury.Treasury : null;
            GrowthEligibilityPolicy.Verdict verdict = eligibility.Evaluate(placementActive, settlement,
                MaxSettlements, ZonePainter.Zones.GetZonedCells(), lastGrownZone, states.Count,
                Definitions, treasury);
            if (!verdict.Allowed) return;
            BuildingState state = settlement.Settle(verdict.Definition, verdict.Origin);
            if (state == null) return;
            if (treasury != null && !treasury.TrySpend(verdict.Definition.ConstructionCost, verdict.Definition.Id).Success)
            {
                settlement.Demolish(state);
                GridVisualizer.RefreshOccupied();
                return;
            }
            states.Add(state);
            lastGrownZone = ZonePainter.Zones.GetZone(verdict.Origin);
            View.AddBuilding(state, verdict.Definition, GridVisualizer.Grid.CellSize);
            GridVisualizer.RefreshOccupied();
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
            View.AddBuilding(state, def, GridVisualizer.Grid.CellSize, true);
            GridVisualizer.RefreshOccupied();
            return state;
        }
        // Дом по клетке для сноса. Нет дома — null.
        public BuildingState FindStateAt(GridPosition cell)
        {
            for (int i = 0; i < states.Count; i++)
            {
                List<GridPosition> cells = states[i].Cells;
                for (int c = 0; c < cells.Count; c++) if (cells[c] == cell) return states[i];
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
    }
}
