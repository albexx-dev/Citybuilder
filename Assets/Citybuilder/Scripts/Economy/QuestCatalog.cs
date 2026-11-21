using System.Collections.Generic;

namespace Citybuilder
{
    // Каталог квестов-вех (EPIC-48, ADR-012): 10 стартовых задач и бесконечный генератор вех
    // (жители +50 после 200, баланс ×2 от $5000). Финала нет; недоступные и выполненные скипаются.
    public static class QuestCatalog
    {
        public const int MilestoneStep = 50;
        public const int MilestoneFromResidents = 200;
        public const int BalanceMilestoneFrom = 5000;
        public const int BalanceCityBuildings = 10;

        public const string FireCoverId = "q_fire_cover";
        public const string PoliceCoverId = "q_police_cover";
        public const string HealthCoverId = "q_health_cover";
        public const string ServicesAllId = "q_services_all";

        private static readonly QuestDefinition[] starters =
        {
            new QuestDefinition("q_res_10", "quest_title_res_10", "quest_desc_residents", new object[] { 10 }, 200, c => c.Residents >= 10),
            new QuestDefinition("q_grow_5", "quest_title_grow_5", "quest_desc_buildings", new object[] { 5 }, 250, c => c.Buildings >= 5),
            new QuestDefinition("q_res_50", "quest_title_res_50", "quest_desc_residents", new object[] { 50 }, 400, c => c.Residents >= 50),
            new QuestDefinition("q_park_3", "quest_title_park_3", "quest_desc_parks", new object[] { 3 }, 300, c => c.Parks >= 3),
            new QuestDefinition("q_grow_20", "quest_title_grow_20", "quest_desc_buildings", new object[] { 20 }, 500, c => c.Buildings >= 20),
            new QuestDefinition("q_res_100", "quest_title_res_100", "quest_desc_residents", new object[] { 100 }, 600, c => c.Residents >= 100),
            new QuestDefinition(FireCoverId, "quest_title_fire_cover", "quest_desc_fire_cover", 500, c => c.FireCoverage >= 1f),
            new QuestDefinition(PoliceCoverId, "quest_title_police_cover", "quest_desc_police_cover", 500, c => c.PoliceCoverage >= 1f),
            new QuestDefinition("q_res_200", "quest_title_res_200", "quest_desc_residents", new object[] { 200 }, 1000, c => c.Residents >= 200),
            new QuestDefinition(HealthCoverId, "quest_title_health_cover", "quest_desc_health_cover", 700, c => c.HealthCoverage >= 1f)
        };

        // Стартовые квесты в порядке приоритета: жители, рост, парки, службы.
        public static IReadOnlyList<QuestDefinition> Starters
        {
            get { return starters; }
        }

        // Поиск задачи по id для восстановления слота (QuestSnapshot): стартовые и полный сервис.
        // Вехи (q_res_ms_N) не ищутся: неизвестный id — false, их добирает Fill на тике.
        public static bool TryFind(string id, out QuestDefinition quest)
        {
            quest = default(QuestDefinition);
            if (string.IsNullOrEmpty(id)) return false;
            if (ServicesAllId == id) { quest = ServicesAll; return true; }
            for (int i = 0; i < starters.Length; i++)
            {
                if (starters[i].Id == id) { quest = starters[i]; return true; }
            }
            return false;
        }

        private static readonly QuestDefinition ServicesAll = new QuestDefinition(ServicesAllId,
            "quest_title_services_all", "quest_desc_services_all", 1500,
            c => c.FireCoverage >= 1f && c.PoliceCoverage >= 1f && c.HealthCoverage >= 1f && c.LeisureCoverage >= 1f);

        // Следующий доступный квест: не выполнен, не активен, доступен городу и ещё
        // не выполнен при текущем снимке. Нет подходящего — false (активных 1–3).
        public static bool TryNext(QuestContext context, HashSet<string> completed,
            ICollection<string> activeIds, out QuestDefinition quest)
        {
            for (int i = 0; i < starters.Length; i++)
            {
                if (IsOpen(starters[i], context, completed, activeIds))
                {
                    quest = starters[i];
                    return true;
                }
            }
            return TryNextMilestone(context, completed, activeIds, out quest);
        }

        // Вехи после стартовых: полный сервис, затем жители, затем баланс казны.
        private static bool TryNextMilestone(QuestContext context, HashSet<string> completed,
            ICollection<string> activeIds, out QuestDefinition quest)
        {
            if (ServicesReady(completed) && TryOpen(ServicesAll, context, completed, activeIds, out quest)) return true;
            if (TryOpen(ResidentsMilestone(context), context, completed, activeIds, out quest)) return true;
            return TryOpen(BalanceMilestone(context), context, completed, activeIds, out quest);
        }

        // Полный сервис открывается после трёх квестов покрытия служб.
        private static bool ServicesReady(HashSet<string> completed)
        {
            return completed.Contains(FireCoverId) && completed.Contains(PoliceCoverId)
                && completed.Contains(HealthCoverId);
        }

        // Вехи жителей: после 200 — каждые +50, бесконечно (250, 300, ...).
        private static QuestDefinition ResidentsMilestone(QuestContext context)
        {
            if (context.Residents < MilestoneFromResidents) return default(QuestDefinition);
            int threshold = MilestoneFromResidents + MilestoneStep;
            while (threshold <= context.Residents && threshold <= int.MaxValue - MilestoneStep) threshold += MilestoneStep;
            return new QuestDefinition("q_res_ms_" + threshold, "quest_title_res_milestone",
                "quest_desc_residents", new object[] { threshold }, 200 + threshold,
                c => c.Residents >= threshold);
        }

        // Вехи баланса: от $5000 — удвоение (5000, 10000, ...) в подросшем городе.
        private static QuestDefinition BalanceMilestone(QuestContext context)
        {
            if (context.Buildings < BalanceCityBuildings) return default(QuestDefinition);
            int threshold = BalanceMilestoneFrom;
            while (threshold <= context.Balance && threshold > 0) threshold *= 2;
            if (threshold <= 0) return default(QuestDefinition);
            return new QuestDefinition("q_balance_ms_" + threshold, "quest_title_balance_milestone",
                "quest_desc_balance", new object[] { threshold }, threshold / 5,
                c => c.Balance >= threshold);
        }

        // Веха открыта для выдачи: существует, не выполнена и доступна городу.
        private static bool TryOpen(QuestDefinition quest, QuestContext context,
            HashSet<string> completed, ICollection<string> activeIds, out QuestDefinition opened)
        {
            opened = quest;
            return quest.Id != null && IsOpen(quest, context, completed, activeIds);
        }

        // Квест открыт: не в выполненных, не среди активных, доступен и ещё не выполнен.
        private static bool IsOpen(QuestDefinition quest, QuestContext context,
            HashSet<string> completed, ICollection<string> activeIds)
        {
            if (completed.Contains(quest.Id) || activeIds.Contains(quest.Id)) return false;
            if (!IsAvailable(quest.Id, context)) return false;
            return !quest.IsSatisfied(context);
        }

        // Покрытие служб требует домов: при 0 зданиях город пуст — квест недоступен.
        private static bool IsAvailable(string id, QuestContext context)
        {
            bool cover = id == FireCoverId || id == PoliceCoverId || id == HealthCoverId || id == ServicesAllId;
            return !cover || context.Buildings > 0;
        }
    }
}
