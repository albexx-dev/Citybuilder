# TASKS/010 — Road Placement

## Цель

Строить дороги мышью: drag прямых отрезков, предпросмотр, оплата за клетки, вид дороги.

## Контекст

Зависит от 009 (RoadSystem/Definition). Вид — плоские серые клетки одним мешем.

## Что нужно сделать

1. `RoadView` (MonoBehaviour, модуль Construction): серый Unlit-материал (создать в коде).
   - `Rebuild(List<GridPosition> cells, float cellSize)` — перестраивает единый меш клеток.
2. `RoadPlacementController` (MonoBehaviour, модуль Construction):
   - Inspector: GridVisualizer (обязателен), Definition (RoadDefinition), TreasuryView (опционально), View (RoadView, создаёт сам если пусто — на том же объекте).
   - ЛКМ press на клетке — начало отрезка; drag — предпросмотр прямой линии (ось с большим delta от старта); release — стройка.
   - Предпросмотр: единый меш, зелёный если все клетки свободны/внутри и хватает денег (count × CellCost), красный иначе. Перестраивать только при смене клетки ховера.
   - Release: проверка CanPlace всех → TrySpend(total) → Place каждой. Хоть одна плохая — ничего не строить, денег не снимать.
   - Escape — отмена drag. ПКМ по клетке — снести дорогу (поиск State по клетке).
   - Список states для сноса. Поле placementActive (публичное, по умолчанию true) — режимы стройки переключатся позже через UI.

## Что не нужно делать

- Не делать диагонали, кривые, соединения сегментов, доступность зданий.
- Не трогать Building*, Core/World/Simulation/Presentation/Economy/UI.
- Только старый Input.

## Зависимости

TASKS/009 (Done). Закрывает Этап 4 (без доступности — она отдельной задачей позже).

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Construction/RoadView.cs
- Assets/Citybuilder/Scripts/Construction/RoadPlacementController.cs
- TASKS/010-road-placement.md (этот файл)
- TASKS/010-breakdown.md

Изменить:
- TASKS/README.md (статус 010)

## Критерии готовности

- [ ] 2 файла, каждый до 150 строк.
- [ ] Drag 5 клеток: -50 денег, 5 серых клеток, клетки заняты.
- [ ] Drag через занятую клетку: красное, ничего не построено, деньги целы.
- [ ] Console без ошибок. Конфликт с BuildingPlacementController решается выключением одного (поле placementActive / enabled) — зафиксировать в отчёте.

## План проверки

1. `dotnet`-прогон невозможен для контроллера (Unity) — логика линий: straight-line helper тестируется? Линию считает контроллер; вынести `GetLineCells(from, to)` в чистый static RoadLine в RoadSystem.cs? Нет — не трогать 009. Допустимо: проверка только в Unity.
2. Unity: контроллер на Grid, Play: drag ставит, деньги падают, занятые блокируют.
