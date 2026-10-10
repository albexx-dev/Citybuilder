# TASKS/019-breakdown

Ввод: задача 019, модуль UI (1 новый файл) + 2 микро-правки Construction. Исполнитель: Engineer (Muse, субагент). Вариант: high.
Запрещено трогать: всё кроме UI/BuildModeMenu.cs, Construction/RoadPlacementController.cs (только OnDisable), Construction/BuildingPlacementController.cs (только OnDisable) и TASKS/.
Сдать: ветка task/019-mode-menu, код + ручной план проверки.

- [ ] Шаг 1: RoadPlacementController.OnDisable — сброс drag + HidePreview с гардами.
- [ ] Шаг 2: BuildingPlacementController.OnDisable — скрыть ghost с гардом.
- [ ] Шаг 3: UI/BuildModeMenu.cs — ссылки (ленивый GetComponent), 5 кнопок OnGUI справа вверху, эксклюзивное включение, старт в Выкл.
- [ ] Шаг 4: самопроверка разбором (включение каждого, Выкл, приватный placementActive у building — только enabled, повторные клики, пустые ссылки) + отчёт + commit message.
