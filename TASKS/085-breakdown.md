# TASKS/085-breakdown

Ввод: задача 085, модуль Persistence + Construction. Ручные здания (палитра: дом, пожарная станция) живут только в памяти — после загрузки слота пропадают, хотя выросшие дома сохраняются. Пишем их в снапшот (Version 3, совместимость с v1/v2) и восстанавливаем при загрузке. Заодно разгружаем `BuildingPlacementController` (171 строка, жёсткий предел 200). Исполнитель: Engineer (субагент). После — Reviewer (только чтение).
Запрещено: рост/въезд, потребности, инфраструктурные контроллеры, налоги, сцена, ProjectSettings, старые тесты.
Сдать: коммиты в ветку task/085-save-manual + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `CitySnapshot` (v2) содержит Zones/Buildings/Households/Roads + TaxLevel; `SaveSystem.BuildSnapshot(...)` с перегрузками; `SaveController`/`LoadController` — адаптеры; `BuildingPlacementController` держит `states`/`views`, `Placed` (копия), `TryPlaceAt` (со списанием), `SetDefinition`, использует `BuildingViewFactory`. `BuildingSystem` умеет Place/Demolish поверх `GridSystem`.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Persistence/CitySnapshot.cs, SaveSystem.cs, SaveSlotStorage.cs, SaveController.cs, LoadController.cs, Construction/BuildingPlacementController.cs, BuildingViewFactory.cs, Construction/BuildingSystem.cs, Tests/Economy/TaxSaveTests.cs, Tests/SaveSystemTests.cs.
- [ ] Шаг 2: вынести ручные здания в новый `Construction/ManualBuildingsStore.cs` (~90): списки `states`/`views`, методы `Placed` (копия), `Add(BuildingState, GameObject view)`, `AddView`, `Demolish(BuildingState)`, `Clear(BuildingSystem, GridVisualizer)`; контроллер переключается на стор и ужимается до ~150 строк. Поведение прежнее.
- [ ] Шаг 3: `BuildingPlacementController`: `public List<BuildingDefinition> Definitions` (Tooltip «Типы для восстановления из слота и палитры»), `public bool RestoreManual(string defId, GridPosition origin)` — ищет тип по Id, ставит без списания денег, добавляет вид и освежает занятость; `public void ClearPlaced()` — сносит все ручные без выселения (ручные дома не заселяются в V1).
- [ ] Шаг 4: `CitySnapshot`: `Version = 3`, `public List<BuildingEntry> ManualBuildings` (повторное использование `BuildingEntry`).
- [ ] Шаг 5: `SaveSystem`: `CurrentVersion = 3`; перегрузки `BuildSnapshot` сохраняют прежние сигнатуры (6 и 7 арг) и добавляют 8-арг с `List<BuildingState> manualBuildings`; нормализация: null/пустые Id пропускаются.
- [ ] Шаг 6: `SaveSlotStorage.Validate`: принимать версии 1..3.
- [ ] Шаг 7: `SaveController`: опциональное поле `Placement` (ленивый `FindFirstObjectByType`), передаёт `Placement.Placed` в снапшот; в лог добавить `ручных=`.
- [ ] Шаг 8: `LoadController`: опциональное поле `Placement`; в `ClearAll` — `Placement.ClearPlaced()`; после `ApplyCity` — `ApplyManual(snapshot)` (цикл `RestoreManual`, null-элементы и неизвестные Id пропускаются, счётчик в лог).
- [ ] Шаг 9: тесты `Tests/Persistence/ManualSaveTests.cs` (~70): v3 снапшот с двумя ручными зданиями; roundtrip через JsonUtility сохраняет Id/координаты; null-элементы и пустые Id пропускаются; перегрузка 6/7 арг даёт пустой список; Validate принимает 1/2/3, отклоняет 4.
- [ ] Шаг 10: `.meta` новым файлам (GUID уникальные). Локально прогнать ManualSaveTests + TaxSaveTests + SaveSystemTests через dotnet/csc, приложить результат. Отчёт, commit messages.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммиты ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка: Refresh → Play: поставить станцию и ручной дом → SaveNow → NewGame/LoadNow → оба здания на местах, занятость клеток восстановлена, баланс и налог прежние; Console без ошибок; Stop → тесты 214 + новые → мерж, пуш.
