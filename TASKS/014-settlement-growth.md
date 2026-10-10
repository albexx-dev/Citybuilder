# TASKS/014 — Settlement Growth

## Цель

Дома сами вырастают на жилых зонах: таймер находит свободное место под 2x2, ставит дом-куб, счётчики заселяют. Плюс вид и снос выросших. Закрывает Этап 5.

## Контекст

Зависит от 011 (ZoneSystem), 012 (кисть зон + ZonePaintingController), 013 (SettlementSystem, Capacity/Jobs). Ручная стройка (007) не меняется. Налоги и домохозяйства — Этап 6, не здесь.

## Что нужно сделать

1. `ZonePaintingController` (изменить, модуль World — ТОЛЬКО добавить, ничего не менять):
   - Публичный ленивый геттер `Zones`: `public ZoneSystem Zones { get { if (system == null) EnsureReady(); return system; } }`. Нужен контроллеру роста как источник зон (направление Population → World разрешено, обратного нет).
2. `SettlementView` (MonoBehaviour, модуль Presentation):
   - `AddBuilding(BuildingState state, BuildingDefinition def, float cellSize)` → GameObject: Prefab если задан, иначе куб `Footprint × 1 × Footprint` цветом Unlit/Color бежевый (1.0, 0.9, 0.6). Коллайдер куба удалить.
   - `RemoveBuilding(BuildingState state)` — уничтожить вид (поиск по state Id). `Clear()` — снести все виды.
   - Внутри словарь EntityId → GameObject. Ленивое восстановление после перезагрузки домена не требуется (виды пересоздаёт контроллер; пустой словарь после reload — штатно, зафиксировать).
3. `SettlementGrowthController` (MonoBehaviour, модуль Population):
   - Inspector: GridVisualizer (обязателен), ZonePainter (ZonePaintingController, обязателен — источник зон), Definition (BuildingDefinition дома, обязателен), View (SettlementView, создаёт сам если пусто), GrowthIntervalSec (default 5, минимум 1), MaxSettlements (default 10, минимум 1), placementActive (default true).
   - Ленивый EnsureReady-паттерн: свой BuildingSystem + SettlementSystem поверх GridVisualizer.Grid и ZonePainter.Zones; список states; таймер. Всё восстанавливается после перезагрузки домена (выросшее при reload теряется из списков — штатно для V1, зафиксировать в отчёте).
   - Update: если placementActive и прошла GrowthIntervalSec и SettledCount < MaxSettlements — найти первый origin (перебор GetZonedCells, только Residential как кандидаты) где `settlement.CanSettle(def, origin)`, `Settle` + `View.AddBuilding` + `GridVisualizer.RefreshOccupied()`.
   - ПКМ по клетке — снести выросший дом (поиск state по клетке, `settlement.Demolish` + `View.RemoveBuilding` + RefreshOccupied). ЛКМ не используется (рост автоматический).
   - Контроллер в сцене по умолчанию ВЫКЛЮЧЕН (один контроллер за раз).

## Что не нужно делать

- Не делать налоги, баланс за рост (рост бесплатный — строят поселенцы), домохозяйства, миграцию (Этап 6+).
- Не делать тики через SimulationClock (таймер в Update; привязка к часам — позже).
- Не трогать BuildingPlacementController, Road*, Treasury*, ZoneView, GridVisualizer, Simulation. Только старый Input.

## Зависимости

TASKS/011, 012, 013 (Done). Закрывает Этап 5.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Presentation/SettlementView.cs
- Assets/Citybuilder/Scripts/Population/SettlementGrowthController.cs
- TASKS/014-settlement-growth.md (этот файл)
- TASKS/014-breakdown.md

Изменить:
- Assets/Citybuilder/Scripts/World/ZonePaintingController.cs (только геттер Zones)
- TASKS/README.md (статус 014)

## Критерии готовности

- [ ] 2 новых файла, каждый до 150 строк; правка ZonePaintingController — только геттер.
- [ ] Покрасил жилую полосу → включил контроллер → дома-кубы растут раз в N сек, счётчики SettlementSystem растут (Capacity/Jobs видны через... — счётчики логики; UI нет, проверка через рост домов и отсутствие ошибок).
- [ ] MaxSettlements останавливает рост. ПКМ сносит выросший дом и освобождает клетки.
- [ ] Console без ошибок. Code rules: namespace Citybuilder, 1 файл = 1 класс, методы до 30 строк, без LINQ-цепочек >2, без синглтонов, [Header]/Tooltip/проверка null.

## Железные уроки (не нарушать)

- Только Unlit/Color. Unlit/Transparent запрещён.
- Состояние в MonoBehaviour только ленивое (EnsureReady каждый Update/Start).
- Скопировал файлы в Unity — выполни Assets/Refresh и дождись компиляции.
- Галочки режимов и сцену трогай только при выключенном Play. За раз включён один контроллер (на время проверки роста выключить Building/Road/Zone-контроллеры).

## План проверки

1. Unity (Play ВЫКЛ): контроллер роста на объект Grid, все ссылки заданы, сам контроллер disabled. Play ВКЛ без ошибок.
2. Ручная: кистью покрасить жилую зону → остановить Play → включить ТОЛЬКО рост → Play: дома растут, ПКМ сносит, Console чистый. Редактор после проверки оставить в Stop.
