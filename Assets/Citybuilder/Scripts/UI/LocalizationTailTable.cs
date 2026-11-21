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
            { "settings_autosave", new[] { "Автосейв: каждые {0} ч", "Autosave: every {0} h" } },
            // Инфоменю: заголовки разделов и строка преступности.
            { "info_section_city", new[] { "=== Город ===", "=== City ===" } },
            { "info_section_goal", new[] { "=== Цель ===", "=== Goal ===" } },
            { "info_section_networks", new[] { "=== Сети ===", "=== Utilities ===" } },
            { "info_section_services", new[] { "=== Службы ===", "=== Services ===" } },
            { "info_section_budget", new[] { "=== Бюджет ===", "=== Budget ===" } },
            { "info_section_development", new[] { "=== Развитие ===", "=== Development ===" } },
            { "info_crime", new[] { "Преступность: {0}%", "Crime: {0}%" } },
            // Инспекция выбранного здания и кнопка улучшения.
            { "info_panel_title", new[] { "=== {0} ===", "=== {0} ===" } },
            { "info_panel_cost", new[] { "Стоимость: ${0} • Содержание: ${1}/ч", "Cost: ${0} • Upkeep: ${1}/h" } },
            { "info_panel_residents", new[] { "Жители: {0}/{1} • Дорога: {2}", "Residents: {0}/{1} • Road: {2}" } },
            { "info_yes", new[] { "да", "yes" } },
            { "info_no", new[] { "нет", "no" } },
            { "info_panel_level", new[] { "Уровень: {0}", "Level: {0}" } },
            { "info_panel_supply", new[] { "Свет {0}% • Вода {1}% • Мусор {2}%", "Power {0}% • Water {1}% • Waste {2}%" } },
            { "info_panel_safety", new[] { "Пожарные {0}% • Полиция {1}%", "Fire {0}% • Police {1}%" } },
            { "info_panel_care", new[] { "Больница {0}% • Парк {1}%", "Hospital {0}% • Park {1}%" } },
            { "info_panel_radius", new[] { "Радиус: {0} кл. • Покрыто домов: {1}/{2}", "Radius: {0} tiles • Homes covered: {1}/{2}" } },
            { "info_panel_jobs", new[] { "Рабочих мест: {0}", "Jobs: {0}" } },
            { "info_panel_upgrade", new[] { "Улучшить — ${0}", "Upgrade — ${0}" } },
            // Статус роста: склонение «дом», причины остановки и замедления.
            { "house_one", new[] { "дом", "house" } },
            { "house_few", new[] { "дома", "house" } },
            { "house_many", new[] { "домов", "houses" } },
            { "growth_low_attraction", new[] { " • низкая привлекательность ({0}%)", " • low attractiveness ({0}%)" } },
            { "growth_head", new[] { "Рост: {0} {1}", "Growth: {0} {1}" } },
            { "growth_off", new[] { " — выключен, нажми [4]", " — off, press [4]" } },
            { "growth_no_zones", new[] { " — нет жилых зон, нажми [3]", " — no residential zones, press [3]" } },
            { "growth_capacity", new[] { " — потолок {0} достигнут", " — cap {0} reached" } },
            { "growth_no_money", new[] { " — нет денег: дом стоит ${0}", " — no money: house costs ${0}" } },
            { "growth_deficit", new[] { " — казна пуста: содержание больше дохода", " — treasury empty: upkeep exceeds income" } },
            { "growth_next", new[] { ", следующий через {0} ч", ", next in {0} h" } },
            { "growth_slow_both", new[] { "дефицит снабжения и пробки", "supply shortage and traffic" } },
            { "growth_slow_supply", new[] { "дефицит снабжения", "supply shortage" } },
            { "growth_slow_jams", new[] { "пробки", "traffic" } },
            { "growth_slow", new[] { " (медленнее ×{0}: {1})", " (slower ×{0}: {1})" } }
        };

        public static void AddTo(Dictionary<string, string[]> target)
        {
            foreach (KeyValuePair<string, string[]> pair in values) target[pair.Key] = pair.Value;
        }
    }
}
