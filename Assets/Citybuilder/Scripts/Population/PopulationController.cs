using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Автовъезд домохозяйств в выросшие дома по таймеру, first-fit. Только таймер.
    // Настройка сцены: отдельный объект, Growth обязателен, по умолчанию выключен.
    public class PopulationController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Рост поселений. Обязателен: даёт выросшие дома для заселения.")]
        public SettlementGrowthController Growth;
        [Tooltip("Вид населения. Пусто = создастся на этом же объекте.")]
        public PopulationView View;
        [Header("Move-in")]
        [Tooltip("Размер въезжающего домохозяйства. Минимум 1.")]
        public int HouseholdSize = 2;
        [Tooltip("Пауза между въездами в секундах. Минимум 1.")]
        public float MoveInIntervalSec = 7f;
        [Tooltip("Режим автовъезда. Выключите для ручного заселения.")]
        public bool placementActive = true;
        private PopulationSystem population;
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
        // Первое выросшее жильё, куда домохозяйство влезает целиком.
        private void TryMoveIn()
        {
            List<BuildingState> homes = Growth.Homes;
            if (homes == null) return;
            for (int i = 0; i < homes.Count; i++)
            {
                BuildingState home = homes[i];
                if (population.ResidentsIn(home) + HouseholdSize <= population.CapacityOf(home))
                {
                    population.MoveIn(home, HouseholdSize);
                    return;
                }
            }
        }
        private void ClampLimits()
        {
            if (HouseholdSize < 1) HouseholdSize = 1;
            if (MoveInIntervalSec < 1f) MoveInIntervalSec = 1f;
        }
    }
}
