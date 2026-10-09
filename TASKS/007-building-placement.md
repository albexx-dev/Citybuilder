# TASKS/007 — Building Placement

## Цель

Ставить здание на сетку мышью: выбор, предпросмотр, проверки, установка, удаление.

## Контекст

Зависит от 004 (GridSystem), 005 (GridVisualizer ховер), 006 (BuildingDefinition + House.asset). Эталонное качество: граничные случаи обязательны.

## Что нужно сделать

1. `BuildingState` (plain C#, модуль Construction): EntityId Id, BuildingDefinition Definition, GridPosition Origin, List<GridPosition> Cells.
2. `BuildingSystem` (plain C#, модуль Construction): конструктор принимает GridSystem.
   - `CanPlace(BuildingDefinition, GridPosition)` — все клетки внутри и свободны.
   - `Place(...)` → BuildingState или null: занимает клетки через Occupy с новым EntityId.
   - `Demolish(BuildingState)` — освобождает клетки через Release. Возвращает bool.
3. `BuildingPlacementController` (MonoBehaviour, модуль Construction):
   - Inspector: GridVisualizer (обязательно), Definition (House), ghost-материалы не нужны — цвет через два материала Unlit/Transparent (зелёный/красный, создать в коде).
   - Призрак: куб размером footprint, следует за ховером; зелёный если CanPlace, красный если нет; скрыт вне карты.
   - ЛКМ — поставить (Instantiate Prefab из Definition, fallback — белый куб; зарегистрировать BuildingState).
   - Escape — убрать призрак (выход из режима). ПКМ — снести здание под курсором.
   - Список placed для Demolish: искать State по клетке.

## Что не нужно делать

- Не добавлять жителей, jobs, экономику, стоимость (задача 008).
- Не трогать Core/World/Simulation/Presentation/GridSystem/GridVisualizer.
- Не использовать Input System пакет — только Input.mousePosition / GetMouseButtonDown / GetKeyDown.

## Зависимости

TASKS/004, 005, 006 (Done). Блокирует срез (нужна для 008).

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Construction/BuildingState.cs
- Assets/Citybuilder/Scripts/Construction/BuildingSystem.cs
- Assets/Citybuilder/Scripts/Construction/BuildingPlacementController.cs
- TASKS/007-building-placement.md (этот файл)
- TASKS/007-breakdown.md

Изменить:
- TASKS/README.md (статус 007)

## Критерии готовности

- [ ] 3 файла кода, каждый до 150 строк, суммарно до 250. Чистая логика без Unity в State/System.
- [ ] CanPlace: край карты (48,48) для 2x2 = true; (49,49) = false; занятые = false.
- [ ] В Play: призрак зелёный/красный, ЛКМ ставит, поверх ставить нельзя, Escape отменяет, ПКМ сносит.
- [ ] Console без ошибок. Inspector: GridVisualizer обязателен (проверка null с понятной ошибкой).

## План проверки

1. `dotnet run`: CanPlace/Place/Demolish + границы (край карты, перекрытие, повторный снос).
2. Unity: Prototype.unity + контроллер, Play, шаги из критериев.
3. Проверка границ: Place вне карты = null, клетки не заняты.
