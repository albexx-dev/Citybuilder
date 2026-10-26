using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Citybuilder
{
    // Запись слота сохранений: F5 или публичный SaveNow() пишут slot0.json. Ссылки опциональны.
    public class SaveController : MonoBehaviour
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
        public RoadNetworkStore Roads;
        [Tooltip("Налоговая политика. Пусто = обычный налог в слот.")]
        public TaxPolicyView Policy;
        [Tooltip("Ручные здания. Пусто = ищется в сцене.")]
        public BuildingPlacementController Placement;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveNow();
            }
        }

        // Мост для E2E: собирает снапшот из живых систем и пишет slot0.json. Пустых ссылок не боится.
        public bool SaveNow()
        {
            ResolveRefs();
            CitySnapshot snapshot = SaveSystem.BuildSnapshot(CollectHomes(), CollectHouseholds(), CollectZones(), ReadBalance(), ReadTicks(), CollectRoads(), ReadTaxLevel(), CollectManual());
            string path = Path.Combine(Application.persistentDataPath, SaveSlotStorage.SlotFileName);
            bool written = SaveSlotStorage.WriteSlot(path, snapshot);
            LogResult(written, path, snapshot);
            return written;
        }

        private void ResolveRefs()
        {
            if (Population == null) Population = GetComponent<PopulationController>();
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Zones == null) Zones = GetComponent<ZonePaintingController>();
            if (Treasury == null) Treasury = GetComponent<TreasuryView>();
            if (Runner == null) Runner = GetComponent<SimulationRunner>();
            if (Roads == null) Roads = GetComponent<RoadNetworkStore>();
            if (Policy == null) Policy = FindFirstObjectByType<TaxPolicyView>();
            if (Placement == null) Placement = FindFirstObjectByType<BuildingPlacementController>();
        }

        private List<BuildingState> CollectHomes()
        {
            if (Growth == null)
            {
                return new List<BuildingState>();
            }
            List<BuildingState> homes = Growth.Homes;
            return homes == null ? new List<BuildingState>() : homes;
        }

        // Ручные здания игрока: без ссылки на контроллер строится пустой список.
        private List<BuildingState> CollectManual()
        {
            if (Placement == null) return new List<BuildingState>();
            List<BuildingState> placed = Placement.Placed;
            return placed == null ? new List<BuildingState>() : placed;
        }

        private List<HouseholdState> CollectHouseholds()
        {
            if (Population == null || Population.Population == null)
            {
                return new List<HouseholdState>();
            }
            List<HouseholdState> households = Population.Population.Households;
            return households == null ? new List<HouseholdState>() : households;
        }

        private List<KeyValuePair<GridPosition, ZoneType>> CollectZones()
        {
            if (Zones == null || Zones.Zones == null)
            {
                return new List<KeyValuePair<GridPosition, ZoneType>>();
            }
            List<KeyValuePair<GridPosition, ZoneType>> cells = Zones.Zones.GetZonedCells();
            return cells == null ? new List<KeyValuePair<GridPosition, ZoneType>>() : cells;
        }

        private List<RoadState> CollectRoads()
        {
            if (Roads == null)
            {
                return new List<RoadState>();
            }
            List<RoadState> states = Roads.Roads;
            return states == null ? new List<RoadState>() : states;
        }

        private int ReadBalance()
        {
            if (Treasury == null || Treasury.Treasury == null)
            {
                return 0;
            }
            return Treasury.Treasury.Balance;
        }

        private long ReadTicks()
        {
            if (Runner == null || Runner.Clock == null)
            {
                return 0;
            }
            return Runner.Clock.CurrentTime.TickCount;
        }

        // Уровень для слота: 1/2/3, без политики — 0 («не задано»).
        private int ReadTaxLevel()
        {
            if (Policy == null) return 0;
            return SaveSystem.EncodeTaxLevel(Policy.Policy.Current);
        }

        private static void LogResult(bool written, string path, CitySnapshot snapshot)
        {
            if (!written)
            {
                Debug.LogError("SaveController: не удалось записать слот: " + path);
                return;
            }
            Debug.Log("SaveController: слот записан: домов=" + snapshot.Buildings.Count
                + ", ручных=" + snapshot.ManualBuildings.Count
                + ", домохозяйств=" + snapshot.Households.Count
                + ", зон=" + snapshot.Zones.Count
                + ", дорог=" + snapshot.Roads.Count
                + ", баланс=" + snapshot.Balance
                + ", налог=" + snapshot.TaxLevel
                + ", тик=" + snapshot.TickCount
                + ", файл=" + path);
        }
    }
}
