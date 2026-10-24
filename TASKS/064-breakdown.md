# TASKS/064-breakdown

Ввод: задача 064, модуль Infrastructure (вода, шаг 2 — город и HUD). Точная копия модели 060 (энергия): поля в `BuildingDefinition`, городская сеть `BaseSupply`, лейбл HUD. Влияния на въезд/рост нет — только отображение. Исполнитель: Engineer (субагент). После инженера — Reviewer (субагент, только чтение).

Запрещено трогать: `NeedSystem`, `NeedsView`, `PopulationController`, `PowerController`/`PowerView`/`ElectricitySystem`, сцену (`Prototype.unity` — обвязку делает оркестратор на стопе), `WaterSystem` и его тесты (готовы в 063), все прочие модули.

Сдать: коммит(ы) в ветку `task/064-water-city` + ручной план E2E + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

## Файлы

1. `Assets/Citybuilder/Scripts/Construction/BuildingDefinition.cs` (правка): в `[Header("Infrastructure")]` после энергии добавить
   - `WaterDemand` (int, default 1): «Сколько воды потребляет одно здание. Не может быть отрицательным.»
   - `WaterOutput` (int, default 0): «Сколько воды производит одно здание. Не может быть отрицательным.»
   В `OnValidate` — два клампа `< 0 → 0` с `Debug.LogWarning`, в стиле существующих. Метод уже длинный (долг), новых хелперов не вводить.
2. `Assets/Citybuilder/Scripts/Infrastructure/WaterController.cs` (новый, ~140 строк, 1 класс): копия `PowerController` с водными именами.
   - `[Header("References")] public SettlementGrowthController Growth;`
   - `[Header("City Grid")] public int BaseSupply = 12;` — мощность городского водопровода до появления водонапорных зданий.
   - `[Header("Refresh")] public float RefreshIntervalSec = 2f;`
   - свойства `int Demand`, `int Supply`, `float Rate01` (через `WaterSystem`, `Mathf.RoundToInt`);
   - `bool IsHomeSupplied(BuildingState home)` — дом без `WaterDemand` считается снабжённым; null/без Definition → false;
   - `Start` → Clamp + RefreshNow; `OnValidate` → Clamp; `Update` → таймер → RefreshNow;
   - `RefreshNow()`: `Clear`, регистрация городской сети (`EntityId.New()` один раз, `AddSource(cityGridId, BaseSupply)`), цикл по `Growth.Homes` → `AddHome`;
   - `AddHome`: `WaterDemand > 0 → AddConsumer`, `WaterOutput > 0 → AddSource`;
   - `EnsureReady` (нет Growth → `Debug.LogError("WaterController: задайте Growth в Inspector.")`, `enabled = false`), `EnsureSystem`, `ClampInterval` (интервал ≥0.5, BaseSupply ≥0).
3. `Assets/Citybuilder/Scripts/Infrastructure/WaterView.cs` (новый, ~30 строк, 1 класс): копия `PowerView`, лейбл `"Вода: " + Demand + "/" + Supply + " (" + % + "%)"`.
   - `[Header("References")] public WaterController Controller;`, fallback `GetComponent<WaterController>()`, null → молча выйти;
   - `GUI.Label(new Rect(10, 278, 400, 30), text)` — ниже энергии (246+26+6), не налезать.
4. `Assets/Citybuilder/Tests/Infrastructure/BuildingWaterDefinitionTests.cs` (новый, ~30 строк, 1 класс, NUnit): дефолты (`WaterDemand == 1`, `WaterOutput == 0`), запись значений (3/7 читаются), нулевой спрос остаётся нулём. Клампы `OnValidate` рефлексией не дёргать.

`.meta` для трёх новых файлов: `fileFormatVersion: 2` + уникальный 32-hex lowercase guid (проверить `git grep <guid>`).

## Критерии приёмки (для ревьюера)

1. Поля/клампы Definition корректны, дефолты 1/0, остальные поля не тронуты.
2. `WaterController` семантически эквивалентен `PowerController` (кроме имён и `BaseSupply=12`), без деления на ноль, без обращения к `Growth` до null-проверки.
3. `WaterView` рисует y=278, не падает без контроллера, формат процента как у энергии.
4. Тесты: 3 штуки, без Unity-сцены; `ScriptableObject.CreateInstance` допустим (как в `BuildingPowerDefinitionTests`).
5. Только разрешённые файлы + `.meta`; сцена, NeedSystem, NeedsView не тронуты.
6. Лимиты: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс, коммит ≤5 файлов/250 строк (если не влезает — 2 коммита).

## Ручной план E2E (оркестратор)

1. На стопе: добавить на объект Grid компоненты `WaterController` (Growth = SettlementGrowthController) и `WaterView` (Controller = WaterController); сохранить сцену.
2. Play: HUD «Вода: N/12 (P%)» под «Энергия»; с домами demand растёт; снос уменьшает; Console чистый.
3. Скриншот в `Screenshots/`. Stop. Тесты EditMode (ожидается 121).

## Долги (в очередь)

- `BuildingDefinition.OnValidate` после 064 станет ещё длиннее — нужна декомпозиция (отдельная задача).
- `PowerController` и `WaterController` дублируются; позже общий `SupplyBalance`/базовый контроллер.
- В 063 не покрыт профицит (`supply > demand`) и граница 0.999 — добавить тесты при удобном случае.
