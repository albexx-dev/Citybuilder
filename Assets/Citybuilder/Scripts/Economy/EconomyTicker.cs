using System;
using UnityEngine;

namespace Citybuilder
{
    // Экономика часов: сбор налога, списание содержания и запись событий города на границе тика (EPIC-09 шаг 3, EPIC-19).
    public partial class EconomyTicker : MonoBehaviour
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
        [Tooltip("Налог с одного работающего за тик. 0 выключает сбор. Минимум 0.")] public int TaxPerWorker = 2;
        [Tooltip("Режим автосбора. Выключите для ручного сбора.")] public bool placementActive = true;
        [Header("Fires")]
        [Tooltip("Снимок покрытия служб. Пусто = ищется в сцене; без него дома считаются защищёнными.")] public ServiceCoverageController Coverage;
        private EmploymentSystem employment;
        private TaxSystem tax;
        private CreditLedger credit = new CreditLedger();
        private readonly EventThrottlePolicy eventThrottle = new EventThrottlePolicy(10);
        private readonly FireTicker fireTicker = new FireTicker();
        private readonly CrimeTicker crimeTicker = new CrimeTicker();
        public TaxSystem Tax { get { EnsureSystems(); return tax; } }
        // Снимок пожара для FireOverlayView (EPIC-43).
        public FireSystem Fires { get { return fireTicker.Fires; } }
        // Кражи (EPIC-47) для InfoMenuView: уровень преступности и модификаторы досуга домов.
        public CrimeSystem Crimes { get { return crimeTicker.Crimes; } }
        // Кредит партии: процент списывается на границе тика, долг виден в строке бюджета.
        public CreditLedger Credit { get { return credit; } }
        public void ResetSimulation()
        {
            eventThrottle.Reset(); fireTicker.Reset(); crimeTicker.Reset();
            credit = new CreditLedger();
        }
        // Фактический налоговый доход за час: зональная политика или фиксированная ставка.
        public int IncomePerHour()
        {
            EnsureSystems();
            if (tax == null) return 0;
            return Policy != null ? tax.ComputeTax(Policy.Policy) : tax.ComputeTax(TaxPerWorker);
        }
        // Зональные рабочие места (эпик #49): выросшие здания и ручные постройки.
        private ZonalJobs BuildZonalJobs()
        {
            return ZonalJobSource.Count(Growth != null ? Growth.Homes : null,
                Placement != null ? Placement.Placed : null);
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
            if (Coverage == null) Coverage = FindFirstObjectByType<ServiceCoverageController>();
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
                    total += Math.Max(0, building.Definition.JobsProvided);
            }
            return total;
        }
        private void EnsureSystems()
        {
            if (Population == null || Growth == null || Treasury == null) return;
            if (Population.Population == null || Growth.Settlement == null || Treasury.Treasury == null) return;
            if (tax != null && tax.Treasury == Treasury.Treasury && employment != null) return;
            employment = new EmploymentSystem(Population.Population, Growth.Settlement, GetManualJobs);
            tax = new TaxSystem(employment, Treasury.Treasury, BuildZonalJobs);
        }
        private void Tick(int tickCount)
        {
            // Пассивный доход беззвучен (спецификация владельца): монетка — только награды GoalView.
            EnsureSystems();
            if (tax == null) return;
            if (Policy != null) tax.Collect(Policy.Policy);
            else tax.Collect(TaxPerWorker);
            int baseUpkeep = UpkeepPolicy.UpkeepPerHour(Growth.Homes,
                Placement != null ? Placement.Placed : null);
            Treasury.Treasury.Charge(EffectiveUpkeepPolicy.Apply(baseUpkeep));
            // Процент идёт через Charge внутри CreditLedger: легальный минус, не TrySpend.
            credit.TickInterest(Treasury.Treasury);
            ReportCityEvents(tickCount);
            TickFires(tickCount);
            TickCrimes(tickCount);
        }
        private void ReportCityEvents(int tick)
        {
            if (EventLog == null) EventLog = FindFirstObjectByType<EventLogView>();
            if (EventLog == null) return;
            CityEventLog log = EventLog.Log;
            ReportSupply(log, tick);
            ReportCongestion(log, tick);
            ReportDeficit(log, tick);
            ReportEmployment(log, tick);
            // Звука на тиковые события нет: лента читаема, coin — только награда цели (GoalView).
        }
        // Троттлинг по виду: сообщения с меняющимися аргументами («Энергия 46%») обходят дедуп.
        private void ReportSupply(CityEventLog log, int tick)
        {
            if (Growth == null || Growth.SupplyGrowth == null) return;
            if (eventThrottle.Allow(EventKind.Supply.ToString(), tick)) Growth.SupplyGrowth.ReportTick(log, tick);
            // Блэкаут (EPIC-50) вне троттлинга: сам гейтится изменением числа отключённых.
            Growth.SupplyGrowth.ReportBlackout(log, tick);
        }
        private void ClampLimits() { if (TaxPerWorker < 0) TaxPerWorker = 0; }
    }
}
