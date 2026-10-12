# TASKS/025 — Grid Lines Mesh

## Цель

Убрать диагональные перемычки сетки: вместо LineRenderer (он соединяет все точки подряд — 101 диагональ через карту) рисовать линии мешем с топологией Lines. 1 draw call, как было.

## Контекст

Найдено по скриншоту из живой игры + опросу сцены: GridLines LineRenderer, 204 точки парами, рендер идёт непрерывной лентой — конец линии N соединяется с началом линии N+1. Модуль Presentation, только BuildGridLines (+ совместимость EnsureVisuals).

## Что нужно сделать

1. `GridVisualizer` (изменить, модуль Presentation — ТОЛЬКО BuildGridLines и затронутое в EnsureVisuals):
   - BuildGridLines: GameObject "GridLines" (найти/создать как раньше), MeshFilter + MeshRenderer, материал Unlit/Color цвета LineColor.
   - Вершины: те же координаты, что сейчас кладутся в LineRenderer (vertical+horizontal линий по 2 точки). Индексы 0..N-1, `mesh.SetIndices(indices, MeshTopology.Lines, 0)`. Нормали не нужны (Unlit), bounds посчитать (mesh.RecalculateBounds).
   - EnsureVisuals: ребёнок "GridLines" считается готовым, только если у него есть MeshFilter с непустым мешем; иначе (в т.ч. старый LineRenderer из сохранённой сцены) — ребёнка удалить и построить заново. LineRenderer-компонент в новом коде не создавать вообще.
   - Ширина линий (0.03) уходит вместе с LineRenderer — линии станут толщиной 1px. Зафиксировать в отчёте как следствие.

## Что не нужно делать

- Не делать утолщение линий, шейдеры, другие файлы. Только старый Input (не нужен).

## Зависимости

TASKS/005 (Done). Ничего не блокирует.

## Изменяемые файлы

Изменить:
- Assets/Citybuilder/Scripts/Presentation/GridVisualizer.cs (только BuildGridLines + кусок EnsureVisuals)
- TASKS/README.md (статус 025)

Создать:
- TASKS/025-grid-lines-mesh.md (этот файл)
- TASKS/025-breakdown.md

## Критерии готовности

- [ ] Дифф только по сетке; лимиты строк/методов соблюдены.
- [ ] Скриншот из Play: ровная сетка без диагоналей.
- [ ] Старая сцена с LineRenderer-ребёнком чинится сама (перестройка). Console чистый.

## Железные уроки (не нарушать)

- Только Unlit/Color. Refresh (scope all) + компиляция (город пользователя сбросится — предупредить). Сцену — только при выключенном Play.

## План проверки

1. Unity: Refresh all, компиляция, Console чистый.
2. E2E через мост: скриншот до/после (сетка без диагоналей), опрос child-объекта (MeshFilter, топология Lines, вершин 204).
