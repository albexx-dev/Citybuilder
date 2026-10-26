# TASKS/095-breakdown

Ввод: задача 095, модуль Infrastructure + Population (находка 093). После 094 у игрока появляются источники энергии/воды/мусора, но их дефицит по-прежнему влияет только на строку HUD. Нужен гейт в росте: при нехватке энергии / воды / мусора дома растут медленнее, после постройки станций темп возвращается.

**Задача начинается со сплита `SettlementGrowthController` (195/200, HANDOFF §6: следующий тач только со сплитом).** Расчёт множителей и `RequiredTicks` выносится в отдельный класс `GrowthPace`, контроллер получает готовое число. Файл должен стать короче, а не вырасти.

Исполнитель: Engineer (субагент). После — Reviewer (только чтение).
Запрещено: сцена, ProjectSettings, `SupplyBalance`, `PowerController` / `WaterController` / `WasteController`, рост/въезд/потребности/налоги/сохранения, `SettlementGrowthController.Definitions`.
Сдать: коммиты в ветку task/095-supply-growth + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `SettlementGrowthController` (195) в `OnClockTick` считает `neededTicks = RequiredTicks(TicksPerGrowth, Congestion.GrowthTickMultiplier)`. `PowerController.Rate01`, `WaterController.Rate01`, `WasteController.Rate01` обновляются каждые 2 с и возвращают 0..1 (1 при отсутствии спроса).

## Правила штрафа (согласованы)
- Штраф — **максимум** из множителя пробок и множителя дефицита: `Math.Max(congestion, supply)`, не произведение. Худший случай ×3, а не ×9.
- Штраф по **глубине** дефицита, а не по числу дефицитных систем:
  - `Rate01 >= 0.99` → ×1;
  - `0.5 <= Rate01 < 0.99` → ×2;
  - `Rate01 < 0.5` → ×3.
- Общий supply-множитель — максимум по трём системам (худшая система задаёт темп).
- NaN считается нормой (×1), отрицательное и 0 — глубина ×3.

## Шаги
- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Infrastructure/PowerController.cs, WaterController.cs, WasteController.cs, Core/SupplyBalance.cs, Transport/CongestionController.cs, Population/SettlementGrowthController.cs, Tests/Transport/GrowthCongestionTests.cs, Tests/Infrastructure/SupplyBalanceTests.cs.
- [ ] Шаг 2: новый `Infrastructure/SupplyGrowthPolicy.cs` (~45, чистый C#): `public const float GoodRate = 0.99f`, `public const float SevereRate = 0.5f`; `public static int DepthMultiplier(float rate)` — NaN → 1; `>= GoodRate` → 1; `>= SevereRate` → 2; иначе 3; `public static int Multiplier(float powerRate, float waterRate, float wasteRate)` — максимум из трёх `DepthMultiplier`. Без Unity, без состояния.
- [ ] Шаг 3: новый `Infrastructure/SupplyGrowthController.cs` (~45, MonoBehaviour): `[Header("References")]` с полями `PowerController Power`, `WaterController Water`, `WasteController Waste` (Tooltip «Пусто = штрафа нет»); `public int GrowthTickMultiplier` читает `Rate01` (null → 1f) и зовёт `SupplyGrowthPolicy.Multiplier`. Никаких `RefreshNow` не вызывает.
- [ ] Шаг 4: новый `Population/GrowthPace.cs` (~45, чистый C#, без Unity): перенести сюда `MaxRequiredTicks = 1000`, `Combine(int congestionMultiplier, int supplyMultiplier)` = максимум с клампом 1..3 и `RequiredTicks(int baseTicks, int multiplier)` (base ≤ 0 → 1, multiplier ≤ 0 → 1, результат ≤ 1000); плюс перегрузка `RequiredTicks(int baseTicks, int congestionMultiplier, int supplyMultiplier)`, внутри зовущая `Combine`.
- [ ] Шаг 5: `SettlementGrowthController`: удалить `MaxRequiredTicks` и статический `RequiredTicks` (переехали в `GrowthPace`); добавить поле `[Tooltip("Замедление роста при дефиците энергии/воды/мусора. Пусто = без штрафа.")] public SupplyGrowthController SupplyGrowth;`; в `OnClockTick`:
  ```csharp
  int neededTicks = GrowthPace.RequiredTicks(TicksPerGrowth,
      Congestion != null ? Congestion.GrowthTickMultiplier : 1,
      SupplyGrowth != null ? SupplyGrowth.GrowthTickMultiplier : 1);
  ```
  Публичный API (`Homes`, `GrownCount`, `OnClockTick`, `RestoreGrown`, `FindStateAt`, `DemolishState`) не меняется. Файл должен стать **короче 195** (ориентир ≤185), `OnClockTick` ≤30.
- [ ] Шаг 6: `Tests/Transport/GrowthCongestionTests.cs` — заменить вызовы `SettlementGrowthController.RequiredTicks` на `GrowthPace.RequiredTicks`; ассерты не менять (это часть сплита, не изменение поведения).
- [ ] Шаг 7: тесты `Tests/Infrastructure/SupplyGrowthTests.cs` (~110): глубина — 1 → 1; 0.99 → 1; 0.98 → 2; 0.75 → 2; 0.5 → 2; 0.499 → 3; 0 → 3; −0.2 → 3; NaN → 1; максимум по системам — (0.4, 1, 1) → 3; (1, 0.7, 1) → 2; (0.7, 0.7, 1) → 2; `Combine(1,1)=1`, `Combine(3,2)=3`, `Combine(2,2)=2`, `Combine(0,5)=3` (кламп); `RequiredTicks(5,1)=5`, `(5,3)=15`, `(500,9)=1000`; `RequiredTicks(5,3,2)=15` — максимум, а не 5×6.
- [ ] Шаг 8: `.meta` новым файлам (GUID уникальные). Локально прогнать SupplyGrowthTests + GrowthCongestionTests + SupplyBalanceTests через dotnet/csc, приложить результат. Отчёт, commit messages.

Сцена (делает оркестратор, НЕ инженер): добавить `SupplyGrowthController` на объект Grid, привязать существующие `PowerController` / `WaterController` / `WasteController`, задать его в `SettlementGrowthController.SupplyGrowth`, сохранить сцену на Stop.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммиты ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка: Refresh → Play: 13 выросших зданий (дома, магазины и заводы — у каждого `PowerDemand: 1`) при базе 12 → все три показателя 92%, `GrowthTickMultiplier == 2`, дома растут вдвое медленнее (10 тиков вместо 5); поставить электростанцию, водонапорную башню и мусорный депо → 24/13 = 100%, `GrowthTickMultiplier == 1`, темп 5 тиков; `GrowthPace.Combine(3,2) == 3` (максимум); Console чистый; Stop → тесты 246 + новые → мерж после подтверждения.
