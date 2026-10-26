using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Citybuilder
{
    // Загрузка слота сохранений: F9 или публичный LoadNow() читают slot0.json. Ссылки опциональны.
    public class LoadController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Население. Опционален: иначе ищется на этом же объекте.")]
        public PopulationController Population;
        [Tooltip("Рост поселений. Опционален: иначе ищется на этом же объекте.")]
        public SettlementGrowthController Growth;
        [Tooltip("Снос выросших домов. Опционален: иначе ищется на этом же объекте.")]
        public SettlementDemolitionController Demolition;
        [Tooltip("Кисть зон. Опциональна: иначе ищется на этом же объекте.")]
        public ZonePaintingController Zones;
        [Tooltip("Казна. Опциональна: иначе ищется на этом же объекте.")]
        public TreasuryView Treasury;
        [Tooltip("Драйвер часов. Опционален: иначе ищется на этом же объекте.")]
        public SimulationRunner Runner;
        [Tooltip("Стройка дорог. Опциональна: иначе ищется на этом же объекте.")]
        public RoadNetworkStore Roads;
        [Tooltip("Налоговая политика. Пусто = обычный налог после загрузки.")]
        public TaxPolicyView Policy;
        [Tooltip("Ручные здания. Пусто = ищется в сцене.")]
        public BuildingPlacementController Placement;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F9)) LoadNow();
        }

        public bool LoadNow()
        {
            ResolveRefs();
            string path = Path.Combine(Application.persistentDataPath, SaveSlotStorage.SlotFileName);
            CitySnapshot snapshot = SaveSlotStorage.ReadSlot(path);
            if (snapshot == null || !SaveSlotStorage.Validate(snapshot))
            {
                Debug.LogError("LoadController: слот бит или отсутствует: " + path);
                return false;
            }
            ClearAll();
            List<BuildingState> homes = ApplyCity(snapshot);
            int manual = ApplyManual(snapshot);
            ApplyMeta(snapshot);
            LoadLog.Report(path, snapshot, homes, manual);
            return true;
        }

        public void NewGame()
        {
            ResolveRefs();
            ClearAll();
            if (Treasury != null) Treasury.ResetBalance(Treasury.StartingBalance);
            if (Runner != null && Runner.Clock != null) Runner.Clock.RestoreTime(0);
            if (Policy != null) Policy.Policy.Set(TaxPolicy.Level.Normal);
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
        }

        private void ClearAll()
        {
            List<BuildingState> homes = Growth != null ? Growth.Homes : null;
            if (Demolition != null) Demolition.ClearGrown();
            if (Zones != null) Zones.ClearZones();
            if (Roads != null) Roads.ClearRoads();
            if (Placement != null) Placement.ClearPlaced();
            GoalView goal = FindFirstObjectByType<GoalView>();
            if (goal != null) goal.RestoreStage(0);
            if (Population == null || Population.Population == null || homes == null) return;
            for (int i = 0; i < homes.Count; i++) Population.Population.EvictHome(homes[i]);
        }

        private List<BuildingState> ApplyCity(CitySnapshot snapshot)
        {
            if (Zones != null && Zones.Zones != null)
            {
                LoadSystem.ApplyZones(snapshot, Zones.Zones);
                Zones.RepaintAll();
            }
            if (Roads != null && snapshot.Roads != null)
            {
                for (int i = 0; i < snapshot.Roads.Count; i++)
                {
                    if (snapshot.Roads[i] != null) Roads.RestoreRoad(new GridPosition(snapshot.Roads[i].X, snapshot.Roads[i].Y));
                }
            }
            List<BuildingState> homes = ApplyBuildings(snapshot);
            if (Population != null && Population.Population != null) LoadSystem.ApplyHouseholds(snapshot, Population.Population, homes);
            return homes;
        }

        private List<BuildingState> ApplyBuildings(CitySnapshot snapshot)
        {
            List<BuildingState> homes = new List<BuildingState>();
            if (Growth == null || snapshot.Buildings == null) return homes;
            for (int i = 0; i < snapshot.Buildings.Count; i++)
            {
                if (snapshot.Buildings[i] == null) continue;
                BuildingState state = Growth.RestoreGrown(snapshot.Buildings[i].DefId, new GridPosition(snapshot.Buildings[i].X, snapshot.Buildings[i].Y));
                if (state != null) homes.Add(state);
            }
            return homes;
        }

        private int ApplyManual(CitySnapshot snapshot)
        {
            if (Placement == null || snapshot.ManualBuildings == null) return 0;
            int restored = 0;
            for (int i = 0; i < snapshot.ManualBuildings.Count; i++)
            {
                CitySnapshot.BuildingEntry entry = snapshot.ManualBuildings[i];
                if (entry != null && Placement.RestoreManual(entry.DefId, new GridPosition(entry.X, entry.Y))) restored++;
            }
            return restored;
        }

        private void ApplyMeta(CitySnapshot snapshot)
        {
            if (Treasury != null) Treasury.ResetBalance(snapshot.Balance);
            if (Runner != null && Runner.Clock != null) Runner.Clock.RestoreTime(snapshot.TickCount);
            if (Policy != null) Policy.Policy.Set(SaveSystem.DecodeTaxLevel(snapshot.TaxLevel));
            GoalView goal = FindFirstObjectByType<GoalView>();
            if (goal != null) goal.RestoreStage(snapshot.GoalStage);
        }
    }
}
