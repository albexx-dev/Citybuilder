# TASKS/048-breakdown

Ввод: задача 048, модуль Population (сплит 230→~150+~90 + микроправка вызова). Долг 044/047: SettlementGrowthController 230 строк при жёстком лимите 200. Исполнитель: Engineer (Muse, субагент).
Запрещено трогать: поведение (нули изменений логики, только переезд методов), всё вне Population/SettlementGrowthController.cs, Population/SettlementDemolitionController.cs(+.meta), Persistence/LoadController.cs (только точка вызова ClearGrown) и TASKS/048-breakdown.md.
Сдать: коммит в ветку task/048-split-growth + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: файл 230 строк прочитан оркестратором целиком. Разрез — по ответственности: рост остаётся, снос уезжает.

- [ ] Шаг 1: новый `Population/SettlementDemolitionController.cs` (~90 строк, тот же объект Grid): поля GridVisualizer, SettlementGrowthController Growth, SettlementView View (опционален, добор), PopulationController PopulationSource (опционален); Update → HandleDemolishInput (ПКМ + UiClickGuard, как было); TryDemolish(cell); ClearGrown() (переезд как есть); FindStateIndex — только если нужен внутри (иначе доступ через примитивы роста, см. ниже).
- [ ] Шаг 2: в SettlementGrowthController (~150 итог): states/Homes/Settlement/EnsureReady/TryGrow/FindFreeOrigin/MatchesZone/RestoreGrown/OnClockTick/Start/Validate/ClampLimits остаются; TryDemolish/HandleDemolishInput/FindStateIndex/ClearGrown УДАЛИТЬ (переехали); добавить 2 крошечных public-примитива для сноса (без логики сверх необходимого): `FindStateAt(cell)` и `DemolishState(state)` (settlement.Demolish + View.Remove + states.Remove + RefreshOccupied) — снос и ClearGrown идут только через них.
- [ ] Шаг 3: LoadController.ClearAll — переключить вызов ClearGrown с Growth на Demolition (добавить ссылку тем же опциональным паттерном; старый вызов убрать). Больше ничего в LoadController не трогать.
- [ ] Шаг 4: .meta новому файлу (свежий GUID). Самопроверка (оба файла ≤150, методы ≤30; дифф старых путей — только переезд; ClearGrown извне работает через новый компонент) + отчёт + commit message. Ручной план: Refresh на стопе + добавить SettlementDemolitionController на Grid + связать (Growth/GridVisualizer/View/PopulationSource) + LoadController.ClearAll на новый компонент + Play: рост идёт, ClearGrown через мост (дома 0, жители выселены), ПКМ-путь ревью + Stop + Console + run_tests.
