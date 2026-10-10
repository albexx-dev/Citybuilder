using UnityEngine;

namespace Citybuilder
{
    // Периодический сбор налогов: Employed × ставка в казну по таймеру. Только таймер.
    // Настройка сцены: отдельный объект, Population/Growth/Treasury обязательны, по умолчанию включён.
    public class EconomyTicker : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Население. Обязателен: даёт PopulationSystem для занятости.")]
        public PopulationController Population;
        [Tooltip("Рост поселений. Обязателен: даёт SettlementSystem для рабочих мест.")]
        public SettlementGrowthController Growth;
        [Tooltip("Вид казны. Обязателен: даёт Treasury для зачисления налогов.")]
        public TreasuryView Treasury;
        [Header("Taxes")]
        [Tooltip("Налог с одного работающего за тик. 0 выключает сбор. Минимум 0.")]
        public int TaxPerWorker = 2;
        [Tooltip("Пауза между сборами в секундах. Минимум 1.")]
        public float TickIntervalSec = 10f;
        [Tooltip("Режим автосбора. Выключите для ручного сбора.")]
        public bool placementActive = true;
        private EmploymentSystem employment;
        private TaxSystem tax;
        private float timer;
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
        private void OnValidate()
        {
            ClampLimits();
        }
        // Проверяет ссылки и чинит логику после перезагрузки домена.
        private bool EnsureReady()
        {
            if (Population == null || Growth == null || Treasury == null)
            {
                Debug.LogError("EconomyTicker: задайте Population, Growth и Treasury в Inspector.");
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
        private void Update()
        {
            if (!EnsureReady())
            {
                return;
            }
            if (!placementActive || tax == null)
            {
                return;
            }
            timer += Time.deltaTime;
            if (timer < TickIntervalSec)
            {
                return;
            }
            timer = 0f;
            Tick();
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
            if (TickIntervalSec < 1f)
            {
                TickIntervalSec = 1f;
            }
        }
    }
}
