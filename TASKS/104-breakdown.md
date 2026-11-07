# TASKS/104-breakdown — документация к факту после перехода на WorkBuddy AI

Дата: 07.11.2026. Ветка: `task/104-docs-truth`. Модуль: документация / инфраструктура. C#-код, сцена и ProjectSettings не тронуты.

## Ввод

К `main` (`00c3432`) накопились расхождения между документами и реальностью:
- `HANDOFF.md §1` всё ещё называет следующим шагом пункт 1 чек-листа 093 (закрыт задачей 097);
- роли и шаблоны брифов описаны под OpenCode (`subagent`, `sessionID`, «этот чат»);
- §6 утверждает, что инфраструктура MCP «висит незакоммиченной» (закоммичена 103);
- §8 не знает про `Start-WorkBuddyStack.bat` и Trust;
- `AGENTS.md` — «Текущее состояние (на 102)», `subagent`, `sessionID`;
- `ADR-004` до сих пор «frozen until release», хотя релиз состоялся на 047, а замороженные системы реализованы в 053–102;
- подпись коммитов `avexx-dev` расходится с аккаунтом `albexx-dev`;
- задача планировщика «OpenCode Unity MCP Watcher» (Ready) дёргает `opencode reload`.

## Действие (правило видимости)

Инфраструктурно-документационная задача, игрок её не видит — правило видимости системы не применяется.
Проверяемое действие оркестратора: открыть `HANDOFF.md`, `AGENTS.md`, `ADR-004` и `TASKS/README.md` —
в них нет утверждений про OpenCode как про текущий инструмент, статус 093/102 зафиксирован как закрытый,
`NEXT` указывает на свободные направления после 104.

## Шаги

- [x] `HANDOFF.md`: шапка на 103/104, роли под WorkBuddy AI (Agent tool), сноска о подписи, пункт про 103 в §3, §6/§7/§8/§9/§10 актуализированы; после ревью исправлены «104 в работе» → «закрыта» и leftover `sessionID` в §7.
- [x] `AGENTS.md`: роли, «Текущее состояние (на 103)», строка про MCP-инфраструктуру, `Agent tool` и возобновление агента вместо `sessionID`.
- [x] `DECISIONS/ADR-004-min-release-scope.md`: раздел «Post-release amendment» — заморозка снята после 047, актуальные рамки в `AGENTS.md` → Scope lock, оригинальное решение не переписано.
- [x] `TASKS/README.md`: строка 104 `Done`, новый `NEXT`.
- [x] Watcher «OpenCode Unity MCP Watcher» отключён (не удалён) через `Disable-ScheduledTask`; команда включения/удаления осталась в `UNITY_MCP_SETUP.md`.
- [x] Проверка: диагностика 6/6 OK, Unity `read_console` чист, `telemetry_status` = true, Blender `get_scene_info` = сцена `Scene` / 3 объекта, Unity→Blender bridge `blender_reachable: true`.

## Scope lock

Разрешено: `HANDOFF.md`, `AGENTS.md`, `DECISIONS/ADR-004-min-release-scope.md`, `TASKS/104-breakdown.md`, строка очереди в `TASKS/README.md`.
Запрещено: `Assets/`, `ProjectSettings/`, сцена, любые `.cs`, `Tools/*` (инфраструктура уже принята 103), переписывание истории git и смена `git config`.

## Приёмка

Шлюз упрощён обоснованно, отклонение от полного шлюза `HANDOFF.md §7` зафиксировано оркестратором явно (прецедент — задача 103): C# и сцена не менялись → компиляция Unity, EditMode и Play-E2E не требуются (`AGENTS.md`, п. 5: «если задача трогала логику»). Вместо них — зелёная диагностика, чистый Unity Console через нативный MCP-инструмент, живой Blender, ревьюер PASS, дифф только по разрешённым файлам.
Лимиты: коммит ≤5 файлов / 250 строк diff, мерж ≤400 строк / 1 модуль.

## Вне задачи

Смена `git config user.name` на `albexx-dev` (требует решения владельца, историю не переписываем);
выравнивание аддона Blender под ожидаемый Unity `addon.py` (сейчас `blender_mcp.py`, `blender_bridge` сообщает `installed_addon_found: false`, но сам Blender MCP работает);
косметические замечания ревьюера 103 к `Start-WorkBuddyStack.ps1`.
