using UnityEngine;

namespace Citybuilder
{
    // Сбор налогов по тикам часов: Employed × ставка в казну. Только подписка на часы.
    // Настройка сцены: отдельный объект, Population/Growth/Treasury/Runner обязательны, по умолчанию включён.
    public class EconomyTicker : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Население. Обязателен: даёт PopulationSystem для занятости.")]
        public PopulationController Population;
        [Tooltip("Рост поселений. Обязателен: даёт SettlementSystem для рабочих мест.")]
        public SettlementGrowthController Growth;
        [Tooltip("Вид казны. Обязателен: даёт Treasury для зачисления налогов.")]
        public TreasuryView Treasury;
        [Tooltip("Драйвер часов. Обязателен: даёт тики часов для сбора налогов.")]
        public SimulationRunner Runner;
        [Header("Taxes")]
        [Tooltip("Налог с одного работающего за тик. 0 выключает сбор. Минимум 0.")]
        public int TaxPerWorker = 2;
        [Tooltip("Режим автосбора. Выключите для ручного сбора.")]
        public bool placementActive = true;
        private EmploymentSystem employment;
        private TaxSystem tax;
        // Доступ для E2E-проверок. Может быть null, пока системы не готовы.
        public TaxSystem Tax
        {
            get
            {
                EnsureSystems();
                return tax;
            }
        }
        private void Start()
        {
            ClampLimits();
            EnsureReady();
        }
        // Подписка здесь, а не в Start: OnEnable срабатывает при каждом включении,
        // чинит подписку после дозавязки ссылок и переживает перезагрузку домена.
        private void OnEnable()
        {
            if (!EnsureReady())
            {
                return;
            }
            if (Runner != null && Runner.Clock != null)
            {
                Runner.Clock.TickCompleted += OnClockTick;
            }
        }
        private void OnDisable()
        {
            if (Runner != null && Runner.Clock != null)
            {
                Runner.Clock.TickCompleted -= OnClockTick;
            }
        }
        // Повторный OnEnable после OnDisable переподписывает один раз: отписка в OnDisable это гарантирует.
        // Параметр времени не нужен: сам факт тика = один игровой час = один сбор.
        private void OnClockTick(SimulationTime time)
        {
            if (!placementActive)
            {
                return;
            }
            Tick();
        }
        private void OnValidate()
        {
            ClampLimits();
        }
        // Проверяет ссылки и чинит логику после перезагрузки домена.
        private bool EnsureReady()
        {
            if (Population == null || Growth == null || Treasury == null || Runner == null)
            {
                Debug.LogError("EconomyTicker: задайте Population, Growth, Treasury и Runner в Inspector.");
                enabled = false;
                return false;
            }
            EnsureSystems();
            return true;
        }
        // Тихое создание систем поверх живых симуляций. Любой null — тихо не тикает.
        private void EnsureSystems()
        {
            if (tax != null)
            {
                return;
            }
            if (Population == null || Growth == null || Treasury == null)
            {
                return;
            }
            if (Population.Population == null)
            {
                return;
            }
            if (Growth.Settlement == null)
            {
                return;
            }
            if (Treasury.Treasury == null)
            {
                return;
            }
            employment = new EmploymentSystem(Population.Population, Growth.Settlement);
            tax = new TaxSystem(employment, Treasury.Treasury);
        }
        // Один шаг сбора для E2E. Нет систем — тихо пропускаем.
        private void Tick()
        {
            if (tax == null)
            {
                return;
            }
            tax.Collect(TaxPerWorker);
        }
        private void ClampLimits()
        {
            if (TaxPerWorker < 0)
            {
                TaxPerWorker = 0;
            }
        }
    }
}
