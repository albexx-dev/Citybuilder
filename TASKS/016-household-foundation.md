# TASKS/016 — Household Foundation

## Цель

Фундамент населения: домохозяйства заселяются в дома в пределах вместимости, учёт жителей и выселение. Без вида, контроллера и миграции. Первый шаг Этапа 6.

## Контекст

Этап 6 из ARCHITECTURE.md (население через домохозяйства), GAME_DESIGN §9 (начать с домохозяйств, не с симуляции каждого). Зависит от 013 (SettlementSystem, ResidentCapacity). Вид/панель населения и автовъезд — в 017. Налоги, работа, потребности — позже.

## Что нужно сделать

1. `HouseholdState` (plain C#, модуль Population): снимок домохозяйства.
   - `EntityId Id`, `BuildingState Home`, `int Size` (жителей в домохозяйстве, > 0).
   - Конструктор валидирует: home null или size <= 0 → ArgumentException (какой именно — на усмотрение инженера, зафиксировать в отчёте).
2. `PopulationSystem` (plain C#, модуль Population): учёт заселения домов.
   - Конструктор без параметров (хранит только свои записи; вместимость читает из `home.Definition.ResidentCapacity`, отрицательную правит в 0).
   - `MoveIn(BuildingState home, int size)` → HouseholdState или null: home null / size <= 0 / `ResidentsIn(home) + size > CapacityOf(home)` — null. Иначе создаёт, `HouseholdCount++`, `TotalResidents += size`.
   - `MoveOut(HouseholdState)` → bool: null / неизвестно / уже выехало — false. Иначе убирает, счётчики вниз (флор 0).
   - `EvictHome(BuildingState home)` → int: выселяет ВСЕ домохозяйства дома (нужно при сносе дома), возвращает число выселенных. home null — 0.
   - `ResidentsIn(BuildingState home)` — жителей в конкретном доме (null — 0). `CapacityOf(BuildingState home)` — вместимость (null/Definition null — 0).
   - Свойства только на чтение: `HouseholdCount`, `TotalResidents`.

## Что не нужно делать

- Не делать вид, панель UI, контроллер, автовъезд по таймеру (задача 017).
- Не делать миграцию, рождаемость, работу, налоги, потребности (позже).
- Не делать связь сноса дома с выселением внутри SettlementSystem (она в руках вызывающего через EvictHome; зафиксировать в отчёте).
- Не трогать Construction/World/Economy/UI/Presentation/Simulation. Только старый Input (в 016 не нужен вовсе).

## Зависимости

TASKS/013 (Done). Блокирует 017.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Population/HouseholdState.cs
- Assets/Citybuilder/Scripts/Population/PopulationSystem.cs
- TASKS/016-household-foundation.md (этот файл)
- TASKS/016-breakdown.md

Изменить:
- TASKS/README.md (статус 016)

## Критерии готовности

- [ ] 2 новых файла, каждый до 150 строк, чистый C# без Unity.
- [ ] Дом capacity 4: MoveIn(2) + MoveIn(2) → HouseholdCount=2, TotalResidents=4; MoveIn(1) — null (переполнение).
- [ ] MoveOut одного → 1/2; EvictHome → 0/0, вернул 2. Двойной MoveOut — false, счётчики не меняются.
- [ ] Console без ошибок. Code rules: namespace Citybuilder, 1 файл = 1 класс, методы до 30 строк, без LINQ-цепочек >2, без синглтонов.

## Железные уроки (не нарушать)

- Только Unlit/Color (в 016 материалов нет, правило на будущее).
- Состояние в MonoBehaviour только ленивое (в 016 MonoBehaviour нет).
- Скопировал файлы в Unity — выполни Assets/Refresh (scope all) и дождись компиляции.
- Галочки режимов и сцену трогай только при выключенном Play. За раз включён один контроллер.

## План проверки

1. `dotnet run` или шаги в Unity-редакторе через мост: границы (null, size<=0, переполнение, двойной MoveOut, EvictHome пустого/null, отрицательная capacity).
2. Unity: скопировать файлы, Assets/Refresh all, дождаться компиляции, Console без ошибок, сцена не меняется.
