# TASKS/061-breakdown

Ввод: задача 061, модули Infrastructure + Population (энергия в нуждах). Первое реальное влияние энергии на игру — мягкое: только удовлетворённость, без жёстких гейтов въезда/роста (баланс 047 цел). Исполнитель: Engineer (Muse, субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: гейт MinEmployment, порядок въезда, рост, баланс; всё вне Population/NeedSystem.cs, Population/PopulationController.cs, UI/NeedsView.cs, Tests/Infrastructure/PowerNeedTests.cs(+.meta) и TASKS/061-breakdown.md.
Сдать: коммит в ветку task/061-power-needs + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: NeedSystem (85 строк) считает HousingRoomRate/JobsRoomRate/EmploymentRate и AverageSatisfaction = сумма/3. Старые тесты NeedSystemTests ждут ровно /3 — обратная совместимость обязательна. PowerController даёт Rate01 (0..1). Конструкторы NeedSystem вызываются в PopulationController:150 и NeedsView:51.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ до правок: Population/NeedSystem.cs, Population/PopulationController.cs, UI/NeedsView.cs, Infrastructure/PowerController.cs, Tests/NeedSystemTests.cs (все ожидания!), Tests/Infrastructure/*.
- [ ] Шаг 2: NeedSystem — новый НЕОБЯЗАТЕЛЬНЫЙ параметр конструктора `Func<float> powerRate` (старый конструктор делегирует новому с null → все старые тесты обязаны остаться зелёными). Свойства: `PowerRate` (нет провайдера → 1.0; иначе Clamp01 от значения), `HasPower`. AverageSatisfaction: при HasPower — сумма 4 / 4, иначе 3 / 3. Лимиты: файл ≤150, методы ≤30.
- [ ] Шаг 3: PopulationController — опциональное поле Power (PowerController, добор GetComponent, null-гард); при создании NeedSystem передавать провайдер `Power != null ? (Func<float>)(() => Power.Rate01) : null`. Порядок въезда и гейты не трогать.
- [ ] Шаг 4: NeedsView — опциональное поле Power (добор GetComponent), тот же провайдер при создании NeedSystem; строка потребностей: при HasPower 4 показателя с «энергия W%», иначе прежние 3. Позиция y не меняется.
- [ ] Шаг 5: тесты `Tests/Infrastructure/PowerNeedTests.cs` (~70): 4-частное среднее при провайдере; без провайдера — 3-частное (совместимость); PowerRate клампится 0..1 (провайдер отдаёт 1.5 и -1); HasPower. Ноль Unity-зависимостей.
- [ ] Шаг 6: .meta новому файлу. Самопроверка (лимиты, старые тесты не сломаны, гейты не тронуты) + отчёт + commit message. Ручной план: Refresh на стопе + связать Power в Population и Needs (сцена) + Play: при дефиците энергии панель показывает «энергия» <100% и среднее падает, при избытке — 100% + скриншот + Stop + Console + run_tests.
