using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Вид мусоровозов (EPIC-52): владеет GarbageTicker (как QuestLogView владеет
    // QuestTicker, ADR-012) и пулом моделей GarbageTruckPool. На TickCompleted подаёт
    // дома, полигон, ленту и делегат дорог RouteSystem.FindPath, в Update двигает и
    // рисует мусоровозы. Полигон — ручная постройка с признаком депо (issue #90);
    // нет полигона — мусоровозы не выезжают, мусор копится. Из нескольких депо
    // GarbageTrucks.SelectDepot выбирает самое ёмкое.
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

        private readonly GarbageTicker ticker = new GarbageTicker();
        private GarbageTruckPool pool;
        private bool subscribed;

        // Строка HUD для InfoMenuView (эпик #52): очередь вывоза и мусоровозы в пути.
        public string ServiceText
        {
            get { return "Вывоз мусора: ждут домов " + ticker.Garbage.WaitingCount + ", в пути " + ticker.Garbage.ActiveTrucks; }
        }

        // Домен вывоза для записи/восстановления v6 (ADR-014) и оверлея #94.
        public GarbageSystem Garbage { get { return ticker.Garbage; } }
        public bool HasDepot { get { return FindDepot().HasValue; } }

        // Сброс по команде LoadController.ClearAll; также срабатывает при отмотке часов.
        public void ResetGarbage()
        {
            ticker.Reset();
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

        // Update: повторная подписка, если Economy или Runner не были готовы (как QuestLogView),
        // затем движение по реальным секундам и перерисовка. На паузе часов — только рисуем.
        private void Update()
        {
            if (!subscribed) TrySubscribe();
            SimulationClock clock = Economy != null && Economy.Runner != null ? Economy.Runner.Clock : null;
            if (clock == null) return;
            if (clock.Speed != SimulationSpeed.Paused) ticker.Garbage.Advance(Time.deltaTime);
            if (pool != null) pool.Sync(ticker.Garbage, Visualizer);
        }

        private void TrySubscribe()
        {
            if (Economy == null) Economy = FindFirstObjectByType<EconomyTicker>();
            if (Visualizer == null) Visualizer = FindFirstObjectByType<GridVisualizer>();
            if (Visualizer == null) return;
            if (pool == null) pool = new GarbageTruckPool(transform, TruckModels,
                GarbageTrucks.MaxTrucks, ModelScale, ModelYawOffset, RideHeight, Visualizer.Grid.CellSize);
            GarbageOverlayView.Ensure(this);
            SimulationClock clock = Economy != null && Economy.Runner != null ? Economy.Runner.Clock : null;
            if (subscribed || clock == null) return;
            clock.TickCompleted += OnClockTick;
            subscribed = true;
        }

        // Граница игрового часа: дома, выбранное депо, лента и делегат дорог — в тикер.
        private void OnClockTick(SimulationTime time)
        {
            if (Economy == null || Economy.Growth == null) return;
            ticker.Tick((int)time.TickCount, Economy.Growth.Homes, FindDepot(),
                Economy.EventLog != null ? Economy.EventLog.Log : null, RouteFinder());
        }

        // Делегат дорог из CongestionController.Routes. Клетки депо/домов — НЕ дорожные,
        // а RouteSystem.FindPath требует дороги на обоих концах: концы ноги маппятся на
        // ближайшие дорожные клетки (ревью task/214 — «по дорогам» в живой сцене).
        // Конгестион отсутствует или не готов — null-делегат, GarbageTrucks считает прямой L-маршрут.
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

        // Депо = ручная постройка с IsWasteDepot. Нет депо — null (вывоза нет);
        // несколько — GarbageTrucks.SelectDepot берёт самое ёмкое (issue #90).
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
