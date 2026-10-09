# TASKS/002-breakdown

Ввод: задача 002, модуль Core, чистые типы без Unity. Исполнитель: Engineer (Muse). Вариант: high.
Запрещено трогать: всё кроме Assets/Citybuilder/Scripts/Core/ и TASKS/. Никаких UnityEngine-зависимостей.
Сдать: ветка task/002-domain-primitives, проверка — dotnet build + прогон границ за 1 минуту.

- [x] Шаг 1: GridPosition.cs — struct X/Y, Equals, GetHashCode, оператор +.
- [x] Шаг 2: GridSize.cs — struct Width/Height, Contains, проверка положительных размеров.
- [x] Шаг 3: EntityId.cs — struct-обёртка над int, New() со счётчиком, сравнение.
- [x] Шаг 4: SimulationTime.cs — struct TickCount, Advance(steps), сравнение.
- [x] Шаг 5: dotnet-компиляция всех 4 файлов + мини-прогон границ, коммит.
