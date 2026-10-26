# TASKS/082-breakdown

Ввод: задача 082, модуль Core + UI (прогрессия §15). Пороги разблокировки (080) видны только в палитре при выбранном режиме «Дома». Добавляем HUD-строку «что откроется дальше». Исполнитель: Engineer (субагент). После — Reviewer (только чтение).
Запрещено: рост/въезд, потребности, инфраструктура, сохранения, сцена, ProjectSettings, старые тесты.
Сдать: коммит в ветку task/082-next-unlock + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `UnlockPolicy` (27): `IsUnlocked(def, buildings)`, `Remaining(def, buildings)`; `SettlementGrowthController.GrownCount`; `BuildingDefinition.UnlockBuildings/DisplayName`; HUD-хелпер `HudText.Draw(x, y, text)` (077). Строки HUD: пожарные y=342, кнопка налога y=374 (26 высотой) → свободно y=406.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Core/UnlockPolicy.cs, Tests/Construction/UnlockPolicyTests.cs, UI/HudText.cs, UI/FireCoverageView.cs, Population/SettlementGrowthController.cs (GrownCount), UI/BuildPaletteView.cs.
- [ ] Шаг 2: `UnlockPolicy`: новый `public static BuildingDefinition NextLocked(List<BuildingDefinition> definitions, int buildings)` — среди закрытых типов вернуть тот, у кого наименьший `UnlockBuildings` (при равенстве — первый по списку); нет закрытых или список пуст → null; null-элементы пропускать; отрицательное `buildings` считать 0. Без LINQ.
- [ ] Шаг 3: новый `UI/ProgressionView.cs` (~55): поля `Growth` (SettlementGrowthController, опционален) и `Definitions` (List<BuildingDefinition>); в OnGUI, если Growth/Definitions пусты — молча выйти; `next = UnlockPolicy.NextLocked(Definitions, Growth.GrownCount)`; текст: `next == null` → «Все типы открыты», иначе «Дальше: Имя — N зд.» (N = `next.UnlockBuildings`), `HudText.Draw(10, 406, text)`.
- [ ] Шаг 4: тесты `Tests/Construction/NextUnlockTests.cs` (~60): пустой список → null; все открыты → null; закрыты Shop(4) и Factory(8), зданий 0 → Shop; зданий 4 → Factory; зданий 8 → null; отрицательное → Shop; null-элементы и дубли порогов (первый по списку); без Unity-сцены.
- [ ] Шаг 5: `.meta` новым файлам (свежие GUID). Локально прогнать NextUnlockTests + UnlockPolicyTests через dotnet/csc, приложить результат. Отчёт, commit message.

ЛИМИТЫ: файл ≤150, метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммиты ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка: Refresh → на Grid добавить ProgressionView (Growth = рост, Definitions = House/Shop/Factory/Station) → Play: при 0 зданий «Дальше: Shop — 4 зд.», после 4 — «Factory — 8 зд.», после 8 — «Все типы открыты»; Stop → тесты 202 + новые → мерж, пуш.
