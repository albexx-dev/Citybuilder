# TASKS — очередь эпиков v1.2 «Живой город»

Каждый эпик = GitHub issue (трекинг, #43–#73) + детальный план здесь (`EPIC-XX-*.md` — источник истины по шагам).
Шаг эпика выполняется по циклу AGENTS.md: 1 шаг = 1 ветка `task/XXX-step` = инженер → ревьюер → зелёный шлюз → мерж оркестратором.
Лимиты строк/файлов действуют на шаг. Новый архитектурный паттерн — через ADR до первого шага с кодом.

Доктрина: **партия не имеет конца** (GAME_DESIGN §19). Роадмап-дубль: pinned issue #74 (чекбоксы прогресса).

## Фазы выполнения

| Фаза | Эпики | Смысл |
|------|-------|-------|
| 1. Мир и модели | #54 модели → #55 CREDITS → #56 декор → #57 земля/свет | уйти от кубов и серой подложки |
| 2. Агенты | #44 машины → #60 модели транспорта → #62 оптимизация → #45 пробки v2 → #46 пешеходы | город оживает |
| 3. Механики | #43 пожары → #47 криминал → #48 квесты → #49 налоги по зонам → #50 блэкауты → #51 апгрейды → #52 мусоровозы → #53 баланс | игрок принимает решения |
| 4. Подача | #58 день/ночь → #59 уровни зданий → #61 стройплощадки → #66 иконки → #67 мини-карта → #68 графики → #69 настройки → #71 звук v2 → #70 туториал v3 | подача уровня игры |
| 5. Системы и релиз | #63 статистика (рано, до #68) → #64 сейв v5 → #65 техдолг → #72 локализация (опц.) → #73 релиз v1.2.0 | закрытие версии |

## Новые ADR до кода
- **ADR-009** — пожары (до #43)
- **ADR-010** — источники ассетов, масштаб, полигонаж (до #54)
- **ADR-011** — агентный менеджер и транспорт (до #44)
- **ADR-012** — система квестов (до #48)

## Статус

| Issue | Эпик | Блок | Статус |
|-------|------|------|--------|
| #43 | [Пожары](EPIC-43-fires.md) | mechanics | not started |
| #44 | [Машины-агенты](EPIC-44-vehicle-agents.md) | mechanics | **done** (task/176) |
| #45 | [Пробки v2](EPIC-45-congestion-v2.md) | mechanics | **done** (task/178) |
| #46 | [Пешеходы](EPIC-46-pedestrians.md) | mechanics | **done** (task/179) |
| #47 | [Криминал и полиция](EPIC-47-crime.md) | mechanics | not started |
| #48 | [Квесты-вехи](EPIC-48-quests.md) | mechanics | not started |
| #49 | [Налоги по типам зон](EPIC-49-zonal-taxes.md) | mechanics | not started |
| #50 | [Блэкауты](EPIC-50-blackouts.md) | mechanics | **done** (task/188) |
| #51 | [Апгрейд служб](EPIC-51-service-upgrades.md) | mechanics | not started |
| #52 | [Мусоровозы](EPIC-52-garbage-trucks.md) | mechanics | not started |
| #53 | [Баланс-проход v1.2](EPIC-53-balance-v12.md) | mechanics | not started |
| #54 | [Модели всех зданий](EPIC-54-building-models.md) | content | **done** (task/163) |
| #55 | [CREDITS.md](EPIC-55-credits.md) | content | **done** (task/163) |
| #56 | [Деревья и декор](EPIC-56-decor.md) | content | **done** (task/165) |
| #57 | [Земля и свет](EPIC-57-ground-light.md) | content | **done** (task/167) |
| #58 | [День/ночь](EPIC-58-day-night.md) | ux | **done** (task/184) |
| #59 | [Уровни зданий](EPIC-59-house-levels.md) | ux | **done** (task/185) |
| #60 | [Модели транспорта](EPIC-60-vehicle-models.md) | content | **done** (task/177) |
| #61 | [Стройплощадки](EPIC-61-construction-sites.md) | content | **done** (task/186) |
| #62 | [Оптимизация агентов](EPIC-62-agent-perf.md) | architecture | **done** (task/181) |
| #63 | [Статистика-ядро](EPIC-63-metrics-core.md) | architecture | **done** (task/182; интеграция + гард в task/183) |
| #64 | [Сейв v5](EPIC-64-save-v5.md) | architecture | not started |
| #65 | [Техдолг #42 + сплиты](EPIC-65-tech-debt.md) | architecture | **done** (task/183) |
| #66 | [Иконки UI + тултипы](EPIC-66-ui-icons.md) | ux | not started |
| #67 | [Мини-карта](EPIC-67-minimap.md) | ux | not started |
| #68 | [Графики в инфоменю](EPIC-68-graphs.md) | ux | not started |
| #69 | [Окно настроек](EPIC-69-settings.md) | ux | not started |
| #70 | [Туториал v3](EPIC-70-tutorial-v3.md) | ux | not started |
| #71 | [Звук v2](EPIC-71-sound-v2.md) | ux | optional |
| #72 | [Локализация EN](EPIC-72-localization.md) | ux | optional |
| #73 | [Релиз v1.2.0](EPIC-73-release.md) | release | not started |

Прогресс отмечается здесь и чекбоксами в issue #74. Две задачи параллельно запрещены.
