# TASKS/020 — Employment Foundation

## Цель

Фундамент работы: сколько жителей трудоустроено (мин. из жителей и рабочих мест), сколько безработных, rate. Чистые запросы без состояния. База для налогов (021).

## Контекст

GAME_DESIGN §10 (рабочие места в зданиях). JobsProvided на BuildingDefinition есть с 013, SettlementSystem считает TotalJobs, PopulationSystem — TotalResidents. Назначать конкретных людей на конкретные места — позже; V1: агрегаты. Дети/пенсионеры (нетрудоспособные) — позже, сейчас трудоспособны все.

## Что нужно сделать

1. `EmploymentSystem` (plain C#, модуль Population): только запросы поверх готовых счётчиков, своего изменяемого состояния нет.
   - Конструктор `(PopulationSystem population, SettlementSystem settlement)` — оба обязательны (null → ArgumentNullException). Направление: внутри Population-модуля, циклов нет.
   - `Employed` — `Math.Min(pop.TotalResidents, settlement.TotalJobs)` (система/null невозможны после конструктора, но счётчики читаются живьём каждый раз).
   - `Unemployed` — `TotalResidents − Employed` (флор 0).
   - `EmploymentRate` (float 0..1: residents==0 → 0, иначе Employed/(float)TotalResidents), `JobsFillRate` (float 0..1: jobs==0 → 0, иначе Employed/(float)TotalJobs).

## Что не нужно делать

- Не делать персоны, профессии, поездки на работу, безработицу как симуляцию (позже).
- Не делать налоги/списания (задача 021).
- Не трогать Construction/World/Economy/UI/Presentation/Simulation. Input не нужен.

## Зависимости

TASKS/013 (TotalJobs), 016 (TotalResidents). Блокирует 021.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Population/EmploymentSystem.cs
- TASKS/020-employment-foundation.md (этот файл)
- TASKS/020-breakdown.md

Изменить:
- TASKS/README.md (статус 020)

## Критерии готовности

- [ ] 1 новый файл до 150 строк, чистый C# без Unity, без своего состояния.
- [ ] Residents 8, Jobs 6 → Employed 6, Unemployed 2, Rate 0.75, Fill 1.0. Residents 0 → всё нули. Jobs 0 → Employed 0, rates 0.
- [ ] Console без ошибок. Code rules: namespace Citybuilder, 1 файл = 1 класс, без LINQ >2.

## Железные уроки (не нарушать)

- Только Unlit/Color (материалов нет).
- Refresh (scope all) + компиляция после копирования.
- Сцену трогай только при выключенном Play.

## План проверки

1. dotnet-прогон инженера + шаги в Unity-редакторе через мост (живой стек grid/zone/build/settle/movein).
2. Unity: Refresh all, компиляция, Console чистый, сцена не меняется.
