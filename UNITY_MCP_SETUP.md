# Unity MCP — запуск и диагностика

Источник правды — **корень репозитория**. Именно его нужно открывать в Unity: `Assets/`, `Packages/` и `ProjectSettings/` лежат в git вместе с `.meta`. Никаких абсолютных путей к проекту в конфигурации быть не должно.

## Конфигурация

1. Скопируйте `Tools/mcp.example.json` в конфиг WorkBuddy AI (обычно `~/.workbuddy-ai/mcp.json`), раздел `mcpServers`.
2. При необходимости задайте переменные окружения:
   - `CITYBUILDER_UNITY_EXE` — полный путь к `Unity.exe`;
   - `CITYBUILDER_BLENDER_EXE` — полный путь к `blender.exe`.
   Если переменные не заданы, `Tools/Start-WorkBuddyStack.ps1` ищет редактор в стандартных каталогах, используя версию из `ProjectSettings/ProjectVersion.txt`.
3. Один раз в WorkBuddy AI: **Connectors → Custom Connector → Trust** для новых серверов. Без Trust прописанные серверы не активируются.

## Порядок запуска

1. Открыть Unity Hub → проект из корня репозитория. Мост `mcp-for-unity` стартует вместе с редактором на `127.0.0.1:8080`.
2. Подождать 10–30 секунд после загрузки проекта.
3. Для Blender запустить GUI (не `-b`) и включить аддон «Interface: MCP for Blender»; сокет `127.0.0.1:9876` поднимется сам.
4. Либо один раз запустить `Tools\Start-WorkBuddyStack.bat` — он поднимет Unity и Blender и дождётся обоих портов.

## Диагностика

```powershell
powershell -ExecutionPolicy Bypass -File Tools\Check-UnityMcp.ps1
```

| Проверка не прошла | Что делать |
|---|---|
| Unity не запущен | Открыть корень репозитория в Unity Hub |
| Порт 8080 не слушается | Подождать 30 секунд; проверить окно MCP в Unity; перезапустить Unity |
| Мост не отвечает | Дождаться перекомпиляции скриптов; иначе перезапустить Unity |
| Конфиг не найден | Скопировать `Tools/mcp.example.json` в `~/.workbuddy-ai/mcp.json` |
| `unityMCP` красный при зелёном порте | Нажать **Trust** в Custom Connector |
| Не корень проекта | Запускать скрипт из корня репозитория |
| Blender socket 9876 недоступен | В Blender: 3D View → `N` → **MCP for Blender** → **Start MCP Server** |
| Unity не найден лаунчером | Задать `CITYBUILDER_UNITY_EXE` |
| Blender не найден лаунчером | Задать `CITYBUILDER_BLENDER_EXE` |

Скрипт проверяет процесс Unity, порт 8080, HTTP-ответ моста, конфиг и наличие `unityMCP` / `context7` / `playwright` / `blender`. Статус `connected` внутри WorkBuddy извне не виден — он виден только в UI коннекторов.

## Серверы

| Сервер | Что нужно |
|---|---|
| `unityMCP` | Открытый Unity-редактор с пакетом `com.coplaydev.unity-mcp` (закреплён на `v10.3.0`). |
| `context7` | Удалённый `https://mcp.context7.com/mcp`; API-ключ необязателен. |
| `playwright` | Node 18+ и `@playwright/mcp` через `npx`. |
| `blender` | GUI Blender 4.5 LTS с включённым аддоном; команда `uvx mcp-for-blender`. |

## Конвейер 3D-моделей (Blender → Unity FBX, EPIC-20)

Для повторяемого экспорта low-poly зданий используется аддон `Tools/blender-addon/citybuilder_export.py` и генератор `Tools/blender-addon/generate_models.py`.

1. **Установка аддона в Blender:** `Edit → Preferences → Add-ons → Install from Disk...` → выбрать `Tools/blender-addon/citybuilder_export.py` и включить чекбокс **Import-Export: Citybuilder FBX Exporter**.
2. **Использование из GUI:** в `3D Viewport` нажать `N` → вкладка **Citybuilder** → **Export FBX**. Аддон применяет трансформы (`Location/Rotation/Scale`), проверяет бюджет полигонов (`≤ 2000 tris`) и экспортирует выбранные меши в `Assets/Citybuilder/Models/<id>.fbx` с фиксированными настройками (`scale 1.0`, `Y up`, `-Z forward`, `Bake Space Transform`, без анимаций и костей).
3. **Пакетная генерация без GUI:**
   ```powershell
   & "C:\Program Files\Blender Foundation\Blender 4.5\blender.exe" -b -P Tools\blender-addon\generate_models.py
   ```
4. **Хранение в Git:** low-poly `.fbx` модели (`≤ 30 КБ`) хранятся напрямую в обычном Git (`*.fbx binary` в `.gitattributes`) без Git LFS.

## История

Ранее использовался OpenCode и отдельный watcher в планировщике задач. С v1.1 эта инфраструктура не применяется: конфиг один, лаунчер один, watcher удалён из актуального процесса. Архивные задачи в `TASKS/` могут упоминать старые пути — это история, а не инструкция.
