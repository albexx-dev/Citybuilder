using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Вид мусоровозов (EPIC-52): GarbageTicker + пул, тик даёт дома, депо, ленту и RouteSystem.
    // Полигон — ручная постройка с IsWasteDepot (issue #90); из депо берётся самое ёмкое.
    public class GarbageTruckView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Экономика города: даёт часы, дома, ручную стройку и ленту. Пусто = ищется в сцене.")]
        public EconomyTicker Economy;
        [Tooltip("Сетка: размер клетки и мировые координаты. Обязателен.")]
        public GridVisualizer Visualizer;
        [Tooltip("Пробки: Routes даёт пути по дорогам игрока. Пусто = ищется в сцене; нет конгестиона — прямой L-маршрут.")]
        public CongestionController Congestion;

        [Header("Model")]
        [Tooltip("FBX-модели мусоровозов (Kenney Racing). Пусто = мусоровозы без вида.")]
        public GameObject[] TruckModels;
        [Tooltip("Доворот модели вокруг Y, градусы. Нос Kenney-машины смотрит по −Z: 180 выравнивает по ходу.")]
        public float ModelYawOffset = 180f;
        [Tooltip("Множитель масштаба поверх родного масштаба FBX (машина ~13.5 м: при CellSize 1 попробуйте 0.06).")]
        public float ModelScale = 1f;
        [Tooltip("Высота корпуса над землёй, метры.")] public float RideHeight = 0.05f;
        [Tooltip("Показывать модели мусоровозов. false — логика вывоза, оверлей и HUD работают без моделей.")] public bool ShowTruckVisuals = false;

        private readonly GarbageTicker ticker = new GarbageTicker();
        private GarbageTruckPool pool;
        private bool subscribed;
        private GridPosition? cachedDepot;
        private int placedRevision;

        // Строка HUD для InfoMenuView (эпик #52): очередь вывоза и мусоровозы в пути.
        public string ServiceText => "Вывоз мусора: ждут домов " + ticker.Garbage.WaitingCount + ", в пути " + ticker.Garbage.ActiveTrucks;

        public GarbageSystem Garbage { get { return ticker.Garbage; } }
        public bool HasDepot { get { return cachedDepot.HasValue; } }

        // Сброс по команде LoadController.ClearAll; также срабатывает при отмотке часов.
        public void ResetGarbage()
        {
            ticker.Reset();
            RefreshDepot();
        }

        public void RefreshDepot()
        {
            cachedDepot = FindDepot();
            if (Economy != null && Economy.Placement != null) placedRevision = Economy.Placement.PlacedRevision;
        }

        private void OnEnable()
        {
            subscribed = false;
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (subscribed && Economy != null && Economy.Runner != null && Economy.Runner.Clock != null)
                Economy.Runner.Clock.TickCompleted -= OnClockTick;
            subscribed = false;
        }

        private void OnDestroy()
        {
            if (pool != null) pool.DestroyAll();
        }

        private void Update()
        {
            if (!subscribed) TrySubscribe();
            SimulationClock clock = Economy != null && Economy.Runner != null ? Economy.Runner.Clock : null;
            if (clock == null) return;
            if (Economy != null && Economy.Placement != null && placedRevision != Economy.Placement.PlacedRevision) RefreshDepot();
            if (clock.Speed != SimulationSpeed.Paused) ticker.Garbage.Advance(Time.deltaTime);
            if (pool != null) pool.Sync(ticker.Garbage, Visualizer);
        }

        private void TrySubscribe()
        {
            if (Economy == null) Economy = FindFirstObjectByType<EconomyTicker>();
            if (Visualizer == null) Visualizer = FindFirstObjectByType<GridVisualizer>();
            if (Visualizer == null) return;
            if (pool == null) pool = new GarbageTruckPool(transform, TruckModels, GarbageTrucks.MaxTrucks,
                ModelScale, ModelYawOffset, RideHeight, Visualizer.Grid.CellSize, ShowTruckVisuals);
            GarbageOverlayView.Ensure(this);
            SimulationClock clock = Economy != null && Economy.Runner != null ? Economy.Runner.Clock : null;
            if (subscribed || clock == null) return;
            clock.TickCompleted += OnClockTick;
            subscribed = true;
        }

        private void OnClockTick(SimulationTime time)
        {
            if (Economy == null || Economy.Growth == null) return;
            RefreshDepot();
            ticker.Tick((int)time.TickCount, Economy.Growth.Homes, cachedDepot,
                Economy.EventLog != null ? Economy.EventLog.Log : null, RouteFinder());
        }

        // Нет конгестиона — null-делегат, GarbageTrucks считает прямой L-маршрут.
        private Func<GridPosition, GridPosition, List<GridPosition>> RouteFinder()
        {
            if (Congestion == null) Congestion = FindFirstObjectByType<CongestionController>();
            if (Congestion == null || Congestion.Routes == null) return null;
            List<RoadState> states = Congestion.Roads != null ? Congestion.Roads.Roads : null;
            if (states == null || states.Count == 0) return null;
            List<GridPosition> roadCells = new List<GridPosition>(states.Count);
            for (int i = 0; i < states.Count; i++) roadCells.Add(states[i].Cell);
            return delegate (GridPosition from, GridPosition to)
            {
                GridPosition roadFrom, roadTo;
                if (!TryNearestRoad(from, roadCells, out roadFrom)) return null;
                if (!TryNearestRoad(to, roadCells, out roadTo)) return null;
                return Congestion.Routes.FindPath(roadFrom, roadTo);
            };
        }

        // Ближайшая дорожная клетка к origin (манхэттен). false — дорог в городе нет.
        public static bool TryNearestRoad(GridPosition origin, List<GridPosition> roadCells, out GridPosition nearest)
        {
            nearest = default(GridPosition);
            int best = int.MaxValue;
            for (int i = 0; i < roadCells.Count; i++)
            {
                int dist = Mathf.Abs(roadCells[i].X - origin.X) + Mathf.Abs(roadCells[i].Y - origin.Y);
                if (dist < best) { best = dist; nearest = roadCells[i]; }
            }
            return best < int.MaxValue;
        }

        // Депо = ручная постройка с IsWasteDepot; несколько — SelectDepot берёт самое ёмкое.
        private GridPosition? FindDepot()
        {
            if (Economy == null || Economy.Placement == null || Economy.Placement.Placed == null) return null;
            List<BuildingState> placed = Economy.Placement.Placed;
            List<GarbageTrucks.DepotCandidate> candidates = new List<GarbageTrucks.DepotCandidate>(placed.Count);
            for (int i = 0; i < placed.Count; i++)
            {
                BuildingState depot = placed[i];
                if (depot == null || depot.Definition == null || !depot.Definition.IsWasteDepot) continue;
                candidates.Add(new GarbageTrucks.DepotCandidate(depot.Origin, depot.Definition.WasteCapacity));
            }
            return GarbageTrucks.SelectDepot(candidates);
        }
    }
}
