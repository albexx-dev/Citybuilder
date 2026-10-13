# TASKS/034-breakdown

Ввод: задача 034, модуль Population (1–2 файла), привязка роста к часам. Исполнитель: Engineer (Muse, субагент).
Запрещено трогать: всё кроме Population/*.cs и TASKS/034-breakdown.md (галочки).
Сдать: ветка task/034-growth-clock, код + ручной план проверки + commit message.

Контекст: SettlementGrowthController растит по Update-таймеру (Time.deltaTime, GrowthIntervalSec=5). Пауза его не останавливает, x4 не ускоряет. Налоги уже на тиках (EconomyTicker, 031 — образец подписки: Runner, OnEnable/OnDisable, Tick-пересборка, placementActive-гейт). V1: 1 тик = 1 игровой час.

- [x] Шаг 1: Runner + счётчик тиков вместо таймера:
  - поле `public SimulationRunner Runner` (обязателен, как в EconomyTicker);
  - поле `public int TicksPerGrowth = 5` (кламп ≥1); таймер `timer` и накопление через Time.deltaTime удалить; `GrowthIntervalSec` удалить (в сцене стояло 5 → эквивалент 5 тиков);
  - подписка `Runner.Clock.TickCompleted += OnClockTick` в OnEnable, отписка в OnDisable (копия паттерна 031, переживает перезагрузку домена);
  - счётчик: каждый тик при placementActive → +1, при достижении TicksPerGrowth → TryGrow + сброс; MaxSettlements-гейт сохранить;
  - Update оставляет только HandleDemolishInput (снос по ПКМ — ручной ввод, не сим);
  - EnsureReady проверяет Runner (нет → ошибка + enabled=false, как остальные контроллеры);
  - порядок spend→place из 033 не ломать; Homes/Settlement/Definitions без изменений;
  - лимит: файл сейчас 173 строки, жёсткий предел 200. Не влез — вынести тик-драйвер в новый файл того же модуля (например SettlementGrowthTicker.cs), 1 файл = 1 класс.
- [x] Шаг 2: самопроверка (пауза = стоп роста, x4 = ускорение, дабл-подписка OnEnable/OnDisable, Runner=null гард, spend→place цел) + отчёт + commit message. Ручной план для приёмки через мост: копия ТОЛЬКО на стопе + Refresh scope all + вязка Runner в сцену (только на стопе) + Play-проверка + Stop + Console чистый.

Выполнено (ветка task/034-growth-clock): SettlementGrowthController — убраны timer/Time.deltaTime/GrowthIntervalSec, добавлены Runner + TicksPerGrowth=5 + счётчик tickCount, вход тиков — публичный OnClockTick (гейты placementActive/MaxSettlements сохранены), Update — только снос по ПКМ, TryGrow (spend→place) без изменений. Подписка вынесена в новый SettlementGrowthTicker.cs (тот же модуль, копия паттерна 031: OnEnable-подписка/OnDisable-отписка). Лимиты: контроллер 192 строки (≤200), тикер 50 строк, методы ≤30. Внешние связи целы (Homes, Settlement, placementActive).

Commit message: Update settlement growth to clock ticks with separate ticker

Ручной план проверки через Unity-мост (только на стопе):
1. Stop (Edit Mode). Refresh scope all, дождаться конца компиляции, Console — без ошибок.
2. На объекте роста проверить: поле GrowthIntervalSec пропало, появилось TicksPerGrowth=5; довязать Runner (объект Simulation); добавить компонент SettlementGrowthTicker на тот же объект, в Growth сослаться на контроллер. Один тикер на объект.
3. Play: жилая зона + казна → дом вырастает каждые 5 тиков; пауза — роста нет; x4 — рост быстрее; ПКМ по дому — снос; Console чистый.
4. Stop. Доложить результат.
