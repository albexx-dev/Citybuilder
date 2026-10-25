# TASKS/078-breakdown

Ввод: задача 078, модуль Core + Population + UI. Покрытие пожарных (076) — только цифра в HUD. Делаем его седьмым показателем потребностей по образцу мусора (072): `NeedProviders` уже умеет 0..3 провайдера, расширяем до 0..4. Исполнитель: Engineer (субагент). После инженера — Reviewer (субагент, только чтение).
Запрещено: рост/въезд, инфраструктурные контроллеры, налоги, сохранения, сцена, ProjectSettings, старые тесты.
Сдать: коммиты в ветку task/078-fire-needs + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `NeedProviders` (103): три `Func<float>` + Has*/Rate* + `Sum`/`Count`; `NeedSystem` (95) — обёртка, 5-арг конструктор (power, water, waste); `NeedsView` — строка «Потребности … • мусор X%», ширина по тексту (HudText, 077), фолбэки GetComponent. `FireCoverageController.Rate01` уже есть.

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Core/NeedProviders.cs, Population/NeedSystem.cs, UI/NeedsView.cs, Tests/Infrastructure/NeedProvidersTests.cs, WasteNeedTests.cs. Остальное — grep.
- [ ] Шаг 2: `NeedProviders`: четвёртый провайдер `fireRate`; конструкторы `()` / `(p, w)` / `(p, w, waste)` / `(p, w, waste, fire)`; `HasFire`, `FireRate`; `Sum`/`Count` — до 4.
- [ ] Шаг 3: `NeedSystem`: новый 6-арг конструктор `(pop, set, power, water, waste, fire)`, старые 2/3/4/5-арг делегируют (5-арг → fire null); `HasFire`/`FireRate`; среднее `(3 + providers.Sum) / (3 + providers.Count)` — при 4 провайдерах /7.
- [ ] Шаг 4: `NeedsView`: поле `Fire` (тип `FireCoverageController`, Tooltip, фолбэк GetComponent), четвёртый делегат `() => Fire.Rate01`, сегмент `• пожарные X%` после мусора; координаты/y не менять (ширина теперь по тексту).
- [ ] Шаг 5: тесты `Tests/Infrastructure/FireNeedTests.cs` (~70): 7 показателей — среднее `(3 базовых + 4 ставки)/7`; без пожарных — /6 (регресс 072); `HasFire` false без провайдера и Rate 1; пожарные 0 → показатель 0 и среднее падает; живое чтение делегата. Старые тесты (NeedProvidersTests, WasteNeedTests, NeedSystemTests, Power/WaterNeedTests) НЕ править — они обязаны пройти.
- [ ] Шаг 6: `.meta` новому тесту (свежий GUID). Локально прогнать FireNeedTests + NeedProvidersTests + WasteNeedTests + NeedSystemTests через dotnet/csc (без Unity), приложить результат. Отчёт, commit message.

ЛИМИТЫ: файл ≤150 (жёстко 200), метод ≤30, 1 файл = 1 класс; без LINQ>2, сложных generics, reflection, async. Коммиты ≤5 файлов / ≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка (оркестратор): Refresh → сцена NeedsView.Fire = FireCoverageController → Play: строка из 7 показателей, при 2 станциях «пожарные 50%» входит в среднее; Stop → тесты 178 + новые → мерж, пуш.
