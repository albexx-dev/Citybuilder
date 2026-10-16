# TASKS/053-breakdown

Ввод: задача 053, модуль Transport (новый). Пострелизная программа (заморозка ADR-004 снята релизом 047+052): порядок из GAME_DESIGN §18 — транспорт первый. Этап T1: связность как ИНФО, без гейтов поведения (баланс 047 не трогаем). Исполнитель: Engineer (Muse, субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: поведение игры (новая система никем не используется), всё вне Transport/ConnectivitySystem.cs, Tests/Transport/ConnectivityTests.cs(+.meta при нужде) и TASKS/053-breakdown.md.
Сдать: коммит в ветку task/053-transport + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: V1 связности = здание связано, если клетка (4-соседство) примыкает к дороге. BFS/маршруты — позже. Стиль систем: чистый C#, конструктор от GridSystem (как RoadSystem), без LINQ-цепочек >2, без generics-сложности.

- [x] Шаг 1: прочитать ЦЕЛИКОМ до правок: Construction/RoadSystem.cs (стиль), Construction/RoadState.cs, Population/BuildingState.cs (что такое клетка здания), GridSystem.cs (IsInside/IsOccupied/соседи — если есть хелпер соседей, использовать), Tests/ — как лежат тесты и какой неймспейс/сборка.
- [x] Шаг 2: `Transport/ConnectivitySystem.cs` (~80 строк): конструктор от GridSystem (+ опционально стартовые наборы); `Rebuild(roadCells, buildingCells)` (списки GridPosition — копии внутрь); `IsConnected(cell)` (4-сосед рядом дорога; вне карты/не-здание — false); `ConnectedCount`/`TotalCount`/`ConnectionRate01`; лимиты: файл ≤150, методы ≤30.
- [x] Шаг 3: тесты `Tests/Transport/ConnectivityTests.cs` (~60 строк): пусто — 0/0/rate 0; одна дорога рядом — connected; по диагонали — не connected; Rebuild заменяет (старые дороги забыты); rate 1/2. Ноль Unity-зависимостей (чистый C#).
- [x] Шаг 4: самопроверка (лимиты, ноль поведения, тесты проходят локально — чем может) + отчёт + commit message. Ручной план: Refresh на стопе + run_tests EditMode (65 старых + новые зелёные) + Console. Сцены нет, Play не нужен.
