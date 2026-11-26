namespace Citybuilder
{
    // Хвост реестра локализации (issue #126): новые ключи вынесены в отдельную часть,
    // чтобы LocalizationTable.cs не превышал мягкий лимит 150 строк.
    public static partial class LocalizationTable
    {
        static LocalizationTable()
        {
            values["quest_log_title"] = new[] { "=== Задачи города ===", "=== City quests ===" };
            values["quest_log_empty"] = new[] { "Задач пока нет.", "No quests yet." };
            values["quest_reward"] = new[] { "(+{0}$)", "(+{0}$)" };
            values["metric_name_fires"] = new[] { "Пожары", "Fires" };
            values["metric_name_thefts"] = new[] { "Кражи", "Thefts" };
            values["settings_hud_scale"] = new[] { "Шрифт {0}%", "Font {0}%" };
            values["settings_high_contrast"] = new[] { "Контраст", "Contrast" };
            values["settings_autosave_short"] = new[] { "Автосейв: {0} ч", "Autosave: {0} h" };
            values["building_recycling_depot"] = new[] { "Перерабатывающий склад", "Recycling Depot" };
            values["decor_hedge"] = new[] { "Изгородь", "Hedge" };
            values["decor_flowerbed"] = new[] { "Клумба", "Flower bed" };
            values["decor_topiary"] = new[] { "Фигурное дерево", "Topiary" };
            values["building_brick"] = new[] { "Кирпичная дорога", "Brick Road" };
            values["budget_breakdown_credit"] = new[] { "Налоги {0} - Содержание {1} - Кражи {2} + Награды {3} - Процент {4} = {5} $/ч | Долг {6}", "Tax {0} - Upkeep {1} - Theft {2} + Rewards {3} - Interest {4} = {5} $/h | Debt {6}" };
            values["tick_p95_line"] = new[] { "Тик p95: {0} мс / {1}", "Tick p95: {0} ms / {1}" };
            // Районы и рейтинг мэра (issue #169, ADR-019).
            values["district_name"] = new[] { "Район {0}", "District {0}" };
            values["mayor_rating_line"] = new[] { "Рейтинг мэра: {0} (уровень {1}, +{2} $/ч к налогам)", "Mayor rating: {0} (tier {1}, +{2} $/h tax)" };
            values["event_mayor_rating"] = new[] { "Рейтинг мэра вырос: {0} (уровень {1}, +{2} $/ч)", "Mayor rating rose: {0} (tier {1}, +{2} $/h)" };
        }
    }
}
