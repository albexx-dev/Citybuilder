# TASKS/008 — Basic Treasury

## Цель

Деньги города: начальный баланс, списание за стройку, запрет при нехватке, UI баланса.

## Контекст

Зависит от 003 (часы — позже), 007 (стройка списывает). Закрывает вертикальный срез.

## Что нужно сделать

1. `Treasury` (plain C#, модуль Economy): конструктор со стартовым балансом (V1: 1000).
   - `Balance`, `CanAfford(amount)`, `TrySpend(amount, reason)` → TransactionResult, `Earn(amount)`.
   - Событие `BalanceChanged(int newBalance)`. Отрицательные суммы отклоняются.
   - `TransactionResult` — вложенный struct: Success + Message, фабрики Ok()/Fail(msg).
2. `TreasuryView` (MonoBehaviour, модуль UI): поле StartingBalance (1000).
   - Создаёт Treasury в Awake. Рисует баланс через OnGUI-лейбл (без UI-пакетов, uGUI в проекте нет).
   - Публично: `Treasury Treasury { get; }` для контроллера стройки.
3. Правка `BuildingPlacementController.cs`: поле TreasuryView (опционально, проверка null).
   - В TryPlace: TrySpend(Definition.ConstructionCost) — неуспех = не строить. Цена из Definition.

## Что не нужно делать

- Не добавлять налоги, доходы по тикам, бюджетные категории.
- Не использовать TextMeshPro (нет в проекте) — только UnityEngine.UI.Text.
- Не трогать Core/World/Simulation/Presentation/GridSystem/GridVisualizer.

## Зависимости

TASKS/003, 007 (Done). Ничего не блокирует — закрывает срез.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Economy/Treasury.cs
- Assets/Citybuilder/Scripts/UI/TreasuryView.cs
- TASKS/008-basic-treasury.md (этот файл)
- TASKS/008-breakdown.md

Изменить:
- Assets/Citybuilder/Scripts/Construction/BuildingPlacementController.cs (списание)
- TASKS/README.md (статус 008)

## Критерии готовности

- [ ] Старт 1000. Place дома (100) → 900, UI обновился.
- [ ] 10 домов подряд: 11-й не ставится (не хватает), баланс не в минус.
- [ ] Console без ошибок. Снос денег не возвращает (V1, зафиксировать).

## План проверки

1. `dotnet run`: TrySpend/Earn/границы (0, отрицательные, точная сумма).
2. Unity: Canvas+Text+ wiring, Play: ставить дома, баланс падает; при 0 — запрет.
