using System;
using UnityEngine;

namespace Citybuilder
{
    // Экономика часов: сбор налога, списание содержания и запись событий города на границе тика (EPIC-09 шаг 3).
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
            if (Population == null || Growth == null || Treasury == null || Runner == null)
            {
                Debug.LogError("EconomyTicker: задайте Population, Growth, Treasury и Runner в Inspector.");
                enabled = false;
                return false;
            }
            if (Placement == null) Placement = FindFirstObjectByType<BuildingPlacementController>();
            if (EventLog == null) EventLog = FindFirstObjectByType<EventLogView>();
            EnsureSystems();
            return true;
        }

        private void EnsureSystems()
        {
            if (tax != null || Population == null || Growth == null || Treasury == null) return;
            if (Population.Population == null || Growth.Settlement == null || Treasury.Treasury == null) return;
            employment = new EmploymentSystem(Population.Population, Growth.Settlement);
            tax = new TaxSystem(employment, Treasury.Treasury);
        }

        private void Tick(int tickCount)
        {
            EnsureSystems();
            if (tax == null) return;
            tax.Collect(PerWorker);
            Treasury.Treasury.Charge(UpkeepPolicy.UpkeepPerHour(Growth.Homes,
                Placement != null ? Placement.Placed : null));
            ReportCityEvents(tickCount);
        }

        private void ReportCityEvents(int tick)
        {
            if (EventLog == null) EventLog = FindFirstObjectByType<EventLogView>();
            if (EventLog == null) return;
            CityEventLog log = EventLog.Log;
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
        }

        private void ClampLimits()
        {
            if (TaxPerWorker < 0) TaxPerWorker = 0;
        }
    }
}
