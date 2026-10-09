# TASKS — очередь

Формат: `- [ ] ToDo / - [x] InProgress / - [x] Done + инженер + PR`.

- [x] Done 001-project-foundation (Orchestrator) + branch task/001-project-foundation
- [ ] ToDo 002-domain-primitives (Kimi, xhigh) — GridPosition, GridSize, EntityId, SimulationTime
- [ ] ToDo 003-simulation-clock (Muse, high) — Paused/Playing/FastForward + тики
- [ ] ToDo 004-grid-system (Kimi, xhigh) — IsInside, WorldToGrid, GridToWorld, Occupy/Release
- [ ] ToDo 005-grid-visualization (Muse, high) — показ сетки, подсветка клетки
- [ ] ToDo 006-building-definition (Muse, high) — BuildingDefinition ScriptableObject
- [ ] ToDo 007-building-placement (Kimi, xhigh) — предпросмотр, проверки, установка/удаление
- [ ] ToDo 008-basic-treasury (Kimi, xhigh) — баланс, транзакции, UI денег

Правила: 1 задача = 1 breakdown-файл TASKS/00X-breakdown.md, 1 ветка, 1 PR, 1 модуль. Две задачи параллельно запрещены.
