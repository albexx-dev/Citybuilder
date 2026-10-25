# TASKS/075-breakdown

Ввод: задача 075, модуль Construction + UI (службы §12, второй шаг). Ядро покрытия готово (074). Теперь нужно, чтобы игрок мог поставить пожарную станцию: сегодня `BuildingPlacementController` умеет строить ровно один тип (`Definition`), и постройка/призрак жёстко завязаны на это поле. Добавляем палитру зданий и станцию с радиусом. Эффекта покрытия в этой задаче нет (076). Исполнитель: Engineer (субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: рост/въезд, потребности, энергию/воду/мусор, налоги, сохранения, сцену, ProjectSettings, ServiceCoverage.
Сдать: коммиты в ветку task/075-fire-station + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `BuildingPlacementController` (190 строк, жёсткий предел 200) держит `Definition`, `ghost`, материалы, `states`, `views`; `SpawnView` и `FootprintCenter` читают `Definition` вместо `state.Definition` — при смене типа это баг. `BuildModeMenu` рисует кнопки режимов справа (ширина 90, шаг 34, старт y=10) и ряд кисти зон при режиме Zones на y≈188; `UiClickGuard.Set(key, rect)` обязателен для любой кнопки.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Construction/BuildingPlacementController.cs, Construction/BuildingDefinition.cs, Construction/BuildingState.cs, Construction/BuildingSystem.cs, UI/BuildModeMenu.cs, UI/UiClickGuard.cs, World/ZoneType.cs, Data/Buildings/House.asset, Data/Buildings/House.asset.meta. Остальное — grep.
- [ ] Шаг 2: `BuildingDefinition`: поле `ServiceRadius` (default 0, Tooltip «Радиус службы в клетках. 0 — не служба»), кламп ≥0 через существующий `Clamp`.
- [ ] Шаг 3: новый ассет `Data/Buildings/FireStation.asset` (+.meta, свежий GUID): `Id: fire_station`, `DisplayName: Fire Station`, `Footprint 2x2`, `ConstructionCost: 300`, `AllowedZone: 2` (Commercial), `ResidentCapacity: 0`, `JobsProvided: 2`, `ServiceRadius: 6`; писать в YAML только отличия от дефолтов. Дом не трогать.
- [ ] Шаг 4: вынести презентацию из контроллера в новый `Construction/BuildingViewFactory.cs` (~75): `CreateView(BuildingState state, GridVisualizer grid)` (позиция и масштаб из `state.Definition`!), `CreateGhost(BuildingDefinition def, GridVisualizer grid, Transform parent)`, `CreateGhostMaterial(Color color)` (шейдер `Unlit/Color`). `BuildingPlacementController` использует фабрику и должен ужаться до ~165 строк.
- [ ] Шаг 5: `BuildingPlacementController`:
  - `public void SetDefinition(BuildingDefinition def)` — меняет тип, уничтожает старый призрак (иначе габарит останется прежним) и прячет его;
  - `public bool TryPlaceAt(GridPosition cell)` — публичная обёртка над размещением (для E2E и будущих сохранений), списание денег как раньше;
  - `public List<BuildingState> Placed` — копия списка построенного (нужна следующей задаче);
  - `FootprintCenter` принимает `BuildingDefinition`, `SpawnView` берёт `state.Definition`. Поведение при одном типе — как раньше.
- [ ] Шаг 6: новый `UI/BuildPaletteView.cs` (~75): поля `Placement` (BuildingPlacementController), `Menu` (BuildModeMenu), `Definitions` (List<BuildingDefinition>); рисует кнопки только когда `Menu.CurrentMode == Mode.Houses` (иначе ряд кисти зон на y≈188 пересечётся): `x = Screen.width - 90 - 10`, `y = 188 + i * 34`, ширина 90, высота 30; текст «Дом 100» / «Пожарная 300» (DisplayName + цена); активный тип подсвечен жёлтым (как в `BuildModeMenu`); клик → `Placement.SetDefinition(def)`; `UiClickGuard.Set("palette", общий rect)`; пустой список — молча не рисует.
- [ ] Шаг 7: `.meta` новым файлам (свежие GUID, проверить коллизии). Тест `Tests/Construction/ServiceRadiusDefinitionTests.cs` (~40): новый экземпляр `BuildingDefinition` имеет `ServiceRadius == 0`; присвоение 6 сохраняется; `ClampNonNegative(-3) == 0`. Локально прогнать этот тест + `SupplyBalanceTests` через dotnet/csc во временной папке (без Unity) и приложить результат. Самопроверка, отчёт, commit message.

ЛИМИТЫ: файл ≤150 (жёстко 200, `BuildingPlacementController` обязан быть ≤200), метод ≤30, 1 файл = 1 класс; без LINQ>2, сложных generics, reflection, async.
КОММИТЫ: ≤5 файлов и ≤250 строк каждый (данные/фабрика/палитра — разными коммитами).
ЧУЖОЕ: незакоммиченные Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать и не коммитить.

Ручной план приёмки (оркестратор):
1. Refresh на стопе → Console без ошибок.
2. Сцена: на Grid добавить `BuildPaletteView` (Placement = BuildingPlacementController на Grid, Menu = BuildModeMenu, Definitions = House + FireStation); сохранить.
3. Play: режим «Дома [1]» → справа под кнопками появляется палитра «House 100» / «Fire Station 300»; выбор пожарной → призрак 2x2; клик по сетке ставит станцию, деньги списываются на 300; снос ПКМ работает; после выбора дома призрак снова 2x2 и постройка стоит 100. Скриншот.
4. Stop → Console чистый → run_tests EditMode: 175 + новые.
5. Мерж --no-ff, очередь, push origin main. Следующая 076 — покрытие станций в HUD/эффект.
