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

        [Header("Service")]
        [Tooltip("Вид службы по умолчанию для одиночных геттеров Stations/Covered/Rate01.")]
        public ServiceKind Kind = ServiceKind.Fire;

        [Header("Refresh")]
        [Tooltip("Пауза между пересчётами в секундах. Минимум 0.5.")]
        public float RefreshIntervalSec = 2f;

        private readonly ServiceBuildingSystem system = new ServiceBuildingSystem();
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
            system.BuildSnapshotFromStates(Placement.Placed, Growth.Homes);
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
