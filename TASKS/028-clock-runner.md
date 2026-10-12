# TASKS/028 — Clock Runner

## Цель

Игровые часы тикают в сцене: SimulationRunner крутит SimulationClock каждый кадр, скорость переключается (Пауза/Игра/x4).

## Контекст

SimulationClock (003) — чистая логика без драйвера: в сцене его никто не вызывает. ARCHITECTURE §5: тик 1 сек = 1 игровой час. Привязка налогов/роста к часам — позже; сейчас только бег часов.

## Что нужно сделать

1. `SimulationRunner` (MonoBehaviour, модуль Simulation — первый MonoBehaviour в модуле):
   - Inspector: Speed (SimulationSpeed, default Playing) — enum рисуется сам.
   - Ленивый геттер `Clock` (создаёт SimulationClock, применяет Speed).
   - Start: применить Speed. OnValidate: применить Speed если часы уже созданы (без создания).
   - Update: `Clock.Update(Time.deltaTime)` — всё.
   - Публичный `SetSpeed(SimulationSpeed)` (для E2E и будущего UI).
   - В сцену добавит оркестратор отдельно (объект Simulation).

## Что не нужно делать

- Не привязывать налоги/рост/население к тикам (позже). Не делать UI часов. Не трогать остальное.

## Зависимости

TASKS/003 (Done). Разблокирует будущую привязку систем к часам.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Simulation/SimulationRunner.cs
- TASKS/028-clock-runner.md (этот файл)
- TASKS/028-breakdown.md

Изменить:
- TASKS/README.md (статус 028)

## Критерии готовности

- [ ] 1 новый файл до 150 строк.
- [ ] Update(1f)×3 на Playing → 3 тика; Paused → 0; FastForward → 12 тиков за 3 сек.
- [ ] Console без ошибок. Правила кода соблюдены.

## Железные уроки (не нарушать)

- Refresh (scope all) + компиляция. Код правится только на стопе.

## План проверки

1. Unity: Refresh all, компиляция, Console чистый.
2. E2E через мост: прямые Update с фейковым deltaTime + скорости; сверка TickCount. Сцена: объект Simulation + раннер после приёмки.
