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
        [Tooltip("Кисть зон. Опциональна: иначе ищется на этом же объекте.")]
        public ZonePaintingController Zones;
        [Tooltip("Казна. Опциональна: иначе ищется на этом же объекте.")]
        public TreasuryView Treasury;
        [Tooltip("Драйвер часов. Опционален: иначе ищется на этом же объекте.")]
        public SimulationRunner Runner;
        [Tooltip("Стройка дорог. Опциональна: иначе ищется на этом же объекте.")]
        public RoadPlacementController Roads;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F9)) LoadNow();
        }

        // Мост для E2E: читает slot0.json и перестраивает город. Битый файл — город не трогаем.
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
            ApplyMeta(snapshot);
            LogResult(path, snapshot, homes);
            return true;
        }

        // Новая игра: очистка города + стартовый баланс и нулевое время. В слот не пишет.
        public void NewGame()
        {
            ResolveRefs();
            ClearAll();
            if (Treasury != null) Treasury.ResetBalance(Treasury.StartingBalance);
            if (Runner != null && Runner.Clock != null) Runner.Clock.RestoreTime(0);
            Debug.Log("LoadController: новая игра начата.");
        }

        private void ResolveRefs()
        {
            if (Population == null) Population = GetComponent<PopulationController>();
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Zones == null) Zones = GetComponent<ZonePaintingController>();
            if (Treasury == null) Treasury = GetComponent<TreasuryView>();
            if (Runner == null) Runner = GetComponent<SimulationRunner>();
            if (Roads == null) Roads = GetComponent<RoadPlacementController>();
        }

        private void ClearAll()
        {
            List<BuildingState> homes = Growth != null ? Growth.Homes : null;
            if (Growth != null) Growth.ClearGrown();
            if (Zones != null) Zones.ClearZones();
            if (Roads != null) Roads.ClearRoads();
            EvictAll(homes);
        }

        private void EvictAll(List<BuildingState> homes)
        {
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
            ApplyRoads(snapshot);
            List<BuildingState> homes = ApplyBuildings(snapshot);
            if (Population != null && Population.Population != null) LoadSystem.ApplyHouseholds(snapshot, Population.Population, homes);
            return homes;
        }

        private void ApplyRoads(CitySnapshot snapshot)
        {
            if (Roads == null || snapshot.Roads == null) return;
            for (int i = 0; i < snapshot.Roads.Count; i++)
            {
                if (snapshot.Roads[i] == null) continue;
                Roads.RestoreRoad(new GridPosition(snapshot.Roads[i].X, snapshot.Roads[i].Y));
            }
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

        private void ApplyMeta(CitySnapshot snapshot)
        {
            if (Treasury != null) Treasury.ResetBalance(snapshot.Balance);
            if (Runner != null && Runner.Clock != null) Runner.Clock.RestoreTime(snapshot.TickCount);
        }

        private static void LogResult(string path, CitySnapshot snapshot, List<BuildingState> homes)
        {
            int restored = homes == null ? 0 : homes.Count;
            Debug.Log("LoadController: слот загружен: домов=" + restored
                + ", домохозяйств=" + snapshot.Households.Count
                + ", зон=" + snapshot.Zones.Count
                + ", дорог=" + snapshot.Roads.Count
                + ", баланс=" + snapshot.Balance
                + ", тик=" + snapshot.TickCount
                + ", файл=" + path);
        }
    }
}
