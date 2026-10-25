# TASKS/076-breakdown

Ввод: задача 076, модуль Infrastructure + UI (службы §12, третий шаг). Ядро покрытия (074) и пожарная станция с радиусом (075) готовы, но покрытие нигде не считается: игрок ставит станцию и не видит эффекта. Считаем покрытие выросших домов радиусами поставленных станций и выводим в HUD. Поведение роста не меняем. Исполнитель: Engineer (субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: рост/въезд, потребности, энергию/воду/мусор, налоги (кроме сдвига кнопки по y, см. шаг 4), сохранения, сцену, ProjectSettings, ServiceCoverage (только вызовы).
Сдать: коммиты в ветку task/076-fire-coverage + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `ServiceCoverage` (074): `SetSource(id, pos, radius)`, `SetPoint(id,pos)`, `Clear`, `CoveredPoints`, `PointCount`, `Coverage01`, `Rate01`; `BuildingPlacementController.Placed` — копия построенных вручную зданий (`BuildingState.Id/Origin/Definition`); `SettlementGrowthController.Homes` — выросшие здания (тоже `BuildingState`). HUD: мусор y=310, кнопка налога y=342 (высота 26). Левая колонка — OnGUI-лейблы, кнопки регистрируются в `UiClickGuard`.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Infrastructure/ServiceCoverage.cs, Construction/BuildingPlacementController.cs, Population/SettlementGrowthController.cs, Infrastructure/WasteController.cs, UI/WasteView.cs, UI/TaxPolicyView.cs. Остальное — grep.
- [ ] Шаг 2: `Infrastructure/FireCoverageController.cs` (~110): поля `Growth` (обязателен, LogError + enabled=false), `Placement` (обязателен: даёт станции), `RefreshIntervalSec = 2` (кламп ≥0.5); внутри `ServiceCoverage coverage`; `RefreshNow()` = Clear → для каждого `Placement.Placed` с `Definition.ServiceRadius > 0` взять `SetSource(state.Id, state.Origin, radius)` → для каждого `Growth.Homes` `SetPoint(home.Id, home.Origin)`; геттеры `Stations`, `Homes`, `Covered`, `Rate01` (не бросают без ссылок: 0/0/0/1); `Update` по таймеру; Header/Tooltip/null-гарды.
- [ ] Шаг 3: `UI/FireCoverageView.cs` (~35): `Controller` (опционален, фолбэк GetComponent), строка `Пожарные: N/M (Z%)` (N = Covered, M = Homes) на `Rect(10, 342, 400, 30)`; без контроллера молча не рисует.
- [ ] Шаг 4: `UI/TaxPolicyView.cs`: кнопка переезжает с y=342 на y=374 (ниже новой строки пожарных), комментарий про зазоры обновить; размер и UiClickGuard rect — те же, но с новым y.
- [ ] Шаг 5: `.meta` новым файлам (свежие GUID, проверить коллизии). Тестов на контроллер не добавляем: вся математика уже покрыта 8 тестами `ServiceCoverageTests` (074), а здесь только маппинг станций/домов, который проверяется Play-E2E — явно указать это в отчёте. Локально прогнать `ServiceCoverageTests` + `WasteTests` через dotnet/csc (регресс ядра) и приложить результат. Самопроверка, отчёт, commit message.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2, сложных generics, reflection, async.
КОММИТЫ: ≤5 файлов и ≤250 строк каждый.
ЧУЖОЕ: незакоммиченные Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать и не коммитить.

Ручной план приёмки (оркестратор):
1. Refresh на стопе → Console без ошибок.
2. Сцена: на Grid добавить `FireCoverageController` (Growth = SettlementGrowthController, Placement = BuildingPlacementController) и `FireCoverageView` (Controller = он же); сохранить.
3. Play, режим Рост: дома растут → строка «Пожарные: 0/N (0%)»; поставить станцию (палитра) в центр района → покрытие растёт (например 4/16 → 25%); вторая станция рядом → больше; снос станции → падает; кнопка «Налог» ниже и не налезает. Скриншот.
4. Stop → Console чистый → run_tests EditMode: 178 зелёных (новых нет) — либо больше, если инженер добавит.
5. Мерж --no-ff, очередь, push origin main.
