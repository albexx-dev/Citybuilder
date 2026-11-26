# Changelog

All notable changes to this project are documented here. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

## [v1.4.2] - 2026-11-26

Патч по скриншоту игрока: визуал депо/оверлеев и звук.

### Fixed

- Огромный ящик и «дома-контейнеры»: `WasteKenneyA.prefab` child scale 2.25 → 1, `RecyclingDepotKenney` баки 1 → 0.55, `CompostStation` перенаправлен на `WasteCompostKenney` с маленькими баками; все депо вписываются в 2×2.
- Зелёные высокие блоки над домами: `GarbageOverlayPolicy.Load01` клампится сверху (бак ≤0.63 м при любом накоплении).
- Надоедливый звук: дефолтный кулдаун событий 2.5 → 10 с, громкости Supply/Congestion/Garbage/Employment снижены до 0.1–0.12, питч Garbage 1.6 → 1.05. Монетка по-прежнему только на наградах целей.

## [v1.4.1] - 2026-11-25

Патч по отзыву владельца: деньги тратятся слишком быстро.

### Fixed

- Содержание списывается в 8 раз медленнее: `EffectiveUpkeepPolicy.Apply` делит базовую сумму нацело в единственной точке списания (`EconomyTicker.cs:119-121`). Для сценария 236 $/ч фактический расход стал 29 $/ч; HUD показывает эффективную цифру без двойного деления.
- Базовые `UpkeepPerHour` в ассетах, `UpkeepPolicy`, налоги, `EconomyMath`, кражи, рост и ручная стройка не изменены; при базе < 8 списание 0 (зафиксировано в тестах и GAME_DESIGN §20).

## [v1.4.0] - 2026-11-25

Версия «глубина и качество жизни» — 20 задач (#111–#130). 1145 EditMode-тестов в 169 файлах (937 `[Test]` + 208 `[TestCase]`), 2 PlayMode-теста.

### Added

- **ADR-016** и история метрик: пожары и кражи в графиках, диапазон 8/30 дней с децимацией, переключатель масштаба.
- Тумблеры оверлеев (покрытие/пробки/сетка) и мусоровоза в Настройках; переименование слотов, экспорт/импорт слота через формат v1–v6.
- Зум мини-карты, дистанционно/frustum отсечение машин, анимированное пламя с дымом.
- Доступность: масштаб шрифта HUD и контрастная подложка; полная клавиатурная навигация, J/O хоткеи, единый арбитр Esc.
- Звуки Supply/Congestion/Employment; воспроизводимый генератор WAV (3 клипа бит-в-бит, 5 честно помеченных аппроксимаций); Recycling Depot и 3 новых вида ручного декора.

### Changed

- EN-переводы доведены до автотеста «нет кириллицы в EN»; оформление метрик и подсказок через реестр.

### Fixed

- 36 файлов >150 строк и 15 методов >30 разбиты на `partial`-части/хелперы — 0 WARN валидатора по лимитам (осталось 5 стилевых про enum+class).
- PlayMode выявил и устранил два реальных бага: `AddComponent` тикера до привязки `Growth` и `Shader.Find` в статическом инициализаторе `ConstructionSiteView`.

## [v1.3.3] - 2026-11-21

Финальная полировка: подписи нижней панели «Дороги [2]», «Зоны [3]», «Рост [4]», «Декор [5]», «Здания [1]» больше не переносятся на две строки в узком окне (шрифт 10, `wordWrap=false`, плотные отступы); комментарии туториала обновлены под «Здания [1]». 1057 EditMode-тестов.

## [v1.3.2] - 2026-11-21

Правки по отзывам игрока после v1.3.1.

### Fixed

- Скрыт визуал мусоровоза (`GarbageTruckView.ShowTruckVisuals = false`): домен вывоза, оверлей переполнения и HUD «Вывоз мусора» продолжают работать; спавн обычных авто от дорог не изменён.
- Компостная станция теперь использует эталонный Kenney-префаб `WasteKenneyA` (2×2) вместо огромного ящика с трубами.
- Пункт меню и вкладка палитры «Дома [1]» переименованы в «Здания [1]» (RU) / «Buildings [1]» (EN); обновлены тултипы, квесты и шаги туториала.
- Разведены ярусы верхнего левого HUD (`HudLayout.TopLeftRowY`): деньги 10, население 36, бюджет 62, часы 88, кнопка RU/EN 114 (шаг 26 ≥ подложка 22 + запас 2). Строки больше не накладываются.

## [v1.3.1] - 2026-11-21

Патч после масштабного ревью v1.3.0: 9 исправлений по 10 независимым ревьюерам, 1053 EditMode-теста.

### Fixed

- **HIGH — сцена:** подключены `MetricsRecorder` (история метрик для графиков), `FireOverlayView` и `BlackoutViewController`; графики и визуал пожаров/блэкаутов теперь работают. In-game проверено: история метрик растёт 1→36 за 8 с на ×4.
- **HIGH — Esc:** единый `UiEscapeGuard` по `Time.frameCount` устраняет гонку пяти независимых обработчиков (настройки/слоты/инфопанель/пауза).
- **HIGH — детерминизм:** временные ключи покрытия больше не сжигают глобальный `EntityId`; пересчёт покрытия не меняет вход FNV-хешей пожаров/краж/мусора.
- **HIGH — dokи и ADR:** 1057 теста / 138 файлов, снимок v6, имена слотов, High=4, v1.3 в SECURITY, ADR-005/007/011 обновлены; ADR-015 фиксирует ограниченное исключение для `AudioController`.
- **Сейв v6:** недостроенные площадки с 0 тиков и ручные постройки восстанавливаются; реестр площадок очищается при загрузке; сбрасываются агенты машин и звуковые кулдауны.
- **Экономика/квесты:** запись дефицита казны больше не зависит от звукового кулдауна; вехи после упадка города выдаются заново; дубли active-id в слоте не дают двойную награду.
- **Локализация UI:** туториал, мини-карта, инфоменю и метрики переведены на EN; `InfoMenuView` обновляет кеш на смене языка; компактный баннер не перекрывает мини-карту.
- **Презентация:** устранены утечки Material в Settlement/Construction/Fire/Blackout, кэш депо обновляется по ревизии `Placed` даже на паузе.
- **Тесты и проверки:** изоляция глобального языка, ключи событий из каталога, проверки RU+EN; 15 новых тестов, мутационные проверки на старом коде.

## [v1.3.0] - 2026-11-21

Полная линия v1.3: локализация, видимость систем, сейв v6 и стабильность. 20 задач (#86–#106), 1038 EditMode-тестов.

### Added

- **ADR-013 и локализация ленты (#86–#88, #104):** запись ленты хранит `kind + messageKey + args + tick`; дедуп не зависит от языка; форматирование переехало в view; звук выбирается по подтипу события, а не по тексту. Добавлены сигналы Garbage/Blackout; `coin.wav` звучит только на наградах ступеней цели.
- **Локализация каталога и UI (#89, #100):** имена 11 зданий и 4 видов декора приходят из ассетов через ключи; RU/EN переведены нижняя панель и тултипы, тексты квестов и генератор вех, настройки, инфоменю, инспекция и статус роста (`HousesWord` для 1/2/5/11/21). Реестр разнесён по таблицам, всего 190 ключей без дублей.
- **ADR-014 и сейв v6 (#93, #106):** опциональные `GarbageQueue`, `TheftCounter`, `ComfortStreak`, `ConstructionProgress` с fallback на v5, валидацией и дедупом по клетке; обратная совместимость v1–v5; пожары и агенты остаются несериализуемыми (ADR-009/011).
- **Ангарные варианты (#90):** компостная станция с ёмкостью 20, детерминированный выбор депо по `WasteCapacity` вместо хардкода Id; 6 типов зданий подключены к готовым Kenney-моделям (#103).
- **Видимость систем (#94, #97):** оверлей переполнения мусором и маркер уровня дома; события-подсказки туториала (пожар, награда, вывоз, ночь), кнопка «Повторить обучение» и справка управления.
- **Прозрачный бюджет (#96):** в HUD разложена строка «налоги − содержание − кражи + награды = итог» (баланс не менялся).

### Changed

- **Детерминизм и загрузка (#91):** пешеходы используют FNV-1a вместо `System.Random`; при «Новой игре»/загрузке сбрасываются агенты, пожары, кражи и счётчик `EntityId`.
- **Снимок покрытия (#92):** пересчитывается в такте (`TickStarted`) до пожаров/краж/квестов, а не раз в 2 с реального времени; добавлен идемпотентный биндинг подписки.
- **Настройки и декор (#95):** тумблер «Декор» скрывает и ручной декор; эмбиент применяется одним путём без per-frame чтения профиля; мёртвый авто-спавн декора убран.
- **Малое окно (#98, #99):** мини-карта прячется при открытых «Задачах»/«Настройках», единый гард Tab/L и приоритет Esc; панели клампятся по экрану, меню переходят в два ряда, туториал показывает компактный баннер.
- **Документация и валидатор (#101, #102):** восстановлена истина про `TASKS/` (32 файла отслеживаются); числа тестов и LFS-объектов сведены к факту; `ValidateRepo.py` проверяет лимиты методов/типов, версии и запрещённые API без жёсткого `1.2`.

### Fixed

- Реальные ошибки интеграции, пойманные гейтом: подписка `TreasuryView` под новую сигнатуру `Action<CityEvent>` (`dd604cc`); равенство `CityEvent` по значениям `Args` и непустой ключ «нет события» (`41f8fb1`); дедуп v6 для последовательности A,B,A (`38e7507`); вызов реального тика в событиях снабжения (`cd93efc`). Фантомно-зелёные прогоны `run_tests` при упавшей компиляции больше не принимаются — `read_console errors` обязателен.

## [v1.2.2] - 2026-11-21

Follow-up wave after v1.2.0/1.2.1 owner feedback — all remaining epic tails.

### Added

- **Quests persist (tail #48/#64):** active/completed quest ids in snapshot v5 — chains continue after load; unknown/milestone ids skip silently, broken slots can not double-reward.
- **Garbage trucks route over player roads (tail #52):** route ends map to nearest road cells (review fix), direct L-route stays as fallback when roads are absent.
- **Settings apply step 3 (EPIC-69):** decor toggle hides/shows decor instantly and disables the Decor build mode; autosave interval follows the settings stepper.
- **Event sounds (tail #71):** procedural fire-alarm and theft-dissonance clips played from the city event log with per-kind cooldown; **ambient volume slider** in settings (persisted).
- **Tooltip hover delay 0.4 s** (tail #66) with pure-logic policy tests.
- **RU/EN menus (tail #72):** pause/main/save/load menus and zone brushes through the localization registry (27 new keys, RU outputs byte-identical).

### Fixed

- Quest rewards fire after state mutations; `QuestTicker.lastTick` reset on load (stale-tick rewind guard).
- Issue #42 closed: utility stations excluded from service coverage denominators (confirmed intended), menu speed restore, congestion count symmetry, single hotkey guard.

## [v1.2.1] - 2026-11-21

Patch after v1.2.0 owner feedback (giant cars, spawn flood, backwards driving, fast economy drain).

### Fixed

- **Vehicles spawn only when roads are built:** one new road cell — one vehicle (if a home is connected to a job via roads and the limit allows); time windows removed.
- **Vehicle scale auto-fits road cells** (~1.6 cells long instead of ~6.7) and cars face the driving direction (180° yaw).
- **Vehicle limit 12** with the settings slider actually applying live (8–120, default 12).
- **Red occupied-cell overlays removed** from under buildings and roads (debug grid layer).
- **Softer early economy:** thefts less frequent (0.0005) and fine 20$ instead of 50$.

## [v1.2.0] - 2026-11-21

Complete release of the **v1.2 «Живой город»** product milestone (Epics #43–#48, #50–#53, #58–#68, #71, #72 + owner specs).

### Added

- **Deterministic fires (EPIC-43, ADR-009):** uncovered homes can ignite via FNV-1a hash(tick, EntityId); fire stations extinguish in one tick, uncovered homes burn down after 6 ticks (cells freed, residents evicted); flame overlay; «Пожарные 0%» warning long before the first fire.
- **Crime & police (EPIC-47):** homes below police coverage can be robbed (salt-separated hash), treasury fine and temporary leisure penalty per theft, crime line in the info menu.
- **Quest milestones (EPIC-48, ADR-012):** 10 starter quests + endless milestone generator (residents +50, balance doubling, full service), 3 active quests with domain-computed progress lines, rewards to treasury, «Задачи» panel.
- **Garbage trucks (EPIC-52):** homes accumulate waste (deterministic FNV-1a gains, salt 0x5A), depot dispatches a 2-truck fleet on depot → homes → depot routes (≤4 homes per run), pickup events in the log, «Вывоз мусора» HUD line; Kenney truck models in the scene.
- **Day/night cycle (EPIC-58):** sun/moon/ambient from HourOfDay with 6 key points (0/6/9/12/18/22).
- **House levels (EPIC-59):** levels 1–3 with comfort hysteresis, saved in snapshot v5.
- **Construction sites for all buildings (EPIC-61):** universal scaffolding sites with per-type durations.
- **Minimap (EPIC-67):** schematic 140px minimap (zones/roads/buildings, Kenney color parity), 0.5s repaint throttle, click-to-focus camera teleport.
- **Info-menu graphs (EPIC-68):** population/treasury/needs/power graphs for the last 8 game days (GraphMath decimation with safe axes).
- **Settings window (EPIC-69):** volume/mute, day-night toggle, agent limit, autosave interval, decor toggle, defaults — persisted via GameSettings profile (PlayerPrefs), applied live to AudioController and DayNightController.
- **Ambient day/night audio (EPIC-71):** procedural 24s seamless loops (birds by day, crickets at night), crossfade by HourOfDay scaled by city activity.
- **RU/EN localization (EPIC-72):** Localization registry (19 keys), RU/EN toggle button in the HUD, persisted choice.
- **UI icons & tooltips (EPIC-66):** 16 CC0 Kenney icons on all bottom-bar buttons and zone brushes, hover tooltips, StreamingAssets delivery (works in editor and windows64 build).

### Changed

- **Balance pass v1.2 (EPIC-53):** real upkeep for industry and services (Factory 7, Shop 5, Hospital 12, Fire/Police 6, Park 3), stations hire 3; zonal tax ladder 2/4/6 → **1/2/4** (default Normal 2/2/2). Equilibrium: Normal +164$/h, High +564$/h, Low −36$/h — first service reachable, full coverage is a budget decision.
- **Save v5 (EPIC-64):** building levels and three per-zone tax rates survive save/load with validation.
- **Transport polish (EPIC-44/45/46):** vehicle agents with morning/evening spawn windows, congestion load recoloring, pedestrian crowd.
- **Zonal taxes & service upgrades (EPIC-49/51):** three-zone ladders with per-zone jobs distribution; service stations upgrade levels (base+level radius).

### Fixed

- Quest rewards fire after state mutations (subscriber Reset-safety), panel hotkey exclusivity for Tab/L across all right-side windows, InfoMenu cachedLines sizing.
- Color assertions tolerate RGBA32 quantization; IMGUI-free test seams (MinimapView.ProcessClick/Hide, MetricsGraphView.Advance/GraphWidth, clockOverride) — no GUI calls outside OnGUI in tests.

## [v1.1.0] - 2026-11-09


Complete release of the **v1.1** product milestone (Epics #1–#6, #8–#21).

### Added

- **Bottom Action Bar & Info Menu (EPIC-13, EPIC-14):** Replaced legacy top-left button stacks with a unified bottom bar (`BottomBarView`, `BottomBarStyle`, `HudLayout`) and a collapsible statistics window (`InfoMenuView`, toggled via `Статистика` or `Tab`).
- **Generalized Services v2 & Hospital / Park (EPIC-01, EPIC-02, ADR-006):** Unified service coverage domain (`ServiceSource`, `ServiceBuildingSystem`, `ServiceCoverageController`) supporting `Fire`, `Police`, `Health`, and `Leisure`, plus `Hospital.asset` (`3×3`, radius 7) and `Park.asset` (`2×2`, radius 5).
- **Needs v2 (EPIC-03):** Added `HealthNeedProvider` and `LeisureNeedProvider` (6 providers total), expanded `NeedSystem` to 9 indicators, and gated household move-in when overall satisfaction drops below `50%`.
- **Building Upkeep & City Attractiveness (EPIC-04, EPIC-05):** Added `UpkeepPerHour` to `BuildingDefinition`, `UpkeepPolicy`, `UpkeepStatusPolicy`, `UpkeepView`, negative treasury deficit support (`Treasury.Charge`/`IsInDeficit`), and `AttractivenessPolicy` (`0.5×..1.5×` migration interval multiplier shown in `InfoMenuView` and `GrowthStatusView`).
- **City Event Log (EPIC-09, ADR-007):** Added `CityEventLog` (ring buffer of 50 events with 10-tick deduplication), `EventLogView` (`Лента` button or `L`), and 4 event sources (supply deficit, road congestion, treasury deficit, high unemployment).
- **Building Inspection Panel & Radius Overlay (EPIC-12):** Added `BuildingInfoQuery`, `InfoPanelView`, and `GridVisualizer.ShowRadiusOverlay` to inspect any grown or manual building and highlight its service coverage radius on the grid.
- **Visible Road Congestion (EPIC-15):** Added `CongestionSystem.Load01`, per-tick road heat recoloring in `RoadView` (green `< 0.33`, yellow `0.33..0.66`, red `≥ 0.66`), and congestion summary line in `InfoMenuView`.
- **Saves v2 — 3 Slots & Autosave (EPIC-11, ADR-008):** Added `SaveSlotIndex` (`slots.json`), 3 manual save slots (`slot_0..2.json`), `AutosaveController` (every 24 ticks into `autosave.json`), `SaveMenuView`, and `LoadMenuView`.
- **Main Menu & Pause Menu (EPIC-16):** Reworked `MainMenu` as a centered overlay (`Продолжить`, `Новая игра`, `Загрузить`, `Выход`) and added `PauseMenuView` (`Esc`) with unsaved-changes exit confirmation.
- **Building Growth Stages & Variants (EPIC-17):** Added 3-tick construction scaffolding (`GrowthStageView`) with smooth scale interpolation and 3 deterministic visual variants for houses (`BuildingDefinition.SelectVariant`).
- **Interactive Tutorial v2 (EPIC-18):** Replaced `StartHintPolicy`/`StartHintView` with a 7-step `TutorialPolicy` and `TutorialView` featuring active button highlights and a `Понятно` dismiss button.
- **Blender Pipeline & 3D Models (EPIC-20):** Added `Tools/blender-addon/citybuilder_export.py` and `generate_models.py`, exported deterministic low-poly `hospital.fbx` and `park.fbx` models into `Assets/Citybuilder/Models/`, and wired them to `Hospital.asset` and `Park.asset`.
- **UI Audio Subsystem (EPIC-19):** Added `AudioController`, 4 procedural CC0 WAV clips (`click.wav`, `coin.wav`, `pop.wav`, `swoosh.wav`), and volume/mute controls backed by `PlayerPrefs` in `PauseMenuView`.

### Changed

- **Balance Pass (EPIC-06):** Tuned all 10 building definitions in `Assets/Citybuilder/Data/Buildings/`, raised `MaxSettlements` from 30 to 80 so the 200-resident milestone is reachable, and documented the full balance table in `GAME_DESIGN.md §20`.
- **File Splits (EPIC-10):** Extracted `RoadDragInput` from `RoadPlacementController` and removed unused `Assets/Citybuilder/Scripts/Editor/` folder so all 135+ C# scripts stay strictly under the 200-line hard limit (`ValidateRepo.py` warnings reduced to 10).

### Removed

- **Win/Lose Outcomes (EPIC-08):** Removed `Won`, `Lost`, `LoseDay`, and `LoseTicks` from `GoalSystem` in accordance with the endless-game doctrine (`GAME_DESIGN.md §19`).

## [v1.1] - 2026-11-08

Starting point for the v1.1 line. Built on tasks 001–104, HEAD before this change `7bf10ac`.

### Removed

- `HANDOFF.md`: obsolete handoff document. Entry points at the time were `README.md`, `AGENTS.md`, `CHANGELOG.md` and `DECISIONS/`.
- `com.unity.multiplayer.center`: unused direct dependency removed from `Packages/manifest.json` and `Packages/packages-lock.json`.

### Changed

- Documentation aligned with the actual state after task 104: tax level is saved in `CitySnapshot.TaxLevel` and restored by `LoadController`; the ADR-004 freeze was lifted after task 047; win/lose screens were removed by task 049; `GoalSystem.Won/Lost` is explicitly retained as a basis for a later version.
- `ARCHITECTURE.md` no longer lists non-existent `GameBootstrap` / `GameState` / command-event classes; real events are `SimulationClock.TickStarted/TickCompleted` and `Treasury.BalanceChanged`.
- `GAME_DESIGN.md §18` rewritten from "frozen minimal release" to the v1.1 scope. See `DECISIONS/ADR-005-v1.1-scope.md`.
- `ProjectSettings/ProjectSettings.asset`: `bundleVersion` set to `1.1`.
- README, MCP setup docs and launcher scripts no longer hardcode `C:\Users\admin\Citybuilder`; paths are derived from the repository or environment variables.
- `SaveSlotStorage.Validate` now rejects out-of-map coordinates, invalid tax levels and invalid goal stages.
- Goal-stage rewards moved out of `GoalView.OnGUI()` into a plain C# `GoalRewardController` called from `Update()`.
- Bottom HUD lines clamp to small window heights instead of using fixed y=406/430/454 unconditionally.
- `.gitattributes` gained `* text=auto` for future line-ending normalization.

### Added

- `CONTRIBUTING.md`, `SECURITY.md`, `.github/CODEOWNERS`, `CHANGELOG.md`.
- `DECISIONS/ADR-005-v1.1-scope.md`.
- `Tools/mcp.example.json` with a portable MCP configuration template.
- `.github/workflows/ci.yml` and `Tools/ValidateRepo.py` for static CI checks; optional Unity EditMode job when a Unity license secret is configured.
- `Tests/Persistence/SaveValidationTests.cs` for the stronger snapshot validation.

### Verification baseline

- 288 existing EditMode tests in 39 files before new validation tests.
- Last documented Windows64 build: task 102, ~90 MiB, launch smoke passed.
- No PlayMode automated tests yet; gameplay scenarios are still checked manually.
