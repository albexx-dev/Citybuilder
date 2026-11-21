using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Покрытие служб города (EPIC-01 шаг 2): тонкий MonoBehaviour-адаптер над ServiceBuildingSystem.
    // Собирает источники всех видов из Placement.Placed и дома из Growth.Homes; старые свойства работают по Kind.
    public class ServiceCoverageController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Рост поселений. Обязателен: даёт Homes.")]
        public SettlementGrowthController Growth;
        [Tooltip("Ручная стройка. Обязательна: даёт поставленные станции.")]
        public BuildingPlacementController Placement;
        [Tooltip("Электросеть. Пусто = станции не отключаются при блэкауте (EPIC-50).")]
        public PowerController Power;

        [Header("Service")]
        [Tooltip("Вид службы по умолчанию для одиночных геттеров Stations/Covered/Rate01.")]
        public ServiceKind Kind = ServiceKind.Fire;

        [Header("Refresh")]
        [Tooltip("Пауза между пересчётами в секундах. Минимум 0.5.")]
        public float RefreshIntervalSec = 2f;

        [Header("Tick")]
        [Tooltip("Часы: снимок пересчитывается в каждом тике до пожаров/краж/квестов. Пусто = ищется в сцене.")]
        public SimulationRunner Runner;

        private readonly ServiceBuildingSystem system = new ServiceBuildingSystem();
        private readonly CoverageTickBinding tickBinding = new CoverageTickBinding();
        private float timer;

        public ServiceBuildingSystem System
        {
            get { return system; }
        }

        // Станций выбранного вида в последнем снимке.
        public int Stations
        {
            get { return system.SourcesCount(Kind); }
        }

        // Домов в последнем снимке.
        public int Homes
        {
            get { return system.TotalHomesCount(); }
        }

        // Накрытых домов выбранного вида в последнем снимке.
        public int Covered
        {
            get { return system.CoveredHomesCount(Kind); }
        }

        // Доля накрытых домов 0..1 выбранного вида.
        public float Rate01
        {
            get { return system.Coverage01(Kind); }
        }

        public int CoveredOf(ServiceKind kind)
        {
            return system.CoveredHomesCount(kind);
        }

        public float RateOf(ServiceKind kind)
        {
            return system.Coverage01(kind);
        }

        private void Start()
        {
            ClampSettings();
            if (!EnsureReady()) return;
            RefreshNow();
        }

        private void OnValidate()
        {
            ClampSettings();
        }

        // Тик: TickStarted всегда раньше TickCompleted (пожары/кражи/квесты).
        // Один пересчёт от TickStarted на тик; Update сохраняет 2-секундное HUD-освежение
        // как страховку для паузы/редактора.
        private void OnEnable()
        {
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Runner == null || Runner.Clock == null) return;
            tickBinding.Bind(Runner.Clock, OnTickStarted);
        }

        private void OnDisable()
        {
            tickBinding.Unbind();
        }

        private void OnTickStarted(SimulationTime time) { RefreshNow(); }

        private void Update()
        {
            if (!EnsureReady()) return;
            timer += Time.deltaTime;
            if (timer < RefreshIntervalSec) return;
            timer = 0f;
            RefreshNow();
        }

        // Пересчитать снимок сейчас по всем видам служб.
        public void RefreshNow()
        {
            system.Clear();
            if (Growth == null || Placement == null) return;
            system.BuildSnapshotFromStates(PoweredStations(Placement.Placed), Growth.Homes);
        }

        // Блэкаут (EPIC-50): обесточенная станция не даёт покрытия (выход 0).
        private List<BuildingState> PoweredStations(List<BuildingState> stations)
        {
            if (Power == null || stations == null) return stations;
            List<BuildingState> powered = new List<BuildingState>(stations.Count);
            for (int i = 0; i < stations.Count; i++)
            {
                if (stations[i] != null && !Power.IsHomePowered(stations[i])) continue;
                powered.Add(stations[i]);
            }
            return powered;
        }

        private bool EnsureReady()
        {
            if (Growth == null || Placement == null)
            {
                Debug.LogError("ServiceCoverageController: задайте Growth и Placement в Inspector.");
                enabled = false;
                return false;
            }
            return true;
        }

        private void ClampSettings()
        {
            if (RefreshIntervalSec < 0.5f) RefreshIntervalSec = 0.5f;
        }
    }

}
