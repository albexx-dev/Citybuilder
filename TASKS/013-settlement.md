# TASKS/013 — Settlement Foundation

## Цель

Фундамент заселения: дом знает свою зону, вместимость и рабочие места; чистая логика заселения считает ёмкость и jobs. Без автовырастания, вида и контроллера.

## Контекст

Этап 5 из ARCHITECTURE.md (районы → заселение). Зависит от 006 (BuildingDefinition/System), 011 (ZoneSystem/ZoneType). Автовырастание домов и вид — в 014. Население как симуляция (домохозяйства) — Этап 6, не здесь.

## Что нужно сделать

1. `BuildingDefinition` (изменить, модуль Construction): добавить три поля, старые ассеты не ломать.
   - `AllowedZone : ZoneType` (default Residential) — на какой зоне может стоять тип.
   - `ResidentCapacity : int` (default 4, >= 0) — сколько жителей вмещает одно здание.
   - `JobsProvided : int` (default 0, >= 0) — сколько рабочих мест даёт одно здание.
   - OnValidate: отрицательные capacity/jobs править в 0. Проверок зон в 013 нет — только данные.
2. `SettlementSystem` (plain C#, модуль Population — НОВАЯ папка `Scripts/Population`, первое Population-дело):
   - Конструктор принимает `GridSystem grid, ZoneSystem zones, BuildingSystem buildings` (null → ArgumentNullException). Направление зависимости одно: Population знает World + Construction, обратного нет (циклы запрещены).
   - `CanSettle(BuildingDefinition, GridPosition)` — false если def null; иначе все клетки `CoversCells(origin)`: `zones.GetZone(cell) == def.AllowedZone` (если `AllowedZone == None` — проверку зоны пропустить, здание встаёт где угодно) И `buildings.CanPlace(def, origin)`.
   - `Settle(BuildingDefinition, GridPosition)` → BuildingState или null: если !CanSettle — null; иначе `buildings.Place(...)`, при успехе `SettledCount++`, `TotalCapacity += capacity`, `TotalJobs += jobs`.
   - `Demolish(BuildingState)` → bool: null/пустой — false; иначе `buildings.Demolish(state)`, при успехе вычесть capacity/jobs его Definition (не ниже 0), `SettledCount--` (не ниже 0).
   - Свойства только на чтение: `SettledCount`, `TotalCapacity`, `TotalJobs`.

## Что не нужно делать

- Не делать автовырастание, тики, контроллер, вид (задача 014).
- Не делать домохозяйства, миграцию, потребности, налоги (Этап 6+, не V1).
- Не трогать BuildingPlacementController, ZonePaintingController, Road*, Treasury*, View — только BuildingDefinition.cs + новый файл.
- Не трогать ProjectSettings, не добавлять пакеты. Только старый Input (в 013 Input вообще не нужен — чистой логике он запрещён).

## Зависимости

TASKS/006, 011 (Done). Блокирует 014.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Population/SettlementSystem.cs
- TASKS/013-settlement.md (этот файл)
- TASKS/013-breakdown.md

Изменить:
- Assets/Citybuilder/Scripts/Construction/BuildingDefinition.cs (3 поля + clamp в OnValidate)
- TASKS/README.md (статус 013)

## Критерии готовности

- [ ] 1 новый файл до 150 строк, чистая логика без Unity; правка Definition — только поля + clamp.
- [ ] CanSettle: дом 2x2 на Residential — true; та же точка на пустой зоне — false; поверх занятого — false; AllowedZone=None — true везде где свободно.
- [ ] Settle ×2 дома (capacity 4, jobs 2): SettledCount=2, TotalCapacity=8, TotalJobs=4; Demolish одного: 1/4/2.
- [ ] Console без ошибок. Code rules: namespace Citybuilder, 1 файл = 1 класс, без LINQ-цепочек >2, без синглтонов.

## Железные уроки (не нарушать)

- Unlit/Transparent не красит в этом проекте — только Unlit/Color (в 013 материалов нет, но правило действует дальше).
- Состояние в MonoBehaviour только ленивое (в 013 MonoBehaviour нет — EnsureReady-паттерн ждёт 014).
- Скопировал файлы в Unity — выполни Assets/Refresh и дождись компиляции.
- Галочки режимов и сцену трогай только при выключенном Play. За раз включён один контроллер.

## План проверки

1. `dotnet run` или Unity EditMode-шаги: границы (вне карты, чуждая зона, занято, Demolish null/двойной, отрицательные capacity в Definition правятся в 0).
2. Unity: скопировать файлы в `C:\Users\admin\UnityProjects\CityBuilder`, Assets/Refresh, дождаться компиляции, Console без ошибок, Play без ошибок (сцена не меняется).
