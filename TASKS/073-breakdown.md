# TASKS/073-breakdown

Ввод: задача 073, модуль Economy + UI (политики §15, первый шаг). Сейчас ставка налога — поле `EconomyTicker.TaxPerWorker` (2 за работающего за час), игрок её не видит и не меняет. Добавляем налоговую политику: три уровня (низкий / обычный / высокий), кнопка в левом HUD, доход считается по выбранному уровню. Исполнитель: Engineer (субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено трогать: рост/въезд, потребности и провайдеров, инфраструктуру (энергия/вода/мусор), сохранения, сцену, ProjectSettings. Значение по умолчанию обязано давать прежний доход (2 за работающего).
Сдать: коммиты в ветку task/073-tax-policy + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `TaxSystem.ComputeTax(perWorker) = Employed × perWorker`; `EconomyTicker.Tick()` раз в час зовёт `tax.Collect(TaxPerWorker)`. HUD: левая колонка OnGUI, ниже мусора (y=310) свободно; кнопки рисуются `GUI.Button` и обязаны регистрироваться в `UiClickGuard.Set(key, rect)`, иначе клик проваливается в сетку (см. `BuildModeMenu`).

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Economy/TaxPolicy нет (новый), Economy/TaxSystem.cs, Economy/EconomyTicker.cs, UI/BuildModeMenu.cs, UI/UiClickGuard.cs, Tests/Economy/TaxSystemTests.cs (найди через grep, если лежит иначе). Остальное — grep.
- [ ] Шаг 2: новый `Economy/TaxPolicy.cs` (~65, чистый C#, без Unity): вложенный `enum Level { Low, Normal, High }`; `Current` (по умолчанию Normal), `PerWorker` (1 / 2 / 3), `Label` («Налог: низкий (1)», «… обычный (2)», «… высокий (3)»), `Next()` и `Previous()` с переходом по кругу, `Set(Level)` с фолбэком на Normal при неизвестном значении. Никакой зависимости от Unity.
- [ ] Шаг 3: `EconomyTicker`: опциональное поле `Policy` (тип `TaxPolicyView`, Tooltip «Пусто = фиксированная ставка TaxPerWorker»); новый геттер `PerWorker`, который берёт ставку из политики, если она подключена, иначе `TaxPerWorker`; `Tick()` собирает налог через `PerWorker`. Прежнее поведение без политики — байт-в-байт (2 за работающего).
- [ ] Шаг 4: новый `UI/TaxPolicyView.cs` (~55): поле `Policy` (TaxPolicy) не сериализуется — ленивое создание в геттере `TaxPolicy Policy`; в `OnGUI` кнопка `Rect(10, 342, 240, 26)` с `UiClickGuard.Set("tax", rect)`, текст = `Policy.Label`, клик = `Policy.Next()`; стиль — как у `BuildModeMenu` (жёлтая подсветка не нужна). y=342: ниже мусора (310+26+6).
- [ ] Шаг 5: тесты `Tests/Economy/TaxPolicyTests.cs` (~55, NUnit): по умолчанию Normal и 2; Next даёт High(3) → Low(1) → Normal(2); Previous в обратную сторону; `Set` с неизвестным enum → Normal; Label содержит число ставки; `TaxSystem.ComputeTax` с политикой высокого уровня даёт Employed×3 (сверка с существующим TaxSystemTests, старые тесты НЕ править).
- [ ] Шаг 6: `.meta` новым файлам (свежие GUID, проверить коллизии). Локально прогнать TaxPolicyTests + TaxSystemTests через dotnet/csc во временной папке (без Unity) и приложить результат. Самопроверка, отчёт, commit message.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс (вложенный enum допустим); без LINQ>2, сложных generics, reflection, async.
КОММИТЫ: ≤5 файлов и ≤250 строк каждый.
ЧУЖОЕ: незакоммиченные Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать и не коммитить.

Ручной план приёмки (оркестратор):
1. Refresh на стопе → Console без ошибок.
2. Сцена: добавить `TaxPolicyView` на Grid (ссылок не требует); сохранить.
3. Play: кнопка «Налог: обычный (2)» в левом HUD ниже мусора; клик по кнопке не ставит здание на сетку за ней; переключение на высокий → за час доход вырастает (Employed × 3 против × 2), на низкий → падает; проверить, что при клике на сетку рядом с кнопкой стройка работает как раньше. Скриншот.
4. Stop → Console чистый → run_tests EditMode: 160 + новые.
5. Мерж --no-ff, очередь, push origin main. Политика не сохраняется в слот (сбрасывается в «обычный» после загрузки) — записать в долги.
