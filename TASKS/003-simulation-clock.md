# TASKS/003 — Simulation Clock

## Цель

Создать управляемое игровое время: режимы скорости и периодические тики. Без привязки к сцене.

## Контекст

Зависит от 002 (использует SimulationTime). К тику в V1 подключены только часы и казна. Экономика и жители — позже.

## Что нужно сделать

1. `SimulationSpeed` — enum: Paused, Playing, FastForward.
2. `SimulationClock` — plain C# класс: накапливает реальные секунды, выдаёт тики.
   - Playing: 1 тик в секунду. FastForward: x4 (тик каждые 0.25 сек). Paused: тиков нет.
   - События: TickStarted / TickCompleted с текущим SimulationTime.
   - Метод `Update(float deltaTime)`, свойство `CurrentTime`, `Speed`.

## Что не нужно делать

- Не создавать MonoBehaviour-адаптер (будет в задаче сцены).
- Не подключать экономику, жителей, UI.
- Не использовать async, таймеры Unity, корутины.

## Зависимости

TASKS/002 (Done). Блокирует 008 (казна подпишется на тики).

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Simulation/SimulationSpeed.cs
- Assets/Citybuilder/Scripts/Simulation/SimulationClock.cs
- TASKS/003-simulation-clock.md (этот файл)
- TASKS/003-breakdown.md

Изменить:
- TASKS/README.md (статус 003)

## Критерии готовности

- [ ] Компилируется без Unity.
- [ ] Playing: 1.0 сек = 1 тик. FastForward: 1.0 сек = 4 тика. Paused: 0 тиков.
- [ ] События TickStarted/TickCompleted вызываются на каждый тик.
- [ ] Frame Update (камера/ввод) от тиков не зависит — в этом классе только время.

## План проверки

1. `dotnet run`: прогнать Update с фиксированными delta, сверить счётчики.
2. Проверка границ: отрицательный delta отклоняется; смена скорости посреди накопления не теряет остаток.
