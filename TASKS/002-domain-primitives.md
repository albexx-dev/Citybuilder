# TASKS/002 — Domain Primitives

## Цель

Создать базовые типы, не завязанные на сцену. Только данные, без логики сетки и строительства.

## Контекст

Этап 1 из ARCHITECTURE.md. Зависит от 001 (правила зафиксированы). Нужны всем следующим задачам: сетка, часы, здания.

## Что нужно сделать

1. `GridPosition` — целочисленная позиция клетки (X, Y), struct, сравнение и сложение.
2. `GridSize` — размер карты (Width, Height), метод `Contains(GridPosition)`.
3. `EntityId` — уникальный ID экземпляра, struct, фабрика `New()`.
4. `SimulationTime` — игровое время в тиках/часах, struct, метод `Advance()`.

## Что не нужно делать

- Не реализовывать GridSystem, строительство, экономику, жителей.
- Не использовать UnityEngine, MonoBehaviour, ScriptableObject.
- Не добавлять generics, LINQ, async, reflection.

## Зависимости

TASKS/001 (Done). Блокирует 003, 004.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Core/GridPosition.cs
- Assets/Citybuilder/Scripts/Core/GridSize.cs
- Assets/Citybuilder/Scripts/Core/EntityId.cs
- Assets/Citybuilder/Scripts/Core/SimulationTime.cs
- TASKS/002-domain-primitives.md (этот файл)
- TASKS/002-breakdown.md

Изменить:
- TASKS/README.md (статус 002)

## Критерии готовности

- [ ] 4 файла компилируются без Unity (чистый C#).
- [ ] Каждый файл до 150 строк, 1 класс = 1 файл, namespace Citybuilder.
- [ ] `GridSize.Contains` возвращает false за границами.
- [ ] Два `EntityId.New()` никогда не равны.
- [ ] Проверка границ описана ниже.

## План проверки

1. `dotnet build` временного проекта со всеми 4 файлами — 0 ошибок.
2. Прогон: Contains((50,0)) на карте 50x50 = false; (0,0) = true.
3. Проверка границ: попытка использовать float-координаты или отрицательный размер — конструктор отклоняет.
