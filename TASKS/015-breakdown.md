# TASKS/015-breakdown

Ввод: задача 015, модуль Construction (1 файл), продолжение дороги с конца. Исполнитель: Engineer (Muse, субагент). Вариант: high.
Запрещено трогать: всё кроме Construction/RoadPlacementController.cs и TASKS/.
Сдать: ветка task/015-road-continue, код + ручной план проверки.

- [ ] Шаг 1: IsOwnRoadCell + NewCellsCount хелперы (поиск по states, без LINQ-цепочек >2).
- [ ] Шаг 2: CheckCells с учётом своих клеток + цена только за новые в CheckCells/CommitDrag/UpdatePreview.
- [ ] Шаг 3: CommitDrag ставит только новые (без дубликатов), чужое занятое — abort без списаний.
- [ ] Шаг 4: самопроверка разбором (старт с конца, drag поверх своей середины, drag через дом, пустой Definition, повторный снос) + отчёт + commit message.
