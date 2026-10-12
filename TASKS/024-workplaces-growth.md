# TASKS/024 — Workplaces Growth

## Цель

Рост выращивает не только дома, а тип под зону: жилая → House, коммерческая → Shop, промышленная → Factory. Рабочие места появляются → налоги замыкаются.

## Контекст

Зависит от 013 (AllowedZone/JobsProvided), 014 (рост), 023 (налоги). Сейчас рост растит один Definition. Типы Shop/Factory (ScriptableObject-ассеты) создаёт оркестратор в Unity через мост (как House — ассеты не в git, только скрипты).

## Что нужно сделать

1. `SettlementGrowthController` (изменить, модуль Population):
   - Поле `Definition` ЗАМЕНИТЬ на `Definitions : List<BuildingDefinition>` (Tooltip «Типы под зоны. Пусто = рост не работает»). Старое поле удалить (сцену перепривяжет оркестратор).
   - EnsureReady: `Definitions == null || Count == 0` (после вычистки null) → LogError + enabled=false. Null-элементы списка вычистить молча в EnsureReady.
   - Поиск: `FindFreeOrigin(out BuildingDefinition def, out GridPosition origin)` — перебор зонных клеток-кандидатов; для каждой клетки перебор Definitions: пропуск def null и def с AllowedZone != зоне клетки (None-здания — пропуск? Решение: AllowedZone==None считается подходящим везде, как в SettlementSystem.CanSettle. Не менять семантику CanSettle).
   - Первый подошедший (def, origin) → `settlement.CanSettle` уже внутри? CanSettle проверяет зону+свободу — использовать его как финальный фильтр (как сейчас).
   - TryGrow: Settle найденным def + View.AddBuilding(state, def, ...) этим же def + RefreshOccupied. MaxSettlements/таймер/снос без изменений.
   - using System.Collections.Generic уже есть (List используется).

## Что не нужно делать

- Не создавать .asset-файлы (их делает оркестратор мостом; House — прецедент).
- Не красить рабочие места в другие цвета (следующая задача при нужде).
- Не трогать вместимость/налоги/остальное. Input не нужен.

## Зависимости

TASKS/013, 014, 023 (Done). Замыкает цепочку зоны→рост→jobs→налоги.

## Изменяемые файлы

Изменить:
- Assets/Citybuilder/Scripts/Population/SettlementGrowthController.cs (Definitions + подбор по зоне)
- TASKS/README.md (статус 024)

Создать:
- TASKS/024-workplaces-growth.md (этот файл)
- TASKS/024-breakdown.md

## Критерии готовности

- [ ] 1 файл, дифф компактный (было 137 строк; следить за лимитом 150 — при переполнении вынести FindFreeOrigin-хелперы, методы до 30).
- [ ] Definitions=[House(R), Shop(C)]: жилая клетка растит House, коммерческая — Shop; пустой список — рост отключён с ошибкой.
- [ ] Console без ошибок. Правила кода соблюдены.

## Железные уроки (не нарушать)

- Refresh (scope all) + компиляция. Сцену (Definitions=[House,Shop,Factory]) — только при выключенном Play, делает оркестратор.

## План проверки

1. Unity: Refresh all, компиляция, Console чистый.
2. E2E через мост: Shop/CreateInstance-подмена в Definitions, жилая+коммерческая полосы → выросли оба типа; TotalJobs > 0; Collect дал деньги. Сцена: Shop.asset + Factory.asset мостом, Definitions=[House,Shop,Factory], сохранить.
