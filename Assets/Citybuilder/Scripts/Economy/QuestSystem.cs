using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Квесты-вехи (EPIC-48, ADR-012): до 3 активных задач, выполненные id не повторяются.
    // На границе тика проверяются активные предикаты: выполненная задача начисляет награду
    // казне, пишет событие в ленту и заменяется следующей вехой из каталога. Финала нет.
    public class QuestSystem
    {
        public const int MaxActive = 3;

        private const string ResPrefix = "q_res";
        private const string GrowPrefix = "q_grow";
        private const string BalancePrefix = "q_balance";
        private const string ParkPrefix = "q_park";
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

        // Последний снимок города с тика: источник прогресса для QuestLogView.
        public QuestContext LastContext { get; private set; }

        // Граница тика: проверить активные, начислить награды, добрать задачи из каталога.
        public void Tick(int tick, QuestContext context, Treasury treasury, CityEventLog log)
        {
            LastContext = context;
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
            LastContext = default(QuestContext);
        }

        // Строка прогресса для панели задач («Жители 12/50»); у задачи без цели — null.
        public static string ProgressLine(QuestDefinition quest, QuestContext context)
        {
            string id = quest.Id;
            if (string.IsNullOrEmpty(id)) return null;
            int target = ParseTarget(id);
            if (id.StartsWith(ResPrefix) && target > 0) return "Жители " + context.Residents + "/" + target;
            if (id.StartsWith(GrowPrefix) && target > 0) return "Здания " + context.Buildings + "/" + target;
            if (id.StartsWith(BalancePrefix) && target > 0) return "Баланс " + context.Balance + "/" + target;
            if (id.StartsWith(ParkPrefix) && target > 0) return "Парки " + context.Parks + "/" + target;
            return CoverLine(id, context);
        }

        // Квесты покрытия: процент домов под службой против 100%.
        private static string CoverLine(string id, QuestContext context)
        {
            if (id == QuestCatalog.PoliceCoverId) return "Полиция " + Percent(context.PoliceCoverage) + "/100";
            if (id == QuestCatalog.HealthCoverId) return "Больницы " + Percent(context.HealthCoverage) + "/100";
            if (id == QuestCatalog.ServicesAllId) return "Службы " + Percent(WorstCoverage(context)) + "/100";
            return "Пожарные " + Percent(context.FireCoverage) + "/100";
        }

        // Цель закодирована в id: q_res_50 → 50, q_res_ms_250 → 250, иначе 0.
        private static int ParseTarget(string id)
        {
            int underscore = id.LastIndexOf('_');
            int target = 0;
            if (underscore >= 0) int.TryParse(id.Substring(underscore + 1), out target);
            return target;
        }

        private static float WorstCoverage(QuestContext context)
        {
            float worst = context.FireCoverage;
            if (context.PoliceCoverage < worst) worst = context.PoliceCoverage;
            if (context.HealthCoverage < worst) worst = context.HealthCoverage;
            if (context.LeisureCoverage < worst) worst = context.LeisureCoverage;
            return worst;
        }

        // Процент 0..100 без Unity: округление половиной вверх.
        private static int Percent(float coverage) { return (int)(coverage * 100f + 0.5f); }

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
