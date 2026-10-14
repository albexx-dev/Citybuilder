# TASKS/043-breakdown

Ввод: задача 043, новый модуль Persistence (2–3 файла), ЗАПИСЬ слота сохранений. Исполнитель: Engineer (Muse, субагент). Серия мин-релиза: 043 запись → 044 загрузка → 045 HUD/экраны → 046 меню → 047 сборка+баланс.
Запрещено трогать: геймплейный код (ноль правок вне Persistence/; если для снапшота не хватит крошечного public-геттера — добавить, явно указать в отчёте).
Сдать: коммит в ветку task/043-save-slot + ручной план проверки + commit message. В main НЕ коммитить, НЕ пушить, Unity НЕ трогать.

Контекст: ADR-004 — слот 1, сериализация plain C# State. Архитектурное разрешение (оркестратор): Unity JsonUtility + [Serializable]-DTO — это НЕ «ручная сериализация JSON» из запрета V1, одобрено явно. Загрузка — задача 044, здесь только запись + проверка файла. Кнопок/меню нет: триггер — клавиша F5 через публичный SaveNow() (E2E-мост клавиши не инжектит, как в 035).

Источники данных (API сверить с кодом!): Growth.Homes (BuildingState: Definition.Id + Cells/origin), Population.Population (TotalResidents/HouseholdCount; per-household home+size — проверить API, HouseholdState имеет Home/Size), ZonePainter.Zones.GetZonedCells(), Treasury.Balance, Runner.Clock.CurrentTime.TickCount, дороги — RoadSystem public API (прочитать!).

- [ ] Шаг 1: `Persistence/CitySnapshot.cs` (~60 строк, [Serializable], только public-поля/списки, без Dictionary): version=1, tickCount, balance, zones (x/y/type), buildings (defId/x/y), households (buildingIndex/size), roads (x/y). buildingIndex — индекс в buildings.
- [ ] Шаг 2: `Persistence/SaveSystem.cs` (~80 строк, чистый C#, без Unity кроме JsonUtility): BuildSnapshot(...) из живых систем; ToJson/FromJson; Validate (version==1, неотрицательности, индексы в границах) → bool; WriteSlot(path)/ReadSlot(path) через System.IO.
- [ ] Шаг 3: `Persistence/SaveController.cs` (~50 строк, MonoBehaviour): ссылки Population/Growth/Zones/Treasury/Runner/Roads (опциональные, добор GetComponent, null — молча, как NeedsView); Update: F5 → SaveNow(); публичный SaveNow() для E2E-моста (собирает снапшот, пишет slot0.json в persistentDataPath, Debug.Log итога).
- [ ] Шаг 4: `Persistence/SaveSystemTests.cs` (EditMode): roundtrip ToJson→FromJson→Validate true; битый version → false; индекс вне границ → false; пустой город — валиден.
- [ ] Шаг 5: самопроверка (лимиты: файлы ≤150, методы ≤30; снапшот покрывает всё для будущего restore: баланс/тики/зоны/дома/жители/дороги; без Unity API в SaveSystem кроме JsonUtility+IO) + отчёт + commit message. Ручной план для приёмки: копия ТОЛЬКО на стопе + Refresh + добавить SaveController в сцену (новый объект Persistence, только на стопе) + Play: построить город → SaveNow() через мост → файл slot0.json прочитан с диска, поля сошлись (дома/жители/баланс/тики) + Stop + Console чистый + run_tests зелёные.
