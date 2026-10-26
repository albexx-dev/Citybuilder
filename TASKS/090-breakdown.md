# TASKS/090-breakdown

Ввод: задача 090, модуль Persistence + UI (долг 087). Ступени цели (50/100/200, награды 1000/2000/4000) живут только в памяти `GoalView`, в слот не пишутся. После загрузки сцена сбрасывается на ступень 0, и награда за уже пройденные ступени начисляется повторно. Пишем ступень в снапшот (Version 4, совместимость с v1–v3) и восстанавливаем при загрузке. Исполнитель: Engineer (субагент). После — Reviewer (только чтение).
Запрещено: `GoalSystem`, рост/въезд, потребности, инфраструктура, налоги, сцена, ProjectSettings, `GoalStagesTests`/`GoalSystemTests`.
Сдать: коммиты в ветку task/090-goal-save + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `GoalStages` (61) — `Stage/Target/AllDone/Progress01/Claim`, состояние приватное; `GoalView` (79) держит `readonly GoalStages stages`, клеймит в `OnGUI` только при найденной казне; `CitySnapshot` (53) Version 3 + ManualBuildings; `SaveSystem` (140) CurrentVersion 3, перегрузки 6/7/8 арг; `SaveSlotStorage.Validate` принимает `1..SaveSystem.CurrentVersion`; `SaveController` (150), `LoadController` (149). Тесты версии: `TaxSaveTests` (85), `Persistence/ManualSaveTests` (121).

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Population/GoalStages.cs, UI/GoalView.cs, Persistence/CitySnapshot.cs, SaveSystem.cs, SaveSlotStorage.cs, SaveController.cs, LoadController.cs, Tests/Economy/TaxSaveTests.cs, Tests/Persistence/ManualSaveTests.cs, Tests/GoalStagesTests.cs.
- [ ] Шаг 2: `GoalStages`: `public void Restore(int stage)` — кламп 0..StageCount (отрицательное → 0, больше 3 → 3); ступени/награды/Claim не менять. `GoalView`: `public int Stage { get { return stages.Stage; } }` и `public void RestoreStage(int stage) { stages.Restore(stage); }`; в комментарии к `stages` убрать пометку «долг», написать «хранится в слоте v4».
- [ ] Шаг 3: `CitySnapshot`: `Version = 4`, `public int GoalStage;` (0..3; 0 = ступень не пройдена).
- [ ] Шаг 4: `SaveSystem`: `CurrentVersion = 4`; новая перегрузка 9 арг (`..., List<BuildingState> manualBuildings, int goalStage`); 6/7/8-арг делегируют с `goalStage = 0`; в 9-арг `snapshot.GoalStage = goalStage >= 0 && goalStage <= GoalStages.StageCount ? goalStage : 0`.
- [ ] Шаг 5: `SaveSlotStorage`: `Validate` уже сравнивает с `CurrentVersion` — код не менять, обновить комментарий «1..4: v4 — ступень цели».
- [ ] Шаг 6: `SaveController`: поле `public GoalView Goal;` (Tooltip «Ступень цели. Пусто = ищем в сцене»), строка в `ResolveRefs` (`FindFirstObjectByType`), в `SaveNow` девятый аргумент `Goal == null ? 0 : Goal.Stage`, в лог `, цель=`. Файл ≤150: при необходимости сократить устаревшие комментарии, код не менять.
- [ ] Шаг 7: `LoadController`: в `ClearAll` — сброс `GoalView.RestoreStage(0)` (одной строкой, ленивый поиск), в `ApplyMeta` — `RestoreStage(snapshot.GoalStage)` (одной строкой). Файл ≤150, при необходимости сократить комментарии. В лог загрузки добавить `, цель=`.
- [ ] Шаг 8: тесты: новый `Tests/Persistence/GoalSaveTests.cs` (~80): 9-арг пишет Version 4 + GoalStage 2; JSON-roundtrip сохраняет 2; 8-арг и 6-арг дают GoalStage 0; `goalStage = 99` и `-1` → 0; `Validate` пропускает версии 1..4, отклоняет 5; `GoalStages.Restore(2)` → Stage 2/Target 200, `Restore(-5)` → 0, `Restore(99)` → 3/AllDone. Обновить минимально существующие литералы версии: `TaxSaveTests` (3 → 4, тест «v4 не проходит» → v5), `ManualSaveTests` (3 → 4, `VersionsOneToThree` → `VersionsOneToFour` с циклом 1..4, `VersionFour_Fails` → `VersionFive_Fails`). `GoalStagesTests` не трогать.
- [ ] Шаг 9: `.meta` новым файлам (GUID уникальные). Локально прогнать GoalSaveTests + GoalStagesTests + TaxSaveTests + ManualSaveTests через dotnet/csc, приложить результат. Отчёт, commit messages.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммиты ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка: Refresh → Play: 50 жителей → ступень 1, казна +1000; SaveNow; NewGame (ступень 0); LoadNow → ступень 1, казна как в слоте, повторной +1000 нет; Console чистый; Stop → тесты 236 + новые → мерж, пуш.
