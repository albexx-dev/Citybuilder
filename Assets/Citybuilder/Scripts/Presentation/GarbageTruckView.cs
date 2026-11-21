using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Вид мусоровозов (EPIC-52): владеет GarbageTicker (как QuestLogView владеет
    // QuestTicker, ADR-012) и пулом моделей GarbageTruckPool. На TickCompleted подаёт
    // дома, полигон, ленту и делегат дорог RouteSystem.FindPath, в Update двигает и
    // рисует мусоровозы. Полигон — ручная постройка waste_depot; нет полигона —
    // мусоровозы не выезжают, мусор копится.
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
            SimulationClock clock = Economy != null && Economy.Runner != null ? Economy.Runner.Clock : null;
            if (subscribed || clock == null) return;
            clock.TickCompleted += OnClockTick;
            subscribed = true;
        }

        // Граница игрового часа: дома, полигон waste_depot, лента и делегат дорог — в тикер.
        private void OnClockTick(SimulationTime time)
        {
            if (Economy == null || Economy.Growth == null) return;
            ticker.Tick((int)time.TickCount, Economy.Growth.Homes, FindDepot(),
                Economy.EventLog != null ? Economy.EventLog.Log : null, RouteFinder());
        }

        // Делегат дорог из CongestionController.Routes: путь по дорогам игрока. Конгестион
        // отсутствует или не готов — null-делегат, GarbageTrucks считает прямой L-маршрут.
        private Func<GridPosition, GridPosition, List<GridPosition>> RouteFinder()
        {
            if (Congestion == null) Congestion = FindFirstObjectByType<CongestionController>();
            if (Congestion == null) return null;
            RouteSystem routes = Congestion.Routes;
            if (routes == null) return null;
            return routes.FindPath;
        }

        // Полигон = первая ручная постройка waste_depot. Нет постройки — null (вывоза нет).
        private GridPosition? FindDepot()
        {
            if (Economy.Placement == null || Economy.Placement.Placed == null) return null;
            System.Collections.Generic.List<BuildingState> placed = Economy.Placement.Placed;
            for (int i = 0; i < placed.Count; i++)
            {
                BuildingState depot = placed[i];
                if (depot == null || depot.Definition == null || depot.Definition.Id != "waste_depot") continue;
                return depot.Origin;
            }
            return null;
        }
    }
}
