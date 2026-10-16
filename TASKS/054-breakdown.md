# TASKS/054-breakdown

Ввод: задача 054, модуль Transport (индикатор в сцене). T1-продолжение: связность видна игроку, поведения ноль (гейтов нет, баланс 047 цел). Исполнитель: Engineer (Muse, субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: поведение (только чтение чужих систем, запись только в свои поля), всё вне Transport/ConnectivityController.cs, Transport/ConnectivityView.cs(+.meta), TASKS/054-breakdown.md.
Сдать: коммит в ветку task/054-connview + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: ConnectivitySystem из 053 (Rebuild + IsConnected/Counts/Rate). Дороги: RoadNetworkStore.Roads (RoadState.Cell). Дома: SettlementGrowthController.Homes (BuildingState.Cell). HUD-строки: NeedsView y=114, GoalView y=148 — новую ставить ниже (y=182), не налезать.

- [x] Шаг 1: прочитать ЦЕЛИКОМ до правок: Transport/ConnectivitySystem.cs, Construction/RoadNetworkStore.cs (Roads), Population/SettlementGrowthController.cs (Homes), Population/BuildingState.cs (Cell), UI/NeedsView.cs (стиль OnGUI-лейбла).
- [x] Шаг 2: `Transport/ConnectivityController.cs` (~90 строк, MonoBehaviour): ссылки Store (обязателен), Growth (обязателен), GridVisualizer (обязателен: даёт Grid для системы); владеет ConnectivitySystem (лень по Grid); пересчёт Rebuild по таймеру 2с + публичный RefreshNow (для загрузок/моста); публичные геттеры Connected/Total/Rate01 (вид только читает); Header/Tooltip/null-гарды.
- [x] Шаг 3: `Transport/ConnectivityView.cs` (~40 строк, MonoBehaviour): ссылка Controller (опционален: добор GetComponent); OnGUI один лейбл «Связано: X/Y (Z%)» на y=182; контроллера/систем нет — молчит.
- [x] Шаг 4: .meta новым файлам (свежие GUID, коллизии проверить). Самопроверка (файлы ≤150, методы ≤30; чужое только читается; гейтов ноль) + отчёт + commit message. Ручной план: Refresh на стопе + оба компонента на Grid + связи (Store/Growth/GridVisualizer/Controller) + Play: построить дорогу рядом с домом → лейбл X/Y растёт; скриншот + Stop + Console + run_tests.
