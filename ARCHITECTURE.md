# Citybuilder Architecture

## 1. Project goal

Подробная 3D city-building игра в Unity 6. Акцент — глубокая симуляция связанных систем: дороги влияют на доступность, транспорт на время поездок, работа на доход, доход на потребление, коммуналка и службы на качество жизни и привлекательность города.

## 2. Architectural principles

- Маленькие проверяемые задачи, 1 задача = 1-3 файла, до 200 строк на C#-файл (жёсткий лимит валидатора).
- Модульный монолит, чёткие границы модулей.
- Plain C# domain logic внизу, Unity-адаптеры сверху, UI только команды и отображение.
- State отдельно от View, Definition отдельно от State.
- Никаких огромных Manager-классов и глобальных синглтонов.

## 3. Modular monolith approach

Одна Unity-игра, разделённая на логические модули. Не делаем: микросервисы, ECS/DOTS без нужды, десятки абстракций, глобальную шину событий на всё, плагины.

Поток: `Plain C# domain logic → Unity adapters / MonoBehaviours → Visual presentation / UI`.

## 4. Separation of definition, state and view

- `Definition` (ScriptableObject): тип объекта. Пример `BuildingDefinition`: id, displayName, footprint, constructionCost, upkeepPerHour, serviceKind, serviceRadius, variantCount, prefab.
- `State` (plain C#): экземпляр в городе. Пример `BuildingState`: EntityId, ссылка на definition, позиция, занятые клетки. Никогда не хранить runtime в ScriptableObject.
- `View` (MonoBehaviour): отображение — GameObject, материалы, фаза стройки (`GrowthStageView`), подсветка радиуса. Не решает правила игры.

## 5. Simulation time and ticks

Frame Update каждый кадр: камера, ввод, плавная доводка масштаба стройки, предпросмотр, UI.
Simulation Tick периодически: экономика, производство, жители, сети, перекраска пробок на дорогах, автосейв, запись событий в ленту.

Locked V1 / v1.1: Tick 1 сек = 1 игровой час, день = 24 тика, FastForward x4. Пауза останавливает тики. К тику подключены:
- часы (`SimulationRunner`),
- казна (`EconomyTicker`: сбор налогов, списание содержания `UpkeepPolicy.UpkeepPerHour`, запись событий дефицита/безработицы/пробок/снабжения в `CityEventLog`),
- рост поселения (`SettlementGrowthController` → `GrowthPace`: худший множитель из пробок `CongestionController.GrowthTickMultiplier` и снабжения `SupplyGrowthController.GrowthTickMultiplier`, ×1/×2/×3),
- визуализация пробок (`CongestionController` → `RoadView.RefreshCongestionOncePerTick`),
- автосохранение каждые 24 тика (`AutosaveController`),
- применение отложенных решений (`DecisionController`, ADR-020) на границе тика; сами ожидающие решения не сериализуются.

## 6. Commands, queries and events

UI не меняет мир напрямую: `Команда → проверка → изменение мира → событие → обновление View/UI`.
Событийная лента для игрока (`CityEventLog`, кольцевой буфер на 50 записей с дедупликацией в окне 10 тиков, ADR-007) предназначена исключительно для отображения причин в `EventLogView`, а не для межсистемной связности.

## 7. Coordinate system

Квадратная сетка, клетка 1×1 м, начало (0,0), без высоты (V1, ADR-017). Размер — конфигурация: единственный источник `Width`/`Height`/`CellSize` — `GridVisualizer` (сцена задаёт те же поля в инспекторе); остальные слои читают `GridVisualizer.Grid.Size` / `GridVisualizer.Grid.CellSize`. Целевой конфиг v1.4.5 — 100×100 при `CellSize` 1 м; конкретные числа живут в сцене/инспекторе, а не в коде и не в контракте модуля. Второй константы 50 нет. Уровень (`Level`) вводится только для дорог и мостов по ADR-018; здания, зоны и сети остаются одноуровневыми.
Базовый тип `GridPosition (X, Y)` (без высоты). Операции сетки: IsInside, WorldToGrid, GridToWorld, IsOccupied, Occupy, Release. Для дорог используется составной ключ `RoadCellKey(X, Y, Level)`.

## 8. Core systems

- Core: EntityId, GridPosition, GridSize, SimulationTime, NeedProviders (6 провайдеров, включая `HealthNeedProvider` и `LeisureNeedProvider`), SupplyBalance, UnlockPolicy, CityEventLog, EventKind, CityEvent.
- World: Map, Grid, ZonePainting, снимок клеток.
- Construction: BuildingDefinition/State, PlacementController, RoadPlacementController (`RoadDragInput`), Demolition, View.
- Economy: Treasury (допускает отрицательный баланс/дефицит при списании `Charge`), TaxSystem, TaxPolicy, UpkeepPolicy, UpkeepStatusPolicy, AttractivenessPolicy, EconomyTicker.
- Population: SettlementSystem, SettlementGrowthController, GrowthPace, PopulationController (интервал заселения масштабируется `AttractivenessPolicy.Multiplier` и блокируется при `OverallSatisfaction < 0.5`), HouseholdState, GoalSystem (вехи развития без `Won/Lost`), GoalStages, GoalRewardController.
- Infrastructure: SupplyBalance (энергия / вода / мусор), SupplyGrowthPolicy + SupplyGrowthController, обобщённые службы v2 (`ServiceKind`: Fire, Police, Health, Leisure; `ServiceSource`, `ServiceCoverage`, `ServiceBuildingSystem`, `ServiceCoverageController`, ADR-006), `BuildingInfoQuery` (инспекция клетки/здания и расчёт клеток радиуса), `NeedSystem` (9 показателей).
- Transport: ConnectivitySystem, RouteSystem (A* по дорогам), CongestionSystem (`Load01`, `CongestedCellsCount`, `WorstLoad01`, `FormatSummaryLine`) + CongestionController + `RoadView` (перекраска дорог зелёный/жёлтый/красный по порогам 0.33 и 0.66).
- Presentation / UI:
  - Нижняя панель действий (`BottomBarView`, `BottomBarStyle`, `HudLayout`, EPIC-13),
  - Информационное меню по кнопке «Статистика» / `Tab` (`InfoMenuView`, EPIC-14),
  - Событийная лента по кнопке «Лента» / `L` (`EventLogView`, EPIC-09),
  - Инфопанель выбранного здания с подсветкой радиуса службы на сетке (`InfoPanelView` + `GridVisualizer.ShowRadiusOverlay`, EPIC-12),
  - Главное меню (`MainMenu`), меню паузы по `Esc` с подтверждением несохранённого выхода и настройками звука (`PauseMenuView`, EPIC-16), окна слотов (`SaveMenuView`, `LoadMenuView`),
  - Пошаговый туториал v2 из 7 шагов с подсветкой кнопок (`TutorialPolicy` + `TutorialView`, EPIC-18),
  - Стадии роста зданий (3 тика каркаса 0.6 масштаба) и 3 детерминированных варианта домов (`GrowthStageView`, `SettlementView`, EPIC-17),
  - Звуковая подсистема (`AudioController` + 4 процедурных CC0 WAV-клипа в `Assets/Citybuilder/Audio/`, EPIC-19),
  - Blender-конвейер (`Tools/blender-addon/citybuilder_export.py`, `generate_models.py`) и low-poly FBX-модели `hospital.fbx` и `park.fbx` (EPIC-20).
- Persistence: сейвы v2 (ADR-008, EPIC-11) — 3 ручных слота (`slot0.json`..`slot2.json`), отдельный слот автосейва (`autosave.json`), индекс метаданных (`slots.json` через `SaveSlotIndex`), `AutosaveController` (каждые 24 тика), `SaveSlotStorage`, `SaveController` (`IsDirty`), `LoadController`. Снапшот v7 — см. §12 и ADR-022.
- v1.4.5 (issue #171, ADR-017…022): конфигурируемый размер карты из `GridVisualizer` (100×100); четыре типа дорог и мосты (`RoadCellKey`, съезды 1↔2); `MarketPricePolicy`, `CreditLedger`, районы-оверрайды и рейтинг мэра; `DecisionController` с append-only лентой; сезоны/погода и emissive-окна как presentation-фильтр; сейв v7. Подробности — §16.

## 9. Dependencies

```
Core
 ├── Simulation, World, Economy, Construction, Resources,
 │   Infrastructure, Transport, Population, Persistence
```

## 10. Dependency restrictions

Запрещены циклы вида BuildingSystem → EconomySystem → PopulationSystem → BuildingSystem. Вместо прямых ссылок: команды, запросы, события, порядок симуляции.

## 11. Unity integration

Тонкие MonoBehaviour-адаптеры: ссылка на логику + вызов 1-2 методов. Сцена: `Prototype.unity` (единственная игровая).

## 12. Data persistence

Реализовано: снапшот v7 (`CitySnapshot`, ADR-022) сохраняется в 3 именованных слота (`slot0.json`..`slot2.json`) и автосейв (`autosave.json`) с метаданными в `slots.json` (`SaveSlotIndex`: имя слота, игровое время, население, баланс, штамп времени). Запись — версия 7, чтение — версии 1–7, версия > 7 отклоняется валидацией слота. Опциональные поля v7 (`RoadEntry.Type`/`Level`, `Debt`/`DebtInterestPercent`, `MarketMultiplier`, `DistrictTaxRates`, `MayorRating`, `MilestoneCounterV2`) читаются с fallback на v6; нормализация `SaveV6Snapshot.Normalize` и `SaveV7Snapshot.Normalize` не бросает исключений. Экспорт/импорт `SlotTransfer` использует тот же JSON и те же проверки. Размер сетки не сериализуется, поэтому старые слоты не мигрируют. Отрицательный баланс казны при дефиците корректно сохраняется и восстанавливается (`Treasury.Restore`). Пожары, агенты и отложенные решения не сериализуются (ADR-009/011/020).

## 13. Testing strategy

- EditMode-тесты на всю чистую логику и адаптеры: `X` тестов в `X` файлах (`Assets/Citybuilder/Tests/`, `X` `[Test]` + `X` `[TestCase]`), плюс `X` PlayMode-тестов. Числа — плейсхолдеры `X`, итог фиксирует оркестратор на гейте #171. Статическая валидация — `Tools/ValidateRepo.py` в CI (0 `ERROR`).

## 14. Performance strategy

Ориентир v1.4.5: сетка 100×100 = 10000 клеток только логика, без GameObject на клетку. Визуал сетки и оверлея радиуса — процедурные меши (`GridMeshBuilder`). Перекраска пробок выполняется строго 1 раз за тик часов, а кеш строк `InfoMenuView` и `EventLogView` обновляется не чаще 2 раз в секунду. Лимиты агентов 12 машин и 1..30 пешеходов не меняются без замеров Profiler по триггеру ADR-001 (`TotalResidents > 1000` при давлении на тик либо p95 тика > 5 мс); Jobs/Burst/DOTS требуют отдельного ADR.

## 15. Current implementation scope

v1.1 systems (see v1.1.0 scope history in git) plus the v1.2 «Living city» milestone:

### Simulation (deterministic domain, plain C#)

- **Fires (EPIC-43, ADR-009):** `FireSystem` ignites uncovered homes via FNV-1a hash(tick, EntityId); stations extinguish in one tick, uncovered homes burn down after 6 ticks (cells freed, residents evicted). State is not serialized.
- **Crime (EPIC-47):** `CrimeSystem` thefts below police coverage (salt-separated hash), treasury fine + temporary leisure penalty.
- **Vehicle agents (ADR-011, EPIC-44/45):** `AgentManager` spawns one vehicle **per newly built road cell** (owner decision — no time windows), routes through `RouteSystem` over player roads, congestion slows agents and recolors roads.
- **Pedestrians (EPIC-46):** `PedestrianAgentManager` crowd (1:5 residents).
- **Garbage trucks (EPIC-52):** `GarbageSystem` waste accumulation (salt 0x5A), 2-truck fleet dispatched from depot over roads via `RouteSystem` with nearest-road leg mapping.
- **Quests (EPIC-48, ADR-012):** `QuestSystem`/`QuestCatalog`/`QuestTicker` — 10 starter quests + endless milestone generator, 3 active, treasury rewards; chain persists in snapshot v5 (active/completed ids).

### Presentation

- **Day/night (EPIC-58)** by HourOfDay; **house levels (EPIC-59)** 1–3 with comfort hysteresis; **construction sites (EPIC-61)** for all building types; **Kenney models** for vehicles/buildings/decor; **minimap (EPIC-67)** with click-to-focus; **info-menu graphs (EPIC-68)** for the last 8 game days.
- **Ambient audio (EPIC-71):** procedural day/night loops crossfaded by HourOfDay, scaled by city activity; **event sounds** (fire/crime) via log polling (`AudioEventView`).
- **UI icons & tooltips (EPIC-66):** CC0 Kenney icons on all buttons, 0.4s hover delay; **RU/EN localization (EPIC-72)** registry for HUD/menus (domain event log stays RU).
- **Settings window (EPIC-69):** volume, ambient volume, day/night, agent limit, autosave interval, decor toggle — applied live to systems, persisted in `GameSettings` profile.

### Persistence

- **Save v5 (EPIC-64):** building levels, per-zone tax rates, quest chain ids; validation on load.
- **Save v6 (v1.3, issue #106, ADR-014):** optional garbage/theft/comfort/construction fields with v5 fallback, validation in `SaveSlotStorage`, v1–v5 slots stay loadable. Fires and vehicle agents stay non-serialized (ADR-009/ADR-011).

### Balance (EPIC-53)

- Real upkeep for industry/services, zonal tax ladder **1/2/4** (default Normal), verified by `EconomyBalanceV12Tests` scenarios.


## 16. Future decisions (v1.4+)

v1.4 patterns are fixed by `DECISIONS/ADR-016-v14-scope.md` (issue #111, 2026-11-25), accepted before the first code step:

- **Metrics:** `MetricsHistory` ring grows from 192 (8 days) to 720 hourly samples (30 days) and gains per-hour fire/theft counters; `MetricsRecorder` stays a thin `TickCompleted` adapter, history stays session-only (fires/agents are not serialized — ADR-009/011/014).
- **Overlays:** player toggles in the settings window, persisted in the `GameSettings` profile; presentation layer only — simulation and HUD lines are unchanged.
- **Slot export/import (optional):** reuses `CitySnapshot` v6 with all ADR-014 validation/fallback rules; no new format, serializer or cloud saves; fires and vehicle agents remain ephemeral.
- **Agent LOD/culling:** blocked by ADR-001 until Profiler numbers (`TotalResidents > 1000` with tick pressure or simulation tick p95 > 5 ms); no Jobs/Burst/DOTS without a new ADR.

Earlier decisions already shipped: event-log localization (`DECISIONS/ADR-013-event-localization.md`), catalog names from assets, depot variants and save v6 (`DECISIONS/ADR-014-save-v6.md`). Per-building ambient occlusion remains a candidate, but is outside the v1.4 scope above.

### v1.4.5 «Большой город» — decisions fixed before code (issue #171)

ADR-017…022 зафиксированы до первого шага с кодом и описывают фактический скоуп релиза. Числа тестов и версия сборки в документах помечены `X` — их финализирует оркестратор на гейте #171.

- **ADR-017 — масштаб (`DECISIONS/ADR-017-scale.md`):** сетка 100×100 при `CellSize` 1 м, размеры только из `GridVisualizer`; потолок застройки конфигурируемый (400 домов при `HouseholdSize=4` (1300+ жителей)), точные значения — в сцене/инспекторе. Лимиты агентов 12/30 не меняются без замеров Profiler. `Width`/`Height`/`CellSize` не сериализуются — слоты v1–v6 грузятся без миграции.
- **ADR-018 — дороги и мосты (`DECISIONS/ADR-018-roads-bridges.md`):** четыре `RoadDefinition` — `asphalt`/`brick` на level 1 и `wood`/`stone` на level 2; составной ключ `RoadCellKey(X, Y, Level)`, съезды 1↔2 на концах моста, приоритет верхнего уровня при показе и выборе; снос моста освобождает только его level. Обычные дороги идут через существующий A* без изменений. `RoadEntry` получает опциональные `Type`/`Level` с fallback на `asphalt`/1; версия снимка меняется только в ADR-022. Запрет многоуровневости в `AGENTS.md` снят только для дорог и мостов.
- **ADR-019 + ADR-012-amendment — рынок и власть (`DECISIONS/ADR-019-market-power.md`):** `MarketPricePolicy` с множителем ×1 по умолчанию (тождество), применяется только в точках цены; порядок для содержания `UpkeepPolicy` → множитель → `EffectiveUpkeepPolicy` (÷8 ровно один раз). `CreditLedger` хранит тело и процент как plain C#: выдача через `Treasury.Earn`, погашение и процент — через `Treasury.Charge`, дефицит легален; `TrySpend` остаётся только для добровольных трат. Районы переопределяют ставку налога и множитель над базовым `ZoneType`. Рейтинг мэра — бесконечный производный счётчик из вех, без потолка и экранов финала (GAME_DESIGN §19).
- **ADR-020 — решения игрока (`DECISIONS/ADR-020-life-decisions.md`):** `DecisionController` в домене (plain C#) применяет выбор на границе тика, без `UnityEvent`, C# `event` и новой шины; лента получает две append-only записи с разными `messageKey`/`args`, чтобы дедуп ADR-013 не склеил их. Отложенные решения не сериализуются. `RouteSystem` остаётся доменным, агенты и их очереди — presentation; лимиты 40/12/30 и 9 индикаторов нужд не меняются. UI — IMGUI в «Ленте»/«Статистике».
- **ADR-021 — визуал (`DECISIONS/ADR-021-visual.md`):** сезоны/погода — presentation-фильтр через расширенный `DayNightSample` и те же 6 ключевых точек `DayNightPolicy`; при выключенном цикле — `DayNightPolicy.Noon()`. Emissive-окна — копии материалов по паттерну `BlackoutViewStyle`; оверлеи остаются на `Unlit/Color`. IMGUI-эффекты — через `HudText.Draw`, `BottomBarStyle`, `TutorialArrowView` с возвратом `GUI.color` в `try/finally`. URP/HDRP/UI Toolkit запрещены; тени/LOD — только после замеров ADR-001.
- **ADR-022 — сейв v7 (`DECISIONS/ADR-022-save-v7.md`):** запись версии 7, чтение 1–7, >7 — ошибка валидации; опциональные поля с fallback на v6 и нормализацией без исключений; экспорт/импорт `SlotTransfer` — тот же JSON и та же валидация; `GameSettings` — профиль игрока, не состояние партии.
