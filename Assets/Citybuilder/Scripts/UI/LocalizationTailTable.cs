using System.Collections.Generic;

namespace Citybuilder
{
    // Хвост реестра (issue #100 шаг 3): настройки, инфоменю, инспекция и статус роста.
    // Отдельный файл удерживает LocalizationTable в пределах лимита строк.
    public static class LocalizationTailTable
    {
        private static readonly Dictionary<string, string[]> values = new Dictionary<string, string[]>
        {
            // Настройки: звук, качество, gameplay и футер окна.
            { "settings_title", new[] { "=== Настройки ===", "=== Settings ===" } },
            { "settings_section_sound", new[] { "Звук", "Sound" } },
            { "settings_mute", new[] { "Без звука", "Mute" } },
            { "settings_section_quality", new[] { "Качество и агенты", "Quality and agents" } },
            { "settings_decor", new[] { "Декор на свободных клетках", "Decor on empty tiles" } },
            { "settings_day_night", new[] { "День и ночь", "Day and night" } },
            { "settings_section_game", new[] { "Игра", "Game" } },
            { "settings_defaults", new[] { "По умолчанию", "Defaults" } },
            { "settings_back", new[] { "Назад в паузу [Esc]", "Back to pause [Esc]" } },
            { "settings_replay_tutorial", new[] { "Повторить обучение", "Replay tutorial" } },
            { "settings_controls", new[] { "Управление: WASD / Q E / колесо / ЛКМ", "Controls: WASD / Q E / wheel / LMB" } },
            { "settings_ambient", new[] { "Эмбиент: {0}%", "Ambient: {0}%" } },
            { "settings_agent_limit", new[] { "Машины: лимит {0}", "Vehicles: limit {0}" } },
            { "settings_autosave", new[] { "Автосейв: каждые {0} ч", "Autosave: every {0} h" } }
        };

        public static void AddTo(Dictionary<string, string[]> target)
        {
            foreach (KeyValuePair<string, string[]> pair in values) target[pair.Key] = pair.Value;
        }
    }
}
