# TASKS/016-breakdown

Ввод: задача 016, модуль Population, фундамент домохозяйств без вида. Исполнитель: Engineer (Muse, субагент). Вариант: high.
Запрещено трогать: всё кроме Population/HouseholdState.cs, Population/PopulationSystem.cs и TASKS/.
Сдать: ветка task/016-household-foundation, код + ручной план проверки.

- [ ] Шаг 1: Population/HouseholdState.cs — Id, Home, Size + валидация конструктора.
- [ ] Шаг 2: Population/PopulationSystem.cs — MoveIn / MoveOut / EvictHome / ResidentsIn / CapacityOf + счётчики HouseholdCount, TotalResidents.
- [ ] Шаг 3: самопроверка разбором (null, size<=0, переполнение, двойной MoveOut, EvictHome пустого/null, отрицательная capacity) + отчёт + commit message.
