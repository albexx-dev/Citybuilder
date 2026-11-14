# EPIC-65: Техдолг из ревью #42 + сплиты

Issue: #65 · Ярлык: architecture · Зависимости: нет (лёгкий эпик — антидот перед крупными фазами)

## Проблема

Ревью стека ПРов #30–#41 оставило открытым issue #42: кламп палитры неполный, Tooltips «Обязателен» соврали после auto-resolve, 7 тестовых файлов >150 строк, `RoadPlacementController` 162, camelCase `placementActive`, асимметрия `CongestedCellsCount`, `ResolveMenuRefs` сканирует сцену каждый кадр. Техдолг растёт с каждой фазой v1.2 — закрыть ДО них.

## Что увидит игрок

Кнопки палитры больше не вылезают за фон на 1024px; подсказки в Inspector правдивы; перф-хвосты убраны.

## Архитектурный план

- `BottomBarView.DrawPaletteRow/DrawZoneBrushes`: рисовать только `MaxPaletteItems` кнопок (остальные — «+N ещё» индикатор), тест обновить на честный.
- Tooltips `EconomyTicker`/`PopulationController`: «Пусто = ищется в сцене» (после auto-resolve из ПР #32).
- Сплиты: `RoadPlacementController` 162 → вынести ввод (v1.1 план шага 3, файл уже есть — довести до ≤150), тестовые файлы >150 разбить по темам.
- `BuildingPlacementController.placementActive` → `PlacementActive` (PascalCase) при касании файла; `CongestionSystem.CongestedCellsCount` — симметрия с `WorstMultiplier` (учёт recordedLoads); `SettlementDemolitionController.ResolveMenuRefs` — кеширование результата.

## Шаги

1. **palette-clamp** — кламп отрисовки + честный тест.
2. **tooltips-naming** — тултипы + PascalCase при касании.
3. **splits** — RoadPlacementController + тестовые файлы.
4. **perf-tails** — CongestedCellsCount + ResolveMenuRefs кеш.

## План тестов

- Каждый пункт — свой тест или обновлённый честный тест (без ослабления).
- Валидатор: warning по затронутым файлам исчезает.

## Unity-приёмка (E2E)

1024px окно: палитра не вылезает; клики/режимы/меню работают как раньше; Play-смок чистый.

## Критерий закрытия эпика

Issue #42 закрыт полностью (все чекбоксы), валидатор 0 ошибок, warning ≤15.

## Запрещено и риски

- Не «заодно» рефакторить соседей — только пункты #42.
- Риск: PascalCase-переименование поля затронет сцену (serialized name) — публичное поле переименовывать с FormerlySerializedAs.
