using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Квесты-вехи (EPIC-48, ADR-012): до 3 активных задач, выполненные id не повторяются.
    // На границе тика проверяются активные предикаты: выполненная задача начисляет
    // награду казне, пишет событие в ленту и заменяется следующей вехой из каталога.
    // Финала нет — цепочка вех бесконечна (доктрина GAME_DESIGN §19).
    public class QuestSystem
    {
        public const int MaxActive = 3;

        private readonly List<QuestDefinition> active = new List<QuestDefinition>(MaxActive);
        private readonly HashSet<string> completed = new HashSet<string>();

        // Задача выполнена: определение для звука и обновления панелей.
        public event Action<QuestDefinition> QuestCompleted;

        // Активные задачи (1–3): снимок списка для QuestLogView.
        public IReadOnlyList<QuestDefinition> Active
        {
            get { return active; }
        }

        // Сколько задач выполнено за игру (для тестов и будущих сейвов v5).
        public int CompletedCount
        {
            get { return completed.Count; }
        }

        // Граница тика: проверить активные, начислить награды, добрать задачи из каталога.
        public void Tick(int tick, QuestContext context, Treasury treasury, CityEventLog log)
        {
            // Буфер выполненных: событие стреляет после всех мутаций списка,
            // чтобы подписчик (например, вызвавший Reset) не сломал обход.
            List<QuestDefinition> finished = null;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                QuestDefinition quest = active[i];
                if (!quest.IsSatisfied(context)) continue;
                Complete(tick, quest, treasury, log);
                active.RemoveAt(i);
                if (finished == null) finished = new List<QuestDefinition>();
                finished.Add(quest);
            }
            Fill(context);
            FireCompleted(finished);
        }

        // События после Fill: подписчик может сбросить или пополнить список задач.
        private void FireCompleted(List<QuestDefinition> finished)
        {
            if (finished == null) return;
            for (int i = 0; i < finished.Count; i++)
            {
                if (QuestCompleted != null) QuestCompleted(finished[i]);
            }
        }

        // Сброс «Новая игра»: активные и выполненные очищаются, задачи выдаются заново.
        public void Reset()
        {
            active.Clear();
            completed.Clear();
        }

        // Выполнение ровно один раз: id в выполненные, награда казне, событие в ленту.
        // Само событие QuestCompleted стреляет позже — из FireCompleted после мутаций.
        private void Complete(int tick, QuestDefinition quest, Treasury treasury, CityEventLog log)
        {
            completed.Add(quest.Id);
            if (treasury != null && quest.Reward > 0) treasury.Earn(quest.Reward);
            if (log != null)
            {
                log.Add(EventKind.Treasury,
                    "Задача выполнена: " + quest.Title + " (+" + quest.Reward + "$)", tick);
            }
        }

        // Добрать активные до лимита: генератор скипает выполненные и недоступные.
        private void Fill(QuestContext context)
        {
            while (active.Count < MaxActive)
            {
                QuestDefinition next;
                if (!QuestCatalog.TryNext(context, completed, ActiveIds(), out next)) break;
                active.Add(next);
            }
        }

        private List<string> ActiveIds()
        {
            List<string> ids = new List<string>(active.Count);
            for (int i = 0; i < active.Count; i++) ids.Add(active[i].Id);
            return ids;
        }
    }
}
