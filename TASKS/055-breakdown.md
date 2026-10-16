# TASKS/055-breakdown

Ввод: задача 055, модули Population + Transport (мягкий гейт). T1-гейт: жители предпочитают связанные дома, дефолт без дорог — старое поведение один в один (баланс 047 цел). Исполнитель: Engineer (Muse, субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: жёсткие гейты (MinEmployment и порядок въезда не менять), всё вне Population/PopulationController.cs, Transport/ConnectivityController.cs, новых/изменённых тестов Population/Transport и TASKS/055-breakdown.md.
Сдать: коммит в ветку task/055-conngate + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: PopulationController.TryMoveIn — first-fit по Growth.Homes (копия). ConnectivityController — снимок связности, геттеры Connected/Total/Rate01. Дефолтная игра без дорог: связных домов 0 → обязан работать старый first-fit.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ до правок: Population/PopulationController.cs, Transport/ConnectivityController.cs, Transport/ConnectivitySystem.cs (IsConnected(cell)), Population/PopulationSystem.cs (MoveIn/ResidentsIn/CapacityOf), существующие тесты Population* и Transport (что покрыто, как конструируются).
- [ ] Шаг 2: предпочтение связных (без жёсткого гейта): в PopulationController опциональная ссылка Transport (ConnectivityController, резолв GetComponent, null-гард); в TryMoveIn два прохода — сначала first-fit среди домов, где все/связанные клетки... точнее: дом считается связанным, если ХОТЯ БЫ одна его клетка IsConnected; если связных со свободным местом нет (включая «транспорта нет вообще») — старый проход по всем домам. Порядок внутри проходов не менять. Лимиты: методы ≤30 (TryMoveIn разрезать при нужде).
- [ ] Шаг 3: тесты: связный дом заселяется первым при наличии выбора; без транспорта — старый first-fit; только несвязные — заселяются (не блокируются). Чистый C# где возможно; если TryMoveIn приватный и тестовый хук дорог — покрыть через публичные геттеры/счётчики, без взлома инкапсуляции.
- [ ] Шаг 4: самопроверка (лимиты, дефолт=старое поведение, существующие тесты не сломаны правками) + отчёт + commit message. Ручной план: Refresh на стопе + связать Transport в Population (сцена) + Play-дефолт: рост и въезд идут как раньше (баланс цел) + Play-с-дорогой: связный дом заселяется первым + Stop + Console + run_tests.
