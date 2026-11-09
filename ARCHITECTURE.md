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
- автосохранение каждые 24 тика (`AutosaveController`).

## 6. Commands, queries and events

UI не меняет мир напрямую: `Команда → проверка → изменение мира → событие → обновление View/UI`.
Событийная лента для игрока (`CityEventLog`, кольцевой буфер на 50 записей с дедупликацией в окне 10 тиков, ADR-007) предназначена исключительно для отображения причин в `EventLogView`, а не для межсистемной связности.

## 7. Coordinate system

Locked V1: квадратная сетка, клетка 1x1 м, начало (0,0), карта 50x50, без высоты. Менять только через ADR.
Базовый тип `GridPosition (X, Y)`. Операции сетки: IsInside, WorldToGrid, GridToWorld, IsOccupied, Occupy, Release.

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
- Persistence: сейвы v2 (ADR-008, EPIC-11) — 3 ручных слота (`slot_0..2.json`), отдельный слот автосейва (`autosave.json`), индекс метаданных (`slots.json` через `SaveSlotIndex`), `AutosaveController` (каждые 24 тика), `SaveSlotStorage`, `SaveController` (`IsDirty`), `LoadController`.

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

Реализовано (сейвы v2, EPIC-11): снапшот v4 (`CitySnapshot`) сохраняется в 3 именованных слота (`slot_0.json`..`slot_2.json`) и автосейв (`autosave.json`) с метаданными в `slots.json` (`SaveSlotIndex`: имя слота, игровое время, население, баланс, штамп времени). Отрицательный баланс казны при дефиците корректно сохраняется и восстанавливается (`Treasury.Restore`).

## 13. Testing strategy

- EditMode-тесты на всю чистую логику и адаптеры: 63 тестовых файла (`Assets/Citybuilder/Tests/`), статическая валидация — `Tools/ValidateRepo.py` в CI.

## 14. Performance strategy

Ориентир V1 / v1.1: сетка 50x50 = 2500 клеток только логика, без 2500 GameObject. Визуал сетки и оверлея радиуса — процедурные меши (`GridMeshBuilder`). Перекраска пробок выполняется строго 1 раз за тик часов, а кеш строк `InfoMenuView` и `EventLogView` обновляется не чаще 2 раз в секунду.

## 15. Current implementation scope (v1.1.0)

Все эпики фаз 1–6 версии **v1.1.0** (`EPIC-01`..`EPIC-06`, `EPIC-08`..`EPIC-21`) полностью реализованы, покрыты тестами и смержены в `main`. Доктрина бесконечной партии (`GAME_DESIGN.md §19`) соблюдена: `GoalSystem.Won/Lost` полностью удалены из кодовой базы (EPIC-08).

## 16. Future decisions (v1.2+)

Пожары как активное событие (`EPIC-07`, требует ADR-009), визуальный трафик машин на дорогах, расширение 3D-моделей на остальные типы зданий — в очереди версии v1.2.
