using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хост пешеходов (EPIC-46): тонкий вайринг PedestrianAgentManager к сцене.
    // Тик часов → пересчёт агентов от населения; Update → движение и синхронизация видов.
    public class PedestrianAgentHost : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка. Обязательна: даёт Grid для занятости клеток и видов.")]
        public GridVisualizer GridVisualizer;
        [Tooltip("Рост поселений. Обязателен: даёт выросшие жилые дома как точки спавна.")]
        public SettlementGrowthController Growth;
        [Tooltip("Драйвер часов. Обязателен: даёт тики для пересчёта населения.")]
        public SimulationRunner Runner;
        [Tooltip("Население. Пусто = ищется в сцене: даёт число жителей для пропорции 1:5.")]
        public PopulationController Population;
        [Tooltip("Ручная стройка. Пусто = ищется в сцене: даёт работы и магазины как цели.")]
        public BuildingPlacementController Placement;
        [Tooltip("Вид пешеходов. Пусто = ищется в сцене.")]
        public PedestrianAgentView View;

        private PedestrianAgentManager manager;
        private readonly List<GridPosition> homes = new List<GridPosition>();
        private readonly List<GridPosition> works = new List<GridPosition>();
        private readonly List<GridPosition> shops = new List<GridPosition>();
        private int lastResidents = -1;

        private void Start()
        {
            ResolveReferences();
            if (GridVisualizer == null || Growth == null || Runner == null || Population == null)
            {
                Debug.LogError("PedestrianAgentHost: задайте GridVisualizer, Growth, Runner и Population.");
                enabled = false;
                return;
            }
            manager = new PedestrianAgentManager();
            manager.Wander.IsBlocked = IsBlocked;
        }

        // Сброс по команде загрузки/«Новая игра» (LoadController.ClearAll): без него
        // равное число жителей не вызовет OnPopulationChanged и старые агенты останутся.
        public void ResetAgents()
        {
            if (manager != null) manager.Clear();
            lastResidents = -1;
        }

        // Занятость клетки для блуждания: здания и дороги занимают клетки сетки.
        private bool IsBlocked(GridPosition cell) { return GridVisualizer.Grid.IsOccupied(cell); }

        private void ResolveReferences()
        {
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Population == null) Population = FindFirstObjectByType<PopulationController>();
            if (Placement == null) Placement = FindFirstObjectByType<BuildingPlacementController>();
            if (View == null) View = FindFirstObjectByType<PedestrianAgentView>();
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

        // Граница игрового часа: пересчитать агентов только при изменившемся населении.
        // «Новая» и загрузка идут через ResetAgents — здесь lastResidents уже равен -1.
        private void OnClockTick(SimulationTime time)
        {
            if (manager == null || Population == null) return;
            int residents = Population.Population != null ? Population.Population.TotalResidents : 0;
            if (residents == lastResidents) return;
            lastResidents = residents;
            CollectTargets();
            manager.Wander.SetDestinations(works, shops);
            manager.OnPopulationChanged(residents, homes);
        }

        private void Update()
        {
            if (manager == null || Runner == null || Runner.Clock == null) return;
            if (Runner.Clock.Speed == SimulationSpeed.Paused) return;
            manager.Advance(Time.deltaTime);
            if (View != null) View.Sync(manager);
        }

        // Точки спавна — дома, цели — работа и магазины (выросшие и ручные).
        private void CollectTargets()
        {
            homes.Clear();
            works.Clear();
            shops.Clear();
            List<BuildingState> grown = Growth.Homes;
            for (int i = 0; i < grown.Count; i++) AddTarget(grown[i]);
            if (Placement == null || Placement.Placed == null) return;
            List<BuildingState> placed = Placement.Placed;
            for (int i = 0; i < placed.Count; i++) AddTarget(placed[i]);
        }

        private void AddTarget(BuildingState state)
        {
            BuildingDefinition def = state != null ? state.Definition : null;
            if (def == null) return;
            if (def.AllowedZone == ZoneType.Residential) { homes.Add(state.Origin); return; }
            if (def.JobsProvided <= 0) return;
            if (def.Id == "shop") shops.Add(state.Origin);
            else works.Add(state.Origin);
        }
    }
}
