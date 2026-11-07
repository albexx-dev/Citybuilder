# Unity MCP — запуск и чинка навсегда

Источник правды — **корень репо**
`C:\Users\admin\Citybuilder` (с задачи 039 репо само является
Unity-проектом: Assets/Packages/ProjectSettings лежат в git).
Старый путь `C:\Users\admin\UnityProjects\CityBuilder` — архивный,
оркестратор удаляет его при переезде (бэкап-переименование).

С **06.11.2026** работа ведётся из **WorkBuddy AI**: конфиг MCP —
`C:\Users\admin\.workbuddy-ai\mcp.json`, раздел `mcpServers`. OpenCode больше
не используется; упоминания ниже оставлены как история и для watcher'а.

## Правильный порядок запуска

1. Открыть Unity Hub → проект **CityBuilder**.
   Мост `mcp-for-unity` стартует сам вместе с редактором (порт `127.0.0.1:8080`).
2. Дождаться открытия проекта (мост появляется через ~10–30 сек после запуска).
3. Один раз в WorkBuddy AI: Коннекторы → **Custom Connector** (справа вверху) → **Trust**
   на новых серверах. Без Trust прописанные в конфиге серверы не активируются.
4. Мост живёт только пока открыт Unity. Если редактор перезапустить — подождать 10–30 сек.

## Если снова красный

Запустить проверку из корня репо:

```powershell
powershell -ExecutionPolicy Bypass -File Tools\Check-UnityMcp.ps1
```

| Проверка не прошла | Что делать |
|---|---|
| Unity не запущен | Открыть проект CityBuilder в Unity Hub |
| Порт 8080 не слушается | Подождать 30 сек; проверить окно MCP в Unity; перезапустить Unity |
| Мост не отвечает | Unity перекомпилирует скрипты — подождать; иначе перезапустить Unity |
| Конфиг WorkBuddy не найден | `~/.workbuddy-ai/mcp.json`, `mcpServers.unityMCP.url = http://127.0.0.1:8080/mcp` |
| unityMCP красный при зелёном порте | Нажать **Trust** в Custom Connector (новые серверы не включаются сами) |
| Не корень проекта | Запускать скрипт из корня репо `C:\Users\admin\Citybuilder` |

## Почему так было (корни проблемы, 2026-10-10)

1. Мост жив только пока открыт Unity. Сервис OpenCode пробовал подключиться,
   когда Unity был закрыт, и запоминал провал — лечится рестартом сервиса.
2. Было два глобальных конфига (`opencode.json` + `opencode.jsonc`) с разным
   форматом — объединены в один `opencode.json` (формат V2 `mcp.servers`).
   Старый файл лежит в `opencode.jsonc.bak`.
3. Корень репо — сам Unity-проект (с 039). Дубля скриптов больше нет:
   правило «править в одном месте» выполняется само, скрипт это не проверяет.

## Быстрая проверка связи

```powershell
powershell -ExecutionPolicy Bypass -File Tools\Check-UnityMcp.ps1
```

Скрипт проверяет процесс Unity, порт 8080, HTTP-ответ моста и конфиг WorkBuddy AI
(в т.ч. наличие `unityMCP` / `context7` / `playwright` / `blender`). Статус
`connected` внутри WorkBuddy извне не виден — его видно только в UI коннекторов.

## Автовосстановление (watcher) — legacy OpenCode

> ⚠️ Этот watcher дёргает `opencode reload` и для WorkBuddy AI **не нужен**.
> Если OpenCode окончательно не используется, задачу можно удалить командой ниже.

OpenCode подключает MCP-серверы только при старте, поэтому если Unity поднимается позже,
`unityMCP` остаётся `failed` до ручного `opencode reload`.

С 2026-10-25 это лечится автоматически:

- скрипт: `C:\Users\admin\.config\opencode\unity-mcp-watch.ps1`
- задача планировщика: **OpenCode Unity MCP Watcher** (триггер — вход в систему)
- лог: `C:\Users\admin\.config\opencode\unity-mcp-watch.log`

Watcher раз в 5 секунд смотрит порт 8080. Когда мост появляется (Unity запустили или
перезапустили), он проверяет статус `unityMCP` и при `failed` сам вызывает `opencode reload`.

Управление:

```powershell
Get-ScheduledTask -TaskName "OpenCode Unity MCP Watcher" | Select-Object TaskName,State
Start-ScheduledTask -TaskName "OpenCode Unity MCP Watcher"
Stop-ScheduledTask  -TaskName "OpenCode Unity MCP Watcher"
Unregister-ScheduledTask -TaskName "OpenCode Unity MCP Watcher" -Confirm:$false   # отключить навсегда
```

## Остальные MCP-серверы (в `~/.workbuddy-ai/mcp.json`)

| Сервер | Что нужно для работы |
|---|---|
| `context7` | Ничего, удалённый `https://mcp.context7.com/mcp`. Для повышенных лимитов — API-ключ с `context7.com/dashboard` в заголовке `Authorization: Bearer ...` (не обязательно). |
| `playwright` | Node 18+ и пакет `@playwright/mcp` (подтягивается `npx` сам); браузеры Chromium уже установлены. |
| `blender` | Запущенный **GUI** Blender 4.5 LTS с включённым аддоном «Interface: MCP for Blender». Сокет-сервер на `127.0.0.1:9876` стартует сам, в `-b` (background) — нет. Команда сервера: `uvx mcp-for-blender` (полный путь к `uvx.exe` прописан в конфиге). |

Проверка панели в Blender: навести на 3D-вьюпорт → `N` → вкладка **MCP for Blender**
→ статус `Server Running` / кнопка `Start MCP Server`.
