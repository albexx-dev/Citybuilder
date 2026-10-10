# TASKS/023-breakdown

Ввод: задача 023, модули Economy (2 новых) + Population (1 геттер), налоги. Исполнитель: Engineer (Muse, субагент). Вариант: high.
Запрещено трогать: всё кроме Economy/TaxSystem.cs, Economy/EconomyTicker.cs, Population/SettlementGrowthController.cs (только геттер Settlement) и TASKS/.
Сдать: ветка task/023-taxes, код + ручной план проверки.

- [ ] Шаг 1: SettlementGrowthController — геттер Settlement, остальное нетронуто.
- [ ] Шаг 2: Economy/TaxSystem.cs — ComputeTax/Collect, гарды perWorker.
- [ ] Шаг 3: Economy/EconomyTicker.cs — EnsureReady, таймер, ClampLimits, Tick-шаг приватным методом (для E2E).
- [ ] Шаг 4: самопроверка (6×2, perWorker 0/отрицательный, null-системы, баланс) + отчёт + commit message.
