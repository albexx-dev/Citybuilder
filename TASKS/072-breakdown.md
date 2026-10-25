# TASKS/072-breakdown

Ввод: задача 072, модуль Core + Population + UI. Мусор (071) пока только цифра в HUD; энергия и вода уже входят в «Потребности» через провайдеры в `NeedSystem`. `NeedSystem` — 149 строк при жёстком пределе 150, шестой показатель туда не влезает. Поэтому сначала выносим провайдеров в `Core/NeedProviders.cs`, потом добавляем мусор шестым показателем. Поведение энергии и воды не меняется. Исполнитель: Engineer (субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: поведение роста/въезда, налоги, сохранения, сцену, ProjectSettings, файлы энергии/воды/мусора, старые тесты (NeedSystemTests, PowerNeedTests, WaterNeedTests).
Сдать: коммиты в ветку task/072-waste-needs + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `NeedSystem` держит `Func<float> powerRate/waterRate`, флаги `HasPower/HasWater`, ставки `PowerRate/WaterRate` и среднее `(3 базовых + подключённые) / count`. `NeedsView` собирает строку «Потребности: N% (жильё • работа • занятость • энергия • вода)» и передаёт делегаты `() => Power.Rate01`, `() => Water.Rate01`. `WasteController.Rate01` уже есть.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Population/NeedSystem.cs, UI/NeedsView.cs, Infrastructure/WasteController.cs, Infrastructure/WaterController.cs, Tests/Infrastructure/NeedSystemTests.cs, PowerNeedTests.cs, WaterNeedTests.cs. Остальное — grep.
- [ ] Шаг 2: новый `Core/NeedProviders.cs` (~85, чистый C#, без Unity): поля трёх `Func<float>`; конструкторы `()` → все null, `(power, water)`, `(power, water, waste)`; `HasPower/HasWater/HasWaste`; `PowerRate/WaterRate/WasteRate` (null → 1, иначе Clamp01, живое чтение делегата); `Sum` (сумма только подключённых) и `Count` (0..3). Методы ≤30.
- [ ] Шаг 3: `NeedSystem` — тонкая обёртка: поле `NeedProviders providers`; конструкторы `(pop, set)`, `(pop, set, power)`, `(pop, set, power, water)` делегируют в новый `(pop, set, providers)`; новый конструктор `(pop, set, power, water, waste)` → `new NeedProviders(power, water, waste)`. `HasPower/HasWater/HasWaste`, `PowerRate/WaterRate/WasteRate` проксируют в providers; `AverageSatisfaction = (Housing + Jobs + Employment + providers.Sum) / (3 + providers.Count)`. Поведение при обоих null — ровно `/3`. Файл должен ужаться до ~105 строк.
- [ ] Шаг 4: `NeedsView`: поле `Waste` (WasteController, Tooltip, фолбэк GetComponent), в `EnsureNeeds` третий делегат `() => Waste.Rate01`, в `BuildText` — `• мусор X%` после воды; ширина лейбла 760 → 820 (шесть показателей), y=114 не менять.
- [ ] Шаг 5: тесты. `Tests/Infrastructure/NeedProvidersTests.cs` (~85): без провайдеров (Sum 0, Count 0, все Has=false, Rate=1), только power (Count 1), power+water (Count 2), все три (Count 3), клампы 1.5→1 и −1→0, живое чтение (делегат меняет значение между вызовами), мусор 0.5/0.5/0.5. `Tests/Infrastructure/WasteNeedTests.cs` (~70): `NeedSystem` с 5 аргументами — среднее `(жильё+работа+занятость+энергия+вода+мусор)/6`, HasWaste true/false, вода 0 → показатель 0, старые конструкторы дают /3, /4, /5 (сверка с прежними числами). Старые тестовые файлы НЕ править.
- [ ] Шаг 6: `.meta` новым файлам (свежие GUID, проверить коллизии). Локально прогнать NeedProvidersTests + WasteNeedTests + NeedSystemTests + PowerNeedTests + WaterNeedTests через dotnet/csc во временной папке (без Unity) и приложить результат. Самопроверка, отчёт, commit message.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2, сложных generics, reflection, async.
КОММИТЫ: ≤5 файлов и ≤250 строк каждый.
ЧУЖОЕ: незакоммиченные Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать и не коммитить.

Ручной план приёмки (оркестратор):
1. Refresh на стопе → Console без ошибок.
2. Сцена: `NeedsView.Waste` = WasteController на Grid; сохранить.
3. Play: зоны Residential, дома растут → строка «Потребности: N% (жильё • работа • занятость • энергия • вода • мусор)» без обрезки; при воде/энергии 75% и мусоре 46% среднее падает против прежнего; без WasteController — пять показателей, как раньше. Скриншот.
4. Stop → Console чистый → run_tests EditMode: 150 + новые.
5. Мерж --no-ff, очередь, push origin main.
