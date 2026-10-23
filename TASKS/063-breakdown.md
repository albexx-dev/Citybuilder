# TASKS/063-breakdown

Ввод: задача 063, модуль Infrastructure (вода, шаг 1 — ядро). §12: «по одной службе за раз». Точная копия модели 059 (энергия): спрос/подача и пропорциональное распределение. Без Unity, без влияния на игру. Исполнитель: Engineer (субагент). После инженера — Reviewer (субагент, только чтение).

## Разбиение воды (зафиксировано)

- 063 (эта задача): `WaterSystem` + тесты — чистое ядро.
- 064: `WaterDemand`/`WaterOutput` в `BuildingDefinition`, `WaterController` (BaseSupply), `WaterView` (HUD) + обвязка сцены.
- 065: вода в `NeedSystem` (5-й показатель HUD) + тесты среднего.

Причина: лимит мержа ≤400 строк и ≤1 модуль на мерж.

Запрещено трогать: всё вне `Assets/Citybuilder/Scripts/Infrastructure/WaterSystem.cs`, `Assets/Citybuilder/Tests/Infrastructure/WaterTests.cs` (+ их `.meta`), `TASKS/063-breakdown.md`. `ElectricitySystem` и его тесты не менять, `BuildingDefinition`/`NeedSystem`/сцену не трогать.

Сдать: коммит в ветку `task/063-water-core` + ручной план проверки + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

## Контекст

Модель V1: при нехватке все потребители получают одинаковую долю 0..1 (честный «недолив»), а не «первым повезло». Стиль: чистый C#, конструктор без зависимостей, без LINQ-цепочек >2, без generics, без событий. Комментарии — краткие, по-русски, как в `ElectricitySystem`.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ до правок: `Scripts/Infrastructure/ElectricitySystem.cs`, `Scripts/Core/EntityId.cs`, `Tests/Infrastructure/ElectricityTests.cs`, `Tests/Tests.asmdef` (asmdef `Tests`), `TASKS/059-breakdown.md`.
- [ ] Шаг 2: `Assets/Citybuilder/Scripts/Infrastructure/WaterSystem.cs` (~110 строк, 1 класс, namespace `Citybuilder`):
  - поля: `Dictionary<EntityId, float> consumerDemand`, `Dictionary<EntityId, float> sourceCapacity`;
  - `TotalDemand`, `TotalSupply` — суммы (отрицательные уже обнулены);
  - `SatisfactionRate01`: demand ≤ 0 → 1; supply ≥ demand → 1; иначе supply/demand;
  - `AddConsumer(EntityId id, float demand)`, `AddSource(EntityId id, float capacity)` — дубликат id перезаписывает, отрицательное → 0;
  - `Remove(EntityId id)` — из обоих словарей; `Clear()`;
  - `SatisfactionOf(EntityId id)`: нет потребителя → 0; demand ≤ 0 → 1; иначе общая доля;
  - `IsSatisfied(EntityId id)` — `SatisfactionOf ≥ 0.999f`;
  - приватные `ClampNonNegative`, `Sum`.
- [ ] Шаг 3: `Assets/Citybuilder/Tests/Infrastructure/WaterTests.cs` (~130 строк, 1 класс, NUnit, ноль Unity API): пустая система (0/0/1, чужой id → 0/false); полное покрытие; нехватка 50%; равная доля двум потребителям; потребитель с нулевым спросом → 1; `Remove` и `Clear`; кламп отрицательных (спрос −10 → 0, источник −5 → 0, при спросе 10 и источнике −5 ставка 0); дубликат id перезаписывает. Имена тестов — по образцу `ElectricityTests`.
- [ ] Шаг 4: `.meta` для обоих файлов: `fileFormatVersion: 2` + уникальный `guid` (32 hex, lowercase). Проверить, что guid не встречается в репозитории: `git grep "<guid>"` пусто.
- [ ] Шаг 5: самопроверка лимитов (файл ≤150, метод ≤30), отчёт: что сделано, commit message, ручной план (Refresh на стопе → run_tests EditMode → Console).

## Критерии приёмки (для ревьюера)

1. Публичный API совпадает по смыслу с `ElectricitySystem` (имена — водные: `AddSource`).
2. Поведение: 0 спроса → 1; полное покрытие → 1; пропорциональная доля; клампы; перезапись.
3. Только 2 исходника + 2 `.meta` + breakdown; чужие файлы не тронуты.
4. Лимиты: файл ≤150, метод ≤30, 1 файл = 1 класс.
5. Тесты компилируются в asmdef `Citybuilder.Tests`, без Unity API.
