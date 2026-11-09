using UnityEngine;

namespace Citybuilder
{
    // Налоговая политика города. Кнопка перенесена в InfoMenuView (EPIC-14 шаг 2).
    // Вешать на объект HUD (Grid). Ссылок не требует: политика живёт в памяти.
    public class TaxPolicyView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Экономика: даёт число работающих для строки дохода. Пусто = ищется в сцене.")]
        public EconomyTicker Economy;

        // Создаётся лениво; уровень сохраняется в CitySnapshot.TaxLevel и восстанавливается LoadController.
        private TaxPolicy policy;
        private bool economySearched;

        // Ленивое создание: сцена не требует ссылок, по умолчанию обычный налог.
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

        // Подпись с фактическим сбором. Для InfoMenuView, E2E-моста и проверок.
        public string CurrentLabel
        {
            get { return Policy.Label + IncomeSuffix(); }
        }

        // Сколько капает в час при текущих работающих. Тикер ищется один раз.
        private string IncomeSuffix()
        {
            if (Economy == null && !economySearched)
            {
                Economy = FindFirstObjectByType<EconomyTicker>();
                economySearched = true;
            }
            int employed = Economy != null && Economy.Tax != null ? Economy.Tax.Employed : 0;
            return " +" + Policy.Income(employed) + " $/ч";
        }
    }
}
