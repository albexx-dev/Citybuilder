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
        [Tooltip("Панель квестов: даёт награды за час. Пусто = ищется в сцене.")]
        public QuestLogView Quests;
        [Tooltip("Драйвер часов: номер текущего тика. Пусто = ищется в сцене.")]
        public SimulationRunner Runner;

        private Treasury treasury;
        private bool refsSearched;
        private bool subscribed;
        private int theftTick = -1;
        private int theftFines;
        private int rewardTick = -1;
        private int rewards;

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

        private void OnEnable() { Subscribe(); }
        private void OnDisable() { Unsubscribe(); }

        private void OnGUI()
        {
            if (Treasury == null)
            {
                return;
            }
            ResolveRefs();
            Subscribe();
            Color balanceColor = Treasury.IsInDeficit ? UpkeepView.DeficitColor : Color.white;
            HudText.Draw(HudLayout.TopLeftX, HudLayout.TopLeftRowY(0), "$ " + Treasury.Balance, balanceColor);
            bool netDeficit;
            string summary = Breakdown(out netDeficit);
            if (summary != null)
            {
                Color lineColor = netDeficit ? UpkeepView.DeficitColor : Color.white;
                HudText.Draw(HudLayout.TopLeftX, HudLayout.TopLeftRowY(2), summary, lineColor);
            }
        }

        // Строка разложения за час: налоги − содержание − кражи + награды = итог.
        // Без данных (нет экономики, налога или расхода) — null, строка не рисуется.
        private string Breakdown(out bool netDeficit)
        {
            netDeficit = false;
            if (Economy == null || Economy.Tax == null || Upkeep == null) return null;
            int income = Economy.IncomePerHour();
            int upkeep = Upkeep.CurrentUpkeep();
            int tick = CurrentTick();
            int thefts = theftTick == tick ? theftFines : 0;
            int earned = rewardTick == tick ? rewards : 0;
            CreditLedger credit = Economy.Credit;
            if (credit != null && (credit.Outstanding > 0 || credit.InterestPercent > 0))
            {
                int interest = credit.InterestForTick();
                netDeficit = BudgetBreakdown.Net(income, upkeep, thefts, earned) - interest < 0;
                return BudgetBreakdown.LineWithCredit(income, upkeep, thefts, earned,
                    credit.Outstanding, interest);
            }
            netDeficit = BudgetBreakdown.IsDeficit(income, upkeep, thefts, earned);
            return BudgetBreakdown.Line(income, upkeep, thefts, earned);
        }

        private int CurrentTick()
        {
            return Runner != null && Runner.Clock != null
                ? (int)Runner.Clock.CurrentTime.TickCount : -1;
        }

        // Подписка на уже существующие события домена: новые поля в системах не нужны.
        private void Subscribe()
        {
            if (subscribed) return;
            if (Economy == null || Quests == null || Quests.Quests == null) return;
            Economy.Crimes.Fired += OnCrime;
            Quests.Quests.QuestCompleted += OnQuestReward;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed) return;
            if (Economy != null) Economy.Crimes.Fired -= OnCrime;
            if (Quests != null && Quests.Quests != null) Quests.Quests.QuestCompleted -= OnQuestReward;
            subscribed = false;
        }

        // Штрафы за кражи текущего часа: событие само несёт номер тика (ADR-013: CityEvent).
        private void OnCrime(CityEvent cityEvent)
        {
            if (cityEvent.Kind != EventKind.Crime) return;
            if (cityEvent.Tick != theftTick) { theftTick = cityEvent.Tick; theftFines = 0; }
            theftFines += CrimeSystem.FinePerTheft;
        }

        // Награды текущего часа: в границе тика часы уже обновлены.
        private void OnQuestReward(QuestDefinition quest)
        {
            int tick = CurrentTick();
            if (tick != rewardTick) { rewardTick = tick; rewards = 0; }
            rewards += quest.Reward;
        }

        // Ссылки ищутся один раз: контроллеры живут на других объектах HUD.
        private void ResolveRefs()
        {
            if (refsSearched)
            {
                return;
            }
            refsSearched = true;
            if (Economy == null) Economy = FindFirstObjectByType<EconomyTicker>();
            if (Upkeep == null) Upkeep = FindFirstObjectByType<UpkeepView>();
            if (Quests == null) Quests = FindFirstObjectByType<QuestLogView>();
            if (Runner == null) Runner = FindFirstObjectByType<SimulationRunner>();
        }
    }
}
