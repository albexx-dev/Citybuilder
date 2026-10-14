# TASKS/045-breakdown

Ввод: задача 045, UI + ЧИСТАЯ логика (мини-исключение «1 модуль»: GoalSystem без состояния + GoalView, 2 файла; логика отдельно тестируема). HUD цели + экраны победы/поражения. Исполнитель: Engineer (Muse, субагент).
Запрещено трогать: всё кроме Population/GoalSystem.cs, UI/GoalView.cs (+.meta), Tests/GoalSystemTests.cs (+.meta) и TASKS/045-breakdown.md.
Сдать: коммит в ветку task/045-goal-hud + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: ADR-004 — победа = 50 жителей, поражение = пустой город после дня 3 (72 тика). PopulationSystem публичен (TotalResidents, MoveIn), SimulationClock.CurrentTime.TickCount публичен.

- [ ] Шаг 1: `Population/GoalSystem.cs` (~40 строк, чистый C#, без состояния): ctor(population, clock) с null-гардами; const GoalResidents = 50, LoseDay = 3 (72 тика); геттеры Progress01, Won (residents>=50), Lost (ticks > 72 && residents==0).
- [ ] Шаг 2: `UI/GoalView.cs` (~60 строк): ссылки PopulationController + SimulationRunner (опциональные, добор, молча); GoalSystem лениво; OnGUI: HUD-строка "Цель: N/50 жителей" (Rect 10,148 — под потребностями y=114+30); Won/Lost — центральный бокс (GUI.Box по центру + лейбл "Победа! Город вырос." / "Поражение. Город пуст."). Без кнопок (меню — 046).
- [ ] Шаг 3: `Tests/GoalSystemTests.cs`: Won на границе 49/50/51; Lost (тики 72/73 × жители 0/1); прогресс 0..1.
- [ ] Шаг 4: .meta новым файлам (свежие GUID, коллизии проверить). Самопроверка (лимиты; экраны не блокируют игру — только рисуют) + отчёт + commit message. Ручной план: Refresh на стопе + GoalView на объект Population (ссылки вручную: Population + Simulation) + Play: HUD виден → через мост заселить 25 семей (MoveIn публичен) → экран победы + скриншот + Stop + Console + run_tests.
