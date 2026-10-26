using UnityEngine;

namespace Citybuilder
{
    // Кнопка налоговой политики в левом HUD, ниже строк пожарных. Только OnGUI.
    // Вешать на объект HUD (Grid). Ссылок не требует: политика живёт в памяти.
    public class TaxPolicyView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Экономика: даёт число работающих для строки дохода. Пусто = ищется в сцене.")]
        public EconomyTicker Economy;

        // Не сериализуется: после загрузки снова обычный налог (долг на сохранения).
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

        // Подпись с фактическим сбором. Для E2E-моста и проверок.
        public string CurrentLabel
        {
            get { return Policy.Label + IncomeSuffix(); }
        }

        // Левая колонка HUD: y=374 — ниже пожарных (342 + 30 + зазор 2).
        private void OnGUI()
        {
            Rect rect = new Rect(10f, 374f, 240f, 26f);
            UiClickGuard.Set("tax", rect);
            if (GUI.Button(rect, CurrentLabel))
            {
                Policy.Next();
            }
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
