# TASKS/006 — Building Definition

## Цель

Описать тип здания через ScriptableObject. Первая версия — только данные для стройки.

## Контекст

Зависит от 002 (примитивы) и 004 (размеры в клетках). Жители, рабочие места, производство — после среза.

## Что нужно сделать

`BuildingDefinition : ScriptableObject` (модуль Construction, UnityEngine разрешён):
- Поля: `id` (string), `displayName` (string), `footprint` (Vector2Int, клеток), `constructionCost` (int), `prefab` (GameObject).
- `[CreateAssetMenu(menuName = "Citybuilder/Building Definition")]`.
- `OnValidate`: id не пустой, footprint x/y >= 1, cost >= 0. Некорректное — чинить Clamp + Warn в Console.
- Метод `CoversCells(GridPosition origin)` → List<GridPosition> всех клеток под зданием.
- Пример ассета: Data/Buildings/House (id "house", 2x2, cost 100) — создать в Unity вручную по шагам.

## Что не нужно делать

- Не добавлять жителей, jobs, производство, электричество.
- Не создавать BuildingState/System/View (задача 007).
- Не трогать Core/World/Simulation/Presentation.

## Зависимости

TASKS/002, 004 (Done). Блокирует 007.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Construction/BuildingDefinition.cs
- TASKS/006-building-definition.md (этот файл)
- TASKS/006-breakdown.md

Изменить:
- TASKS/README.md (статус 006)

## Критерии готовности

- [ ] Файл до 150 строк, 1 класс = 1 файл.
- [ ] Ассет House создаётся через Create-меню, поля видны в Inspector.
- [ ] CoversCells((3,3)) для 2x2 = 4 клетки: (3,3),(4,3),(3,4),(4,4).
- [ ] Отрицательный cost в Inspector правится в 0 с предупреждением.

## План проверки

1. Console Unity без ошибок после компиляции.
2. House.asset создан, CoversCells проверен dotnet-прогоном логики.
3. Проверка границ: footprint 0x0 отклоняется в OnValidate.
