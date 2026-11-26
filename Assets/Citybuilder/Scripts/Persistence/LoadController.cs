using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Загрузка слота сохранений: F9 грузит слот 0; LoadNow(slot) поддерживает слоты 0..2 и autosave (EPIC-11).
    public partial class LoadController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Население. Опционален: иначе ищется на этом же объекте.")] public PopulationController Population;
        [Tooltip("Рост поселений. Опционален: иначе ищется на этом же объекте.")] public SettlementGrowthController Growth;
        [Tooltip("Снос выросших домов. Опционален: иначе ищется на этом же объекте.")] public SettlementDemolitionController Demolition;
        [Tooltip("Кисть зон. Опциональна: иначе ищется на этом же объекте.")] public ZonePaintingController Zones;
        [Tooltip("Казна. Опциональна: иначе ищется на этом же объекте.")] public TreasuryView Treasury;
        [Tooltip("Драйвер часов. Опционален: иначе ищется на этом же объекте.")] public SimulationRunner Runner;
        [Tooltip("Стройка дорог. Опциональна: иначе ищется на этом же объекте.")] public RoadNetworkStore Roads;
        [Tooltip("Налоговая политика. Пусто = обычный налог после загрузки.")] public TaxPolicyView Policy;
        [Tooltip("Ручные здания. Пусто = ищется в сцене.")] public BuildingPlacementController Placement;
        [Tooltip("Пробки. Опционален: иначе ищется на этом же объекте.")] public CongestionController Congestion;

        private void Update() { if (Input.GetKeyDown(KeyCode.F9)) LoadNow(0); }

        public bool LoadNow() { return LoadNow(0); }

        public bool LoadNow(int slot)
        {
            ResolveRefs();
            string file = SaveSlotIndex.FileNameForSlot(slot);
            CitySnapshot snapshot = SaveSlotStorage.Load(Application.persistentDataPath, slot);
            if (snapshot == null)
            {
                Debug.LogError("LoadController: слот бит или отсутствует: " + file);
                return false;
            }
            ClearAll();
            List<BuildingState> homes = ApplyCity(snapshot);
            int manual = ApplyManual(snapshot);
            SnapshotLevels.ApplyAll(snapshot, homes, Placement == null ? null : Placement.Placed);
            LoadedHouseViewRefresh.Apply(Growth, homes);
            ApplyV6(snapshot, homes);
            ApplyV7(snapshot);
            ApplyMeta(snapshot);
            SaveController save = GetComponent<SaveController>();
            if (save != null) save.ClearDirty();
            TutorialView tut = FindFirstObjectByType<TutorialView>();
            if (tut != null) tut.Dismiss();
            LoadLog.Report(file, snapshot, homes, manual);
            return true;
        }

        public void NewGame()
        {
            ResolveRefs();
            ClearAll();
            if (Treasury != null) Treasury.ResetBalance(Treasury.StartingBalance);
            if (Runner != null && Runner.Clock != null) Runner.Clock.RestoreTime(0);
            if (Policy != null) Policy.Policy.Set(TaxPolicy.Level.Normal);
            RestoredMayorRating = 0;
            RestoredMilestoneCounterV2 = 0;
            SaveController save = GetComponent<SaveController>();
            if (save != null) save.ClearDirty();
            TutorialView tut = FindFirstObjectByType<TutorialView>();
            if (tut != null) tut.ResetTutorial();
            Debug.Log("LoadController: новая игра начата.");
        }

        private void ResolveRefs()
        {
            if (Population == null) Population = GetComponent<PopulationController>();
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Demolition == null) Demolition = GetComponent<SettlementDemolitionController>();
            if (Zones == null) Zones = GetComponent<ZonePaintingController>();
            if (Treasury == null) Treasury = GetComponent<TreasuryView>();
            if (Runner == null) Runner = GetComponent<SimulationRunner>();
            if (Roads == null) Roads = GetComponent<RoadNetworkStore>();
            if (Policy == null) Policy = FindFirstObjectByType<TaxPolicyView>();
            if (Placement == null) Placement = FindFirstObjectByType<BuildingPlacementController>();
            if (Congestion == null) Congestion = GetComponent<CongestionController>();
            if (Congestion == null && Growth != null) Congestion = Growth.Congestion;
            if (Congestion == null) Congestion = FindFirstObjectByType<CongestionController>();
        }

        public void ClearAll()
        {
            // ID, горение и кражи не сериализуются: повторная загрузка даёт те же номера и чистое состояние.
            EntityId.Reset();
            ConstructionSnapshot.Clear();
            if (Demolition != null) Demolition.ClearGrown();
            if (Zones != null) Zones.ClearZones();
            if (Roads != null) Roads.ClearRoads();
            if (Placement != null) Placement.ClearPlaced();
            if (Growth != null) Growth.ResetGrowth();
            if (Congestion != null) Congestion.ClearLoads();
            if (Population != null && Population.Population != null) Population.Population.Clear();
            GoalView goal = FindFirstObjectByType<GoalView>();
            if (goal != null) goal.RestoreStage(0);
            EventLogView eventLog = FindFirstObjectByType<EventLogView>();
            if (eventLog != null) eventLog.ClearLog();
            DecisionPanel decisions = FindFirstObjectByType<DecisionPanel>();
            if (decisions != null) decisions.ResetDecisions();
            EconomyTicker ticker = FindFirstObjectByType<EconomyTicker>();
            if (ticker != null) ticker.ResetSimulation();
            MetricsRecorder metricsRecorder = FindFirstObjectByType<MetricsRecorder>();
            if (metricsRecorder != null) metricsRecorder.Clear();
            QuestLogView questLog = FindFirstObjectByType<QuestLogView>();
            if (questLog != null) questLog.ResetQuests();
            ResetRuntimeViews();
        }

        // Агенты и FX пересчитываются от нового города: сброс до восстановления слота.
        private void ResetRuntimeViews()
        {
            GarbageTruckView garbage = FindFirstObjectByType<GarbageTruckView>();
            if (garbage != null) garbage.ResetGarbage();
            FindFirstObjectByType<AudioEventView>()?.Reset();
            PedestrianAgentHost pedestrians = FindFirstObjectByType<PedestrianAgentHost>();
            if (pedestrians != null) pedestrians.ResetAgents();
            VehicleAgentHost vehicles = FindFirstObjectByType<VehicleAgentHost>();
            if (vehicles != null) vehicles.ResetAgents();
        }

        private List<BuildingState> ApplyCity(CitySnapshot snapshot)
        {
            if (Zones != null && Zones.Zones != null)
            {
                LoadSystem.ApplyZones(snapshot, Zones.Zones);
                Zones.RepaintAll();
            }
            // Дороги восстанавливаются в ApplyV7 с типом и уровнем из слота.
            List<BuildingState> homes = ApplyBuildings(snapshot);
            if (Population != null && Population.Population != null) LoadSystem.ApplyHouseholds(snapshot, Population.Population, homes);
            return homes;
        }

    }
}
