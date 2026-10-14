# TASKS/036-breakdown

Ввод: задача 036, модуль Population (1 новый файл), ядро потребностей. Исполнитель: Engineer (Muse, субагент). Серия «потребности жителей»: 036 ядро → 037 панель → 038 гейт въезда.
Запрещено трогать: всё кроме Population/NeedSystem.cs и TASKS/036-breakdown.md (галочки).
Сдать: коммит в ветку task/036-needs-foundation + ручной план проверки + commit message. В main НЕ коммитить, НЕ пушить, Unity НЕ трогать.

Контекст: GAME_DESIGN §11 — потребности влияют на счастье и миграцию. Готовые счётчики: PopulationSystem (TotalResidents, CapacityOf через дома — см. PopulationController.GrownCapacity), SettlementSystem.TotalJobs, EmploymentSystem (образец ctor с null-гардами и агрегатов без состояния). Въезд/панель — следующие задачи, здесь только ядро.

- [x] Шаг 1: новый `Population/NeedSystem.cs` (~70 строк, чистый C#, без Unity, своего состояния нет):
  - ctor `(PopulationSystem population, SettlementSystem settlement)` с null-гардами как в EmploymentSystem;
  - `HousingRoomRate`: доля свободного жилья = 1 − residents/capacity; capacity 0 (домов нет) → 0. Capacity считать суммой CapacityOf по домам поселения (нужен доступ к домам: settlement даёт список? проверить SettlementSystem API — если нет списка домов, принять capacity третьим параметром ctor или методом; выбрать простое);
  - `JobsRoomRate`: мест нет → 0, иначе 1 − employed/jobs, employed = min(residents, jobs);
  - `EmploymentRate`: жителей нет → 1 (некого обижать), иначе employed/residents;
  - `AverageSatisfaction` = среднее трёх, всё в 0..1;
  - правило пустого города: жителей нет → все три = 1 (нет неудовлетворённости), кроме HousingRoomRate при capacity 0 → 0;
  - лимиты: файл ≤150, методы ≤30, namespace Citybuilder, русские комментарии точечно.
- [x] Шаг 2: самопроверка (границы: пусто/нет домов/нет мест/всё полно/всё свободно; null-гарды ctor; все значения в 0..1; Average = среднее) + отчёт + commit message. Проверки —DOTNET? Проект без юнит-инфры: статическая самопроверка + таблица кейсов в отчёте. Ручной план для приёмки: только компиляция через мост (копия на стопе + Refresh + Console), сцену не трогать (MonoBehaviour нет).

## Решение по capacity (Шаг 1)
Выбран `SettlementSystem.TotalCapacity`: он уже ведётся как сумма ResidentCapacity при Settle/Demolish и равен сумме CapacityOf по домам (ср. PopulationController.GrownCapacity). Списка домов в SettlementSystem API нет, третий параметр ctor не понадобился — самый простой вариант, без изменения чужих файлов.

## Отклонения от EmploymentSystem (осознанные)
`EmploymentRate` при residents = 0 возвращает 1 («некого обижать»), а не 0 как в EmploymentSystem: там это доля факта, здесь — отсутствие неудовлетворённости. `HousingRoomRate` прибран полом `Math.Max(0, …)` — страховка от перенаселения (residents > capacity после сноса без выселения), остальные метрики ограничены 0..1 конструктивно.

## Самопроверка (dotnet-харнес в %TEMP%, стабы под настоящий API, файл из репозитория скопирован как есть)
| Кейс | Вход (res/cap/jobs) | H | J | E | Avg | Итог |
|---|---|---|---|---|---|---|
| Пусто, домов нет | 0/0/0 | 0 | 1 | 1 | 0.667 | PASS |
| Пусто, дома есть | 0/4/0 | 1 | 1 | 1 | 1 | PASS |
| Нет мест | 4/4/0 | 0 | 0 | 0 | 0 | PASS |
| Всё полно | 4/4/4 | 0 | 0 | 1 | 0.333 | PASS |
| Всё свободно | 2/8/8 | 0.75 | 0.75 | 1 | 0.833 | PASS |
| Перенаселение | 6/4/2 | 0 (пол) | 0 | 0.333 | 0.111 | PASS |
| Мест больше | 2/4/10 | 0.5 | 0.8 | 1 | 0.767 | PASS |
| ctor null population | — | ArgumentNullException | | | | PASS |
| ctor null settlement | — | ArgumentNullException | | | | PASS |
Везде: значения в 0..1, Average = среднее трёх. Лимиты: файл 83 строки, методы ≤10 строк, namespace Citybuilder, без Unity/LINQ/generics.

## Commit message
Add NeedSystem needs core (housing/jobs/employment rates)

## Ручной план приёмки (Unity не трогать, сцены нет — MonoBehaviour нет)
1. Мост Unity MCP на стопе (копия уже проверялась вне Unity, этот шаг — только компиляция).
2. В Unity: Assets → Refresh, открыть Console.
3. Ожидается: ошибок компиляции нет. Предупреждений от NeedSystem.cs нет.
4. Сцену не открывать, Play не нажимать, префабы не создавать — проверять нечего, это чистое ядро для задач 037/038.
