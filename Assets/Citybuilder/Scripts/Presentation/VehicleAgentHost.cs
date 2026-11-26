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
        [Tooltip("Настройки (EPIC-69): лимит машин применяется на лету. Пусто = ищется в сцене.")]
        public SettingsView Settings;

        private AgentManager manager;
        private int lastRoadCount;
        private readonly List<GridPosition> homes = new List<GridPosition>();
        private readonly List<GridPosition> works = new List<GridPosition>();
        private readonly List<GridPosition> shops = new List<GridPosition>();
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
            if (Settings == null) Settings = FindFirstObjectByType<SettingsView>();
        }

        // Сброс по команде загрузки/«Новая игра» (LoadController.ClearAll): агенты и счётчик
        // дорог обнуляются, и вид сразу гасит старые машины.
        public void ResetAgents()
        {
            if (manager != null) manager.Clear();
            lastRoadCount = 0;
            if (View != null && manager != null) View.Sync(manager);
        }

        private List<GridPosition> FindRoute(GridPosition from, GridPosition to)
        {
            RouteSystem routes = Congestion != null ? Congestion.Routes : null;
            return routes != null ? routes.FindPath(from, to) : null;
        }

        private void Update()
        {
            if (manager == null || Runner == null) return;
            if (Settings != null && Settings.Settings != null) manager.SetActiveLimit(Settings.Settings.AgentLimit);
            SpawnOnNewRoads();
            if (Runner.Clock == null || Runner.Clock.Speed == SimulationSpeed.Paused) return;
            manager.Advance(Time.deltaTime);
            if (View != null) View.Sync(manager);
        }

        // Спавн машин привязан к СТРОИТЕЛЬСТВУ дорог (решение владельца): одна новая
        // клетка дороги — одна машина (если дом соединён с работой и лимит позволяет).
        private void SpawnOnNewRoads()
        {
            RoadNetworkStore store = Congestion != null ? Congestion.Roads : null;
            if (store == null) store = FindFirstObjectByType<RoadNetworkStore>();
            List<RoadState> states = store != null ? store.Roads : null;
            int count = states != null ? states.Count : 0;
            if (count <= lastRoadCount)
            {
                lastRoadCount = count;
                return;
            }
            RoadState latest = states[count - 1];
            lastRoadCount = count;
            CollectTargets();
            CollectRoads();
            manager.SpawnForNewRoad(latest.Cell, homes, works, shops, roads);
        }

        // Дома — жилые здания; магазины — shop; работы — остальные здания с рабочими местами.
        private void CollectTargets()
        {
            homes.Clear();
            works.Clear();
            shops.Clear();
            List<BuildingState> grown = Growth.Homes;
            for (int i = 0; i < grown.Count; i++)
            {
                BuildingDefinition def = grown[i] != null ? grown[i].Definition : null;
                if (def == null) continue;
                if (def.AllowedZone == ZoneType.Residential) homes.Add(grown[i].Origin);
                else if (def.Id == "shop") shops.Add(grown[i].Origin);
                else if (def.JobsProvided > 0) works.Add(grown[i].Origin);
            }
            if (Placement == null || Placement.Placed == null) return;
            List<BuildingState> placed = Placement.Placed;
            for (int i = 0; i < placed.Count; i++)
            {
                BuildingDefinition def = placed[i] != null ? placed[i].Definition : null;
                if (def == null) continue;
                if (def.Id == "shop") shops.Add(placed[i].Origin);
                else if (def.JobsProvided > 0) works.Add(placed[i].Origin);
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
