# TASKS/081-breakdown

Ввод: задача 081, модуль Persistence + Economy (долг из 073). Налоговая политика живёт только в памяти: после загрузки слота всегда «обычный». Сохраняем уровень в снапшот (Version 2) с обратной совместимостью (слот версии 1 читается как «обычный»). Исполнитель: Engineer (субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено: рост/въезд, потребности, инфраструктура, сцена, ProjectSettings, `TaxPolicy` (только чтение/Set).
Сдать: коммиты в ветку task/081-save-policy + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `CitySnapshot` (Version=1, plain-поля для JsonUtility), `SaveSystem.BuildSnapshot(...)` собирает снапшот из живых систем, `SaveSlotStorage.Validate` проверяет слот, `SaveController.SaveNow`/`LoadController.LoadNow` — Unity-адаптеры; `TaxPolicy` (073) — уровни Low/Normal/High (enum 0/1/2), `Set(Level)`, `Current`, `Label`.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Persistence/CitySnapshot.cs, SaveSystem.cs, SaveSlotStorage.cs, SaveController.cs, LoadController.cs, Economy/TaxPolicy.cs, UI/TaxPolicyView.cs, Tests/SaveSystemTests.cs, Tests/Economy/TaxPolicyTests.cs.
- [ ] Шаг 2: `CitySnapshot`: поле `public int TaxLevel;` (0 = не задано → «обычный»; 1/2/3 = Low/Normal/High), `Version = 2`.
- [ ] Шаг 3: `SaveSystem`: `CurrentVersion = 2`; `BuildSnapshot` получает `int taxLevel` последним параметром (значение 1..3) и пишет в снапшот; сохранить старую сигнатуру перегрузкой, которая подставляет 0 (старые тесты не править) — либо обновить все вызовы, если старых тестов на сигнатуру нет.
- [ ] Шаг 4: `SaveSlotStorage.Validate`: принимать Version 1 и 2 (v1 — без TaxLevel, считается 0).
- [ ] Шаг 5: `SaveController`: опциональное поле `TaxPolicyView Policy` (Tooltip «Пусто = обычный налог в слот»), ленивый поиск `FindFirstObjectByType` при сохранении; в снапшот писать `(int)Policy.Policy.Current + 1`, без политики — 0.
- [ ] Шаг 6: `LoadController`: такое же поле `Policy`; после применения города — `Policy.Policy.Set(FromSnapshot(snapshot.TaxLevel))`, где 0 или неизвестное → `Level.Normal`, 1..3 → соответствующий уровень. Приватный статический маппер (≤10 строк).
- [ ] Шаг 7: тесты `Tests/Economy/TaxSaveTests.cs` (~70): снапшот с TaxLevel 3 хранит 3 и Version 2; BuildSnapshot с 0 → 0; маппер: 0→Normal, 1→Low, 2→Normal, 3→High, 99→Normal. `SaveSystemTests` не править (если падает из-за версии — сообщить в отчёте, не «чинить» тест).
- [ ] Шаг 8: без новых .meta (новый один тестовый файл — .meta создать, GUID уникальный). Локально прогнать TaxSaveTests + SaveSystemTests + TaxPolicyTests через dotnet/csc, приложить результат. Отчёт, commit messages.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммиты ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка (оркестратор): Refresh → Play: поставить «высокий», F5-сейв (SaveNow), сменить на «низкий», загрузить (LoadNow) → снова «высокий»; слот из старой версии (если остался) грузится как «обычный»; Stop → тесты 193 + новые → мерж, пуш.
