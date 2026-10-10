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
        [Tooltip("Что выращивать. Обычно House 2x2 с AllowedZone Residential.")] public BuildingDefinition Definition;
        [Tooltip("Вид выросших домов. Пусто = создастся на этом же объекте.")] public SettlementView View;
        [Header("Growth")]
        [Tooltip("Пауза между домами в секундах. Минимум 1.")] public float GrowthIntervalSec = 5f;
        [Tooltip("Потолок выросших домов. Минимум 1.")] public int MaxSettlements = 10;
        [Tooltip("Режим автовырастания. Выключите для ручной стройки.")] public bool placementActive = true;
        private BuildingSystem buildings;
        private SettlementSystem settlement;
        private readonly List<BuildingState> states = new List<BuildingState>();
        private float timer;
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
            if (GridVisualizer == null || ZonePainter == null || Definition == null)
            {
                Debug.LogError("SettlementGrowthController: задайте GridVisualizer, ZonePainter и Definition.");
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
            GridPosition origin;
            if (!FindFreeOrigin(out origin)) return;
            BuildingState state = settlement.Settle(Definition, origin);
            if (state == null) return;
            states.Add(state);
            View.AddBuilding(state, Definition, GridVisualizer.Grid.CellSize);
            GridVisualizer.RefreshOccupied();
        }
        // Первый Residential-кандидат, куда дом влезает целиком.
        private bool FindFreeOrigin(out GridPosition origin)
        {
            List<KeyValuePair<GridPosition, ZoneType>> zoned = ZonePainter.Zones.GetZonedCells();
            for (int i = 0; i < zoned.Count; i++)
            {
                if (zoned[i].Value != ZoneType.Residential) continue;
                if (settlement.CanSettle(Definition, zoned[i].Key))
                {
                    origin = zoned[i].Key;
                    return true;
                }
            }
            origin = default(GridPosition);
            return false;
        }
        private void HandleDemolishInput()
        {
            if (!Input.GetMouseButtonDown(1)) return;
            GridPosition cell;
            if (!GridVisualizer.TryGetHoveredCell(out cell)) return;
            TryDemolish(cell);
        }
        private void TryDemolish(GridPosition cell)
        {
            int index = FindStateIndex(cell);
            if (index < 0) return;
            settlement.Demolish(states[index]);
            View.RemoveBuilding(states[index]);
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
