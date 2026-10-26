# TASKS/087-breakdown

Ввод: задача 087, модуль Core + UI (прогрессия §15). Цель одна и статичная (50 жителей), после неё игра «закончилась» — нет повода развиваться дальше. Вводим ступени: 50 → 100 → 200 жителей, за каждую ступень разовая награда в казну. Исполнитель: Engineer (субагент). После — Reviewer (только чтение).
Запрещено: `GoalSystem` (старые тесты на него), рост/въезд, потребности, инфраструктура, сохранения, сцена, ProjectSettings, старые тесты.
Сдать: коммит в ветку task/087-goal-stages + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `GoalSystem` (56) — статичная цель 50 и поражение; `GoalView` (45) рисует «Цель: X/50 жителей» через `HudText.Draw(10, 148, …)`; `TreasuryView.Treasury.Earn(amount)` для награды.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Population/GoalSystem.cs, UI/GoalView.cs, Tests/GoalSystemTests.cs, UI/TreasuryView.cs, UI/HudText.cs.
- [ ] Шаг 2: новый `Population/GoalStages.cs` (~70, чистый C#, без Unity): ступени `{50, 100, 200}` и награды `{1000, 2000, 4000}`; `Stage` (0..3, 3 = все взяты), `Target` (текущая цель; после последней — 200), `AllDone`, `Progress01(int residents)` (0..1 внутри текущей ступени), `Claim(int residents)` — возвращает сумму наград за все ПЕРЕЙДЕННЫЕ ступени и сдвигает `Stage`; повторный `Claim` без новых достижений → 0; отрицательное население → 0; без состояния снаружи.
- [ ] Шаг 3: `GoalView`: опциональное поле `Treasury` (TreasuryView, ленивый GetComponent); в OnGUI после EnsureGoals: `int reward = stages.Claim(goals.Residents); if (reward > 0 && Treasury != null) Treasury.Treasury.Earn(reward);` — ступени живут в поле `GoalStages stages = new GoalStages()`; текст: `Цель: X/Target жителей (ступень N/3)`; после всех — `Цель выполнена: X жителей`; координаты не менять, `HudText` использовать.
- [ ] Шаг 4: тесты `Tests/GoalStagesTests.cs` (~80): старт Stage 0/Target 50/Progress 0; 49 → 0 и Stage 0; 50 → 1000, Stage 1, Target 100; повторный 50 → 0; 100 → 2000; 200 → 4000 и AllDone; прыжок 0 → 250 → 7000 за один вызов; 300 → 0; отрицательное → 0; Progress01 на ступени 100: 150 → 0.5.
- [ ] Шаг 5: `.meta` новым файлам (GUID уникальные). Локально прогнать GoalStagesTests + GoalSystemTests через dotnet/csc, приложить результат. Отчёт, commit message.

ЛИМИТЫ: файл ≤150, метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммит ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка: Refresh → Play: строка «Цель: X/50 жителей (ступень 1/3)»; при 50 жителях казна +1000 и цель 100; при 100 → +2000 и цель 200; при 200 → +4000 и «Цель выполнена»; Stop → тесты 221 + новые → мерж, пуш.
