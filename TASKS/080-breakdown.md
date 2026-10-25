# TASKS/080-breakdown

Ввод: задача 080, модуль Core + Construction + UI (прогрессия §15, первый шаг). Сейчас все типы зданий доступны с первой минуты: магазины и фабрики растут сразу, прибыли/рабочих мест хватает без развития. Вводим разблокировку по числу жителей: магазин — с 10, фабрика — с 20. Дом и пожарная станция — всегда. Исполнитель: Engineer (субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено: инфраструктурные контроллеры, потребности, налоги, сохранения, сцена, ProjectSettings, старые тесты.
Сдать: коммиты в ветку task/080-unlocks + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `BuildingDefinition` (122) — данные зданий; `SettlementGrowthController.FindFreeOrigin` перебирает `Definitions` и берёт первый подходящий по зоне (`MatchesZone`), `Settlement` даёт клетки; `BuildPaletteView` рисует кнопки `DisplayName + " " + ConstructionCost` и вызывает `Placement.SetDefinition`; население — `PopulationController.Population.TotalResidents`.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Construction/BuildingDefinition.cs, Population/SettlementGrowthController.cs (метод FindFreeOrigin и рядом), UI/BuildPaletteView.cs, Population/PopulationController.cs, Data/Buildings/Shop.asset, Factory.asset, Tests/Construction/ServiceRadiusDefinitionTests.cs. Остальное — grep.
- [ ] Шаг 2: `BuildingDefinition`: поле `UnlockResidents` (default 0, Tooltip «Нужно жителей в городе. 0 — доступно сразу»), кламп ≥0 через `Clamp`.
- [ ] Шаг 3: новый `Core/UnlockPolicy.cs` (~50, чистый C#, без Unity): `IsUnlocked(BuildingDefinition def, int residents)` (null → false; residents < 0 → 0; сравнение `>=`), `Remaining(BuildingDefinition def, int residents)` (сколько жителей не хватает, 0 если открыто; null → 0). Без LINQ.
- [ ] Шаг 4: `SettlementGrowthController`: опциональное поле `Population` (`PopulationController`, Tooltip «Пусто = без разблокировки»); в `FindFreeOrigin` перед `MatchesZone` пропускать `!UnlockPolicy.IsUnlocked(candidate, Residents)`; приватный `Residents` = население, если `Population` подключён и готов, иначе `int.MaxValue` (старое поведение). `RestoreGrown` не фильтровать (загрузка сохранений важнее).
- [ ] Шаг 5: `BuildPaletteView`: опциональное поле `Population`; заблокированная кнопка неактивна (`GUI.enabled = false` на время отрисовки, затем вернуть) и рисует `DisplayName + " — " + Remaining + " жит."` вместо цены; клик по заблокированной игнорируется; разблокированная — как сейчас.
- [ ] Шаг 6: данные — `Shop.asset` → `UnlockResidents: 10`, `Factory.asset` → `UnlockResidents: 20` (только отличия от дефолта).
- [ ] Шаг 7: тесты `Tests/Construction/UnlockPolicyTests.cs` (~60): порог 0 открыт при 0; порог 10 — закрыт при 9, открыт при 10 и 11; отрицательное население = 0; null def → не открыт; `Remaining` 10/9 → 1, 10/3 → 7, 10/10 → 0; дефолт нового `BuildingDefinition` — 0.
- [ ] Шаг 8: `.meta` новым файлам (свежие GUID). Локально прогнать UnlockPolicyTests + UnlockPolicy-зависимые старые тесты через dotnet/csc (без Unity), приложить результат. Отчёт, commit message.

ЛИМИТЫ: файл ≤150 (жёстко 200, `SettlementGrowthController` сейчас 195 — новые строки только если уложишься; цель — вынести Residents в приватный геттер и не превысить 200), метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммиты ≤5 файлов / ≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка (оркестратор): Refresh → сцена Growth.Population = PopulationController, Palette.Population = он же → Play: при 0 жителей магазин/фабрика в палитре серые с «— 10 жит.»/«— 20 жит.», рост на Commercial/Industrial ничего не ставит; после 10 жителей магазины растут и кнопка активна; после 20 — фабрики; Stop → тесты 184 + новые → мерж, пуш.
