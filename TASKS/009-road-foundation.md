# TASKS/009 — Road Foundation

## Цель

Фундамент дорог: тип, состояние, система single-cell стройки на сетке. Без контроллера и вида.

## Контекст

Этап 4 из ARCHITECTURE.md. Зависит от 002, 004, 006 (паттерн BuildingDefinition/System). Drag-placement — в 010.

## Что нужно сделать

1. `RoadDefinition : ScriptableObject` (модуль Construction): Id, DisplayName, CostPerCell (int >= 0).
   - CreateAssetMenu "Citybuilder/Road Definition". OnValidate: cost >= 0, id не пустой.
2. `RoadState` (plain C#): EntityId Id, GridPosition Cell (дорога = 1 клетка).
3. `RoadSystem` (plain C#): конструктор принимает GridSystem.
   - `CanPlace(GridPosition)` — внутри и свободно.
   - `Place(GridPosition)` → RoadState или null.
   - `Demolish(RoadState)` → bool.
   - `CellCost(RoadDefinition)` → costPerCell (цена позже умножится на длину в 010).

## Что не нужно делать

- Не делать drag/линии, контроллер, вид (задача 010).
- Не делать соединения сегментов и доступность (позже отдельной задачей).
- Не трогать Core/World/Simulation/Presentation/Economy/UI/Building*.

## Зависимости

TASKS/002, 004 (Done). Блокирует 010.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Construction/RoadDefinition.cs
- Assets/Citybuilder/Scripts/Construction/RoadState.cs
- Assets/Citybuilder/Scripts/Construction/RoadSystem.cs
- TASKS/009-road-foundation.md (этот файл)
- TASKS/009-breakdown.md

Изменить:
- TASKS/README.md (очередь 009-010)

## Критерии готовности

- [ ] 3 файла, каждый до 150 строк. Чистая логика без Unity в State/System.
- [ ] Place/Place/Demolish цикл: клетка занимается и освобождается.
- [ ] CostPerCell отрицательный правится в 0.

## План проверки

1. `dotnet run`: границы (край карты, занято, повторный снос).
2. Unity: компиляция без ошибок.
