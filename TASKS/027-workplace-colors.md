# TASKS/027 — Workplace Colors

## Цель

Выросшие магазины и заводы отличаются от домов цветом стен: коммерция — светло-синяя, промышленность — тёплая серая. Жилые остаются бежевыми.

## Контекст

После 024 все типы растут бежевыми (один FallbackColor) — на глаз не отличить. Модуль Presentation, только SettlementView. Крыши не трогать (коричневые у всех).

## Что нужно сделать

1. `SettlementView` (изменить, модуль Presentation — ТОЛЬКО выбор цвета стен):
   - Метод `WallColor(BuildingDefinition def)`: Residential (и None/fallback) → FallbackColor; Commercial → (0.6, 0.75, 0.9); Industrial → (0.55, 0.5, 0.45). Unlit/Color opaque, как раньше.
   - BuildCube использует WallColor вместо прямого FallbackColor. def null невозможен (гард в AddBuilding), но метод держать null-safe (null → FallbackColor).
   - Остальное (ужатие, высоты, крыша, префаб-путь) не трогать.

## Что не нужно делать

- Не красить крыши по-разному, не делать окна/вывески. Не трогать остальное.

## Зависимости

TASKS/014, 022, 024 (Done). Ничего не блокирует.

## Изменяемые файлы

Изменить:
- Assets/Citybuilder/Scripts/Presentation/SettlementView.cs (только цвет стен)
- TASKS/README.md (статус 027)

Создать:
- TASKS/027-workplace-colors.md (этот файл)
- TASKS/027-breakdown.md

## Критерии готовности

- [ ] 1 файл, дифф ~15 строк, лимиты соблюдены.
- [ ] Rebuild-путь: дом бежевый, магазин синий, завод серый (сверка material.color впрямую).
- [ ] Скриншот из Play при наличии. Console чистый.

## Железные уроки (не нарушать)

- Только Unlit/Color. Refresh (scope all) + компиляция. Код правится только на стопе.

## План проверки

1. Unity: Refresh all, компиляция, Console чистый.
2. E2E через мост: AddBuilding тремя типами (CreateInstance) → сверка цветов стен; RemoveBuilding чистит.
