# TASKS/008-breakdown

Ввод: задача 008, модули Economy + UI, закрывает срез. Исполнитель: Engineer (Muse). Вариант: high.
Запрещено трогать: всё кроме Economy/, UI/, TASKS/ и малой правки BuildingPlacementController.cs (только списание в TryPlace).
Сдать: ветка task/008-basic-treasury, dotnet-прогон за 1 минуту. Сцену и wiring делает оркестратор.

- [x] Шаг 1: Treasury.cs — Balance, TrySpend/Earn, BalanceChanged, вложенный TransactionResult.
- [x] Шаг 2: TreasuryView.cs — Awake создаёт Treasury, OnGUI-лейбл (uGUI нет в проекте).
- [x] Шаг 3: правка TryPlace — CanPlace, затем TrySpend перед Place, неуспех = не строить.
- [x] Шаг 4: dotnet-прогон (точные суммы, 0, отрицательные, серия трат), отчёт + commit message.
