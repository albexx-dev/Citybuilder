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
        private bool isOpen;
        private Vector2 scroll;

        // Домен квестов: для сброса «Новая игра» (LoadController) и тестов.
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

        private void OnEnable()
        {
            ResolveRefs();
            if (Quests != null) Quests.QuestCompleted += OnQuestCompleted;
            if (Economy != null && Economy.Runner != null && Economy.Runner.Clock != null)
            {
                Economy.Runner.Clock.TickCompleted += OnClockTick;
            }
        }

        private void OnDisable()
        {
            if (Quests != null) Quests.QuestCompleted -= OnQuestCompleted;
            if (Economy != null && Economy.Runner != null && Economy.Runner.Clock != null)
            {
                Economy.Runner.Clock.TickCompleted -= OnClockTick;
            }
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
            int lines = 1 + (quests != null ? quests.Active.Count * 2 : 0);
            Rect view = new Rect(0f, 0f, inner.width - 20f, lines * LineHeight + 8f);
            scroll = GUI.BeginScrollView(inner, scroll, view);
            DrawQuests(quests);
            GUI.EndScrollView();
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
                y = DrawQuest(y, quests.Active[i]);
            }
        }

        // Строка задачи: титул с наградой и описание с целью.
        private static float DrawQuest(float y, QuestDefinition quest)
        {
            HudText.Draw(12f, y, quest.Title + "  (+" + quest.Reward + "$)");
            HudText.Draw(20f, y + LineHeight, quest.Description);
            return y + LineHeight * 2f;
        }
    }
}
