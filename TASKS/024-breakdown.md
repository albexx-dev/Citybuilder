# TASKS/024-breakdown

Ввод: задача 024, модуль Population (1 правка), рост по зонам. Исполнитель: Engineer (Muse, субагент). Вариант: high.
Запрещено трогать: всё кроме Population/SettlementGrowthController.cs и TASKS/.
Сдать: ветка task/024-workplaces-growth, код + ручной план проверки.

- [ ] Шаг 1: поле Definitions, EnsureReady с вычисткой null и гардом пустоты.
- [ ] Шаг 2: FindFreeOrigin с подбором (def, origin) по зоне + TryGrow этим def.
- [ ] Шаг 3: самопроверка (R→House, C→Shop, пустой список, null в списке, AllowedZone=None, лимит 150 строк) + отчёт + commit message.
