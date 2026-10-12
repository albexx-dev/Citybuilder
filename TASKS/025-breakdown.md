# TASKS/025-breakdown

Ввод: задача 025, модуль Presentation (1 правка), меш линий сетки. Исполнитель: Engineer (Muse, субагент). Вариант: high.
Запрещено трогать: всё кроме Presentation/GridVisualizer.cs и TASKS/.
Сдать: ветка task/025-grid-lines-mesh, код + ручной план проверки.

- [ ] Шаг 1: BuildGridLines на MeshTopology.Lines (вершины+индексы, материал LineColor).
- [ ] Шаг 2: EnsureVisuals — готовность только по MeshFilter с мешем, старого ребёнка с LineRenderer удалять и перестраивать.
- [ ] Шаг 3: самопроверка (число вершин (51+51)*2, индексы, нет LineRenderer, bounds) + отчёт + commit message.
