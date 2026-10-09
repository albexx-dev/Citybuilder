# TASKS/006-breakdown

Ввод: задача 006, модуль Construction, ScriptableObject-шаблон. Исполнитель: Engineer (Muse). Вариант: high.
Запрещено трогать: Core/World/Simulation/Presentation, ProjectSettings, чужие сцены. Runtime-state в Definition запрещён.
Сдать: ветка task/006-building-definition, проверка в Unity + dotnet за 1 минуту.

- [x] Шаг 1: BuildingDefinition.cs — поля, CreateAssetMenu, OnValidate.
- [x] Шаг 2: CoversCells(origin) — перебор клеток прямоугольника.
- [x] Шаг 3: копия в Unity-проект, Console чистый, ассет House 2x2 cost 100 создан.
- [x] Шаг 4: dotnet-прогон CoversCells + границ, отчёт + commit message.
