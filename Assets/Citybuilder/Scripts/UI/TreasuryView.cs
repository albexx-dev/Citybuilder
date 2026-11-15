using UnityEngine;

namespace Citybuilder
{
    // Показывает баланс и строку бюджета. V1: штатные OnGUI-лейблы без UI-пакетов.
    // Позже заменить на uGUI/UI Toolkit отдельным проходом.
    public class TreasuryView : MonoBehaviour
    {
        [Tooltip("Стартовый баланс города.")]
        public int StartingBalance = 1500;
        [Header("References")]
        [Tooltip("Экономика: даёт работающих и ставку налога. Пусто = ищется в сцене.")]
        public EconomyTicker Economy;
        [Tooltip("Строка расхода: даёт содержание зданий. Пусто = ищется в сцене.")]
        public UpkeepView Upkeep;

        private Treasury treasury;
        private bool refsSearched;

        // Ленивый доступ: после перезагрузки домена казна пересоздаётся.
        // Баланс при этом сбрасывается (чинятся сохранениями, не V1).
        public Treasury Treasury
        {
            get
            {
                if (treasury == null)
                {
                    treasury = new Treasury(StartingBalance);
                }
                return treasury;
            }
        }

        // Сброс баланса из сохранения: обновляет существующую казну без разрыва ссылок.
        public void ResetBalance(int balance)
        {
            if (treasury == null)
            {
                treasury = Treasury.Restore(balance);
            }
            else
            {
                treasury.Reset(balance);
            }
        }

        private void OnGUI()
        {
            if (Treasury == null)
            {
                return;
            }
            ResolveRefs();
            Color balanceColor = Treasury.IsInDeficit ? UpkeepView.DeficitColor : Color.white;
            HudText.Draw(10f, 10f, "$ " + Treasury.Balance, balanceColor);
            bool netDeficit;
            string summary = BudgetLine(out netDeficit);
            if (summary != null)
            {
                Color lineColor = netDeficit ? UpkeepView.DeficitColor : Color.white;
                HudText.Draw(10f, 32f, summary, lineColor);
            }
        }

        // Строка бюджета «Доход - Расход = Итог» и признак минусового итога.
        // Без данных (нет экономики, налога или расхода) — null, строка не рисуется.
        private string BudgetLine(out bool netDeficit)
        {
            netDeficit = false;
            if (Economy == null || Economy.Tax == null || Upkeep == null)
            {
                return null;
            }
            int income = IncomePerHour();
            int upkeep = Upkeep.CurrentUpkeep();
            netDeficit = BudgetSummaryPolicy.IsNetDeficit(income, upkeep);
            return BudgetSummaryPolicy.Line(income, upkeep);
        }

        // Доход за час по правилу TaxSystem: работающие × ставка.
        private int IncomePerHour()
        {
            return Economy.Tax.ComputeTax(Economy.PerWorker);
        }

        // Ссылки ищутся один раз: экономика и строка расхода живут на других объектах HUD.
        private void ResolveRefs()
        {
            if (refsSearched)
            {
                return;
            }
            refsSearched = true;
            if (Economy == null) Economy = FindFirstObjectByType<EconomyTicker>();
            if (Upkeep == null) Upkeep = FindFirstObjectByType<UpkeepView>();
        }
    }
}
