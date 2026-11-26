using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Состояние дорожной сети: система, список и вид. Ввод живёт в RoadPlacementController.
    public partial class RoadNetworkStore : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязателен: даёт Grid для RoadSystem.")]
        public GridVisualizer GridVisualizer;
        [Tooltip("Доступные типы дорог. Первый непустой — активный по умолчанию.")]
        public RoadDefinition[] Definitions;
        [Tooltip("Вид дорог. Пусто = создастся на этом же объекте.")]
        public RoadView View;
        private RoadSystem system;
        private readonly List<RoadState> states = new List<RoadState>();
        public RoadDefinition ActiveDefinition { get; private set; }
        public string ActiveTypeId { get { return ActiveDefinition == null || string.IsNullOrEmpty(ActiveDefinition.Id) ? "asphalt" : ActiveDefinition.Id; } }
        // Копия построенных дорог для сохранений. Отдаём копию, а не сам список.
        public List<RoadState> Roads { get { return new List<RoadState>(states); } }
        // Выбор активного типа из списка. Возвращает false для неизвестного id.
        public bool SetActiveDefinition(string id)
        {
            EnsureReady();
            if (Definitions == null || string.IsNullOrEmpty(id)) return false;
            for (int i = 0; i < Definitions.Length; i++)
                if (Definitions[i] != null && Definitions[i].Id == id) { ActiveDefinition = Definitions[i]; return true; }
            return false;
        }
        // Готовит систему по GridVisualizer.Grid, добирает View и активный тип. Без GridVisualizer гаснет.
        private bool EnsureReady()
        {
            if (GridVisualizer == null)
            {
                Debug.LogError("RoadNetworkStore: задайте GridVisualizer в Inspector.");
                enabled = false;
                return false;
            }
            if (ActiveDefinition == null && Definitions != null)
                for (int i = 0; i < Definitions.Length; i++)
                    if (Definitions[i] != null) { ActiveDefinition = Definitions[i]; break; }
            if (View == null)
            {
                View = GetComponent<RoadView>();
                if (View == null) View = gameObject.AddComponent<RoadView>();
            }
            if (system == null) system = new RoadSystem(GridVisualizer.Grid);
            if (bridge == null) bridge = new BridgeRoadNetwork(GridVisualizer.Grid);
            return true;
        }
        // Мост уровня 2 кладётся через BridgeRoadNetwork, обычная дорога — через RoadSystem.
        public RoadState Place(GridPosition cell)
        {
            if (!EnsureReady() || system == null) return null;
            if (IsBridgeActive())
            {
                RoadState bridgeState = PlaceBridge(cell);
                if (bridgeState != null) states.Add(bridgeState);
                return bridgeState;
            }
            if (IsOwnRoadCell(cell)) return null;
            RoadState placed = system.Place(cell);
            if (placed == null) return null;
            RegisterStreet(cell);
            states.Add(placed);
            return placed;
        }
        // Найти построенное на клетке: верхний уровень (мост) имеет приоритет.
        public RoadState FindStateAt(GridPosition cell) { return FindTopStateAt(cell); }
        // Снести построенное: снятие с сетки, учёт и RefreshOccupied. Вид обновляет вызывающий.
        public bool DemolishState(RoadState state)
        {
            if (!EnsureReady() || system == null || state == null) return false;
            if (!DemolishLayer(state)) return false;
            states.Remove(state);
            GridVisualizer.RefreshOccupied();
            return true;
        }
        // Восстановить дорогу из сохранения: без оплаты, тип из записи (пусто = asphalt).
        public RoadState RestoreRoad(GridPosition cell, string typeId = "asphalt")
        {
            RoadState state = PlaceRestored(cell, typeId);
            if (state == null) return null;
            states.Add(state);
            RefreshView();
            GridVisualizer.RefreshOccupied();
            return state;
        }
        // Снести все дороги (первый шаг загрузки слота).
        public void ClearRoads()
        {
            if (!EnsureReady() || system == null) return;
            for (int i = states.Count - 1; i >= 0; i--) DemolishLayer(states[i]);
            states.Clear();
            RefreshView();
            GridVisualizer.RefreshOccupied();
        }
        // Свободна ли клетка для стройки (не занята сеткой).
        public bool CanPlace(GridPosition cell)
        {
            if (!EnsureReady() || system == null) return false;
            if (IsBridgeActive()) return bridge.CanPlace(BridgeKey(cell), ActiveTypeId);
            return system.CanPlace(cell);
        }
        // Цена одной клетки по активному Definition. Без системы — 0.
        public int CellCost()
        {
            if (!EnsureReady() || system == null) return 0;
            return system.CellCost(ActiveDefinition);
        }
        // Перестроить вид: у каждого типа дороги свой цвет полотна, мосты — отдельно.
        public void RefreshView()
        {
            if (!EnsureReady()) return;
            Dictionary<string, List<GridPosition>> groups = new Dictionary<string, List<GridPosition>>();
            Dictionary<string, Color> colors = new Dictionary<string, Color>();
            if (Definitions != null)
                for (int i = 0; i < Definitions.Length; i++)
                {
                    if (Definitions[i] == null || string.IsNullOrEmpty(Definitions[i].Id)) continue;
                    colors[Definitions[i].Id] = Definitions[i].SurfaceColor;
                }
            for (int i = 0; i < states.Count; i++)
            {
                string typeId = states[i].TypeId;
                if (IsBridgeTypeId(typeId)) continue;
                List<GridPosition> cells;
                if (!groups.TryGetValue(typeId, out cells)) { cells = new List<GridPosition>(); groups[typeId] = cells; }
                cells.Add(states[i].Cell);
            }
            View.RebuildByType(groups, GridVisualizer.Grid.CellSize, colors);
            RefreshBridgeView(colors);
        }
    }
}
