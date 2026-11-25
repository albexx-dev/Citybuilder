using System.Collections.Generic;

namespace Citybuilder
{
    // Хвост реестра (issue #100 шаг 3): настройки, инфоменю, инспекция и статус роста.
    // Отдельный файл удерживает LocalizationTable в пределах лимита строк.
    public static class LocalizationTailTable
    {
        private static readonly Dictionary<string, string[]> values = new Dictionary<string, string[]>
        {
            // Переименование слота (#117): кнопка и подсказка валидации имени.
            { "save_rename", new[] { "Переименовать", "Rename" } },
            { "save_name_hint", new[] { "Имя: 1–{0} симв.", "Name: 1–{0} chars" } },
            { "save_name_invalid", new[] { "Имя пустое или длиннее {0} симв.", "Name empty or over {0} chars" } },
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
            { "hint_hotkeys_panels", new[] { "Панели: Tab статика · L лента · J задачи · O настройки", "Panels: Tab info · L log · J quests · O settings" } },
            { "hint_hotkeys_keys", new[] { "Клавиши: 1-5 режимы · Esc панель/режим/пауза · F5 сохранить · F9 загрузить", "Keys: 1-5 modes · Esc panel/mode/pause · F5 save · F9 load" } },
            { "hint_hotkeys_camera", new[] { "Камера: WASD / Q E / колесо / ПКМ — поворот / ЛКМ — стройка", "Camera: WASD / Q E / wheel / RMB rotate / LMB place" } },
            { "settings_ambient", new[] { "Эмбиент: {0}%", "Ambient: {0}%" } },
            { "settings_agent_limit", new[] { "Машины: лимит {0}", "Vehicles: limit {0}" } },
            { "settings_autosave", new[] { "Автосейв: каждые {0} ч", "Autosave: every {0} h" } },
            { "settings_overlay_coverage", new[] { "Покрытие", "Coverage" } },
            { "settings_overlay_congestion", new[] { "Пробки", "Traffic" } },
            { "settings_overlay_grid", new[] { "Сетка", "Grid" } },
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
            { "growth_slow", new[] { " (медленнее ×{0}: {1})", " (slower ×{0}: {1})" } },
            // Туториал v3 (EPIC-70) и подсказки событий (эпик 97): RU — старые тексты байт-в-байт.
            { "tutorial_dismiss", new[] { "Понятно", "Got it" } },
            { "minimap_tip", new[] { "Клик — перейти к клетке", "Click to focus the cell" } },
            { "tutorial_select", new[] { "Тип выбран: кликни по свободной клетке на карте", "Type selected: click a free tile on the map" } },
            { "tutorial_step_1", new[] { "Шаг 1/7. Нажми кнопку «Зоны [3]» на нижней панели, выбери кисть «Жилая» и закрась участок около 4×4 клетки на карте.", "Step 1/7. Press «Zones [3]» on the bottom bar, pick the «Residential» brush and paint an area of about 4x4 tiles on the map." } },
            { "tutorial_step_2", new[] { "Шаг 2/7. Нажми кнопку «Рост [4]» на нижней панели: дома на закрашенных клетках вырастут сами.", "Step 2/7. Press «Growth [4]» on the bottom bar: houses on painted tiles will grow on their own." } },
            { "tutorial_step_3", new[] { "Шаг 3/7. Дождись, когда вырастет первый дом. Кнопка «x4» на нижней панели ускоряет время.", "Step 3/7. Wait for the first house to grow. The «x4» button on the bottom bar speeds up time." } },
            { "tutorial_step_4", new[] { "Шаг 4/7. Нажми кнопку «Здания [1]», выбери {0} в палитре НАД ПАНЕЛЬЮ и кликни свободную клетку на карте.", "Step 4/7. Press «Buildings [1]», pick {0} in the palette ABOVE THE BAR and click a free tile on the map." } },
            { "tutorial_step_5", new[] { "Шаг 5/7. Нажми кнопку «Здания [1]», выбери {0} в палитре НАД ПАНЕЛЬЮ и кликни свободную клетку на карте.", "Step 5/7. Press «Buildings [1]», pick {0} in the palette ABOVE THE BAR and click a free tile on the map." } },
            { "tutorial_step_6", new[] { "Шаг 6/7. Нажми кнопку «Здания [1]», выбери {0} в палитре НАД ПАНЕЛЬЮ и кликни свободную клетку на карте.", "Step 6/7. Press «Buildings [1]», pick {0} in the palette ABOVE THE BAR and click a free tile on the map." } },
            { "tutorial_step_7", new[] { "Шаг 7/7. Нажми кнопку «Здания [1]», выбери {0} в палитре НАД ПАНЕЛЬЮ и кликни свободную клетку на карте — людям приятнее жить.", "Step 7/7. Press «Buildings [1]», pick {0} in the palette ABOVE THE BAR and click a free tile on the map — people enjoy living nearby." } },
            { "tutorial_event_fire", new[] { "Пожар! Построй пожарную станцию: открой вкладку «Службы» и выбери карточку в каталоге над панелью.", "Fire! Build a fire station: open the «Services» tab and pick the card in the catalog above the bar." } },
            { "tutorial_event_reward", new[] { "Задача выполнена — награда зачислена в казну. Так город растёт дальше.", "Quest complete — the reward was added to the treasury. That is how the city keeps growing." } },
            { "tutorial_event_waste", new[] { "Мусор впервые вывезен: мусоровоз забрал его из дома. Следи за очередью в статистике.", "Waste collected for the first time: the truck picked it up from a house. Watch the queue in Statistics." } },
            { "tutorial_event_night", new[] { "Наступила ночь! Город живёт по часам — проверь часы в HUD.", "Night has fallen! The city runs on a clock — check the clock in the HUD." } },
            // Строки инфоменю: кешируются InfoMenuView, поэтому язык меняется по событию LanguageChanged.
            { "needs_base", new[] { "Потребности: {0}% (жильё {1}% • работа {2}% • занятость {3}%", "Needs: {0}% (housing {1}% • jobs {2}% • employment {3}%" } },
            { "needs_power", new[] { " • энергия {0}%", " • power {0}%" } },
            { "needs_water", new[] { " • вода {0}%", " • water {0}%" } },
            { "needs_waste", new[] { " • мусор {0}%", " • waste {0}%" } },
            { "needs_fire", new[] { " • пожарные {0}%", " • fire {0}%" } },
            { "needs_health", new[] { " • здоровье {0}%", " • health {0}%" } },
            { "needs_leisure", new[] { " • досуг {0}%", " • leisure {0}%" } },
            { "service_label_fire", new[] { "Пожарные", "Fire" } },
            { "service_label_police", new[] { "Полиция", "Police" } },
            { "service_label_health", new[] { "Больницы", "Hospitals" } },
            { "service_label_leisure", new[] { "Досуг", "Leisure" } },
            { "goal_done", new[] { "Цель выполнена: {0} жителей", "Goal reached: {0} residents" } },
            { "goal_progress", new[] { "Цель: {0}/{1} жителей (ступень {2}/{3})", "Goal: {0}/{1} residents (stage {2}/{3})" } },
            { "progression_all", new[] { "Все типы открыты", "All types unlocked" } },
            { "progression_next", new[] { "Дальше: {0} — {1} зд.", "Next: {0} — {1} buildings" } },
            { "metrics_title", new[] { "=== История ===", "=== History ===" } },
            // Масштаб графиков (#113): кнопка показывает текущий диапазон и переключает 8/30 дней.
            { "metrics_range", new[] { "Масштаб: {0} дн. (8/30)", "Range: {0} d (8/30)" } },
            { "metrics_empty", new[] { "Копится с началом игровых часов", "Accumulates as game hours pass" } },
            { "metric_name_population", new[] { "Население", "Population" } },
            { "metric_name_treasury", new[] { "Казна", "Treasury" } },
            { "metric_name_needs", new[] { "Нужды", "Needs" } },
            { "metric_name_power", new[] { "Энергия", "Power" } }
        };

        public static void AddTo(Dictionary<string, string[]> target)
        {
            foreach (KeyValuePair<string, string[]> pair in values) target[pair.Key] = pair.Value;
        }
    }
}
