# TASKS/089-breakdown

Ввод: задача 089, модуль World + Infrastructure + UI (службы §12, вторая служба). Покрытие считается только для пожарных: любой источник с радиусом считается пожарным. Вводим тип службы и полицейский участок. Исполнитель: Engineer (субагент). После — Reviewer (только чтение).
Запрещено: рост/въезд, потребности (строку потребностей не трогаем), энергия/вода/мусор, налоги, сохранения, сцена, ProjectSettings, старые тесты.
Сдать: коммит в ветку task/089-police + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `BuildingDefinition` (127): `ServiceRadius`; `FireCoverageController` (125) строит `ServiceCoverage` из `Placement.Placed` с `ServiceRadius > 0` и `Growth.Homes`; `FireCoverageView` (30) рисует «Пожарные: N/M (Z%)» на y=342 через `HudText`; в сцене один контроллер.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Construction/BuildingDefinition.cs, Infrastructure/FireCoverageController.cs, UI/FireCoverageView.cs, UI/BuildPaletteView.cs, Data/Buildings/FireStation.asset, Tests/Construction/ServiceRadiusDefinitionTests.cs.
- [ ] Шаг 2: новый `World/ServiceKind.cs` (~15): `public enum ServiceKind { None = 0, Fire = 1, Police = 2 }`.
- [ ] Шаг 3: `BuildingDefinition`: поле `ServiceKind ServiceKind = ServiceKind.None` с Tooltip «Тип службы. None — не служба»; в `OnValidate` неизвестное значение (вне 0..2) → None.
- [ ] Шаг 4: `FireCoverageController`: поле `public ServiceKind Kind = ServiceKind.Fire;` (Tooltip «Какую службу считает этот контроллер») и фильтр источников: радиус > 0 **и** `Definition.ServiceKind == Kind`. Публичный API (`Stations/Homes/Covered/Rate01/RefreshNow`) не менять.
- [ ] Шаг 5: `FireCoverageView`: опциональное поле `Police` (FireCoverageController, Tooltip «Второй контроллер — полиция. Пусто = одна строка»); текст собирается из сегментов: «Пожарные: N/M (Z%)», при наличии полиции добавляется « • Полиция: N/M (Z%)»; `HudText.Draw(10, 342, text)`.
- [ ] Шаг 6: новый ассет `Data/Buildings/PoliceStation.asset` (+.meta, свежий GUID): `police_station`, `Police Station`, cost 250, `AllowedZone: 2`, `JobsProvided: 2`, `ResidentCapacity: 0`, `ServiceRadius: 5`, `ServiceKind: 2`, `UnlockBuildings: 0`; писать только отличия от дефолтов. FireStation — `ServiceKind: 1` (иначе не считается пожарным!). Остальные здания не писать (дефолт None).
- [ ] Шаг 7: тест `Tests/Infrastructure/ServiceKindTests.cs` (~45): новый `BuildingDefinition` имеет `ServiceKind.None`; присвоение `Police` сохраняется; приведение `(ServiceKind)99` не ломает `ServiceRadius`/`UnlockBuildings`.
- [ ] Шаг 8: `.meta` новым файлам (GUID уникальные). Локально прогнать ServiceKindTests + ServiceRadiusDefinitionTests через dotnet/csc, приложить результат. Отчёт, commit message.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммиты ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка: Refresh → сцена: существующий контроллер Kind=Fire; второй экземпляр на Grid Kind=Police; `FireCoverageView.Police` = он; палитра и Placement.Definitions получают PoliceStation → Play: поставить участок в районе → «Пожарные: … • Полиция: X/Y (Z%)», участок не увеличивает пожарное покрытие; станция не увеличивает полицейское; Stop → тесты 232 + новые → мерж, пуш.
