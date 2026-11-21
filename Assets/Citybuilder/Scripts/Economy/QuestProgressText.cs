namespace Citybuilder
{
    // Строки прогресса задач для панели QuestLogView (EPIC-48, ADR-012): вынесены из
    // QuestSystem при сплите файла. Чистые функции над снимком города, без состояния.
    public static class QuestProgressText
    {
        private const string ResPrefix = "q_res";
        private const string GrowPrefix = "q_grow";
        private const string BalancePrefix = "q_balance";
        private const string ParkPrefix = "q_park";

        // Строка прогресса для панели задач («Жители 12/50»); у задачи без цели — null.
        public static string ProgressLine(QuestDefinition quest, QuestContext context)
        {
            string id = quest.Id;
            if (string.IsNullOrEmpty(id)) return null;
            int target = ParseTarget(id);
            if (id.StartsWith(ResPrefix) && target > 0) return Localization.T("quest_progress_residents", context.Residents, target);
            if (id.StartsWith(GrowPrefix) && target > 0) return Localization.T("quest_progress_buildings", context.Buildings, target);
            if (id.StartsWith(BalancePrefix) && target > 0) return Localization.T("quest_progress_balance", context.Balance, target);
            if (id.StartsWith(ParkPrefix) && target > 0) return Localization.T("quest_progress_parks", context.Parks, target);
            return CoverLine(id, context);
        }

        // Квесты покрытия: процент домов под службой против 100%.
        private static string CoverLine(string id, QuestContext context)
        {
            if (id == QuestCatalog.PoliceCoverId) return Localization.T("quest_progress_police", Percent(context.PoliceCoverage));
            if (id == QuestCatalog.HealthCoverId) return Localization.T("quest_progress_health", Percent(context.HealthCoverage));
            if (id == QuestCatalog.ServicesAllId) return Localization.T("quest_progress_services", Percent(WorstCoverage(context)));
            return Localization.T("quest_progress_fire", Percent(context.FireCoverage));
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
    }
}
