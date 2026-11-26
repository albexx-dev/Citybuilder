using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Мосты level 2 поверх обычных дорог (ADR-018): occupation, съезды и вид в RoadNetworkStore.
    public partial class RoadNetworkStore : MonoBehaviour
    {
        private BridgeRoadNetwork bridge;
        private BridgeRoadView bridgeView;

        private bool IsBridgeActive() { return activeLevel >= RoadCellKey.BridgeLevel; }
        private static bool IsBridgeTypeId(string id) { return id == BridgeRoadNetwork.Wood || id == BridgeRoadNetwork.Stone; }
        private static RoadCellKey BridgeKey(GridPosition cell) { return new RoadCellKey(cell.X, cell.Y, RoadCellKey.BridgeLevel); }
        private static RoadCellKey StreetKey(GridPosition cell) { return new RoadCellKey(cell.X, cell.Y, RoadCellKey.StreetLevel); }

        // Дорога другого уровня не делает клетку «своей» для текущей кисти.
        private bool HasRoadAt(GridPosition cell, bool bridgeLayer)
        {
            for (int i = 0; i < states.Count; i++)
                if (states[i].Cell == cell && IsBridgeTypeId(states[i].TypeId) == bridgeLayer) return true;
            return false;
        }
        public bool IsOwnRoadCell(GridPosition cell) { return HasRoadAt(cell, IsBridgeActive()); }

        // При выборе и сносе верхний уровень (мост) имеет приоритет.
        public RoadState FindTopStateAt(GridPosition cell)
        {
            for (int i = 0; i < states.Count; i++)
                if (states[i].Cell == cell && IsBridgeTypeId(states[i].TypeId)) return states[i];
            for (int i = 0; i < states.Count; i++)
                if (states[i].Cell == cell) return states[i];
            return null;
        }

        private RoadState PlaceBridgeAs(GridPosition cell, string typeId, int level = RoadCellKey.BridgeLevel)
        {
            RoadCellKey key = BridgeKey(cell);
            if (!bridge.Place(key, typeId)) return null;
            if (bridge.IsRoad(StreetKey(cell))) bridge.SetRamp(StreetKey(cell), key);
            return new RoadState(EntityId.New(), cell, typeId, key.Level);
        }
        private RoadState PlaceBridge(GridPosition cell) { return PlaceBridgeAs(cell, ActiveTypeId, ActiveLevel); }

        // Легаси-дорогу добавляем в bridge без повторного Occupy: под мостом она остаётся маршрутной.
        private void RegisterStreet(GridPosition cell)
        {
            bridge.RegisterStreet(StreetKey(cell));
            if (bridge.IsRoad(BridgeKey(cell))) bridge.SetRamp(StreetKey(cell), BridgeKey(cell));
        }
        private RoadState PlaceRestored(GridPosition cell, string typeId, int level = 1)
        {
            if (!EnsureReady() || system == null) return null;
            if (IsBridgeTypeId(typeId)) return PlaceBridgeAs(cell, typeId, level);
            if (IsOwnRoadCell(cell)) return null;
            RoadState placed = system.Place(cell);
            if (placed == null) return null;
            RegisterStreet(cell);
            return new RoadState(placed.Id, placed.Cell, typeId, level);
        }

        private bool DemolishLayer(RoadState state)
        {
            bool isBridge = IsBridgeTypeId(state.TypeId);
            if (isBridge) return bridge.Demolish(BridgeKey(state.Cell));
            if (!system.Demolish(state)) return false;
            bridge.Demolish(StreetKey(state.Cell));
            return true;
        }

        // A* по мостам: старт и цель берутся на верхнем уровне, если он есть.
        public List<RoadCellKey> FindBridgePath(GridPosition from, GridPosition to)
        {
            if (!EnsureReady() || bridge == null) return null;
            return bridge.FindPath(AtTopLevel(from), AtTopLevel(to));
        }
        private RoadCellKey AtTopLevel(GridPosition cell)
        {
            int level = bridge.TopLevel(cell);
            return new RoadCellKey(cell.X, cell.Y, level < 1 ? RoadCellKey.StreetLevel : level);
        }

        // Палуба с перилами рисуется отдельным видом, не плоским полотном.
        private void RefreshBridgeView(Dictionary<string, Color> colors)
        {
            if (bridgeView == null)
            {
                bridgeView = GetComponent<BridgeRoadView>();
                if (bridgeView == null) bridgeView = gameObject.AddComponent<BridgeRoadView>();
            }
            if (bridgeView == null) return;
            Dictionary<string, List<RoadCellKey>> decks = new Dictionary<string, List<RoadCellKey>>();
            for (int i = 0; i < states.Count; i++)
            {
                if (!IsBridgeTypeId(states[i].TypeId)) continue;
                List<RoadCellKey> cells;
                if (!decks.TryGetValue(states[i].TypeId, out cells)) { cells = new List<RoadCellKey>(); decks[states[i].TypeId] = cells; }
                cells.Add(BridgeKey(states[i].Cell));
            }
            bridgeView.Rebuild(decks, GridVisualizer.Grid.CellSize, colors);
        }
    }
}
