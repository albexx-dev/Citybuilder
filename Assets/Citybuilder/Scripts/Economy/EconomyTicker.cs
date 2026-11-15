using System;
using UnityEngine;

namespace Citybuilder
{
    // Экономика часов: сбор налога, списание содержания и запись событий города на границе тика (EPIC-09 шаг 3, EPIC-19).
    public class EconomyTicker : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Население: даёт PopulationSystem для занятости. Пусто = ищется в сцене.")] public PopulationController Population;
        [Tooltip("Рост поселений: даёт SettlementSystem для рабочих мест. Пусто = ищется в сцене.")] public SettlementGrowthController Growth;
        [Tooltip("Вид казны: даёт Treasury для зачисления налогов. Пусто = ищется в сцене.")] public TreasuryView Treasury;
        [Tooltip("Драйвер часов: даёт тики для сбора налогов. Пусто = ищется в сцене.")] public SimulationRunner Runner;
        [Tooltip("Ручная стройка. Пусто = ищется в сцене.")] public BuildingPlacementController Placement;
        [Tooltip("Лента событий города (EPIC-09). Пусто = ищется в сцене.")] public EventLogView EventLog;
        [Header("Taxes")]
        [Tooltip("Налоговая политика из HUD. Пусто = фиксированная ставка TaxPerWorker.")] public TaxPolicyView Policy;
        [Tooltip("Налог с одного работающего за тик. 0 выключает сбор. Минимум 0.")] public int TaxPerWorker = 4;
        [Tooltip("Режим автосбора. Выключите для ручного сбора.")] public bool placementActive = true;

        private EmploymentSystem employment;
        private TaxSystem tax;
        private readonly SoundCooldownPolicy soundPolicy = new SoundCooldownPolicy();
        private readonly EventThrottlePolicy eventThrottle = new EventThrottlePolicy(10);

        public TaxSystem Tax { get { EnsureSystems(); return tax; } }

        public void ResetSound()
        {
            soundPolicy.Reset();
            eventThrottle.Reset();
        }

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
            // Пассивный доход беззвучен (спецификация владельца): монетка — только
            // на единовременные награды (GoalView), лента — без звука на каждый тик.
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
            // Троттлинг по виду события: сообщения с меняющимися числами («Энергия 46%»)
            // обходят дедуп по строке — каждый вид проходит не чаще раза в 10 тиков.
            if (Growth != null && Growth.SupplyGrowth != null
                && eventThrottle.Allow(EventKind.Supply.ToString(), tick))
            {
                Growth.SupplyGrowth.ReportTick(log, tick);
            }
            if (Growth != null && Growth.Congestion != null && Growth.Congestion.GrowthTickMultiplier > 1
                && eventThrottle.Allow(EventKind.Congestion.ToString(), tick))
            {
                log.Add(EventKind.Congestion, "Пробки замедляют рост ×" + Growth.Congestion.GrowthTickMultiplier, tick);
            }
            if (Treasury != null && Treasury.Treasury != null && Treasury.Treasury.IsInDeficit
                && eventThrottle.Allow(EventKind.Treasury.ToString(), tick)
                && soundPolicy.ShouldPlayDeficit(Time.unscaledTime))
            {
                log.Add(EventKind.Treasury, "Дефицит казны: баланс $" + Treasury.Treasury.Balance, tick);
            }
            if (employment != null && Population.Population.TotalResidents > 0 && employment.EmploymentRate < Population.MinEmployment
                && eventThrottle.Allow(EventKind.Employment.ToString(), tick))
            {
                int pct = (int)Math.Round(employment.EmploymentRate * 100f);
                log.Add(EventKind.Employment, "Высокая безработица: занятость " + pct + "%", tick);
            }
            // Звука на тиковые события нет: лента читаема (троттлинг по виду),
            // поп-спам устранён (жалоба владельца). Единовременные события
            // (награды целей) звучат в GoalView.
        }

        private void ClampLimits()
        {
            if (TaxPerWorker < 0) TaxPerWorker = 0;
        }
    }
}
