# Онбординг: Citybuilder с нуля

Это публичный репозиторий, и **корень репозитория уже является проектом Unity**. Всё необходимое для запуска лежит в git: `Assets/` (вместе с `.meta`), `Packages/`, `ProjectSettings/`, сцены, модели, звуки и документация. Никаких дополнительных архивов, общих дисков и ручных копирований не требуется.

Папки `Library/`, `Temp/`, `Logs/`, `Obj/`, `UserSettings/`, `Builds/`, `Screenshots/` Unity создаёт сам — они в `.gitignore` и никогда не коммитятся.

## 1. Что установить

| Инструмент | Версия | Зачем |
| --- | --- | --- |
| Git | любая свежая | клонирование, ветки, PR и разрешение git-зависимости пакетов |
| Unity Hub + Unity Editor | **6000.3.25f1** (`e1dba0a9aba4`) | точная версия из `ProjectSettings/ProjectVersion.txt` |
| Python | 3.10+ | опционально: статический валидатор `Tools/ValidateRepo.py` |
| Build Support (Windows / macOS / Linux) | под свою ОС | только если нужно собрать билд; для Play не обязателен |

> При первом открытии нужен интернет: `Packages/manifest.json` тянет редакторский пакет `com.coplaydev.unity-mcp` по git-URL с GitHub. На саму игру он не влияет, но без установленного Git и сети Unity не сможет разресить зависимость.

## 2. Скачать проект

**Просто запустить / посмотреть:**

```bash
git clone https://github.com/albexx-dev/Citybuilder.git
cd Citybuilder
```

**Участвовать в разработке:** сначала нажмите **Fork** на странице репозитория, затем:

```bash
git clone https://github.com/<ВАШ_ЛОГИН>/Citybuilder.git
cd Citybuilder
git remote add upstream https://github.com/albexx-dev/Citybuilder.git
```

> Не используйте кнопку **Download ZIP**: без папки `.git` не будет ни веток, ни пулл-реквестов, а Unity не сможет подтянуть git-пакет. Модели `.fbx` и звуки `.wav` хранятся прямо в обычном git, поэтому **Git LFS для клонирования не нужен** (в репозитории сейчас 0 LFS-объектов).

## 3. Открыть и запустить

1. Unity Hub → **Add / Add project from disk** → выбрать **корень** папки `Citybuilder` (ту, где лежат `Assets/`, `Packages/`, `ProjectSettings/`), а не папку `Assets`.
2. Выбрать редактор `6000.3.25f1`. Если Hub предлагает открыть другой версией — поставьте нужную, не соглашайтесь на апгрейд проекта.
3. Первое открытие занимает 1–5 минут: Unity пересоздаёт `Library/` и компилирует скрипты. Не прерывайте процесс.
4. Открыть `Assets/Citybuilder/Scenes/Prototype.unity` (сцена уже прописана в Build Settings) → **Play** → в главном меню **«Новая игра»**.
5. Дальше на экране 7-шаговый туториал. Горячие клавиши: `1` Дома, `2` Дороги, `3` Зоны, `4` Рост, `Esc` Выкл / Пауза, `Tab` Статистика, `L` Лента событий. ЛКМ по зданию в режиме «Выкл» открывает инфопанель и радиус обслуживания.

## 4. Проверить, что версия полностью рабочая

```bash
python Tools/ValidateRepo.py
```

Ожидается строка `OK: N tracked files, M unique GUIDs, K warning(s)`. `WARN` про мягкий лимит в 150 строк на C#-файл — это не ошибка, `ERROR` быть не должно.

В Unity: **Window → General → Test Runner → EditMode → Run All**. Актуальная база — **434 теста**, все должны быть зелёными.

Сборка: **File → Build Settings** → сцена `Prototype.unity` уже добавлена → **Build**.

## 5. Типичные проблемы

| Симптом | Причина | Что делать |
| --- | --- | --- |
| `An error occurred while resolving packages` / `Git is not installed` | нет Git в PATH или нет интернета | установить Git, перезапустить Unity, дождаться восстановления пакетов |
| «Project was created with a different Unity version» | opened не той версией | поставить ровно `6000.3.25f1` через Unity Hub |
| Розовые материалы, потерянные ссылки на скрипты | не хватает `.meta` или ассет переименован вне Unity | переименовывать только внутри Unity, коммитить ассет вместе с `.meta` |
| `Library/` разрослась на гигабайты | это локальный кеш импорта | нормально, в git не попадает; можно удалить при закрытом Unity |
| Нет моделей/звуков | файлы не скачались | проверить, что `Assets/Citybuilder/Models/*.fbx` и `Audio/*.wav` не являются текстовыми LFS-указателями |
| CI красный | локально не проходит валидатор | запустить `python Tools/ValidateRepo.py` и исправить `ERROR` |
| Консоль Unity пугает предупреждением про Input Manager | штатный deprecated-warning | не ошибка, игра работает |

## 6. Как стать контрибьютором

Репозиторий публичный, поэтому **fork + пулл-реквест работает даже без приглашения**. Если владелец прислал приглашение collaborator — примите его в уведомлениях GitHub (или по письму), после чего можно пушить ветки напрямую в общий репозиторий.

Ветка `main` защищена: обязателен зелёный чек **Static validation**, ветка должна быть актуальной, force-push и удаление ветки запрещены. Поэтому все изменения идут через PR.

```bash
git checkout -b task/XXX-short-name
# ... правки ...
python Tools/ValidateRepo.py
git add .
git commit -m "task XXX: короткое описание"
git push -u origin task/XXX-short-name
```

Дальше на GitHub: **Compare & pull request** → base `albexx-dev/Citybuilder:main` → заполнить шаблон PR → дождаться зелёного `Static validation` → ревью от `@albexx-dev` (см. `.github/CODEOWNERS`) → merge.

Подтянуть свежий `main` в форк:

```bash
git fetch upstream
git rebase upstream/main
```

Правила кода и формат коммитов — в [CONTRIBUTING.md](CONTRIBUTING.md), архитектура — [ARCHITECTURE.md](ARCHITECTURE.md), игровые правила — [GAME_DESIGN.md](GAME_DESIGN.md), желаемая задача — в GitHub Issues.

## 7. Что коммитить, а что нет

| Коммитим | Не коммитим |
| --- | --- |
| `Assets/**`, **обязательно вместе с `.meta`** | `Library/`, `Temp/`, `Logs/`, `Obj/`, `UserSettings/` |
| `Packages/manifest.json` и `packages-lock.json` | `Builds/`, `Screenshots/`, `*.apk`, `*.unitypackage` |
| `ProjectSettings/**` (только по согласованию) | `*.csproj`, `*.sln`, `.vs/`, `.idea/` |
| документацию, `Tools/`, `.github/` | локальные конфиги MCP, ключи и пароли |

Любой новый ассет порождает файл `<имя>.meta` — коммитьте оба. Изменения в `ProjectSettings/` и новые сторонние пакеты требуют отдельной договорённости (см. `CONTRIBUTING.md` и `DECISIONS/`).

## 8. MCP-стек — необязательно

Unity/Blender MCP нужен только для автоматизированной работы с редактором и экспорта 3D-моделей. Играть, собирать билд и писать код без него можно. Подключение: [UNITY_MCP_SETUP.md](UNITY_MCP_SETUP.md), пример конфигурации — `Tools/mcp.example.json` (в нём нет абсолютных и персональных путей).

## 9. Лицензия

MIT, см. [LICENSE](LICENSE).
