# TASKS/103-breakdown — перенос MCP-инфраструктуры с OpenCode на WorkBuddy AI

Дата: 07.11.2026. Ветка: `task/103-workbuddy-migration`. Модуль: инфраструктура MCP / документация. C#-код, сцена и ProjectSettings не тронуты.

## Ввод

С 06.11.2026 оркестратор — WorkBuddy AI, но инфраструктура осталась заточена под OpenCode:
`Tools/Check-UnityMcp.ps1` проверял `~/.config/opencode/opencode.json` и `opencode mcp list`,
`UNITY_MCP_SETUP.md` предписывал `opencode reload`, а конфиг MCP был только в OpenCode.
Дополнительно `Get-NetTCPConnection` через CIM не видел loopback-листенер 8080 и давал ложный FAIL.

## Действие (правило видимости)

Задача инфраструктурная, игрок её не видит — правило видимости системы к ней не применяется.
Проверяемое действие оркестратора: запустить Unity Editor и Blender GUI, затем из корня репо
выполнить `powershell -ExecutionPolicy Bypass -File Tools\Check-UnityMcp.ps1` → 6/6 `[OK]`, `exit 0`.
Плюс два handshake: `initialize` на `http://127.0.0.1:8080/mcp` → 200 с `mcp-session-id`;
JSON `get_scene_info` на `127.0.0.1:9876` → `status: success` с составом сцены.

## Шаги

- [x] Конфиг WorkBuddy AI `~/.workbuddy-ai/mcp.json`: `unityMCP` (8080), `context7`, `playwright`, `blender` (вне репо).
- [x] Blender 4.5.10 LTS + аддон `blender_mcp` установлен и включён (вне репо).
- [x] `Tools/Check-UnityMcp.ps1`: проверка `~/.workbuddy-ai/mcp.json` вместо opencode.json, проверка всех 4 серверов, подсказка про Trust, TCP-коннект вместо `Get-NetTCPConnection`.
- [x] `UNITY_MCP_SETUP.md`: конфиг WorkBuddy, шаг Trust, таблица неисправностей, раздел по остальным MCP-серверам, watcher помечен legacy.
- [x] `Tools/Start-WorkBuddyStack.ps1` + `.bat`: один запуск поднимает Unity и Blender и ждёт оба порта; не трогает сцены, код и git.
- [x] `uv`/`uvx` добавлены в User PATH (вне репо).
- [x] Проверка: диагностика 6/6 OK; Unity `mcp-for-unity-server 3.4.11` ответил initialize; Blender вернул сцену (Cube / Light / Camera).

## Scope lock

Разрешено: `Tools/*`, `UNITY_MCP_SETUP.md`, `TASKS/103-breakdown.md`, строка очереди в `TASKS/README.md`.
Запрещено: `Assets/`, `ProjectSettings/`, сцена `Prototype.unity`, любые `.cs`, существующие отчёты и ADR.

## Приёмка

Шлюз упрощён обоснованно: C# и сцена не менялись → компиляция Unity, EditMode и Play-E2E не требуются
(`AGENTS.md`, чек-лист приёмки, п. 5: «если задача трогала логику»). Отклонение от полного шлюза
`HANDOFF.md §7` зафиксировано оркестратором явно. Вместо них: `Check-UnityMcp.ps1` = `exit 0`,
MCP-initialize 200, Blender socket `status: success`, ревьюер PASS, дифф только по разрешённым файлам.
Лимиты: коммит ≤5 файлов / 250 строк diff, мерж ≤400 строк / 1 модуль.

## Вне задачи (кандидаты на 104)

`HANDOFF.md §1` устаревший «Следующий шаг: пункт 1 чек-листа 093» (закрыт 097); `ADR-004` всё ещё
«frozen» при реализованных транспорте/ресурсах/службах; подпись коммитов `avexx-dev` ≠ аккаунт
`albexx-dev`; задача планировщика «OpenCode Unity MCP Watcher» в статусе Ready.
