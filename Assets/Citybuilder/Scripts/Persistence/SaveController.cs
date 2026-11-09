using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Запись слота сохранений: F5 пишет слот 0; SaveNow(slot) поддерживает слоты 0..2 и autosave (EPIC-11).
    public class SaveController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Население. Опционален: иначе ищется на этом же объекте.")] public PopulationController Population;
        [Tooltip("Рост поселений. Опционален: иначе ищется на этом же объекте.")] public SettlementGrowthController Growth;
        [Tooltip("Кисть зон. Опциональна: иначе ищется на этом же объекте.")] public ZonePaintingController Zones;
        [Tooltip("Казна. Опциональна: иначе ищется на этом же объекте.")] public TreasuryView Treasury;
        [Tooltip("Драйвер часов. Опционален: иначе ищется на этом же объекте.")] public SimulationRunner Runner;
        [Tooltip("Стройка дорог. Опциональна: иначе ищется на этом же объекте.")] public RoadNetworkStore Roads;
        [Tooltip("Налоговая политика. Пусто = обычный налог в слот.")] public TaxPolicyView Policy;
        [Tooltip("Ручные здания. Пусто = ищется в сцене.")] public BuildingPlacementController Placement;
        [Tooltip("Ступень цели. Пусто = ищется в сцене.")] public GoalView Goal;

        private long lastManualSaveTick = -1;
        private bool isDirty;

        public long LastManualSaveTick { get { return lastManualSaveTick; } }
        public bool IsDirty { get { return isDirty; } }

        public void MarkDirty() { isDirty = true; }
        public void ClearDirty() { isDirty = false; }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5)) SaveNow(0);
        }

        public bool SaveNow()
        {
            return SaveNow(0, null);
        }

        public bool SaveNow(int slot, string slotName = null)
        {
            ResolveRefs();
            CitySnapshot snapshot = BuildCurrentSnapshot();
            bool written = SaveSlotStorage.Save(Application.persistentDataPath, slot, snapshot, slotName);
            if (written && slot >= 0)
            {
                lastManualSaveTick = snapshot.TickCount;
                isDirty = false;
            }
            LogResult(written, SaveSlotIndex.FileNameForSlot(slot), snapshot);
            return written;
        }

        public CitySnapshot BuildCurrentSnapshot()
        {
            ResolveRefs();
            int goalStage = Goal == null ? 0 : Goal.Stage;
            return SaveSystem.BuildSnapshot(CollectHomes(), CollectHouseholds(), CollectZones(),
                ReadBalance(), ReadTicks(), CollectRoads(), ReadTaxLevel(), CollectManual(), goalStage);
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
            if (Goal == null) Goal = FindFirstObjectByType<GoalView>();
        }

        private List<BuildingState> CollectHomes()
        {
            return Growth != null && Growth.Homes != null ? Growth.Homes : new List<BuildingState>();
        }

        private List<BuildingState> CollectManual()
        {
            return Placement != null && Placement.Placed != null ? Placement.Placed : new List<BuildingState>();
        }

        private List<HouseholdState> CollectHouseholds()
        {
            if (Population == null || Population.Population == null || Population.Population.Households == null)
            {
                return new List<HouseholdState>();
            }
            return Population.Population.Households;
        }

        private List<KeyValuePair<GridPosition, ZoneType>> CollectZones()
        {
            if (Zones == null || Zones.Zones == null) return new List<KeyValuePair<GridPosition, ZoneType>>();
            return Zones.Zones.GetZonedCells() ?? new List<KeyValuePair<GridPosition, ZoneType>>();
        }

        private List<RoadState> CollectRoads()
        {
            return Roads != null && Roads.Roads != null ? Roads.Roads : new List<RoadState>();
        }

        private int ReadBalance()
        {
            return Treasury != null && Treasury.Treasury != null ? Treasury.Treasury.Balance : 0;
        }

        private long ReadTicks()
        {
            return Runner != null && Runner.Clock != null ? Runner.Clock.CurrentTime.TickCount : 0;
        }

        private int ReadTaxLevel()
        {
            return Policy == null ? 0 : SaveSystem.EncodeTaxLevel(Policy.Policy.Current);
        }

        private static void LogResult(bool written, string file, CitySnapshot snapshot)
        {
            if (!written)
            {
                Debug.LogError("SaveController: не удалось записать слот: " + file);
                return;
            }
            Debug.Log("SaveController: слот записан: домов=" + snapshot.Buildings.Count
                + ", баланс=" + snapshot.Balance + ", тик=" + snapshot.TickCount + ", файл=" + file);
        }
    }
}
