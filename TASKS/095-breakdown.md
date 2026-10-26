# TASKS/095-breakdown

Ввод: задача 095, модуль Infrastructure + Population (находка 093). После 094 у игрока появляются источники энергии/воды/мусора, но их дефицит по-прежнему влияет только на строку HUD. Нужен гейт в росте по образцу `CongestionController.GrowthTickMultiplier`: при нехватке энергии / воды / мусора дома растут медленнее, после постройки станции темп возвращается. Исполнитель: Engineer (субагент). После — Reviewer (только чтение).

Запрещено: сцена, ProjectSettings, `SupplyBalance`, `PowerController` / `WaterController` / `WasteController`, рост/въезд/потребности/налоги/сохранения, старые тесты, `SettlementGrowthController.Definitions`.
Сдать: коммиты в ветку task/095-supply-growth + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `SettlementGrowthController` (195/200) в `OnClockTick` считает `neededTicks = RequiredTicks(TicksPerGrowth, Congestion.GrowthTickMultiplier)`; `RequiredTicks` клампит 1..1000. `PowerController.Rate01`, `WaterController.Rate01`, `WasteController.Rate01` обновляются каждые 2 с и возвращают 0..1 (1 при отсутствии спроса). Образец чистого правила — `CongestionSystem.GrowthTickMultiplier`.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Infrastructure/PowerController.cs, WaterController.cs, WasteController.cs, Core/SupplyBalance.cs, Transport/CongestionController.cs, Population/SettlementGrowthController.cs, Tests/Transport/GrowthCongestionTests.cs, Tests/Infrastructure/SupplyBalanceTests.cs.
- [ ] Шаг 2: новый `Infrastructure/SupplyGrowthPolicy.cs` (~40, чистый C#): `DeficitThreshold = 0.999f`, `MaxMultiplier = 3`; `public static int Multiplier(float powerRate, float waterRate, float wasteRate)` — считает дефицитные системы (rate < threshold), возвращает 1 при нуле, иначе `min(3, 1 + deficits)`; NaN считается нормой (не наказываем за сломанный расчёт), отрицательное и 0 — дефицит. Без Unity, без состояния.
- [ ] Шаг 3: новый `Infrastructure/SupplyGrowthController.cs` (~45, MonoBehaviour): `[Header("References")]` с тремя полями `PowerController Power`, `WaterController Water`, `WasteController Waste` (каждое с Tooltip «Пусто = штрафа нет»); `public int GrowthTickMultiplier` читает `Rate01` (null → 1f) и зовёт `SupplyGrowthPolicy.Multiplier`. Никаких RefreshNow не вызывает — контроллеры обновляются сами.
- [ ] Шаг 4: `SettlementGrowthController`: поле `[Tooltip("Замедление роста при дефиците энергии/воды/мусора. Пусто = без штрафа.")] public SupplyGrowthController SupplyGrowth;` и в `OnClockTick` заменить расчёт на:
  `int multiplier = (Congestion != null ? Congestion.GrowthTickMultiplier : 1) * (SupplyGrowth != null ? SupplyGrowth.GrowthTickMultiplier : 1);`
  `int neededTicks = RequiredTicks(TicksPerGrowth, multiplier);`
  Файл должен остаться ≤200 строк, `OnClockTick` ≤30, `RequiredTicks` и публичный API не менять.
- [ ] Шаг 5: тесты `Tests/Infrastructure/SupplyGrowthTests.cs` (~90): все 1 → 1; только энергия 0.75 → 2; энергия+вода → 3; все три 0.5 → 3; NaN,1,1 → 1; 0.999 → 1 и 0.998 → 2 (граница); отрицательный → 2; 0 → 2; связка с ростом: `SettlementGrowthController.RequiredTicks(5, 1 * 3) == 15`, `RequiredTicks(5, 3 * 3) == 45`, `RequiredTicks(500, 9) == 1000`.
- [ ] Шаг 6: `.meta` новым файлам (GUID уникальные). Прогнать SupplyGrowthTests + GrowthCongestionTests + SupplyBalanceTests локально через dotnet/csc, приложить результат. Отчёт, commit messages.

Сцена (делает оркестратор, НЕ инженер): добавить `SupplyGrowthController` на объект Grid, привязать существующие `PowerController` / `WaterController` / `WasteController`, задать его в `SettlementGrowthController.SupplyGrowth`, сохранить сцену на Stop.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммиты ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка: Refresh → Play: 14 домов при базе 12 → все три показателя ниже 100%, `GrowthTickMultiplier == 3`; поставить электростанцию → энергия 100%, множитель 2; добавить водонапорную башню → 2; добавить мусорный депо → 1; после каждого шага новые дома появляются, Console чистый; Stop → тесты 246 + новые → мерж после подтверждения.
