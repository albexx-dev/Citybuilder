using UnityEngine;

namespace Citybuilder
{
    // Зональная налоговая политика (эпик #49): подписи трёх строк бюджета инфоменю.
    // Вешать на объект HUD (Grid). Ссылок не требует: политика живёт в памяти.
    public class TaxPolicyView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Экономика: даёт занятых по зонам для строк дохода. Пусто = ищется в сцене.")]
        public EconomyTicker Economy;

        // Создаётся лениво; уровни живут в памяти сцены (сейв v4 хранит только общий уровень,
        // раздельное хранение ставок — сейв v5, эпик #64).
        private TaxPolicy policy;
        private bool economySearched;

        // Ленивое создание: сцена не требует ссылок, по умолчанию 4/4/4.
        public TaxPolicy Policy
        {
            get
            {
                if (policy == null)
                {
                    policy = new TaxPolicy();
                }
                return policy;
            }
        }

        // Строка бюджета зоны с фактическим сбором: «Жилая: обычный (4) +8 $/ч».
        public string LabelFor(ZoneType zone)
        {
            return Policy.ZoneLabel(zone, ZoneEmployed(zone));
        }

        // Занятые зоны: распределение общей занятости по долям рабочих мест (TaxSystem).
        private int ZoneEmployed(ZoneType zone)
        {
            if (Economy == null && !economySearched)
            {
                Economy = FindFirstObjectByType<EconomyTicker>();
                economySearched = true;
            }
            return Economy != null && Economy.Tax != null
                ? Economy.Tax.EmployedPerZone().ByZone(zone)
                : 0;
        }
    }
}
