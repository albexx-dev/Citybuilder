# TASKS/039-breakdown

Ввод: задача 039, ИНФРА (исключение из «1 модуль / 400 строк»: bulk-импорт + сгенерированные файлы, своего кода минимум). P0-lite + тесты одним мержем: разрывать нельзя — тестовый asmdef ссылается на игровой через GUID/.meta. Исполнитель: Engineer (Muse, субагент) — этапы A–C коммитами в ветку; переоткрытие Unity, E2E и пуш — за оркестратором.
Запрещено трогать: геймплейный код (ноль правок .cs кроме переноса), Tools/Check-UnityMcp.ps1 (чужая грязная правка — не трогать).
Сдать: 3 коммита в ветку task/039-p0-lite-tests + отчёт. В main НЕ коммитить, НЕ пушить, Unity НЕ трогать и НЕ переоткрывать.

Контекст: origin/main = be17a49 (срез 012), локально 038 — бэкапа нет. Живой проект: C:\Users\admin\UnityProjects\CityBuilder (Unity 6.0.3.25f1, MCP-пин v10.3.0). В git сейчас только скрипты. Проверено оркестратором: Packages+ProjectSettings = 24 файла/0.06 МБ; .gitignore стандартный (.meta можно); git-lfs 3.7.1 есть; бинарников в Assets нет (31 .png — E2E-скриншоты, в git НЕ брать); сцены Prototype.unity + CityBuilder.unity; ассеты House/Shop/Factory/AsphaltRoad.

- [x] Шаг A: импорт + .gitignore + .gitattributes + пути (коммит a679f38; 135/135 хешей сошлись; +синк сцены 038-полем b8987ba).
- [x] Шаг B: asmdef игры + тестов + .meta (коммит 5492bca; test-framework 1.6.0 из кэша).
- [x] Шаг C: 3 тестовых файла (коммит a81bdbe).
- [x] Шаг D: приёмка оркестратором — переоткрытие на корне репо, EditMode 42/42, smoke-Play (9 домов, гейт 1 семья), Console чистый.
  - Скопировать в корень репо: Assets/ (КРОМЕ Assets/Screenshots), Packages/, ProjectSettings/ — включая все .meta. Скрипты Assets/Citybuilder/Scripts уже в git и равны (синхро-чек 5/5) — копия побайтово поверх, сверить хеши после.
  - .gitignore: добавить `Assets/Screenshots/` (E2E-артефакты, только локально).
  - .gitattributes: LFS-паттерны на будущее (*.fbx *.obj *.png *.wav *.mp3 *.exr *.tga *.psd *.zip filter=lfs diff=lfs merge=lfs -text). Совпадающих файлов сейчас нет — безопасно.
  - Tools/Check-UnityMcp.ps1: copy-compare убран, + проверка «CWD = корень проекта». UNITY_MCP_SETUP.md обновлён; opencode.json хардкода путей не имел.
- [x] Шаг B: asmdef игры + тестов + .meta (коммит 5492bca; test-framework 1.6.0 из кэша, GUID свежие).
  - Assets/Citybuilder/Scripts/Citybuilder.asmdef (игровой код, дефолтные refs). Правок .cs — ноль, весь код подхватывается папкой.
  - Assets/Citybuilder/Tests/Tests.asmdef: references [Citybuilder + Test Framework]. Проверить Packages/manifest.json: есть ли com.unity.test-framework; нет — добавить строкой в manifest (текст, версию под Unity 6 сверить с реестром/докетой, не гадать).
  - .meta новым asmdef: сгенерировать свежие GUID вручную ([guid]::NewGuid(), формат N, шаблон взять с любого .meta asmdef-стиля — fileFormatVersion: 2, importer NavMesh? нет: для asmdef importer AssemblyDefinitionImporter). Проверить формат по существующим .meta в проекте.
- [x] Шаг C: 3 тестовых файла (коммит a81bdbe; Grid/Treasury/NeedSystem, только NUnit).
- [x] Шаг D: приёмка оркестратором — переоткрытие на корне репо, EditMode 42/42, smoke-Play (9 домов, гейт 1 семья), Console чистый.

Ручной план оркестратору (мост): Stop+save → закрыть редактор → бэкап-переименование UnityProjects → открыть корень репо как проект → реимпорт → bridge refresh/console → run_tests EditMode → smoke-Play (рост+налоги) → мерж → git push origin main (первый пуш после 012!).
