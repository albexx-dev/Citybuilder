# TASKS/074-breakdown

Ввод: задача 074, модуль Core + Infrastructure (первый шаг служб §12). Нужен чистый расчёт покрытия: у источников (станции) есть позиция и радиус, у точек спроса (здания) — позиция; считаем, какие точки накрыты, и общую долю покрытия. Поведения игры не меняет, Unity не требует. Исполнитель: Engineer (субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: рост/въезд, потребности, энергию/воду/мусор, налоги, сохранения, сцену, ProjectSettings, существующие системы.
Сдать: коммит в ветку task/074-service-coverage + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `GridPosition` (X/Y, struct, Equals/GetHashCode есть), `EntityId` (struct, `IsValid`, `New()`). Чистые системы проекта — без Unity: см. `Core/SupplyBalance.cs` (словари + геттеры) и `Infrastructure/WaterSystem.cs` (тонкая обёртка). Радиус службы — в клетках, расстояние евклидово (сравнение по квадратам, без корня).

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Core/GridPosition.cs, Core/EntityId.cs, Core/SupplyBalance.cs, Tests/Infrastructure/SupplyBalanceTests.cs. Остальное — grep.
- [ ] Шаг 2: новый `Infrastructure/ServiceCoverage.cs` (~130, чистый C#, без Unity):
  - приватная структура `Source { GridPosition Position; int Radius; }`;
  - `SetSource(EntityId id, GridPosition position, int radius)` (радиус < 0 → 0), `RemoveSource(id)`, `ClearSources()`;
  - `SetPoint(EntityId id, GridPosition position)`, `RemovePoint(id)`, `Clear()`;
  - `SourceCount`, `PointCount`;
  - `IsCovered(GridPosition point)` — хотя бы один источник с `dx*dx + dy*dy <= radius*radius`;
  - `RateOf(EntityId pointId)` — точки нет → 0, накрыта → 1, иначе 0;
  - `CoveredPoints` (int) — сколько точек накрыто;
  - `Coverage01` — доля накрытых 0..1, точек нет → 1;
  - `NearestDistance(GridPosition point)` — минимальное евклидово расстояние до источника, округлённое вниз; источников нет → −1.
  Все методы ≤30 строк, без LINQ, без Unity, без событий.
- [ ] Шаг 3: тесты `Tests/Infrastructure/ServiceCoverageTests.cs` (~110, NUnit):
  - пусто: SourceCount 0, PointCount 0, Coverage01 == 1, NearestDistance == −1, RateOf неизвестной точки == 0;
  - радиус 0 накрывает только свою клетку;
  - граница: точка на расстоянии ровно radius накрыта, radius+1 — нет (проверить по горизонтали и по диагонали 3-4-5);
  - два источника: объединение покрытия, перекрытие не двоит `CoveredPoints`;
  - `RemoveSource`/`ClearSources` снимают покрытие, точки остаются;
  - `Coverage01`: 0 из 3 → 0, 3 из 3 → 1, 2 из 4 → 0.5;
  - отрицательный радиус → источник не накрывает соседнюю клетку;
  - `RemovePoint` уменьшает `PointCount` и меняет `Coverage01`.
- [ ] Шаг 4: `.meta` новым файлам (свежие GUID, проверить коллизии). Локально прогнать ServiceCoverageTests + SupplyBalanceTests через dotnet/csc во временной папке (без Unity) и приложить результат. Самопроверка, отчёт, commit message.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс (вложенная структура допустима); без LINQ>2, сложных generics, reflection, async.
КОММИТЫ: ≤5 файлов и ≤250 строк каждый.
ЧУЖОЕ: незакоммиченные Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать и не коммитить.

Ручной план приёмки (оркестратор):
1. Refresh на стопе → Console без ошибок; файл и .meta видны в Infrastructure.
2. Play: поведение не меняется (в сцене компонентов нет) — только проверка, что ошибок нет и HUD прежний. Скриншот не нужен.
3. Stop → run_tests EditMode: 167 + новые.
4. Мерж --no-ff, очередь, push origin main. Следующая задача 075 — пожарная станция и палитра зданий.
