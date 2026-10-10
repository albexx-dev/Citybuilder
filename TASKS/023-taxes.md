# TASKS/023 — Taxes

## Цель

Казна пополняется с работающих: тикер раз в N секунд берёт Employed × ставку и кладёт в Treasury. Первый доход города (GAME_DESIGN §8).

## Контекст

Зависит от 008 (Treasury/TrySpend/Earn), 016 (PopulationSystem), 020 (EmploymentSystem). SimulationClock в сцене не заведён — тикер свой по таймеру (привязка к часам позже, зафиксировать). Безработица/ставки прогрессивные — позже.

## Что нужно сделать

1. `SettlementGrowthController` (изменить, модуль Population — ТОЛЬКО добавить):
   - Публичный геттер `Settlement`: вернуть приватный `settlement` (может быть null до EnsureReady — вызывающий проверяет; как Zones).
2. `TaxSystem` (plain C#, модуль Economy): логика без состояния.
   - Конструктор `(EmploymentSystem employment, Treasury treasury)` — null → ArgumentNullException.
   - `ComputeTax(int perWorker)` → int: perWorker <= 0 → 0; иначе `employment.Employed * perWorker`.
   - `Collect(int perWorker)` → Treasury.TransactionResult: сумма 0 → Fail (не дёргать Earn); иначе `treasury.Earn(total)` и вернуть его результат.
3. `EconomyTicker` (MonoBehaviour, модуль Economy):
   - Inspector: Population (PopulationController, обязателен), Growth (SettlementGrowthController, обязателен), Treasury (TreasuryView, обязателен), TaxPerWorker (default 2, минимум 0 — 0 выключает сбор), TickIntervalSec (default 10, минимум 1), placementActive (default true).
   - Ленивый EnsureReady: свой EmploymentSystem + TaxSystem поверх Population.Population и Growth.Settlement (любой null → тихо не тикает, без спама).
   - Update: таймер → `Tax.Collect(TaxPerWorker)`. TreasuryView-лейбл обновится сам (баланс).
   - В сцене по умолчанию ВКЛЮЧЁН (конфликтов ввода нет) — но добавление в сцену делает оркестратор отдельно после приёмки.

## Что не нужно делать

- Не делать расходы, бюджет, дефицит, политику ставок (позже).
- Не заводить SimulationClock (отдельная задача).
- Не трогать остальное. Input не нужен.

## Зависимости

TASKS/008, 016, 020 (Done). Доходная часть Этапа 6+8.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Economy/TaxSystem.cs
- Assets/Citybuilder/Scripts/Economy/EconomyTicker.cs
- TASKS/023-taxes.md (этот файл)
- TASKS/023-breakdown.md

Изменить:
- Assets/Citybuilder/Scripts/Population/SettlementGrowthController.cs (только геттер Settlement)
- TASKS/README.md (статус 023)

## Критерии готовности

- [ ] 2 новых файла до 150 строк; правка роста — только геттер.
- [ ] Employed 6 × 2 → баланс +12 за тик; perWorker 0 → сбора нет, Earn не вызывается.
- [ ] Console без ошибок. [Header]/Tooltip/null, методы до 30, без LINQ >2.

## Железные уроки (не нарушать)

- Только Unlit/Color (материалов нет). Refresh (scope all) + компиляция. Сцену — только при выключенном Play.

## План проверки

1. Unity: Refresh all, компиляция, Console чистый.
2. E2E через мост: живой стек (settle/movein) → Collect напрямую → баланс вырос; тикерный шаг через рефлексию; perWorker 0. Сцена: объект Economy + тикер после приёмки.
