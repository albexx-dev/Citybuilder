# TASKS/032-breakdown

Ввод: задача 032, UI-хелпер + гарды в 4 контроллерах (без SettlementGrowthController!). Исполнитель: Engineer (Muse, субагент). Вариант: high.
Запрещено трогать: всё кроме UI/UiClickGuard.cs, UI/BuildModeMenu.cs, UI/ClockView.cs, Construction/BuildingPlacementController.cs, Construction/RoadPlacementController.cs, World/ZonePaintingController.cs, Presentation/FreeCameraController.cs и TASKS/.
Сдать: ветка task/032-menu-click-guard, код + ручной план проверки.

- [ ] Шаг 1: UiClickGuard (Set/IsOverMenu с конвертацией Y).
- [ ] Шаг 2: регистрация rects в меню и часах.
- [ ] Шаг 3: гарды building/road (Down), zone (флаг штриха), camera (флаг обзора).
- [ ] Шаг 4: самопроверка (кнопка/карта true-false, штрих с кнопки, отпуск над меню, null-гарды) + отчёт + commit message.
