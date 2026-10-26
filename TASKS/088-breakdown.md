# TASKS/088-breakdown

Ввод: задача 088, модуль Infrastructure (долг). `PowerController` и `WaterController` — почти копии (по 150 строк, ровно лимит, следующая правка невозможна) и обе теперь учитывают выросшие и ручные здания. Выносим общий снимок снабжения в отдельный класс. Поведение не меняется. Исполнитель: Engineer (субагент). После — Reviewer (только чтение).
Запрещено: рост/въезд, потребности, мусор, налоги, сохранения, сцена, ProjectSettings, `SupplyBalance`/`ElectricitySystem`/`WaterSystem`, старые тесты.
Сдать: коммит в ветку task/088-power-water-split + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: контроллеры держат `ElectricitySystem`/`WaterSystem` (обёртки `SupplyBalance`), `cityGridId`, `BaseSupply`, `Growth`, `Placement`, `RefreshIntervalSec`; `RefreshNow` = Clear → база → `AddStates(Growth.Homes)` → `AddStates(Placement.Placed)`; `AddHome` берёт `PowerDemand/PowerOutput` (или `WaterDemand/WaterOutput`) и зовёт `AddConsumer`/`AddPlant` (`AddSource`).

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Infrastructure/PowerController.cs, WaterController.cs, Core/SupplyBalance.cs, Infrastructure/ElectricitySystem.cs, WaterSystem.cs, Construction/BuildingPlacementController.cs.
- [ ] Шаг 2: новый `Infrastructure/SupplyFeed.cs` (~110, класс без MonoBehaviour): конструктор `SupplyFeed(SupplyBalance balance, int baseSupply, Func<BuildingDefinition,int> demand, Func<BuildingDefinition,int> output)`; `Refresh(List<BuildingState> grown, List<BuildingState> manual)` — Clear, база (ленивый `EntityId`), обход обоих списков с null-гардами, `SetDemand`/`SetSupply` только при значениях > 0; `BaseSupply` сеттер с клампом ≥0 (вызывается из OnValidate контроллера).
- [ ] Шаг 3: `PowerController`/`WaterController` — оставить публичный API (`Demand/Supply/Rate01/IsHomeSupplied/IsHomePowered`, `RefreshNow`, `Growth`, `Placement`, `BaseSupply`, `RefreshIntervalSec`, `Start/Update/OnValidate/EnsureReady`) без изменений, внутри — `SupplyFeed` с `def.PowerDemand/def.PowerOutput` и `def.WaterDemand/def.WaterOutput`. `IsHomeSupplied` по-прежнему читает `IsSatisfied(home.Id)` из системы (`ElectricitySystem`/`WaterSystem` остаются как источник правды для проверки по id). Файлы должны ужаться до ≤110 строк.
- [ ] Шаг 4: тесты не добавляем (только проводка Unity; математика в SupplyBalanceTests). Локально прогнать ElectricityTests + WaterTests + SupplyBalanceTests через dotnet/csc, приложить результат. Отчёт, commit message.

ЛИМИТЫ: файл ≤150, метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async (Func допустим, как в проекте). Коммит ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка: Refresh → Play: энергия/вода показывают те же значения, что до задачи (выросшие + ручные + база), `BaseSupply` в инспекторе работает, снос/постановка ручного здания меняет значения, Console чистый; Stop → тесты 232 → мерж, пуш.
