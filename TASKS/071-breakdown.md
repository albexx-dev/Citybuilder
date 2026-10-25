# TASKS/071-breakdown

Ввод: задача 071, модуль Infrastructure + данные зданий. Мусор — третья инфраструктурная величина после энергии (059–061) и воды (063–065). Считаем на общем `SupplyBalance` (070), новых систем-копий не заводим. Исполнитель: Engineer (субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: поведение роста/въезда, налоги, сохранения, сцену, ProjectSettings, NeedSystem/NeedsView (мусор в потребности — отдельная задача), файлы энергии и воды.
Сдать: коммиты в ветку task/071-waste + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `PowerController`/`WaterController` (по ~139 строк) собирают снимок из `Growth.Homes` и складывают его в систему; HUD-строки рисуют `PowerView` (y=246) и `WaterView` (y=278). Мусор — городская величина: спрос = сумма `WasteOutput` выросших зданий, предложение = `WasteCapacity` зданий + городская база `BaseCapacity`. Доли покрытия и порог 0.999 уже реализованы в `SupplyBalance` (070) — использовать его напрямую, обёртку `WasteSystem` НЕ создавать.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Infrastructure/WaterController.cs, Infrastructure/WaterView.cs, Core/SupplyBalance.cs, Construction/BuildingDefinition.cs, Data/Buildings/House.asset, Shop.asset, Factory.asset, Tests/Infrastructure/SupplyBalanceTests.cs. Остальное — grep.
- [ ] Шаг 2: `BuildingDefinition`: новые поля `WasteOutput` (default 1, «сколько мусора производит здание») и `WasteCapacity` (default 0, «сколько мусора утилизирует») в секции Infrastructure, с Tooltip и клампом ≥0. Клампы всех числовых полей вынести в один приватный хелпер `Clamp(int value, string field)` (кладёт в 0 и пишет одно LogWarning с именем поля), который опирается на `public static int ClampNonNegative(int value)` — чистая часть без логирования, её и тестируем. Так `OnValidate` становится ≤30 строк — это снимает старый долг (было ~45 строк).
- [ ] Шаг 3: данные — `Shop.asset` → `WasteOutput: 2`; `Factory.asset` → `WasteOutput: 3`. Дом остаётся на дефолте 1 (в YAML не пишем). Значения пишем только явно отличающиеся от дефолта.
- [ ] Шаг 4: `Infrastructure/WasteController.cs` (~110): поле `Growth` (обязательно, иначе LogError + `enabled=false`), `BaseCapacity = 12` (кламп ≥0), `RefreshIntervalSec = 2` (кламп ≥0.5), `SupplyBalance balance`, ленивый `cityGridId`; `Demand`, `Supply` (int, округление), `Rate01`; `RefreshNow()` (Clear + база + обход `Growth.Homes`, `WasteOutput > 0` → SetDemand, `WasteCapacity > 0` → SetSupply); `Update` по таймеру; Header/Tooltip/null-гарды; геттеры не бросают без Growth.
- [ ] Шаг 5: `Infrastructure/WasteView.cs` (~30): `Controller` (опционален, фолбэк GetComponent), строка `Мусор: D/S (R%)` на `Rect(10, 310, 400, 30)` — ниже воды (278+30+2), без наложений; без контроллера молча не рисует.
- [ ] Шаг 6: `Tests/Infrastructure/WasteTests.cs` (~50, только NUnit): `BuildingDefinition.ClampNonNegative` (отрицательное → 0, `int.MinValue` → 0, 0 → 0, 7 → 7) + дефолты нового экземпляра через `ScriptableObject.CreateInstance<BuildingDefinition>()` (WasteOutput == 1, WasteCapacity == 0). Без сцены и без Play.
- [ ] Шаг 7: `.meta` новым файлам (свежие GUID, проверить коллизии). Локально прогнать WasteTests + SupplyBalanceTests через dotnet/csc во временной папке (без Unity) и приложить результат. Самопроверка, отчёт, commit message.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2, сложных generics, reflection, async.
КОММИТЫ: ≤5 файлов и ≤250 строк каждый (данные и код — разными коммитами). Брейкдаун коммитится планом заранее.
ЧУЖОЕ: в рабочей копии есть незакоммиченные Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать и не коммитить.

Ручной план приёмки (оркестратор):
1. Refresh на стопе → Console без ошибок; в Core/Infrastructure видны WasteController/WasteView с .meta, GUID не дублируются.
2. Сцена: на объект Grid добавить WasteController (Growth = Grid) и WasteView (Controller = этот WasteController); сохранить.
3. Play: зоны Residential 8x8, растут дома → HUD «Мусор: N/12 (R%)», число растёт с домами; поставить магазин/фабрику → спрос растёт быстрее (2/3 на здание); при базе 12 и 16 домах ожидаем 16/12 → 75%. Скриншот.
4. Stop → Console чистый → run_tests EditMode: 145 + новые.
5. Мерж --no-ff, очередь, push origin main.
