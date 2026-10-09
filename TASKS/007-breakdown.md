# TASKS/007-breakdown

Ввод: задача 007, модуль Construction, эталонное качество. Исполнитель: Engineer Kimi K3. Вариант: xhigh.
Запрещено трогать: Core/World/Simulation/Presentation, ProjectSettings, чужие сцены. Один PR — только Construction.
Сдать: ветка task/007-building-placement (уже создана и запушена с доками), проверка dotnet + Unity за 1 минуту.

- [ ] Шаг 1: BuildingState.cs — Id, Definition, Origin, Cells (plain C#).
- [ ] Шаг 2: BuildingSystem.cs — CanPlace/Place/Demolish поверх GridSystem, возврат понятного null/false.
- [ ] Шаг 3: BuildingPlacementController.cs — призрак зелен/красен, ЛКМ/Escape/ПКМ, список placed.
- [ ] Шаг 4: dotnet-прогон границ (край карты, перекрытие, снос, повторный снос).
- [ ] Шаг 5: копия в Unity-проект невозможна из этой среды — оркестратор проверит в Unity сам. Отчёт + commit message.
