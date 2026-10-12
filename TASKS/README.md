# TASKS — очередь

Формат: `- [ ] ToDo / - [x] InProgress / - [x] Done + инженер + PR`.

- [x] Done 001-project-foundation (Orchestrator) + branch task/001-project-foundation
- [x] Done 002-domain-primitives + merged
- [x] Done 003-simulation-clock + merged
- [x] Done 004-grid-system + merged
- [x] Done 005-grid-visualization + merged
- [x] Done 006-building-definition + merged
- [x] Done 007-building-placement + merged (Muse, Kimi на паузе)
- [x] Done 008-basic-treasury + merged — СРЕЗ ЗАКРЫТ
- [x] Done 009-road-foundation + merged
- [x] Done 010-road-placement + merged (исключение по лимиту зафиксировано)
- [x] Done 011-zone-foundation + merged
- [x] Done 012-zone-painting + merged
- [x] Done 013-settlement + merged (Muse: BuildingDefinition AllowedZone/Capacity/Jobs + Population/SettlementSystem, dotnet 32/32, Unity Console чистый)
- [x] Done 014-settlement-growth + merged (Muse: Zones-геттер + SettlementView + SettlementGrowthController; E2E в Unity: рост ×3, снос, Console чистый. Этап 5 ЗАКРЫТ)
- [x] Done 015-road-continue + merged (Muse: IsOwnRoadCell/NewCellsCount, плата только за новые; E2E: 3 клетки −30, продолжение −20, чужое блокирует, Console чистый)
- [x] Done 016-household-foundation + merged (Muse: HouseholdState + PopulationSystem MoveIn/MoveOut/EvictHome, dotnet 30/30, Unity 6/6, Console чистый)
- [x] Done 017-population-panel + merged (Muse: PopulationController автовъезд + PopulationView + Homes/EvictHome в росте; E2E: 8/8 и 4 hh, снос выселил 4/2, Console чистый)
- [x] Done 018-free-camera + merged (Muse: FreeCameraController WASD/колесо/QE/ПКМ, 147 строк; E2E: pan/yaw/zoom/клампы сошлись; компонент добавлен на MainCamera в сцену, Console чистый)
- [x] Done 019-mode-menu + merged (Muse: BuildModeMenu Выкл/Дома/Дороги/Зоны/Рост + OnDisable-чистка road/building; E2E + скриншот: кнопки видны, эксклюзивность сходится, Console чистый; компонент добавлен на Grid в сцену)
- [x] Done 020-employment-foundation + merged (Muse: EmploymentSystem Employed/Unemployed/rates без состояния, dotnet 21/21, Unity живой стек 6/2/0.75/1.0, Console чистый)
- [x] Done 021-camera-zones-polish + merged (Muse: скорости 30/40, DragSpeed 240, Shift ×3, inset-квадраты зон + глухие цвета; E2E: inset 2.15–2.85, Console чистый; скорости 30/40 прописаны в сцену)
- [x] Done 022-houses-look + merged (Muse + фикс крыши оркестратором: контр-scale; швы 0.9, высоты 1/1.5/2, крыши; E2E: геометрия точная, Console чистый)
- [x] Done 023-taxes + merged (Muse: TaxSystem Collect + EconomyTicker + геттер Settlement; E2E: 6×2=+12, Collect(0)=Fail, тикер на пустом тихо; объект Economy в сцене, Console чистый)
- [x] Done 024-workplaces-growth + merged (Muse: Definitions + подбор по зоне, 167 строк — исключение из лимита; E2E: house+shop выросли, jobs 6, налог +4; Shop/Factory-ассеты в Unity, Definitions=[H,C,I] в сцене, Console чистый)
- [x] Done 025-grid-lines-mesh + merged (Muse: меш Lines вместо LineRenderer; пользователь подтвердил — диагоналей нет)
- [x] Done 026-zone-brush-menu + merged (Muse: SetBrush + ряд Жилая/Коммерция/Промзона в режиме Зоны, 148/150 строк; E2E: 3 типа + None-игнор, Console чистый)
- [x] Done 027-workplace-colors + merged (Muse: WallColor по зоне — дома бежевые, магазины синие, заводы серые; E2E: цвета точные, Console чистый)
- [x] Done 028-clock-runner + merged (Muse: SimulationRunner 61 строка; E2E: Playing 3 тика, пауза 0, x4 12 тиков, Console чистый; объект Simulation в сцене)
- [x] Done 029-clock-view + merged (Muse: ClockView 30 строк, формат проверен в Unity: Day/Hour/Пауза/Игра/x4; в сцену добавить после Stop)
- NEXT: 017 (панель населения, автовъезд) — Этап 6 продолжается

Правила: 1 задача = 1 breakdown-файл TASKS/00X-breakdown.md, 1 ветка, 1 PR, 1 модуль. Две задачи параллельно запрещены.
