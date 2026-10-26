# TASKS/083-breakdown

Ввод: задача 083, модуль Infrastructure. Энергия, вода и мусор считают только выросшие дома (`Growth.Homes`), а здания, поставленные вручную через палитру (дом за 100, станция за 300), в балансе не участвуют — хотя у них есть PowerDemand/WaterDemand/WasteOutput. Подключаем `Placement.Placed` к трём контроллерам. Исполнитель: Engineer (субагент). После — Reviewer (только чтение).
Запрещено: рост/въезд, потребности, налоги, сохранения, сцена, ProjectSettings, ServiceCoverage, старые тесты.
Сдать: коммит в ветку task/083-manual-infra + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `PowerController`/`WaterController` (~139) в `RefreshNow` чистят систему, регистрируют городскую базу и обходят `Growth.Homes`, вызывая `AddHome`; `WasteController` (118) — то же для мусора; `BuildingPlacementController.Placed` — копия построенных вручную `BuildingState`. Поля контроллеров обязательны, кроме новых.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Infrastructure/PowerController.cs, WaterController.cs, WasteController.cs, Construction/BuildingPlacementController.cs (Placed).
- [ ] Шаг 2: в каждый из трёх контроллеров добавить `[Header("Manual buildings")] [Tooltip("Ручная стройка. Пусто = только выросшие дома.")] public BuildingPlacementController Placement;`; в `RefreshNow` после обхода `Growth.Homes` пройти `Placement.Placed` теми же `AddHome`/`AddManual` методами (у мусора — `AddBuilding`), без дублей логики: переиспользовать существующие приватные методы, которые уже принимают `BuildingState`.
- [ ] Шаг 3: поведение без `Placement` — байт-в-байт прежнее (нулевая ссылка пропускается); null-элементы в списке пропускаются; файлы должны остаться ≤150 строк (сейчас 139/139/118 — при нехватке вынести добавление в приватный `AddManual`).
- [ ] Шаг 4: без новых файлов и тестов (только проводка Unity-компонентов, математика уже покрыта ElectricityTests/WaterTests/SupplyBalanceTests). Локально прогнать эти три набора через dotnet/csc, приложить результат. Отчёт, commit message.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммит ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка: Refresh → сцена: у Power/Water/Waste контроллеров Placement = BuildingPlacementController → Play: поставить станцию вручную → спрос энергии и воды +1, мусор +1 (у станции значения 1/1/1), поставить ручной дом → +1 к каждому; снос станции ПКМ → значения возвращаются; без Placement — как раньше; Stop → тесты 209 → мерж, пуш.
