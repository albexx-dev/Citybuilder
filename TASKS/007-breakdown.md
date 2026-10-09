# TASKS/007-breakdown

Ввод: задача 007, модуль Construction, эталонное качество. Исполнитель: Engineer Kimi K3. Вариант: xhigh.
Запрещено трогать: Core/World/Simulation/Presentation, ProjectSettings, чужие сцены. Один PR — только Construction.
Сдать: ветка task/007-building-placement (уже создана и запушена с доками), проверка dotnet + Unity за 1 минуту.

- [x] Шаг 1: BuildingState.cs — Id, Definition, Origin, Cells (plain C#).
- [x] Шаг 2: BuildingSystem.cs — CanPlace/Place/Demolish поверх GridSystem, возврат понятного null/false.
- [x] Шаг 3: BuildingPlacementController.cs — призрак зелен/красен, ЛКМ/Escape/ПКМ, список placed.
- [x] Шаг 4: dotnet-прогон границ (край карты, перекрытие, снос, повторный снос).
- [x] Шаг 5: Unity — контроллер на Grid, Play без ошибок, скриншот. Отчёт + commit message.
