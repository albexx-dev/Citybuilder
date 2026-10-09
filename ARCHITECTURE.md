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

Locked V1: Tick 1 сек = 1 игровой час, день = 24 тика, FastForward x4. Пауза останавливает тики. В V1 к тику подключены только часы и казна.
Режимы: Paused / Playing / FastForward. События: SimulationTickStarted / SimulationTickCompleted.

## 6. Commands, queries and events

UI не меняет мир напрямую: `Команда → проверка → изменение мира → событие → обновление View/UI`.

Команды: PlaceBuildingCommand, DemolishBuildingCommand, BuildRoadCommand и др.
События: BuildingPlacedEvent, MoneyChangedEvent и др.
Связи без циклов: через запросы (GetAvailableJobsQuery), интерфейсы чтения, снимки данных.

## 7. Coordinate system

Locked V1: квадратная сетка, клетка 1x1 м, начало (0,0), карта 50x50, без высоты. Менять только через ADR.

Базовый тип `GridPosition (X, Y)`. Операции сетки: IsInside, WorldToGrid, GridToWorld, IsOccupied, Occupy, Release. Здания занимают прямоугольник, поворот меняет ширину/длину. Не нужны: гексы, свободное строительство, мосты, подземные уровни.

## 8. Core systems

- Core: GameBootstrap, GameState, EntityId, SimulationClock, Command, Event.
- World: Map, Grid, Terrain, Districts, WorldBounds.
- Construction: BuildingDefinition/State/System, PlacementController, View, DemolitionSystem.
- Economy: Treasury, MoneyWallet, Transaction, Budget.
- Resources / Infrastructure / Transport / Population: только после среза, по порядку этапов.
- Simulation: Runner, TickScheduler, порядок систем, статистика.
- Presentation: только отображение. UI: BuildMenu, MoneyView, панели. Persistence: позже, но State уже сейчас отдельно от GameObject.

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

Тонкие MonoBehaviour-адаптеры: ссылка на логику + вызов 1-2 методов. Сцены: Bootstrap.unity (запуск), Prototype.unity (срез). Папки растут только с кодом: Scripts/Core, World, Construction, Economy, UI + Prefabs, Data/Buildings, Scenes.

## 12. Data persistence

Полное сохранение откладывается. Требование V1: весь State — plain C# классы с EntityId, отдельно от GameObject/View, чтобы позже добавить сериализацию без рефакторинга.

## 13. Testing strategy

Минимум V1: 1 проверка логики в Unity (шаги) + 1 проверка границ (что запрещено) на задачу. Юнит-тесты только для чистой логики Grid и Treasury без Unity-зависимостей.

## 14. Performance strategy

Ориентир V1: сетка 50x50 = 2500 клеток только логика, без 2500 GameObject. Визуал сетки — 1 draw call. Тик не дольше 5 мс. Запрещено до Этапа 8: Jobs/Burst, LOD, оптимизации ради оптимизаций. Замеры только через Profiler.

## 15. Current implementation scope

Этап 0 (этот документ + TASKS/001). Далее: primitives → clock → grid → visualization → building definition → placement → treasury. V1: 1 дом 2x2, кубы-примитивы, начальный бюджет, UI баланса, отмена и удаление.

## 16. Future decisions

Дороги (Этап 4), районы (Этап 5), население через домохозяйства (Этап 6), коммуналка по одной службе (Этап 7), транспорт и пробки (Этап 8). Спорные изменения — только через DECISIONS/ADR после рабочего среза.
