# TASKS/056-breakdown

Ввод: задача 056, модуль Transport (маршруты). T2: чистая логика поиска пути по дорогам + нагрузка клеток (фундамент для пробок). Исполнитель: Engineer (Muse, субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: поведение (новая система никем не используется), всё вне Transport/RouteSystem.cs, Tests/Transport/RouteTests.cs(+.meta при нужде) и TASKS/056-breakdown.md.
Сдать: коммит в ветку task/056-routes + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: дороги хранятся как набор клеток (RoadNetworkStore.Roads → RoadState.Cell). Маршрут существует только по занятым-дорогой клеткам; вне сетки/не-дорога = непроходимо. Путь = список клеток от старта до цели включительно. A* с манхэттенской эвристикой, 4-соседство, равная стоимость 1. Нагрузка — счётчик прохождений на клетку (для будущих пробок). Стиль: чистый C#, конструктор от GridSystem (как RoadSystem/ConnectivitySystem), без LINQ-цепочек >2, без сложных generics.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ до правок: Construction/RoadSystem.cs, Transport/ConnectivitySystem.cs (стиль), GridSystem.cs (IsInside/IsOccupied/соседи), Tests/Transport (как лежат).
- [ ] Шаг 2: `Transport/RouteSystem.cs` (~130 строк): конструктор от GridSystem; `FindPath(from, to)` → List<GridPosition> или null (старт==цель → список из одной клетки; старт/цель вне карты или не дорога → null; недостижимо → null); `TrackLoad(path)` (null/пустой — no-op; увеличивает счётчик на каждую клетку); `LoadOf(cell)`; `ClearLoads()`; `HasLoad`. Лимиты: файл ≤150, методы ≤30.
- [ ] Шаг 3: тесты `Tests/Transport/RouteTests.cs` (~70 строк): старт==цель; прямая линия; L-образный путь; нет пути (разрыв); старт не дорога → null; TrackLoad + LoadOf + ClearLoads; HasLoad. Ноль Unity-зависимостей.
- [ ] Шаг 4: самопроверка (лимиты, логика A*, тесты проходят локально) + отчёт + commit message. Ручной план: Refresh на стопе + run_tests EditMode (все зелёные) + Console.
