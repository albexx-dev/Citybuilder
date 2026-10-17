# TASKS/057-breakdown

Ввод: задача 057, модуль Transport (пробки). T3: нагрузка → множитель времени + замедление роста (мягкое, без гейтов баланса). Исполнитель: Engineer (Muse, субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: жёсткие гейты (MinEmployment, порядок въезда, баланс 047), всё вне Transport/CongestionSystem.cs, Tests/Transport/CongestionTests.cs(+.meta при нужде) и TASKS/057-breakdown.md.
Сдать: коммит в ветку task/057-congestion + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: RouteSystem (056) даёт нагрузку клеток (LoadOf). Пробка — клетка с нагрузкой выше порога. Множитель времени = 1 + (load-1)*factor (clamped 1..max). Порог и фактор — параметры (по умолчанию порог 4, фактор 0.25, max 3.0). Замедление роста: множитель тиков (целый) = max(1, (int)round(congestionMultiplier)); применять — в следующей задаче (контроллер), здесь только чистая логика.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ до правок: Transport/RouteSystem.cs (LoadOf/HasLoad), Tests/Transport (стиль).
- [ ] Шаг 2: `Transport/CongestionSystem.cs` (~90 строк): конструктор от RouteSystem (или от Func<int,int> нагрузки — проще от RouteSystem); `MultiplierFor(load)` (чистая функция, порог/фактор/max — поля с дефолтами и клампами в конструкторе/OnValidate-эквиваленте); `WorstMultiplier()` — max по всем клеткам с нагрузкой; `GrowthTickMultiplier()` — целое ≥1; `IsCongested(load)` (load ≥ threshold). Лимиты: файл ≤150, методы ≤30.
- [ ] Шаг 3: тесты `Tests/Transport/CongestionTests.cs` (~60 строк): ниже порога → 1.0; на пороге → 1.0+factor; кламп к max; GrowthTickMultiplier ≥1; WorstMultiplier на пустой нагрузке = 1.0. Ноль Unity-зависимостей.
- [ ] Шаг 4: самопроверка (лимиты, логика, тесты проходят локально) + отчёт + commit message. Ручной план: Refresh на стопе + run_tests EditMode (все зелёные) + Console.
