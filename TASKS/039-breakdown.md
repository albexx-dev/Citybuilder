# TASKS/039-breakdown

Ввод: задача 039, ИНФРА (исключение из «1 модуль / 400 строк»: bulk-импорт + сгенерированные файлы, своего кода минимум). P0-lite + тесты одним мержем: разрывать нельзя — тестовый asmdef ссылается на игровой через GUID/.meta. Исполнитель: Engineer (Muse, субагент) — этапы A–C коммитами в ветку; переоткрытие Unity, E2E и пуш — за оркестратором.
Запрещено трогать: геймплейный код (ноль правок .cs кроме переноса), Tools/Check-UnityMcp.ps1 (чужая грязная правка — не трогать).
Сдать: 3 коммита в ветку task/039-p0-lite-tests + отчёт. В main НЕ коммитить, НЕ пушить, Unity НЕ трогать и НЕ переоткрывать.

Контекст: origin/main = be17a49 (срез 012), локально 038 — бэкапа нет. Живой проект: C:\Users\admin\UnityProjects\CityBuilder (Unity 6.0.3.25f1, MCP-пин v10.3.0). В git сейчас только скрипты. Проверено оркестратором: Packages+ProjectSettings = 24 файла/0.06 МБ; .gitignore стандартный (.meta можно); git-lfs 3.7.1 есть; бинарников в Assets нет (31 .png — E2E-скриншоты, в git НЕ брать); сцены Prototype.unity + CityBuilder.unity; ассеты House/Shop/Factory/AsphaltRoad.

- [ ] Этап A — импорт (коммит «Import Unity project structure with meta»):
  - Скопировать в корень репо: Assets/ (КРОМЕ Assets/Screenshots), Packages/, ProjectSettings/ — включая все .meta. Скрипты Assets/Citybuilder/Scripts уже в git и равны (синхро-чек 5/5) — копия побайтово поверх, сверить хеши после.
  - .gitignore: добавить `Assets/Screenshots/` (E2E-артефакты, только локально).
  - .gitattributes: LFS-паттерны на будущее (*.fbx *.obj *.png *.wav *.mp3 *.exr *.tga *.psd *.zip filter=lfs diff=lfs merge=lfs -text). Совпадающих файлов сейчас нет — безопасно.
  - Tools/Check-UnityMcp.ps1: убрать copy-compare (корень един — проверять нечего), оставить процесс/порт/handshake/конфиг + проверка «CWD = корень проекта». UNITY_MCP_SETUP.md и opencode.json — обновить пути, если хардкодят UnityProjects (найти где лежит opencode.json, обычно %APPDATA%/.config/opencode).
- [ ] Этап B — asmdef (коммит «Add Citybuilder assemblies»):
  - Assets/Citybuilder/Scripts/Citybuilder.asmdef (игровой код, дефолтные refs). Правок .cs — ноль, весь код подхватывается папкой.
  - Assets/Citybuilder/Tests/Tests.asmdef: references [Citybuilder + Test Framework]. Проверить Packages/manifest.json: есть ли com.unity.test-framework; нет — добавить строкой в manifest (текст, версию под Unity 6 сверить с реестром/докетой, не гадать).
  - .meta новым asmdef: сгенерировать свежие GUID вручную ([guid]::NewGuid(), формат N, шаблон взять с любого .meta asmdef-стиля — fileFormatVersion: 2, importer NavMesh? нет: для asmdef importer AssemblyDefinitionImporter). Проверить формат по существующим .meta в проекте.
- [ ] Этап C — тесты (коммит «Add EditMode tests for pure logic»):
  - Tests/GridSystemTests.cs: IsInside/границы, Occupy/Release/IsOccupied, WorldToGrid/GridToWorld (сверить API с кодом!).
  - Tests/TreasuryTests.cs: TrySpend/Earn/ноль/отрицательные/нехватка.
  - Tests/NeedSystemTests.cs: таблица границ из 036 (8 кейсов).
  - Только NUnit [Test]/[TestCase], без Unity API в проверках (EditMode, чистая логика). Самому тесты НЕ запускать (проект открывать нельзя — он занят живым редактором); компилируемость проверит оркестратор при переоткрытии.
- [ ] Этап D — отчёт: список скопированного (файлы/сцены/ассеты/.meta — сошлось ли), что изменено в скриптах/путях, commit messages трёх коммитов, известные риски для переоткрытия.

Ручной план оркестратору (мост): Stop+save → закрыть редактор → бэкап-переименование UnityProjects → открыть корень репо как проект → реимпорт → bridge refresh/console → run_tests EditMode → smoke-Play (рост+налоги) → мерж → git push origin main (первый пуш после 012!).
