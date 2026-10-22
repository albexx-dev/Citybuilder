# TASKS/059-breakdown

Ввод: задача 059, модуль Infrastructure (электричество, фундамент). §12: «по одной службе за раз». Первый шаг — чистая логика баланса энергии без Unity и без влияния на игру. Исполнитель: Engineer (Muse, субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: поведение (система никем не используется), всё вне Infrastructure/ElectricitySystem.cs, Tests/Infrastructure/ElectricityTests.cs(+.meta при нужде), TASKS/059-breakdown.md.
Сдать: коммит в ветку task/059-electricity + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: нужен учёт спроса и генерации. Модель V1: пропорциональное распределение — при нехватке все потребители получают одинаковую долю (0..1), а не «первым повезло». Это даёт честный brownout и позже ляжет в нужды. Стиль: чистый C#, конструктор без зависимостей, без LINQ-цепочек >2, без сложных generics.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ до правок: Transport/ConnectivitySystem.cs и Transport/RouteSystem.cs (стиль систем), Treasury/Treasury.cs (стиль чистых систем), Tests/Transport/* (стиль тестов, asmdef).
- [ ] Шаг 2: `Infrastructure/ElectricitySystem.cs` (~110 строк): списки потребителей (id → demand) и генераторов (id → capacity); `AddConsumer(id, demand)`, `AddPlant(id, capacity)`, `Remove(id)`, `Clear()`; `TotalDemand`, `TotalSupply`, `SatisfactionRate01` (при demand=0 → 1.0; при supply≥demand → 1.0; иначе supply/demand); `SatisfactionOf(id)` (та же доля для потребителя, 0 если не найден или demand 0 → 1.0 для существующего с нулевым спросом); `IsSatisfied(id)` (≥ 0.999). Отрицательные значения клампятся к 0; дубликат id — перезапись. Без событий и Unity-типов.
- [ ] Шаг 3: тесты `Tests/Infrastructure/ElectricityTests.cs` (~80 строк): пустая система; полное покрытие; нехватка 50%; пропорциональность (оба потребителя получают одинаковую долю); потребление 0 → 1.0; удаление и Clear; кламп отрицательных; перезапись дубликата. Ноль Unity-зависимостей.
- [ ] Шаг 4: самопроверка (лимиты, логика, тесты проходят локально) + отчёт + commit message. Ручной план: Refresh на стопе + run_tests EditMode (все зелёные) + Console.
