# TASKS/026 — Zone Brush Menu

## Цель

Цвет кисти зон переключается кнопками в меню (Жилая/Коммерция/Промзона), без лазанья в инспектор. Видно только в режиме Зоны.

## Контекст

Сейчас Brush — поле ZonePaintingController в инспекторе (пользователь уже перепутал его с цветами сетки). BuildModeMenu есть (019). ZonePainter.Brush публичный — других правок не нужно.

## Что нужно сделать

1. `BuildModeMenu` (изменить, модуль UI — ТОЛЬКО этот файл):
   - Публичный метод `SetBrush(ZoneType zone)`: None/невалидное — игнор; иначе ZonePainter.Brush = zone (ZonePainter может быть null — гард).
   - OnGUI: если CurrentMode == Zones — второй ряд кнопок под режимами: [Жилая] [Коммерция] [Промзона]; активная подсвечена (текущая ZonePainter.Brush, ZonePainter null — без подсветки).
   - Экономно по строкам: файл был 120/150! Уложиться в 150 (вынести DrawBrushButton-хелпер, переиспользовать логику подсветки).
   - Остальное не трогать.

## Что не нужно делать

- Не трогать ZonePaintingController, остальные файлы. Хоткеи не делать.

## Зависимости

TASKS/012 (кисть), 019 (меню). Ничего не блокирует.

## Изменяемые файлы

Изменить:
- Assets/Citybuilder/Scripts/UI/BuildModeMenu.cs (кнопки кисти)
- TASKS/README.md (статус 026)

Создать:
- TASKS/026-zone-brush-menu.md (этот файл)
- TASKS/026-breakdown.md

## Критерии готовности

- [ ] 1 файл, ≤150 строк итого.
- [ ] SetBrush(Commercial) → ZonePainter.Brush == Commercial; SetBrush(None) — игнор.
- [ ] Скриншот: в режиме Зоны видны 3 кнопки цвета. Console чистый.

## Железные уроки (не нарушать)

- Refresh (scope all) + компиляция. Код правится только на стопе (пользователь на Stop).

## План проверки

1. Unity: Refresh all, компиляция, Console чистый.
2. E2E через мост в Play: SetMode(Zones) + SetBrush каждым типом + сверка Brush; скриншот кнопок.
