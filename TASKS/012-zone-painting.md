# TASKS/012 — Zone Painting

## Цель

Рисовать зоны кистью в Unity: выбор типа, drag-закраска, стирание, цветной вид.

## Контекст

Зависит от 011 (ZoneSystem). Цвета: жилая — зелёная, коммерческая — синяя, промышленная — оранжевая.

## Что нужно сделать

1. `ZoneView` (MonoBehaviour, модуль World... нет — вид относится к Presentation. Решение: модуль Presentation).
   - Три Unlit/Transparent-материала (зелёный/синий/оранжевый 0.3), три меша.
   - `Rebuild(Dictionary<ZoneType, List<GridPosition>> zones, float cellSize)` — перестраивает все три.
2. `ZonePaintingController` (MonoBehaviour, модуль World — логика поверх ZoneSystem; вид дёргает Presentation):
   - Inspector: GridVisualizer (обязателен), View (ZoneView, создаёт сам если пусто), Brush (ZoneType, default Residential), placementActive (default true).
   - ЛКМ drag: SetZone(hover, Brush) на каждой новой клетке, перестроить вид.
   - ПКМ drag или ПКМ: SetZone(hover, None) — стирание.
   - Escape — ничего (кисти нечего отменять, поле для совместимости не нужно).

## Что не нужно делать

- Не делать районы-имена, заселение, дома на зонах.
- Не трогать другие модули. Только старый Input.

## Зависимости

TASKS/011 (Done).

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Presentation/ZoneView.cs
- Assets/Citybuilder/Scripts/World/ZonePaintingController.cs
- TASKS/012-zone-painting.md (этот файл)
- TASKS/012-breakdown.md

Изменить:
- TASKS/README.md (статус 012)

## Критерии готовности

- [ ] 2 файла, каждый до 150 строк.
- [ ] Drag красит полосу, ПКМ стирает, цвета различаются.
- [ ] Console без ошибок. Контроллер по умолчанию выключен в сцене (конфликт режимов).

## План проверки

1. Unity: контроллер на Grid (disabled), Play без ошибок.
2. Ручная: вкл. контроллер, drag красит, ПКМ стирает.
