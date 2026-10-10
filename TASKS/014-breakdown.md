# TASKS/014-breakdown

Ввод: задача 014, модули Population + Presentation (+1 геттер в World), автовырастание домов. Исполнитель: Engineer (Muse, субагент). Вариант: high.
Запрещено трогать: всё кроме Population/SettlementGrowthController.cs, Presentation/SettlementView.cs, World/ZonePaintingController.cs (только геттер Zones) и TASKS/.
Сдать: ветка task/014-settlement-growth, код + ручной план проверки.

- [ ] Шаг 1: ZonePaintingController.cs — публичный ленивый геттер Zones, остальное нетронуто.
- [ ] Шаг 2: Presentation/SettlementView.cs — AddBuilding (префаб-or-бежевый куб Unlit/Color) / RemoveBuilding / Clear.
- [ ] Шаг 3: Population/SettlementGrowthController.cs — EnsureReady, таймер роста, first-fit поиск origin, ПКМ-снос, placementActive, лимиты в Inspector.
- [ ] Шаг 4: самопроверка разбором (нет зон, зона занята, MaxSettlements, снос чужого/пустого, reload) + отчёт + commit message.
