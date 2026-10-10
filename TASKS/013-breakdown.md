# TASKS/013-breakdown

Ввод: задача 013, модули Construction (правка) + Population (новый), фундамент заселения без вида. Исполнитель: Engineer (Muse, субагент). Вариант: high.
Запрещено трогать: всё кроме Construction/BuildingDefinition.cs, Population/SettlementSystem.cs и TASKS/.
Сдать: ветка task/013-settlement, код + ручной план проверки.

- [ ] Шаг 1: BuildingDefinition.cs — поля AllowedZone (default Residential), ResidentCapacity (default 4), JobsProvided (default 0) + clamp отрицательных в 0 в OnValidate. Без проверок зон.
- [ ] Шаг 2: Population/SettlementSystem.cs — CanSettle / Settle / Demolish + счётчики SettledCount, TotalCapacity, TotalJobs. Зависимость одним направлением: Population → World + Construction.
- [ ] Шаг 3: самопроверка разбором (чужая зона, пустая зона, AllowedZone=None, занято, двойной Demolish, clamp) + отчёт + commit message.
