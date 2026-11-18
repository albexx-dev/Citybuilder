using UnityEngine;

namespace Citybuilder
{
    // Панель задач-вех (EPIC-48, ADR-012): кнопка «Задачи» в нижней панели, окно QuestLogRect.
    // Владеет QuestTicker (как GoalView владеет целью): на TickCompleted подаёт контекст
    // города из полей EconomyTicker в QuestSystem, награды идут в казну, события — в ленту.
    // По образцу EventLogView (EPIC-09). Хоткей не назначен: Q занята поворотом камеры.
    public class QuestLogView : MonoBehaviour
    {
        private const float LineHeight = 24f;

        [Header("References")]
        [Tooltip("Экономика города: даёт часы, казну, службы и ленту. Пусто = ищется в сцене.")]
        public EconomyTicker Economy;

        private readonly QuestTicker questTicker = new QuestTicker();
        private bool subscribed;
        private bool isOpen;
        private Vector2 scroll;

        // Домен квестов: сброс из LoadController.ClearAll (новая игра/загрузка) и тесты.
        public QuestSystem Quests
        {
            get { return questTicker.Quests; }
        }

        public bool IsOpen
        {
            get { return isOpen; }
        }

        public void Toggle()
        {
            SetOpen(!isOpen);
        }

        public void SetOpen(bool open)
        {
            if (isOpen != open) AudioController.PlaySwooshGlobal();
            isOpen = open;
            if (!isOpen) UiClickGuard.Set("quest_log", new Rect());
        }

        private void OnEnable() { TrySubscribe(); }

        private void OnDisable() { Unsubscribe(); }

        // Update: повторная подписка, если Economy или Runner не были готовы.
        private void Update()
        {
            if (!subscribed) TrySubscribe();
        }

        private void TrySubscribe()
        {
            ResolveRefs();
            if (subscribed) return;
            if (Economy == null || Economy.Runner == null || Economy.Runner.Clock == null) return;
            Economy.Runner.Clock.TickCompleted += OnClockTick;
            Quests.QuestCompleted += OnQuestCompleted;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed) return;
            Economy.Runner.Clock.TickCompleted -= OnClockTick;
            Quests.QuestCompleted -= OnQuestCompleted;
            subscribed = false;
        }

        // Граница тика: снимок города, проверка активных задач, награды и лента.
        private void OnClockTick(SimulationTime time)
        {
            EconomyTicker economy = Economy;
            questTicker.Tick((int)time.TickCount,
                economy.Population != null ? economy.Population.Population : null,
                economy.Growth != null ? economy.Growth.GrownCount : 0,
                economy.Coverage != null ? economy.Coverage.System : null,
                economy.Treasury != null ? economy.Treasury.Treasury : null,
                economy.EventLog != null ? economy.EventLog.Log : null);
        }

        // Награда выполнена: «поп» как у ленты событий (EventLogView.Record).
        private void OnQuestCompleted(QuestDefinition quest)
        {
            AudioController.PlayPopGlobal();
        }

        private void ResolveRefs()
        {
            if (Economy == null) Economy = FindFirstObjectByType<EconomyTicker>();
        }

        private void OnGUI()
        {
            if (!isOpen) return;
            ResolveRefs();
            Rect panel = HudLayout.QuestLogRect(Screen.width, Screen.height);
            UiClickGuard.Set("quest_log", panel);
            BottomBarStyle.DrawBarBackground(panel);
            Rect inner = new Rect(panel.x + 10f, panel.y + 8f, panel.width - 20f, panel.height - 16f);
            QuestSystem quests = Quests;
            Rect view = new Rect(0f, 0f, inner.width - 20f, QuestLines(quests) * LineHeight + 8f);
            scroll = GUI.BeginScrollView(inner, scroll, view);
            DrawQuests(quests);
            GUI.EndScrollView();
        }

        // Высота содержимого: шапка + 2 строки на задачу и 3 для задач с прогрессом.
        private static int QuestLines(QuestSystem quests)
        {
            if (quests == null) return 1;
            int lines = 1;
            for (int i = 0; i < quests.Active.Count; i++)
            {
                lines += QuestSystem.ProgressLine(quests.Active[i], quests.LastContext) != null ? 3 : 2;
            }
            return lines;
        }

        private void DrawQuests(QuestSystem quests)
        {
            float y = 4f;
            HudText.Draw(4f, y, "=== Задачи города ===");
            y += LineHeight;
            if (quests == null || quests.Active.Count == 0)
            {
                HudText.Draw(12f, y, "Задач пока нет.");
                return;
            }
            for (int i = 0; i < quests.Active.Count; i++)
            {
                y = DrawQuest(y, quests.Active[i], quests.LastContext);
            }
        }

        // Строка задачи: титул с наградой, описание и прогресс («Жители 12/50»).
        private static float DrawQuest(float y, QuestDefinition quest, QuestContext context)
        {
            HudText.Draw(12f, y, quest.Title + "  (+" + quest.Reward + "$)");
            HudText.Draw(20f, y + LineHeight, quest.Description);
            string progress = QuestSystem.ProgressLine(quest, context);
            if (progress == null) return y + LineHeight * 2f;
            HudText.Draw(20f, y + LineHeight * 2f, progress);
            return y + LineHeight * 3f;
        }
    }
}
