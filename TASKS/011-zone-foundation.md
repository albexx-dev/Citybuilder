# TASKS/011 — Zone Foundation

## Цель

Фундамент зонирования: типы зон и назначение зоны клеткам. Без рисования и заселения.

## Контекст

Этап 5 из ARCHITECTURE.md. Продолжение дорог с конца — в бэклоге (BACKLOG-road-continue), не в этой задаче.

## Что нужно сделать

1. `ZoneType` (enum, модуль Population... нет — модуль World? Зоны — свойство карты. Решение: модуль World).
   - None, Residential, Commercial, Industrial.
2. `ZoneSystem` (plain C#, модуль World): конструктор принимает GridSystem.
   - `SetZone(GridPosition, ZoneType)` — false если вне карты. None снимает зону.
   - `GetZone(GridPosition)` — вне карты = None.
   - `CountZone(ZoneType)` — число клеток зоны (для статистики).
   - Дороги и здания зону не стирают (V1, зафиксировать).

## Что не нужно делать

- Не делать рисование зон, вид, контроллер (задача 012).
- Не делать дома, jobs, заселение (задачи 013+).
- Не трогать другие модули.

## Зависимости

TASKS/004 (Done). Блокирует 012.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/World/ZoneType.cs
- Assets/Citybuilder/Scripts/World/ZoneSystem.cs
- TASKS/011-zone-foundation.md (этот файл)
- TASKS/011-breakdown.md

Изменить:
- TASKS/README.md (очередь + BACKLOG-road-continue)

## Критерии готовности

- [ ] 2 файла, чистый C#, до 150 строк каждый.
- [ ] SetZone вне карты = false, GetZone вне карты = None.
- [ ] CountZone считает верно после серии назначений и снятий.

## План проверки

1. `dotnet run`: границы и подсчёты.
2. Unity: компиляция без ошибок.
