using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Автовъезд домохозяйств в выросшие дома по таймеру. Связные раньше, внутри — first-fit. Только таймер.
    // Настройка сцены: отдельный объект, Growth обязателен, по умолчанию выключен.
    public class PopulationController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Рост поселений. Обязателен: даёт выросшие дома для заселения.")]
        public SettlementGrowthController Growth;
        [Tooltip("Вид населения. Пусто = создастся на этом же объекте.")]
        public PopulationView View;
        [Tooltip("Энергия города. Пусто = ищется на этом же объекте; нет — без учёта энергии.")]
        public PowerController Power;
        [Header("Transport")]
        [Tooltip("Связность дорог. Пусто = старое поведение first-fit.")]
        public ConnectivityController Connectivity;
        [Header("Move-in")]
        [Tooltip("Размер въезжающего домохозяйства. Минимум 1.")]
        public int HouseholdSize = 2;
        [Tooltip("Пауза между въездами в секундах. Минимум 1.")]
        public float MoveInIntervalSec = 7f;
        [Tooltip("Минимальная занятость для въезда. Ниже — новые не въезжают.")]
        public float MinEmployment = 0.4f;
        [Tooltip("Режим автовъезда. Выключите для ручного заселения.")]
        public bool placementActive = true;
        private PopulationSystem population;
        private NeedSystem needs;
        private float timer;
        // Ленивый доступ для вида и выселения при сносе.
        // Заселённое при перезагрузке домена теряется (не V1).
        public PopulationSystem Population
        {
            get
            {
                EnsurePopulation();
                return population;
            }
        }
        // Суммарная вместимость выросших домов. Нет роста — 0.
        public int GrownCapacity
        {
            get
            {
                if (Growth == null) return 0;
                List<BuildingState> homes = Growth.Homes;
                if (homes == null) return 0;
                EnsurePopulation();
                int total = 0;
                for (int i = 0; i < homes.Count; i++)
                {
                    total += population.CapacityOf(homes[i]);
                }
                return total;
            }
        }
        private void Start()
        {
            ClampLimits();
            EnsureReady();
            EnsureView();
        }
        private void OnValidate()
        {
            ClampLimits();
        }
        // Чинит систему после перезагрузки домена. Без роста отключается.
        private bool EnsureReady()
        {
            if (Growth == null)
            {
                Debug.LogError("PopulationController: задайте Growth в Inspector.");
                enabled = false;
                return false;
            }
            EnsurePopulation();
            return true;
        }
        private void EnsureView()
        {
            if (View == null) View = GetComponent<PopulationView>();
            if (View == null) View = gameObject.AddComponent<PopulationView>();
        }
        // Тихое создание системы для геттеров: без логов, вид опрашивает каждый кадр.
        private void EnsurePopulation()
        {
            if (population == null) population = new PopulationSystem();
        }
        private void Update()
        {
            if (!EnsureReady()) return;
            if (!placementActive || population == null) return;
            timer += Time.deltaTime;
            if (timer < MoveInIntervalSec) return;
            timer = 0f;
            TryMoveIn();
        }
        // Первое выросшее жильё, куда домохозяйство влезает целиком. Связные раньше.
        private void TryMoveIn()
        {
            EnsureNeeds();
            // Гейт: низкая занятость — въезд закрыт. Систем нет — старое поведение.
            if (needs != null && needs.EmploymentRate < MinEmployment) return;
            List<BuildingState> homes = Growth.Homes;
            if (homes == null) return;
            BuildingState home = FindMoveInHome(homes, population, HouseholdSize, IsHomeConnected);
            if (home != null) population.MoveIn(home, HouseholdSize);
        }
        // Выбор дома для въезда: сначала связные со свободным местом, потом любые.
        // Порядок внутри проходов не меняем. Предиката нет — старый first-fit один в один.
        public static BuildingState FindMoveInHome(
            List<BuildingState> homes,
            PopulationSystem population,
            int householdSize,
            Func<BuildingState, bool> isConnected)
        {
            if (homes == null || population == null) return null;
            if (isConnected != null)
            {
                for (int i = 0; i < homes.Count; i++)
                {
                    if (isConnected(homes[i]) && HasRoomFor(population, homes[i], householdSize)) return homes[i];
                }
            }
            for (int i = 0; i < homes.Count; i++)
            {
                if (HasRoomFor(population, homes[i], householdSize)) return homes[i];
            }
            return null;
        }
        private static bool HasRoomFor(PopulationSystem population, BuildingState home, int householdSize)
        {
            if (home == null || householdSize <= 0) return false;
            return population.ResidentsIn(home) + householdSize <= population.CapacityOf(home);
        }
        // Связность дома через транспорт. Транспорта нет — false, дальше first-fit.
        private bool IsHomeConnected(BuildingState home)
        {
            if (Connectivity == null) Connectivity = GetComponent<ConnectivityController>();
            if (Connectivity == null) return false;
            return Connectivity.IsHomeConnected(home);
        }
        // Ленивая сборка гейта: поселения не готовы — старое поведение.
        private void EnsureNeeds()
        {
            if (needs != null) return;
            if (population == null) return;
            if (Growth == null || Growth.Settlement == null) return;
            if (Power == null) Power = GetComponent<PowerController>();
            Func<float> rate = Power != null ? (Func<float>)(() => Power.Rate01) : null;
            needs = new NeedSystem(population, Growth.Settlement, rate);
        }
        private void ClampLimits()
        {
            if (HouseholdSize < 1) HouseholdSize = 1;
            if (MoveInIntervalSec < 1f) MoveInIntervalSec = 1f;
            if (MinEmployment < 0f) MinEmployment = 0f;
            if (MinEmployment > 1f) MinEmployment = 1f;
        }
    }
}
