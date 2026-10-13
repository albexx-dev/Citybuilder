using UnityEngine;

namespace Citybuilder
{
    // Драйвер роста поселений по тикам часов. Сам рост считает и строит Growth.
    // Настройка сцены: тот же объект, что SettlementGrowthController, по умолчанию включён.
    public class SettlementGrowthTicker : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Рост поселений. Обязателен: даёт Runner для подписки и счёт тиков.")]
        public SettlementGrowthController Growth;
        private void Start()
        {
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
            if (Growth.Runner.Clock != null)
            {
                Growth.Runner.Clock.TickCompleted += Growth.OnClockTick;
            }
        }
        private void OnDisable()
        {
            if (Growth == null || Growth.Runner == null || Growth.Runner.Clock == null)
            {
                return;
            }
            Growth.Runner.Clock.TickCompleted -= Growth.OnClockTick;
        }
        // Повторный OnEnable после OnDisable переподписывает один раз: отписка в OnDisable это гарантирует.
        // Проверяет ссылки и отключается без них, как остальные контроллеры.
        private bool EnsureReady()
        {
            if (Growth == null || Growth.Runner == null)
            {
                Debug.LogError("SettlementGrowthTicker: задайте Growth и Runner в Inspector.");
                enabled = false;
                return false;
            }
            return true;
        }
    }
}
