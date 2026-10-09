# Citybuilder Development Rules

## Language

- Общение с разработчиком вести только на русском языке.
- Код, названия классов, методов и переменных писать на английском языке.
- Комментарии в коде писать на русском только при необходимости.
- Инструкции по настройке Unity объяснять пошагово и простыми словами.

## Project

Citybuilder is a 3D city-building simulation made with Unity 6 and C#.

The game focuses on connected systems:
- construction;
- roads;
- economy;
- resources;
- infrastructure;
- transport;
- population;
- citizen needs.

## Roles

Architect (Orchestrator Muse Spark 1.3):
- plans architecture;
- analyzes dependencies;
- creates small tasks;
- updates documentation;
- does not implement large gameplay systems without approval.

Engineer (Muse Spark 1.3 — обычные задачи / Kimi K3 — сложный кодинг):
- implements one concrete task;
- studies existing code;
- writes tests when appropriate;
- explains changed files;
- provides Unity verification instructions;
- creates a Git commit after a successful implementation.
- 1 задача = 1 инженер. Kimi только сложный кодинг и эталонное качество.

## Code rules

- Use C#.
- Use namespace Citybuilder.
- Use PascalCase for classes and public members.
- Use camelCase for private fields and local variables.
- Keep MonoBehaviours small and focused.
- Prefer composition over large manager classes.
- File size limit: 1 C# file max 150 строк, жёсткий предел 200 строк. Превысил — разбить на 2 класса.
- Method limit: 1 метод max 30 строк. Длиннее — вынести приватные хелперы.
- 1 файл = 1 класс, код распределять равномерно по модулям, а не складывать в 1-2 файла.
- Do not create global singletons unless explicitly approved.
- Do not add third-party packages without approval.
- Do not modify ProjectSettings without explaining why.
- Do not delete existing files without explaining why.
- Do not put all game logic into one GameManager.
- Keep simulation state separate from Unity presentation.
- Use ScriptableObject for static definitions, not runtime city state.
- Prefer commands, queries and events for communication between systems.
- Avoid cyclic dependencies between systems.
- V1 запрещены без одобрения: сложные generics, reflection, async/await, multithreading, LINQ-цепочки длиннее 2 вызовов, DI-контейнеры, regex, ручная сериализация JSON.

## Unity rules

- Do not invent Unity APIs.
- If a Unity API may differ between versions, check current documentation (Context7 MCP / Unity MCP).
- Explain scene setup step by step.
- Do not assume that a GameObject, prefab or reference already exists.
- Clearly list required Inspector references. Каждый MonoBehaviour: [Header], Tooltip на неочевидных, проверка null.
- Keep Unity-specific code at the presentation or adapter layer when possible.
- Стандарт V1: Unity 6 LTS, 2 сцены (Bootstrap.unity, Prototype.unity), URP или Built-in выбирается 1 раз.

## Current milestone

Create a playable first vertical slice where the player can:
1. See a ground grid.
2. Select a building.
3. Preview the building.
4. Place it on the grid.
5. Prevent overlapping buildings.
6. Spend money when placing a building.
7. See current money in the UI.
8. Cancel placement mode.
9. Remove a building.

Scope lock V1: жителей нет, дорог нет, только дом 2x2. Population / Transport / Infrastructure / Roads запрещены до закрытия среза.

## Workflow

Before editing multiple files:
1. Explain the plan.
2. List files that will be created or changed.
3. List dependencies.
4. Explain how to test the result.
5. Ask for approval if the change is larger than one small task.

After implementation:
1. Summarize changes.
2. List created and modified files.
3. Explain Unity scene setup.
4. Explain how to test.
5. Report limitations.
6. Suggest a Git commit message.

## Git rules

- 1 задача = 1 ветка task/XXX-name = 1 PR.
- 1 коммит max 5 файлов и max 250 строк diff.
- 1 PR max 400 строк diff, только 1 модуль.
- Прямой пуш в main от инженеров запрещён, мержит только оркестратор.
- Формат коммита: Add / Fix / Update + что.
