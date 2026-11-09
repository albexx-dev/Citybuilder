using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Автовъезд домохозяйств в выросшие дома по таймеру. Связные раньше, внутри — first-fit.
    // Гейты занятости и удовлетворения (EPIC-03) + множитель привлекательности города (EPIC-05).
    public class PopulationController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Рост поселений. Обязателен: даёт выросшие дома для заселения.")] public SettlementGrowthController Growth;
        [Tooltip("Вид населения. Пусто = создастся на этом же объекте.")] public PopulationView View;
        [Tooltip("Энергия города. Пусто = ищется на объекте Growth.")] public PowerController Power;
        [Tooltip("Вода города. Пусто = ищется на объекте Growth.")] public WaterController Water;
        [Tooltip("Мусор города. Пусто = ищется на объекте Growth.")] public WasteController Waste;
        [Tooltip("Службы города (пожарные, здоровье, досуг). Пусто = ищется на объекте Growth.")] public ServiceCoverageController Services;
        [Header("Transport")]
        [Tooltip("Связность дорог. Пусто = старое поведение first-fit.")] public ConnectivityController Connectivity;
        [Header("Move-in")]
        [Tooltip("Размер въезжающего домохозяйства. Минимум 1.")] public int HouseholdSize = 2;
        [Tooltip("Пауза между въездами в секундах. Минимум 1.")] public float MoveInIntervalSec = 7f;
        [Tooltip("Минимальная занятость для въезда. Ниже — новые не въезжают.")] public float MinEmployment = 0.4f;
        [Tooltip("Минимальное среднее удовлетворение потребностей (EPIC-03).")] public float MinSatisfaction = 0.5f;
        [Tooltip("Режим автовъезда. Выключите для ручного заселения.")] public bool placementActive = true;

        private PopulationSystem population;
        private NeedSystem needs;
        private float timer;

        public PopulationSystem Population
        {
            get { EnsurePopulation(); return population; }
        }

        public float CurrentAttractiveness
        {
            get
            {
                ResolveCityRefs();
                float leisure = Services != null ? Services.RateOf(ServiceKind.Leisure) : 0f;
                float health = Services != null ? Services.RateOf(ServiceKind.Health) : 0f;
                float power = Power != null ? Power.Rate01 : 1f;
                float water = Water != null ? Water.Rate01 : 1f;
                float waste = Waste != null ? Waste.Rate01 : 1f;
                float jams = Growth != null && Growth.Congestion != null ? Growth.Congestion.Multiplier : 1f;
                bool deficit = Growth != null && Growth.Treasury != null && Growth.Treasury.Treasury != null && Growth.Treasury.Treasury.IsInDeficit;
                return AttractivenessPolicy.Evaluate(leisure, health, power, water, waste, jams, deficit);
            }
        }

        public int GrownCapacity
        {
            get
            {
                if (Growth == null) return 0;
                List<BuildingState> homes = Growth.Homes;
                if (homes == null) return 0;
                EnsurePopulation();
                int total = 0;
                for (int i = 0; i < homes.Count; i++) total += population.CapacityOf(homes[i]);
                return total;
            }
        }

        private void Start()
        {
            ClampLimits();
            EnsureReady();
            EnsureView();
        }

        private void OnValidate() { ClampLimits(); }

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

        private void EnsurePopulation()
        {
            if (population == null) population = new PopulationSystem();
        }

        private void Update()
        {
            if (!EnsureReady() || !placementActive || population == null) return;
            timer += Time.deltaTime;
            if (timer < EffectiveInterval(MoveInIntervalSec, CurrentAttractiveness)) return;
            timer = 0f;
            TryMoveIn();
        }

        // Делит базовый интервал на множитель привлекательности 0.5..1.5 (EPIC-05 шаг 2).
        public static float EffectiveInterval(float baseIntervalSec, float attractivenessMultiplier)
        {
            float baseSec = baseIntervalSec < 1f ? 1f : baseIntervalSec;
            float mult = float.IsNaN(attractivenessMultiplier) ? 1f
                : Mathf.Clamp(attractivenessMultiplier, AttractivenessPolicy.MinMultiplier, AttractivenessPolicy.MaxMultiplier);
            return baseSec / mult;
        }

        // Проходят ли гейты занятости и 9-компонентного удовлетворения (EPIC-03 шаг 3).
        public static bool CanMoveInByNeeds(NeedSystem needs, float minEmployment, float minSatisfaction)
        {
            if (needs == null) return true;
            if (needs.EmploymentRate < minEmployment) return false;
            return needs.AverageSatisfaction >= minSatisfaction;
        }

        private void TryMoveIn()
        {
            EnsureNeeds();
            if (!CanMoveInByNeeds(needs, MinEmployment, MinSatisfaction)) return;
            List<BuildingState> homes = Growth.Homes;
            if (homes == null) return;
            BuildingState home = FindMoveInHome(homes, population, HouseholdSize, IsHomeConnected);
            if (home != null) population.MoveIn(home, HouseholdSize);
        }

        public static BuildingState FindMoveInHome(
            List<BuildingState> homes, PopulationSystem population, int householdSize, Func<BuildingState, bool> isConnected)
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

        private bool IsHomeConnected(BuildingState home)
        {
            if (Connectivity == null && Growth != null) Connectivity = Growth.GetComponent<ConnectivityController>();
            if (Connectivity == null) return false;
            return Connectivity.IsHomeConnected(home);
        }

        private void ResolveCityRefs()
        {
            if (Growth == null) return;
            if (Power == null) Power = Growth.GetComponent<PowerController>();
            if (Water == null) Water = Growth.GetComponent<WaterController>();
            if (Waste == null) Waste = Growth.GetComponent<WasteController>();
            if (Services == null) Services = Growth.GetComponent<ServiceCoverageController>();
        }

        private void EnsureNeeds()
        {
            if (needs != null || population == null || Growth == null || Growth.Settlement == null) return;
            ResolveCityRefs();
            Func<float> power = Power != null ? (Func<float>)(() => Power.Rate01) : null;
            Func<float> water = Water != null ? (Func<float>)(() => Water.Rate01) : null;
            Func<float> waste = Waste != null ? (Func<float>)(() => Waste.Rate01) : null;
            Func<float> fire = Services != null ? (Func<float>)(() => Services.RateOf(ServiceKind.Fire)) : null;
            Func<float> health = Services != null ? NeedProviders.Health(Services.System) : null;
            Func<float> leisure = Services != null ? NeedProviders.Leisure(Services.System) : null;
            needs = new NeedSystem(population, Growth.Settlement, power, water, waste, fire, health, leisure);
        }

        private void ClampLimits()
        {
            if (HouseholdSize < 1) HouseholdSize = 1;
            if (MoveInIntervalSec < 1f) MoveInIntervalSec = 1f;
            MinEmployment = Mathf.Clamp01(MinEmployment);
            MinSatisfaction = Mathf.Clamp01(MinSatisfaction);
        }
    }
}
