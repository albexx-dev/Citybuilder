# TASKS/017-breakdown

Ввод: задача 017, модули Population + UI, панель населения и автовъезд. Исполнитель: Engineer (Muse, субагент). Вариант: high.
Запрещено трогать: всё кроме Population/PopulationController.cs, UI/PopulationView.cs, Population/SettlementGrowthController.cs (только геттер Homes + EvictHome в TryDemolish) и TASKS/.
Сдать: ветка task/017-population-panel, код + ручной план проверки.

- [ ] Шаг 1: SettlementGrowthController.cs — геттер Homes (копия списка) + опциональный PopulationSource с EvictHome в TryDemolish, остальное нетронуто.
- [ ] Шаг 2: UI/PopulationView.cs — OnGUI-лейбл Pop/Capacity/Households через контроллер.
- [ ] Шаг 3: Population/PopulationController.cs — EnsureReady, геттеры Population/GrownCapacity, таймер автовъезда first-fit, лимиты Inspector.
- [ ] Шаг 4: самопроверка разбором (нет домов, дома полны, снос заселённого без/с источником, reload) + отчёт + commit message.
