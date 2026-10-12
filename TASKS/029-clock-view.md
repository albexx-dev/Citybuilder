# TASKS/029 — Clock View

## Цель

Игровое время видно: OnGUI-лейбл «День N, час H» + скорость (Пауза/Игра/x4).

## Контекст

028 завёл часы, но UI нет. Паттерн — TreasuryView/PopulationView (OnGUI-лейблы слева вверху).

## Что нужно сделать

1. `ClockView` (MonoBehaviour, модуль UI):
   - Inspector: Runner (SimulationRunner, опционален — иначе GetComponent).
   - OnGUI-лейбл на Rect(10, 50, 300, 30) (под Treasury y=10 и Pop y=30): `"Day " + DayNumber + ", Hour " + HourOfDay + " — " + SpeedName`, где SpeedName: Paused→«Пауза», Playing→«Игра», FastForward→«x4».
   - Нет раннера — молча не рисует. Часов нет — нули не рисует (return).
   - В сцену добавит оркестратор (на объект Simulation).

## Что не нужно делать

- Не делать кнопки скоростей (позже). Не трогать остальное.

## Зависимости

TASKS/003 (SimulationTime), 028 (Runner). Ничего не блокирует.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/UI/ClockView.cs
- TASKS/029-clock-view.md (этот файл)
- TASKS/029-breakdown.md

Изменить:
- TASKS/README.md (статус 029)

## Критерии готовности

- [ ] 1 новый файл до 150 строк.
- [ ] Лейбл показывает день/час/скорость; без раннера молчит. Console чистый.

## Железные уроки (не нарушать)

- Refresh (scope all) + компиляция. Код правится только на стопе.

## План проверки

1. Unity: Refresh all, компиляция, Console чистый.
2. E2E через мост: раннер + прокрутка часов напрямую → сверка текста через рефлексию приватного форматтера (вынести Format в приватный метод для проверки) или чтение CurrentTime. Сцена: ClockView на Simulation после приёмки.
