using System;
using UnityEngine;

namespace Citybuilder
{
    // Экономика часов: сбор налога, списание содержания и запись событий города на границе тика (EPIC-09 шаг 3, EPIC-19).
    public class EconomyTicker : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Население. Обязателен: даёт PopulationSystem для занятости.")] public PopulationController Population;
        [Tooltip("Рост поселений. Обязателен: даёт SettlementSystem для рабочих мест.")] public SettlementGrowthController Growth;
        [Tooltip("Вид казны. Обязателен: даёт Treasury для зачисления налогов.")] public TreasuryView Treasury;
        [Tooltip("Драйвер часов. Обязателен: даёт тики часов для сбора налогов.")] public SimulationRunner Runner;
        [Tooltip("Ручная стройка. Пусто = ищется в сцене.")] public BuildingPlacementController Placement;
        [Tooltip("Лента событий города (EPIC-09). Пусто = ищется в сцене.")] public EventLogView EventLog;
        [Header("Taxes")]
        [Tooltip("Налоговая политика из HUD. Пусто = фиксированная ставка TaxPerWorker.")] public TaxPolicyView Policy;
        [Tooltip("Налог с одного работающего за тик. 0 выключает сбор. Минимум 0.")] public int TaxPerWorker = 2;
        [Tooltip("Режим автосбора. Выключите для ручного сбора.")] public bool placementActive = true;

        private EmploymentSystem employment;
        private TaxSystem tax;

        public TaxSystem Tax { get { EnsureSystems(); return tax; } }

        public int PerWorker
        {
            get { return Policy != null ? Policy.Policy.PerWorker : TaxPerWorker; }
        }

        private void Start()
        {
            ClampLimits();
            EnsureReady();
        }

        private void OnEnable()
        {
            if (!EnsureReady()) return;
            if (Runner != null && Runner.Clock != null) Runner.Clock.TickCompleted += OnClockTick;
        }

        private void OnDisable()
        {
            if (Runner != null && Runner.Clock != null) Runner.Clock.TickCompleted -= OnClockTick;
        }

        private void OnClockTick(SimulationTime time)
        {
            if (!placementActive) return;
            Tick((int)time.TickCount);
        }

        private void OnValidate() { ClampLimits(); }

        private bool EnsureReady()
        {
            ResolveReferences();
            if (Population == null || Growth == null || Treasury == null || Runner == null)
            {
                Debug.LogError("EconomyTicker: задайте Population, Growth, Treasury и Runner в Inspector.");
                enabled = false;
                return false;
            }
            EnsureSystems();
            return true;
        }

        private void ResolveReferences()
        {
            if (Population == null) Population = FindFirstObjectByType<PopulationController>();
            if (Growth == null) Growth = FindFirstObjectByType<SettlementGrowthController>();
            if (Treasury == null) Treasury = FindFirstObjectByType<TreasuryView>();
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
            if (Placement == null) Placement = FindFirstObjectByType<BuildingPlacementController>();
            if (EventLog == null) EventLog = FindFirstObjectByType<EventLogView>();
        }

        private int GetManualJobs()
        {
            if (Placement == null || Placement.Placed == null) return 0;
            int total = 0;
            var list = Placement.Placed;
            for (int i = 0; i < list.Count; i++)
            {
                var building = list[i];
                if (building != null && building.Definition != null)
                {
                    total += Math.Max(0, building.Definition.JobsProvided);
                }
            }
            return total;
        }

        private void EnsureSystems()
        {
            if (Population == null || Growth == null || Treasury == null) return;
            if (Population.Population == null || Growth.Settlement == null || Treasury.Treasury == null) return;
            if (tax != null && tax.Treasury == Treasury.Treasury && employment != null) return;
            employment = new EmploymentSystem(Population.Population, Growth.Settlement, GetManualJobs);
            tax = new TaxSystem(employment, Treasury.Treasury);
        }

        private void Tick(int tickCount)
        {
            EnsureSystems();
            if (tax == null) return;
            if (tax.Collect(PerWorker).Success) AudioController.PlayCoinGlobal();
            Treasury.Treasury.Charge(UpkeepPolicy.UpkeepPerHour(Growth.Homes,
                Placement != null ? Placement.Placed : null));
            ReportCityEvents(tickCount);
        }

        private void ReportCityEvents(int tick)
        {
            if (EventLog == null) EventLog = FindFirstObjectByType<EventLogView>();
            if (EventLog == null) return;
            CityEventLog log = EventLog.Log;
            int before = log.Count;
            if (Growth != null && Growth.SupplyGrowth != null) Growth.SupplyGrowth.ReportTick(log, tick);
            if (Growth != null && Growth.Congestion != null && Growth.Congestion.GrowthTickMultiplier > 1)
            {
                log.Add(EventKind.Congestion, "Пробки замедляют рост ×" + Growth.Congestion.GrowthTickMultiplier, tick);
            }
            if (Treasury != null && Treasury.Treasury != null && Treasury.Treasury.IsInDeficit)
            {
                log.Add(EventKind.Treasury, "Дефицит казны: баланс $" + Treasury.Treasury.Balance, tick);
            }
            if (employment != null && Population.Population.TotalResidents > 0 && employment.EmploymentRate < Population.MinEmployment)
            {
                int pct = (int)Math.Round(employment.EmploymentRate * 100f);
                log.Add(EventKind.Employment, "Высокая безработица: занятость " + pct + "%", tick);
            }
            if (log.Count > before) AudioController.PlayPopGlobal();
        }

        private void ClampLimits()
        {
            if (TaxPerWorker < 0) TaxPerWorker = 0;
        }
    }
}
