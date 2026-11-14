using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Мост пробок в рост (067), запрос нагрузки клеток, перекраска дорог раз в тик и сводка HUD (EPIC-15).
    public class CongestionController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязателен: даёт Grid для маршрутов и подсчёта пробок.")]
        public GridVisualizer GridVisualizer;
        [Tooltip("Сеть дорог (для перекраски по нагрузке). Пусто = ищется на этом же объекте.")]
        public RoadNetworkStore Roads;
        [Tooltip("Драйвер часов (для обновления цвета раз в тик). Пусто = ищется в сцене.")]
        public SimulationRunner Runner;

        [Header("Congestion")]
        [Tooltip("С какой нагрузки клетки начинается пробка. Минимум 1.")]
        public int Threshold = 4;
        [Tooltip("Прибавка множителя за поездку сверх порога. Минимум 0.")]
        public float Factor = 0.25f;
        [Tooltip("Потолок множителя. Минимум 1.")]
        public float MaxMultiplier = 3f;

        private RouteSystem routes;
        private CongestionSystem congestion;

        public CongestionSystem System { get { BuildSystems(); return congestion; } }
        public RouteSystem Routes { get { BuildSystems(); return routes; } }
        public float Multiplier { get { BuildSystems(); return congestion == null ? 1f : congestion.WorstMultiplier(); } }
        public int GrowthTickMultiplier { get { BuildSystems(); return congestion == null ? 1 : congestion.GrowthTickMultiplier(); } }
        public string CurrentSummaryText { get { BuildSystems(); return congestion == null ? "Пробки: 0 (худшая клетка 0%, рост ×1)" : congestion.FormatSummaryLine(); } }

        public float Load01(GridPosition cell) { BuildSystems(); return congestion == null ? 0f : congestion.Load01(cell); }

        private void Start() { ClampLimits(); EnsureReady(); }
        private void OnValidate() { ClampLimits(); ApplySettings(); }

        private void OnEnable()
        {
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Runner != null && Runner.Clock != null) Runner.Clock.TickCompleted += OnClockTick;
        }

        private void OnDisable()
        {
            if (Runner != null && Runner.Clock != null) Runner.Clock.TickCompleted -= OnClockTick;
        }

        private void OnClockTick(SimulationTime time) => RefreshRoadColors((int)time.TickCount);

        public bool RefreshRoadColors(int tick)
        {
            if (!EnsureReady()) return false;
            if (Roads == null) Roads = GetComponent<RoadNetworkStore>();
            if (Roads == null || Roads.View == null) return false;
            List<RoadState> states = Roads.Roads;
            List<GridPosition> cells = new List<GridPosition>(states.Count);
            for (int i = 0; i < states.Count; i++) cells.Add(states[i].Cell);
            if (routes != null) routes.Rebuild(cells);
            return Roads.View.RefreshCongestionOncePerTick(tick, cells, GridVisualizer.Grid.CellSize, Load01);
        }

        public void RebuildRoutes()
        {
            if (routes == null) return;
            if (Roads == null) Roads = GetComponent<RoadNetworkStore>();
            if (Roads == null || Roads.Roads == null) return;
            List<RoadState> states = Roads.Roads;
            List<GridPosition> cells = new List<GridPosition>(states.Count);
            for (int i = 0; i < states.Count; i++) cells.Add(states[i].Cell);
            routes.Rebuild(cells);
        }

        public void TrackTrip(IReadOnlyList<GridPosition> path)
        {
            if (path == null || path.Count == 0 || !EnsureReady()) return;
            if (congestion != null) congestion.TrackTrip(path);
            if (routes != null)
            {
                List<GridPosition> list = path as List<GridPosition>;
                if (list == null)
                {
                    list = new List<GridPosition>(path.Count);
                    for (int i = 0; i < path.Count; i++) list.Add(path[i]);
                }
                routes.TrackLoad(list);
            }
        }

        public void ClearLoads()
        {
            if (!EnsureReady()) return;
            if (congestion != null) congestion.ClearLoads();
            if (routes != null) routes.ClearLoads();
        }

        private bool EnsureReady()
        {
            if (GridVisualizer == null)
            {
                Debug.LogError("CongestionController: задайте GridVisualizer в Inspector.");
                enabled = false;
                return false;
            }
            if (Roads == null) Roads = GetComponent<RoadNetworkStore>();
            BuildSystems();
            return true;
        }

        private void BuildSystems()
        {
            if (GridVisualizer == null) return;
            if (routes == null)
            {
                routes = new RouteSystem(GridVisualizer.Grid);
                RebuildRoutes();
            }
            if (congestion == null)
            {
                congestion = new CongestionSystem(routes, GridVisualizer.Grid);
                ApplySettings();
            }
        }

        private void ApplySettings()
        {
            if (congestion == null) return;
            congestion.Threshold = Threshold;
            congestion.Factor = Factor;
            congestion.MaxMultiplier = MaxMultiplier;
        }

        private void ClampLimits()
        {
            if (Threshold < 1) Threshold = 1;
            if (Factor < 0f) Factor = 0f;
            if (MaxMultiplier < 1f) MaxMultiplier = 1f;
        }
    }
}
