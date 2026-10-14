# TASKS/036-breakdown

Ввод: задача 036, модуль Population (1 новый файл), ядро потребностей. Исполнитель: Engineer (Muse, субагент). Серия «потребности жителей»: 036 ядро → 037 панель → 038 гейт въезда.
Запрещено трогать: всё кроме Population/NeedSystem.cs и TASKS/036-breakdown.md (галочки).
Сдать: коммит в ветку task/036-needs-foundation + ручной план проверки + commit message. В main НЕ коммитить, НЕ пушить, Unity НЕ трогать.

Контекст: GAME_DESIGN §11 — потребности влияют на счастье и миграцию. Готовые счётчики: PopulationSystem (TotalResidents, CapacityOf через дома — см. PopulationController.GrownCapacity), SettlementSystem.TotalJobs, EmploymentSystem (образец ctor с null-гардами и агрегатов без состояния). Въезд/панель — следующие задачи, здесь только ядро.

- [ ] Шаг 1: новый `Population/NeedSystem.cs` (~70 строк, чистый C#, без Unity, своего состояния нет):
  - ctor `(PopulationSystem population, SettlementSystem settlement)` с null-гардами как в EmploymentSystem;
  - `HousingRoomRate`: доля свободного жилья = 1 − residents/capacity; capacity 0 (домов нет) → 0. Capacity считать суммой CapacityOf по домам поселения (нужен доступ к домам: settlement даёт список? проверить SettlementSystem API — если нет списка домов, принять capacity третьим параметром ctor или методом; выбрать простое);
  - `JobsRoomRate`: мест нет → 0, иначе 1 − employed/jobs, employed = min(residents, jobs);
  - `EmploymentRate`: жителей нет → 1 (некого обижать), иначе employed/residents;
  - `AverageSatisfaction` = среднее трёх, всё в 0..1;
  - правило пустого города: жителей нет → все три = 1 (нет неудовлетворённости), кроме HousingRoomRate при capacity 0 → 0;
  - лимиты: файл ≤150, методы ≤30, namespace Citybuilder, русские комментарии точечно.
- [ ] Шаг 2: самопроверка (границы: пусто/нет домов/нет мест/всё полно/всё свободно; null-гарды ctor; все значения в 0..1; Average = среднее) + отчёт + commit message. Проверки —DOTNET? Проект без юнит-инфры: статическая самопроверка + таблица кейсов в отчёте. Ручной план для приёмки: только компиляция через мост (копия на стопе + Refresh + Console), сцену не трогать (MonoBehaviour нет).
