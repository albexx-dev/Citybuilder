using System.Collections.Generic;

namespace Citybuilder
{
    // Ключи всех сообщений ленты и их RU/EN-тексты (ADR-013, issue #88).
    // Ключи живут в домене; реестр Localization получает таблицу при старте типа.
    public static class EventMessageCatalog
    {
        public const string CongestionSlowdown = "event_congestion_slowdown";
        public const string TreasuryDeficit = "event_treasury_deficit";
        public const string UnemploymentHigh = "event_unemployment_high";
        public const string QuestCompleted = "event_quest_completed";
        public const string SupplyPowerDeficit = "event_supply_power_deficit";
        public const string SupplyWaterDeficit = "event_supply_water_deficit";
        public const string SupplyWasteDeficit = "event_supply_waste_deficit";
        public const string SupplyRestored = "event_supply_restored";
        public const string Blackout = "event_blackout";
        public const string BlackoutRestored = "event_blackout_restored";
        public const string FireStart = "event_fire_start";
        public const string FireExtinguished = "event_fire_extinguished";
        public const string FireBurned = "event_fire_burned";
        public const string CrimeTheft = "event_crime_theft";
        public const string GarbageCollected = "event_garbage_collected";
        public const string DecisionOffered = "event_decision_offered";
        public const string DecisionSuppressed = "event_decision_suppressed";
        public const string DecisionIgnored = "event_decision_ignored";
        public const string DecisionTitle = "decision_title";
        public const string DecisionSuppressButton = "decision_button_suppress";
        public const string DecisionIgnoreButton = "decision_button_ignore";

        // Все ключи каталога: тест полноты берёт перечень отсюда, не хардкодя 15 строк.
        public static ICollection<string> Keys
        {
            get { return Messages.Keys; }
        }

        private static readonly Dictionary<string, string[]> Messages = new Dictionary<string, string[]>
        {
            { CongestionSlowdown, new[] { "Пробки замедляют рост ×{0}", "Traffic slows growth ×{0}" } },
            { TreasuryDeficit, new[] { "Дефицит казны: баланс ${0}", "Treasury deficit: balance ${0}" } },
            { UnemploymentHigh, new[] { "Высокая безработица: занятость {0}%", "High unemployment: employment {0}%" } },
            { QuestCompleted, new[] { "Задача выполнена: {0} (+{1}$)", "Quest completed: {0} (+{1}$)" } },
            { SupplyPowerDeficit, new[] { "Энергия {0}%: рост замедлен", "Power {0}%: growth slowed" } },
            { SupplyWaterDeficit, new[] { "Вода {0}%: рост замедлен", "Water {0}%: growth slowed" } },
            { SupplyWasteDeficit, new[] { "Мусор {0}%: рост замедлен", "Waste {0}%: growth slowed" } },
            { SupplyRestored, new[] { "Снабжение восстановлено: 100%", "Supply restored: 100%" } },
            { Blackout, new[] { "Блэкаут: {0} зданий без энергии", "Blackout: {0} buildings without power" } },
            { BlackoutRestored, new[] { "Электроснабжение восстановлено", "Power supply restored" } },
            { FireStart, new[] { "Пожар в доме #{0}!", "Fire in house #{0}!" } },
            { FireExtinguished, new[] { "Пожар в доме #{0} потушен", "Fire in house #{0} extinguished" } },
            { FireBurned, new[] { "Дом #{0} сгорел дотла", "House #{0} burned down" } },
            { CrimeTheft, new[] { "Кража в доме #{0} (День {1})", "Theft in house #{0} (Day {1})" } },
            { GarbageCollected, new[] { "Мусоровоз вывез мусор из дома #{0}", "Garbage truck collected waste from house #{0}" } },
            { DecisionOffered, new[] { "Пожар в доме #{0}: тушить за ${1}?", "Fire in house #{0}: fight for ${1}?" } },
            { DecisionSuppressed, new[] { "Пожар в доме #{0} потушен за ${1}", "Fire in house #{0} extinguished for ${1}" } },
            { DecisionIgnored, new[] { "Пожар в доме #{0} проигнорирован", "Fire in house #{0} ignored" } },
            { DecisionTitle, new[] { "Решение: пожар в доме #{0}", "Decision: fire in house #{0}" } },
            { DecisionSuppressButton, new[] { "Тушить за ${0}", "Fight for ${0}" } },
            { DecisionIgnoreButton, new[] { "Игнорировать", "Ignore" } }
        };

        // Регистрация в общем реестре: ключи ленты попадают в Localization.Keys.
        public static void RegisterTo(Dictionary<string, string[]> table)
        {
            foreach (KeyValuePair<string, string[]> pair in Messages) table[pair.Key] = pair.Value;
        }
    }
}
