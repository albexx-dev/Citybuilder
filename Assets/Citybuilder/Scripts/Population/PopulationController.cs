using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Автовъезд домохозяйств в выросшие дома по таймеру со шкалированием скорости.
    public class PopulationController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Рост поселений. Обязателен: даёт выросшие дома для заселения.")] public SettlementGrowthController Growth;
        [Tooltip("Вид населения. Пусто = создастся на этом же объекте.")] public PopulationView View;
        [Tooltip("Драйвер часов.")] public SimulationRunner Runner;
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

        public float Timer { get { return timer; } set { timer = value; } }
        public PopulationSystem Population { get { EnsurePopulation(); return population; } }

        public float SpeedMultiplier
        {
            get
            {
                SimulationRunner r = Runner != null ? Runner : (Growth != null ? Growth.Runner : null);
                if (r == null) r = GetComponent<SimulationRunner>();
                if (r == null) r = FindFirstObjectByType<SimulationRunner>();
                return r != null ? (float)r.Speed : 1f;
            }
        }

        public float CurrentAttractiveness
        {
            get
            {
                ResolveCityRefs();
                float leisure = Services != null ? Services.RateOf(ServiceKind.Leisure) : 0f;
                float health = Services != null ? Services.RateOf(ServiceKind.Health) : 0f;
                float p = Power != null ? Power.Rate01 : 1f, w = Water != null ? Water.Rate01 : 1f, ws = Waste != null ? Waste.Rate01 : 1f;
                float jams = Growth != null && Growth.Congestion != null ? Growth.Congestion.Multiplier : 1f;
                bool def = Growth != null && Growth.Treasury != null && Growth.Treasury.Treasury != null && Growth.Treasury.Treasury.IsInDeficit;
                return AttractivenessPolicy.Evaluate(leisure, health, p, w, ws, jams, def);
            }
        }

        public int GrownCapacity
        {
            get
            {
                if (Growth == null || Growth.Homes == null) return 0;
                EnsurePopulation();
                int total = 0;
                for (int i = 0; i < Growth.Homes.Count; i++) total += population.CapacityOf(Growth.Homes[i]);
                return total;
            }
        }

        private void Start() { ClampLimits(); EnsureReady(); EnsureView(); }
        private void OnValidate() { ClampLimits(); }

        private bool EnsureReady()
        {
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Growth == null) Growth = FindFirstObjectByType<SettlementGrowthController>();
            if (Growth == null) { Debug.LogError("PopulationController: задайте Growth."); enabled = false; return false; }
            if (Runner == null && Growth != null) Runner = Growth.Runner;
            if (Runner == null) Runner = GetComponent<SimulationRunner>();
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            EnsurePopulation();
            return true;
        }

        private void EnsureView() { if (View == null) View = GetComponent<PopulationView>() ?? gameObject.AddComponent<PopulationView>(); }
        private void EnsurePopulation() { if (population == null) population = new PopulationSystem(); }

        private void Update() { if (EnsureReady()) Tick(Time.deltaTime); }

        public void Tick(float deltaTime)
        {
            if (!placementActive) return;
            float speed = SpeedMultiplier;
            if (speed <= 0f) return;
            timer += deltaTime * speed;
            if (timer < EffectiveInterval(MoveInIntervalSec, CurrentAttractiveness)) return;
            timer = 0f;
            TryMoveIn();
        }

        public static float EffectiveInterval(float baseIntervalSec, float attractivenessMultiplier)
        {
            float baseSec = float.IsNaN(baseIntervalSec) || baseIntervalSec < 1f ? 1f : baseIntervalSec;
            float mult = float.IsNaN(attractivenessMultiplier) || attractivenessMultiplier <= 0f ? 1f
                : Mathf.Clamp(attractivenessMultiplier, AttractivenessPolicy.MinMultiplier, AttractivenessPolicy.MaxMultiplier);
            return baseSec / mult;
        }

        public static bool CanMoveInByNeeds(NeedSystem needs, float minEmployment, float minSatisfaction)
        {
            if (needs == null) return true;
            return needs.EmploymentRate >= minEmployment && needs.AverageSatisfaction >= minSatisfaction;
        }

        private void TryMoveIn()
        {
            EnsurePopulation();
            EnsureNeeds();
            if (!CanMoveInByNeeds(needs, MinEmployment, MinSatisfaction) || Growth == null || Growth.Homes == null) return;
            BuildingState home = FindMoveInHome(Growth.Homes, population, HouseholdSize, IsHomeConnected);
            if (home != null) population.MoveIn(home, HouseholdSize);
        }

        public static BuildingState FindMoveInHome(
            List<BuildingState> homes, PopulationSystem population, int householdSize, Func<BuildingState, bool> isConnected)
        {
            if (homes == null || population == null || householdSize <= 0) return null;
            if (isConnected != null)
            {
                for (int i = 0; i < homes.Count; i++)
                    if (homes[i] != null && HasRoomFor(population, homes[i], householdSize) && isConnected(homes[i])) return homes[i];
            }
            for (int i = 0; i < homes.Count; i++)
                if (homes[i] != null && HasRoomFor(population, homes[i], householdSize)) return homes[i];
            return null;
        }

        private static bool HasRoomFor(PopulationSystem population, BuildingState home, int householdSize) =>
            home != null && householdSize > 0 && population.ResidentsIn(home) + householdSize <= population.CapacityOf(home);

        private bool IsHomeConnected(BuildingState home)
        {
            if (Connectivity == null && Growth != null) Connectivity = Growth.GetComponent<ConnectivityController>();
            return Connectivity != null && Connectivity.IsHomeConnected(home);
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
            needs = new NeedSystem(population, Growth.Settlement,
                Power != null ? () => Power.Rate01 : (Func<float>)null,
                Water != null ? () => Water.Rate01 : (Func<float>)null,
                Waste != null ? () => Waste.Rate01 : (Func<float>)null,
                Services != null ? () => Services.RateOf(ServiceKind.Fire) : (Func<float>)null,
                Services != null ? NeedProviders.Health(Services.System) : null,
                Services != null ? NeedProviders.Leisure(Services.System) : null);
        }

        private void ClampLimits()
        {
            HouseholdSize = Mathf.Max(1, HouseholdSize);
            MoveInIntervalSec = Mathf.Max(1f, MoveInIntervalSec);
            MinEmployment = Mathf.Clamp01(MinEmployment);
            MinSatisfaction = Mathf.Clamp01(MinSatisfaction);
        }
    }
}
