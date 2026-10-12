using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Автовырастание домов на жилых зонах: таймер, first-fit, ПКМ-снос. Только старый Input.
    // Настройка сцены: компонент на объекте с GridVisualizer, по умолчанию выключен.
    public class SettlementGrowthController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязателен: даёт Grid и клетку под курсором.")] public GridVisualizer GridVisualizer;
        [Tooltip("Кисть зон. Обязательна: даёт ZoneSystem с жилыми клетками.")] public ZonePaintingController ZonePainter;
        [Tooltip("Вид выросших домов. Пусто = создастся на этом же объекте.")] public SettlementView View;
        [Tooltip("Казна для оплаты роста. Пусто = рост бесплатный.")] public TreasuryView Treasury;
        [Tooltip("Население. Опционален: при сносе выселяет жителей дома.")] public PopulationController PopulationSource;
        [Header("Growth")]
        [Tooltip("Типы под зоны. Пусто = рост не работает.")] public List<BuildingDefinition> Definitions = new List<BuildingDefinition>();
        [Tooltip("Пауза между домами в секундах. Минимум 1.")] public float GrowthIntervalSec = 5f;
        [Tooltip("Потолок выросших домов. Минимум 1.")] public int MaxSettlements = 10;
        [Tooltip("Режим автовырастания. Выключите для ручной стройки.")] public bool placementActive = true;
        private BuildingSystem buildings;
        private SettlementSystem settlement;
        private readonly List<BuildingState> states = new List<BuildingState>();
        private float timer;
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
        private void OnValidate()
        {
            ClampLimits();
        }
        // Проверяет ссылки и чинит логику после перезагрузки домена. Выросшее при reload теряется (не V1).
        private bool EnsureReady()
        {
            if (GridVisualizer == null || ZonePainter == null)
            {
                Debug.LogError("SettlementGrowthController: задайте GridVisualizer и ZonePainter.");
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
        private void Update()
        {
            if (!EnsureReady()) return;
            HandleDemolishInput();
            if (!placementActive || settlement == null) return;
            if (settlement.SettledCount >= MaxSettlements) return;
            timer += Time.deltaTime;
            if (timer < GrowthIntervalSec) return;
            timer = 0f;
            TryGrow();
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
            View.AddBuilding(state, def, GridVisualizer.Grid.CellSize);
            GridVisualizer.RefreshOccupied();
        }
        // Первая зонная клетка и первый подходящий тип под её зону.
        private bool FindFreeOrigin(out BuildingDefinition def, out GridPosition origin)
        {
            List<KeyValuePair<GridPosition, ZoneType>> zoned = ZonePainter.Zones.GetZonedCells();
            for (int i = 0; i < zoned.Count; i++)
            {
                GridPosition cell = zoned[i].Key;
                ZoneType zone = zoned[i].Value;
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
            def = null;
            origin = default(GridPosition);
            return false;
        }
        private bool MatchesZone(BuildingDefinition def, ZoneType zone)
        {
            return def.AllowedZone == ZoneType.None || def.AllowedZone == zone;
        }
        private void HandleDemolishInput()
        {
            if (!Input.GetMouseButtonDown(1) || UiClickGuard.IsOverMenu(Input.mousePosition)) return;
            GridPosition cell;
            if (!GridVisualizer.TryGetHoveredCell(out cell)) return;
            TryDemolish(cell);
        }
        private void TryDemolish(GridPosition cell)
        {
            int index = FindStateIndex(cell);
            if (index < 0) return;
            BuildingState state = states[index];
            settlement.Demolish(state);
            if (PopulationSource != null && PopulationSource.Population != null)
            {
                PopulationSource.Population.EvictHome(state);
            }
            View.RemoveBuilding(state);
            states.RemoveAt(index);
            GridVisualizer.RefreshOccupied();
        }
        private int FindStateIndex(GridPosition cell)
        {
            for (int i = 0; i < states.Count; i++)
            {
                List<GridPosition> cells = states[i].Cells;
                for (int c = 0; c < cells.Count; c++)
                {
                    if (cells[c] == cell) return i;
                }
            }
            return -1;
        }
        private void ClampLimits()
        {
            if (GrowthIntervalSec < 1f) GrowthIntervalSec = 1f;
            if (MaxSettlements < 1) MaxSettlements = 1;
        }
    }
}
