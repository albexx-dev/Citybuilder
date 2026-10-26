# TASKS/096-breakdown

Ввод: баг сохранений, найден в 095-ревью (HANDOFF §6). Слот хранит домохозяйство как `BuildingIndex` — позицию дома в списке выросших зданий. При загрузке `LoadController.ApplyBuildings` восстанавливает здания по порядку и **пропускает** те, что не встали (`RestoreGrown` → `null`, например зона под домом перекрашена), а `LoadSystem.ApplyHouseholds` продолжает брать индекс из слота. Индекс сдвигается: жители следующих домов либо пропадают (`index >= homes.Count`), либо вселяются в чужой дом. Это потеря жителей после F9 → пункт 7 чек-листа 093, не откладывается.

Исполнитель: Engineer (субагент). После — Reviewer (только чтение).
Запрещено: сцена, ProjectSettings, рост/въезд/потребности/налоги/цель, `LoadController` (остаётся 150 строк, не трогать), `SaveSlotStorage.Validate`, `Tools/Check-UnityMcp.ps1`, `UNITY_MCP_SETUP.md`.
Сдать: коммиты в ветку task/096-save-households + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

## Решение
- Новый слот пишет привязку **координатами** дома (стабильная ссылка), индекс остаётся для совместимости.
- Старые слоты (без координат) читаются по индексу как раньше.
- Формат остаётся v4: поля аддитивные, старый билд их игнорирует; `Validate` не меняется.

## Шаги
- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Persistence/CitySnapshot.cs, SaveSystem.cs (150 — начинать со сплита!), LoadSystem.cs, LoadController.cs (только чтение), SaveSlotStorage.cs (только чтение), Tests/LoadSystemTests.cs, Tests/SaveSystemTests.cs.
- [ ] Шаг 2: `CitySnapshot.HouseholdEntry` + `public bool HasHome; public int HomeX; public int HomeY;` с комментарием «v4: координаты дома; HasHome=false — старый слот, читать по индексу». `BuildingIndex` не удалять.
- [ ] Шаг 3: сплит `SaveSystem` — вынести запись домохозяйств в новый `Persistence/SnapshotHouseholds.cs` (~35): `public static void Write(CitySnapshot snapshot, List<HouseholdState> households, Dictionary<BuildingState, int> indexByHome)`; тела цикла из `AddHouseholds`; заполняет `BuildingIndex`, `HasHome = true`, `HomeX/HomeY` из `household.Home.Origin`. `SaveSystem` зовёт его, свой `AddHouseholds` удаляет; файл должен стать ≤140.
- [ ] Шаг 4: новый `Persistence/HouseholdBinder.cs` (~60, чистый C#): `public static void Apply(CitySnapshot snapshot, PopulationSystem population, List<BuildingState> homes)` — пропуск null/Size<=0; дом ищется через `FindHome`: при `HasHome` — по `Origin` (перебор, null-элементы пропускать), иначе по `BuildingIndex` с проверкой границ; не найден — домохозяйство пропускается, чужой дом не подставляется. `MoveIn` только для найденного.
- [ ] Шаг 5: `LoadSystem.ApplyHouseholds` — тело заменить вызовом `HouseholdBinder.Apply(snapshot, population, homes)`; сигнатура и поведение для null не меняются. Файл должен стать короче 130.
- [ ] Шаг 6: новый `Tests/Persistence/HouseholdBinderTests.cs` или `Tests/HouseholdBinderTests.cs` по принятой раскладке (~100):
  1. три дома A/B/C, жители в A(2) и C(3), слот; при загрузке B не восстановился (`homes = [A, C]`) → 5 жителей, 2 в A, 3 в C;
  2. старый слот (`HasHome = false`), `BuildingIndex = 0` → жители в A;
  3. `HasHome = true`, координаты дома отсутствуют в `homes` → 0 жителей, в чужой дом не вселяются;
  4. JSON-roundtrip через `SaveSlotStorage.ToJson/FromJson` сохраняет `HasHome/HomeX/HomeY`.
- [ ] Шаг 7: `.meta` новым файлам. Прогнать тесты через dotnet/csc, приложить результат. Commit messages.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммиты ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка (оркестратор): EditMode зелёные (270 + новые); Play: F5 → перекрасить/стереть зону под одним выросшим домом → F9: население совпадает с сохранённым минус жители снесённого дома, остальные живут в своих домах; старый slot0.json (если есть) читается; Console чистый.
