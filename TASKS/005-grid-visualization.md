# TASKS/005 — Grid Visualization

## Цель

Показать логическую сетку в Unity: линии клеток, подсветка клетки под мышью, занятые клетки.

## Контекст

Зависит от 004 (GridSystem). Первая задача со сценой. Проект на Built-in RP (URP-пакетов нет).

## Что нужно сделать

`GridVisualizer` (MonoBehaviour, тонкий адаптер, модуль Presentation):
- Inspector: Width (50), Height (50), CellSize (1), цвета линий/ховера/занятых.
- Строит GridSystem, рисует линии одним LineRenderer.
- Ховер: жёлтый полупрозрачный квадрат следует за мышью (луч в плоскость y=0).
- Занятые клетки: красный полупрозрачный меш, перестраивается через RefreshOccupied().
- Публично: Grid, HoveredCell, TryGetHoveredCell — задел для задачи 007.
- Материалы Unlit/Transparent создаются в коде, сцена собирается за 3 шага.

## Что не нужно делать

- Не делать предпросмотр строительства (задача 007).
- Не трогать Core/World/Simulation.
- Не создавать префабы впрок, не менять ProjectSettings.

## Зависимости

TASKS/004 (Done). Блокирует 007.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Presentation/GridVisualizer.cs
- TASKS/005-grid-visualization.md (этот файл)
- TASKS/005-breakdown.md

Изменить:
- TASKS/README.md (статус 005)

## Критерии готовности

- [ ] Сетка видна в Play без ошибок в Console.
- [ ] Квадрат ховера следует за мышью, за границами прячется.
- [ ] После Occupy + RefreshOccupied клетка краснеет.
- [ ] Inspector-ссылки: нет (всё создаётся в коде).

## План проверки

1. Скопировать скрипты в Unity-проект, Console без ошибок.
2. Сцена: пустой GameObject + GridVisualizer, камера сверху. Play — видна сетка.
3. Проверка границ: мышь за краем карты — ховер скрыт.
