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

Роли — субагенты (не отдельные сессии): с 06.11.2026 оркестратор — WorkBuddy AI, запускает их инструментом Agent.

Architect / Orchestrator (постоянный чат):
- plans architecture;
- analyzes dependencies;
- creates small tasks;
- updates documentation;
- does not implement large gameplay systems without approval;
- принимает работу в Unity через мост, мержит и пушит;
- ведёт TASKS/README.md и CHANGELOG.md.

Engineer (субагент, одна задача):
- implements one concrete task;
- studies existing code;
- writes tests when appropriate;
- explains changed files;
- provides Unity verification instructions;
- creates a Git commit after a successful implementation.
- 1 задача = 1 инженер = 1 субагент. Не пушит, не мержит, Unity не трогает.

Reviewer (субагент, только чтение):
- проверяет коммит инженера против AGENTS.md и breakdown;
- выносит PASS/FAIL по пунктам с файл:строка;
- не меняет код, не коммитит, не пушит, Unity не трогает.
- Обязателен после КАЖДОЙ задачи инженера.

Почему субагенты, а не одноразовые чаты: субагент тоже читает файлы один раз, но его отчёт
попадает в оркестратора автоматически (без копипаста), а при доработке можно возобновить того же
агента — без повторного чтения репозитория. Отдельная сессия оправдана только когда
нужна параллельность, другая модель или очень длинная задача.

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
- Стандарт: Unity 6 LTS **6000.3.25f1** (не обновлять), Built-in Render Pipeline, одна игровая сцена `Assets/Citybuilder/Scenes/Prototype.unity`.

## Правило видимости системы

Новая система не начинается, пока предыдущая не видна игроку.

«Готово» — это когда игрок это чувствует: HUD, поведение зданий, деньги, время. Не «класс есть, тесты зелёные, система где-то подключена».

Перед стартом задачи сформулируй **одно действие игрока**, после которого изменение заметно.
Не формулируется — задача не начинается.

## Текущее состояние (v1.1, после задачи 104)

`HANDOFF.md` удалён в v1.1. Актуальные точки входа: `README.md`, `CHANGELOG.md`, `TASKS/README.md`, `DECISIONS/`. Исторические breakdown-файлы в `TASKS/` — архив.

Играбельно в редакторе и в билде windows64 (90 МиБ, пересобран на 102, smoke пройден):

- сетка 50×50, зоны: жилая / коммерция / промзона;
- рост домов, магазинов и заводов по тикам, плата за рост из казны;
- налоговая политика 1/2/3, автовъезд с гейтом занятости 0.4;
- снос зданий и дорог, ручная установка станций;
- энергия, вода, мусор — общий `SupplyBalance`; пожарные и полиция — радиус покрытия;
- дефицит энергии / воды / мусора замедляет рост (×1/×2/×3, задаёт худшая система), станции возвращают темп; пожарные и полиция пока только в HUD;
- связность дорог (доля связанных домов) и пробки (A* + нагрузка клеток, влияют на скорость роста);
- HUD: часы, население, потребности (7 показателей), цель со ступенями 50/100/200, связность, энергия, вода, мусор, пожарные, полиция, «Дальше: …»;
- стартовая подсказка ведёт «зоны → рост → первый дом» и гаснет после первого здания;
- статус роста называет причину остановки (режим / зоны / деньги / потолок) и часы до следующего дома;
- кнопка налога показывает фактический сбор «+N $/ч», «Новая» сбрасывает политику на обычную;
- сохранение и загрузка, снапшот v4 (совместим с v1–v3), домохозяйства привязаны к дому по координатам;
- разблокировка типов зданий по числу выросших зданий.

288 EditMode-тестов в 39 файлах на момент задачи 104; новые тесты валидации сейвов добавляются в v1.1.

С v1.1 инфраструктура MCP работает на WorkBuddy AI: шаблон конфига `Tools/mcp.example.json`, диагностика `Tools/Check-UnityMcp.ps1`, запуск стека `Tools/Start-WorkBuddyStack.bat`. Абсолютные пути заменены переменными окружения `CITYBUILDER_UNITY_EXE` / `CITYBUILDER_BLENDER_EXE` и автоопределением от корня репо. OpenCode-watcher более не используется.

## Scope lock

Разрешено в v1.1 (эпики, очередь в GitHub Issues): службы v2 по ADR-006 (больница, парк), нужды v2, содержание зданий и привлекательность, событийная лента (ADR-007), сейвы v2 — 3 слота + автосейв (ADR-008), UI v2 — нижняя панель действий и информационное меню на IMGUI, видимость пробок и покрытия, баланс, Blender-конвейер, сплиты крупных файлов, релизный процесс, документация, багфиксы, тесты.

Доктрина: партия не имеет конца (GAME_DESIGN §19) — экранов победы/поражения нет, `GoalSystem.Won/Lost` удаляется.

Запрещено без ADR: транспорт с машинами, многоуровневость, облачные сейвы, Jobs/Burst, DI-контейнеры, async/await, новые сторонние пакеты, включая UI Toolkit — UI v2 остаётся на IMGUI.

## Жирные задачи (эпики)

С v1.1 крупные изменения планируются **эпиками**. Эпик = GitHub issue с ярлыком темы (`mechanics` / `architecture` / `ux` / `content` / `release`).

- Эпик живёт несколько сессий; внутри — шаги по обычному циклу: 1 шаг = 1 ветка `task/XXX` = 1 breakdown-файл + инженер + ревьюер + зелёный шлюз.
- Лимиты (файл ≤150/200 строк, метод ≤30, коммит ≤5 файлов/≤250 строк) действуют на **шаг**, а не на эпик.
- Чек-лист шагов ведётся в теле issue; `TASKS/README.md` ссылается на issue.
- Эпик закрывается, когда его системы видны игроку и шлюзы всех шагов зелёные.
- Новый архитектурный паттерн внутри эпика — через ADR в `DECISIONS/` до первого шага с кодом.

## Workflow (субагенты)

Оркестратор сам запускает инженера и ревьюера через Agent tool — без ручного копипаста между чатами.

Цикл одной задачи:
1. Оркестратор пишет `TASKS/XXX-breakdown.md` и коммитит в `task/XXX`.
2. Оркестратор запускает субагента-инженера с брифом → инженер коммитит в ту же ветку.
3. Оркестратор запускает субагента-ревьюера (read-only) → PASS/FAIL.
4. Оркестратор читает дифф, принимает в Unity (Refresh, Play-E2E, тесты), мержит и пушит.
5. Ветка удаляется, очередь обновляется.

Бриф инженера обязан содержать: путь и ветку, ссылку на breakdown, список файлов для чтения (а не «изучи репозиторий»), запрещённые файлы, лимиты, требование «не пушить/не мержить/Unity не трогать», формат компактного отчёта.
Бриф ревьюера обязан содержать: ветку, хеши коммитов, пункты проверки, требование «только чтение».
Экономия токенов: читать только перечисленные файлы, диффы в чат не вставлять (оркестратор читает их через git), отчёт — размерами и выводами, без пересказа кода.

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

- 1 задача = 1 ветка task/XXX-name = 1 локальный мерж. PR не обязательны, но CI `.github/workflows/ci.yml` должен быть зелёным; мержит только оркестратор через `git merge --no-ff`.
- Каждая задача заканчивается пушем main в origin (бэкап!). Без пуша задача не закрыта.
- Смерженные task-ветки удаляются (локально и в origin) — сразу после мержа или следующей задачей-процесса.
- 1 коммит max 5 файлов и max 250 строк diff (инфра-импорты — зафиксированное исключение).
- 1 мерж max 400 строк diff, только 1 модуль (инфра — исключение с обоснованием).
- Прямой пуш в main от инженеров запрещён, мержит только оркестратор. Планы и очередь — только в task-ветке, в main только мерж.
- Формат коммита: Add / Fix / Update + что.
- Идентичность коммитов и пушей: все коммиты и пуши делаются от имени владельца репозитория — `avexx-dev <tamerlanturagul@gmail.com>` (git config user.name/user.email настроены глобально и локально). Оркестратор и инженеры не подписывают коммиты своими именами.

## Приёмка задачи (чек-лист оркестратора)

**Шлюз качества: мерж только после зелёного шлюза.** Коммит инженера — кандидат, а не готовая работа.
Пока шлюз красный, мержа нет: задача возвращается тому же инженеру (возобновлением агента) с логом ошибок.

1. Дифф ветки прочитан (только разрешённые модули/файлы).
2. Компиляция чистая на стопе после мержа (Refresh scope all при нужде); правки кода — только через git, ручной копии в Unity нет.
3. Сцена тронута только на стопе; сохранена оркестратором; изменения сцены — только свои.
4. Play-E2E по плану инженера (+ скриншот при UI-изменениях); Stop; Console без ошибок и варнингов.
5. Тесты EditMode зелёные (если задача трогала логику).
6. Мерж --no-ff в main, очередь обновлена, пуш origin main.

Обязанности инженера до коммита: если задача чисто логическая — прогнать свои тесты через `dotnet`/`csc` во временной папке (без Unity) и приложить результат. Unity-специфику (компиляция сборок проекта, EditMode, Play) проверяет только оркестратор — редактор один на машину.
