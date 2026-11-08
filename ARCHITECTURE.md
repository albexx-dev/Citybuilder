# Citybuilder Architecture

## 1. Project goal

Подробная 3D city-building игра в Unity 6. Акцент — глубокая симуляция связанных систем: дороги влияют на доступность, транспорт на время поездок, работа на доход, доход на потребление, коммуналка на качество жизни.

Первый результат: вертикальный срез (сетка → выбор дома → предпросмотр → установка → списание денег).

## 2. Architectural principles

- Маленькие проверяемые задачи, 1 задача = 1-3 файла, до 200 строк нового кода.
- Модульный монолит, чёткие границы модулей.
- Plain C# domain logic внизу, Unity-адаптеры сверху, UI только команды и отображение.
- State отдельно от View, Definition отдельно от State.
- Никаких огромных Manager-классов и глобальных синглтонов.

## 3. Modular monolith approach

Одна Unity-игра, разделённая на логические модули. Не делаем: микросервисы, ECS/DOTS без нужды, десятки абстракций, глобальную шину событий на всё, плагины.

Поток: `Plain C# domain logic → Unity adapters / MonoBehaviours → Visual presentation / UI`.

## 4. Separation of definition, state and view

- `Definition` (ScriptableObject): тип объекта. Пример `BuildingDefinition`: id, displayName, size, constructionCost, prefab.
- `State` (plain C#): экземпляр в городе. Пример `BuildingState`: EntityId, ссылка на definition, позиция, поворот, занятые клетки. Никогда не хранить runtime в ScriptableObject.
- `View` (MonoBehaviour): отображение — GameObject, материалы, подсветка. Не решает правила игры.

## 5. Simulation time and ticks

Frame Update каждый кадр: камера, ввод, анимации, предпросмотр, UI.
Simulation Tick периодически: экономика, производство, жители, сети, статистика.

Locked V1: Tick 1 сек = 1 игровой час, день = 24 тика, FastForward x4. Пауза останавливает тики. К тику подключены: часы, казна (`EconomyTicker`), рост поселения (`SettlementGrowthController` → `GrowthPace`: берётся худший из множителей — пробки `CongestionController.GrowthTickMultiplier` и дефицит снабжения `SupplyGrowthController.GrowthTickMultiplier`, ×1/×2/×3).
Режимы: Paused / Playing / FastForward. События: `SimulationClock.TickStarted` / `TickCompleted` и `Treasury.BalanceChanged`; отдельных классов Command/Event и глобальной шины нет.

## 6. Commands, queries and events

UI не меняет мир напрямую: `Команда → проверка → изменение мира → событие → обновление View/UI`.

Команды: PlaceBuildingCommand, DemolishBuildingCommand, BuildRoadCommand и др.
События: BuildingPlacedEvent, MoneyChangedEvent и др.
Связи без циклов: через запросы (GetAvailableJobsQuery), интерфейсы чтения, снимки данных.

## 7. Coordinate system

Locked V1: квадратная сетка, клетка 1x1 м, начало (0,0), карта 50x50, без высоты. Менять только через ADR.

Базовый тип `GridPosition (X, Y)`. Операции сетки: IsInside, WorldToGrid, GridToWorld, IsOccupied, Occupy, Release. Здания занимают прямоугольник, поворот меняет ширину/длину. Не нужны: гексы, свободное строительство, мосты, подземные уровни.

## 8. Core systems

- Core: EntityId, GridPosition, GridSize, SimulationTime, NeedProviders, SupplyBalance, UnlockPolicy.
- World: Map, Grid, ZonePainting, снимок клеток.
- Construction: BuildingDefinition/State, PlacementController, RoadPlacementController, Demolition, View.
- Economy: Treasury, TaxSystem, TaxPolicy, EconomyTicker.
- Population: SettlementSystem, SettlementGrowthController, GrowthPace, PopulationController, HouseholdState, GoalStages.
- Infrastructure: SupplyBalance (энергия / вода / мусор), SupplyGrowthPolicy + SupplyGrowthController (дефицит замедляет рост), ServiceCoverage, пожарные и полиция, NeedProviders, NeedSystem.
- Transport: ConnectivitySystem, RouteSystem (A* по дорогам), CongestionSystem + CongestionController.
- Simulation: Runner, порядок систем, статистика.
- Presentation / UI: только отображение и команды, строки HUD через `HudText`; подсказки — чистая `StartHintPolicy` + `StartHintView`, статус роста — `GrowthStatusPolicy` + `GrowthStatusView`.
- Persistence: CitySnapshot (v1–v4), SaveSystem, SnapshotHouseholds, HouseholdBinder, SaveSlotStorage, SaveController, LoadController, LoadLog.

## 9. Dependencies

```
Core
 ├── Simulation, World, Economy, Construction, Resources,
 │   Infrastructure, Transport, Population, Persistence
```

World не знает про деньги и жителей. Construction знает World + стоимость через интерфейс экономики. Economy не создаёт жителей и здания напрямую. UI только отображает и шлёт команды.

## 10. Dependency restrictions

Запрещены циклы вида BuildingSystem → EconomySystem → PopulationSystem → BuildingSystem. Вместо прямых ссылок: команды, запросы, события, порядок симуляции.

## 11. Unity integration

Тонкие MonoBehaviour-адаптеры: ссылка на логику + вызов 1-2 методов. Сцена: `Prototype.unity` (единственная игровая). Папки растут только с кодом: Scripts/Core, World, Construction, Economy, Population, Infrastructure, Transport, Persistence, Simulation, UI, Presentation + Prefabs, Data/Buildings, Scenes.

## 12. Data persistence

Реализовано: снапшот v4 (`CitySnapshot`) — версия, тики, баланс, налоговая ставка, ступень цели, зоны, выросшие здания, домохозяйства (индекс + координаты дома), дороги, ручные здания. Версии 1–4 читаются совместимо: `SaveSlotStorage.Validate` принимает `1..CurrentVersion`, отсутствующие в старом слоте поля трактуются как значение по умолчанию (v1 → налог «обычный», ступень 0). Домохозяйства новый слот привязывает к дому по координатам (`HasHome`), старый — по индексу: `HouseholdBinder`.

Отложено: облачные сейвы, автосейв по таймеру, несколько слотов на диске.

## 13. Testing strategy

- EditMode-тесты на всю чистую логику: 288+ тестов в 39+ файлах (`Assets/Citybuilder/Tests/`), прогон в редакторе — оркестратор; статическая валидация — `Tools/ValidateRepo.py` в CI.
- Перед коммитом инженер гоняет чистый C# через `dotnet`/`csc` во временной папке, без Unity, и прикладывает результат.
- Unity-специфика (компиляция сборок проекта, EditMode в редакторе, Play-E2E) — только оркестратор: редактор один на машину.
- PlayMode-автотестов нет: игровой сценарий проверяется руками по чек-листу задачи.

## 14. Performance strategy

Ориентир V1: сетка 50x50 = 2500 клеток только логика, без 2500 GameObject. Визуал сетки — 1 draw call. Тик не дольше 5 мс. Запрещено до Этапа 8: Jobs/Burst, LOD, оптимизации ради оптимизаций. Замеры только через Profiler.

## 15. Current implementation scope

Задачи 001–104 смержены и запушены; на этом состоянии начинается **v1.1** (`DECISIONS/ADR-005-v1.1-scope.md`). Играбельно в редакторе и в билде windows64 102 (90 МиБ): сетка 50×50, зоны, рост по тикам, налоги с видимым сбором, энергия / вода / мусор со станциями-источниками, дефицит снабжения замедляет рост, пожарные и полиция, связность дорог и пробки, цель со ступенями, стартовая подсказка и статус роста, сейв и лоад v4 с налогом, ступенью цели и привязкой жителей к домам по координатам.

Дальше: CI и автоматическая валидация, читаемость HUD на низких окнах, сплиты крупных файлов, службы по готовому образцу. Экраны победы/поражения в v1.1 не возвращаются; `GoalSystem.Won/Lost` сохранён как протестированная доменная логика для следующей версии.

## 16. Future decisions

Транспорт с машинами, многоуровневость, облачные сейвы, Jobs/Burst — только через ADR в `DECISIONS/`, после того как текущий срез проверен на живом билде. Спорные изменения архитектуры без ADR не начинаются.

## 17. v1.1 roadmap (эпики)

Крупные изменения v1.1 планируются эпиками (GitHub Issues) и фиксируются ADR **до** кода: обобщённая система служб (ADR-006), событийная лента города (ADR-007), сейвы v2 — три слота + автосейв (ADR-008). По правилу §15 этот раздел не описывает планируемые системы как факт — после реализации каждого эпика факты переезжают в §8 и связанные разделы.

Доктрина дизайна: партия не имеет конца (GAME_DESIGN §19) — исходов и финальных экранов нет, цель остаётся вехами роста.
