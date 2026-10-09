# TASKS/004 — Grid System

## Цель

Создать логическую квадратную сетку: границы, преобразование координат, занятость клеток.

## Контекст

Зависит от 002 (GridPosition, GridSize, EntityId). Нужна для 005 (визуализация) и 007 (строительство).

## Что нужно сделать

`GridSystem` (plain C#, без UnityEngine):
- Конструктор: GridSize + cellSize (> 0). V1: 50x50, клетка 1 м.
- `IsInside(GridPosition)` — границы через GridSize.Contains.
- `WorldToGrid(float worldX, float worldZ)` — мировые метры в клетку (floor).
- `GridToWorld(GridPosition, out float x, out float z)` — центр клетки в метрах.
- `IsOccupied(GridPosition)` — занятость. Вне карты = занято (безопасный запрет стройки).
- `Occupy(GridPosition, EntityId)` — false если вне карты или уже занято.
- `Release(GridPosition)` — освободить.
- `OccupiedCount` — число занятых клеток.

## Что не нужно делать

- Не использовать Vector3/UnityEngine (мировые координаты — float).
- Не делать визуализацию, строительство, дороги.
- Не использовать 2D-массивы GameObject — только логика.

## Зависимости

TASKS/002 (Done). Блокирует 005, 007.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/World/GridSystem.cs
- TASKS/004-grid-system.md (этот файл)
- TASKS/004-breakdown.md

Изменить:
- TASKS/README.md (статус 004)

## Критерии готовности

- [ ] Компилируется без Unity, 1 файл до 150 строк.
- [ ] WorldToGrid(2.7, 3.2) = (2, 3) при клетке 1 м.
- [ ] Занятая клетка повторно не занимается, Release освобождает.
- [ ] Стройка вне карты запрещена через IsOccupied = true.

## План проверки

1. `dotnet run`: преобразования, Occupy/Release, границы.
2. Проверка границ: Occupy вне карты = false; отрицательный cellSize отклоняется.
