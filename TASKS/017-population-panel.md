# TASKS/017 — Population Panel

## Цель

Население видно и живёт: автовъезд домохозяйств в выросшие дома по таймеру, OnGUI-панель (жители/вместимость/домохозяйства), выселение при сносе дома. Закрывает базу Этапа 6.

## Контекст

Зависит от 013 (SettlementSystem/Capacity), 014 (рост + вид), 016 (PopulationSystem). Миграция, работа, налоги, потребности — позже.

## Что нужно сделать

1. `SettlementGrowthController` (изменить, модуль Population — ТОЛЬКО добавить):
   - Публичный геттер `Homes`: копия `states` (`new List<BuildingState>(states)`, не сам список).
   - Опциональное поле `PopulationSource` (PopulationController, может быть пустым): в `TryDemolish` после `settlement.Demolish` вызвать `PopulationSource.Population?.EvictHome(state)` — иначе счётчики жителей утекут при сносе. Null-safe, старое поведение без источника не меняется.
2. `PopulationView` (MonoBehaviour, модуль UI — рядом с TreasuryView):
   - Inspector: Controller (PopulationController, опционален — иначе `GetComponent`).
   - OnGUI-лейбл: `Pop: {TotalResidents} / {GrownCapacity} — {HouseholdCount} households`. GrownCapacity берёт из контроллера (сумма CapacityOf по Homes, считает контроллер).
3. `PopulationController` (MonoBehaviour, модуль Population):
   - Inspector: Growth (SettlementGrowthController, обязателен — источник домов), View (PopulationView, создаёт сам если пусто), HouseholdSize (default 2, минимум 1), MoveInIntervalSec (default 7, минимум 1), placementActive (default true).
   - Ленивый EnsureReady: свой PopulationSystem (пересоздаётся после reload, заселённое теряется — штатно V1).
   - Публичный геттер `Population` (для EvictHome из роста) и `GrownCapacity` (сумма CapacityOf по Growth.Homes).
   - Update: если placementActive и прошёл интервал — первый дом из Growth.Homes где `Population.ResidentsIn(home) + HouseholdSize <= Population.CapacityOf(home)` → `MoveIn(home, HouseholdSize)`.
   - Контроллер в сцене по умолчанию ВЫКЛЮЧЕН.

## Что не нужно делать

- Не делать миграцию, рождаемость/смертность, работу, налоги, потребности, счастье.
- Не делать въезд в дома ручной стройки (только выросшие; ручные — позже).
- Не трогать Building*/Road*/Zone*/Treasury*/Simulation. Только старый Input (в 017 не нужен вовсе — таймер).

## Зависимости

TASKS/013, 014, 016 (Done). База Этапа 6 готова после неё.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Population/PopulationController.cs
- Assets/Citybuilder/Scripts/UI/PopulationView.cs
- TASKS/017-population-panel.md (этот файл)
- TASKS/017-breakdown.md

Изменить:
- Assets/Citybuilder/Scripts/Population/SettlementGrowthController.cs (только геттер Homes + EvictHome в TryDemolish)
- TASKS/README.md (статус 017)

## Критерии готовности

- [ ] 2 новых файла до 150 строк; правка роста — только геттер + выселение.
- [ ] Выросшие дома постепенно заселяются (Pop растёт до суммарной вместимости, дальше стоп).
- [ ] Снос заселённого дома: жители вычтены (EvictHome), панель обновилась.
- [ ] Console без ошибок. [Header]/Tooltip/null-проверки, методы до 30 строк, без LINQ >2.

## Железные уроки (не нарушать)

- Только Unlit/Color. Unlit/Transparent запрещён.
- Состояние в MonoBehaviour только ленивое.
- Скопировал файлы в Unity — Assets/Refresh (scope all), дождись компиляции.
- Галочки и сцену — только при выключенном Play. Один контроллер за раз.

## План проверки

1. Unity (Play ВЫКЛ): файлы скопированы, Refresh all, компиляция, Console чистый.
2. E2E через мост в Play: вырастить дома прямыми вызовами, въехать тиками контроллера (прямые вызовы приватного шага через рефлексию), снести заселённый — счётчики и панельные данные сходятся. Ручной визуальный прогон — за разработчиком.
