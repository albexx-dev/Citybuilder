using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хост машин-агентов (ADR-011, EPIC-44): тонкий вайринг AgentManager к сцене.
    // Часы → спавн по окнам расписания; Update → движение агентов и синхронизация видов.
    public class VehicleAgentHost : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязателен: общая Grid для маршрутов и видов.")]
        public GridVisualizer GridVisualizer;
        [Tooltip("Пробки. Обязателен: даёт RouteSystem для путей и CongestionSystem для фида нагрузки.")]
        public CongestionController Congestion;
        [Tooltip("Рост поселений. Обязателен: даёт выросшие дома и рабочие здания.")]
        public SettlementGrowthController Growth;
        [Tooltip("Драйвер часов. Обязателен: даёт игровые часы спавна.")]
        public SimulationRunner Runner;
        [Tooltip("Ручная стройка. Пусто = ищется в сцене: даёт коммерцию/промзону как цели.")]
        public BuildingPlacementController Placement;
        [Tooltip("Вид машин. Пусто = ищется в сцене.")]
        public VehicleAgentView View;

        private AgentManager manager;
        private readonly List<GridPosition> homes = new List<GridPosition>();
        private readonly List<GridPosition> works = new List<GridPosition>();
        private readonly List<GridPosition> roads = new List<GridPosition>();

        private void Start()
        {
            ResolveReferences();
            if (GridVisualizer == null || Congestion == null || Growth == null || Runner == null)
            {
                Debug.LogError("VehicleAgentHost: задайте GridVisualizer, Congestion, Growth и Runner.");
                enabled = false;
                return;
            }
            manager = new AgentManager(Congestion.System);
            manager.PathFinder = FindRoute;
            manager.CellSpeedFactor = CellSpeed;
        }

        // Множитель скорости клетки (EPIC-45): загруженная клетка физически замедляет машину.
        private float CellSpeed(GridPosition cell)
        {
            CongestionSystem congestion = Congestion.System;
            return congestion != null ? CongestionSpeedPolicy.SpeedForLoad01(congestion.Load01(cell)) : 1f;
        }

        private void ResolveReferences()
        {
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Placement == null) Placement = FindFirstObjectByType<BuildingPlacementController>();
            if (View == null) View = FindFirstObjectByType<VehicleAgentView>();
        }

        private List<GridPosition> FindRoute(GridPosition from, GridPosition to)
        {
            RouteSystem routes = Congestion != null ? Congestion.Routes : null;
            return routes != null ? routes.FindPath(from, to) : null;
        }

        private void OnEnable()
        {
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Runner != null && Runner.Clock != null) Runner.Clock.TickCompleted += OnClockTick;
        }

        private void OnDisable()
        {
            if (Runner != null && Runner.Clock != null) Runner.Clock.TickCompleted -= OnClockTick;
        }

        // Граница игрового часа: обновить цели и дороги, спавн по окну расписания.
        // Вне окон спавна пропускаем всё: OnGameHour там no-op, а сбор целей копирует списки.
        private void OnClockTick(SimulationTime time)
        {
            if (manager == null || !AgentManager.IsSpawnHour(time.HourOfDay)) return;
            CollectTargets();
            CollectRoads();
            manager.OnGameHour(time.HourOfDay, homes, works, roads);
        }

        private void Update()
        {
            if (manager == null || Runner == null || Runner.Clock == null) return;
            if (Runner.Clock.Speed == SimulationSpeed.Paused) return;
            manager.Advance(Time.deltaTime);
            if (View != null) View.Sync(manager);
        }

        // Дома — жилые здания; работы — остальные выросшие и ручные с рабочими местами.
        private void CollectTargets()
        {
            homes.Clear();
            works.Clear();
            List<BuildingState> grown = Growth.Homes;
            for (int i = 0; i < grown.Count; i++)
            {
                BuildingDefinition def = grown[i] != null ? grown[i].Definition : null;
                if (def == null) continue;
                if (def.AllowedZone == ZoneType.Residential) homes.Add(grown[i].Origin);
                else if (def.JobsProvided > 0) works.Add(grown[i].Origin);
            }
            if (Placement == null || Placement.Placed == null) return;
            List<BuildingState> placed = Placement.Placed;
            for (int i = 0; i < placed.Count; i++)
            {
                BuildingDefinition placedDef = placed[i] != null ? placed[i].Definition : null;
                if (placedDef != null && placedDef.JobsProvided > 0) works.Add(placed[i].Origin);
            }
        }

        // Клетки дорог из сети дорог: к ним привязываются концы маршрутов.
        private void CollectRoads()
        {
            roads.Clear();
            RoadNetworkStore store = Congestion != null ? Congestion.Roads : null;
            if (store == null) store = FindFirstObjectByType<RoadNetworkStore>();
            if (store == null) return;
            List<RoadState> states = store.Roads;
            for (int i = 0; i < states.Count; i++) roads.Add(states[i].Cell);
        }
    }
}
