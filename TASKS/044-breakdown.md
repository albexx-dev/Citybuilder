# TASKS/044-breakdown

Ввод: задача 044, модуль Persistence + КРОШЕЧНЫЕ хуки (исключение «1 модуль»: restore по природе сквозной; каждому хуку ≤10 строк, поведения существующих путей не менять, каждый хук указать в отчёте). ЗАГРУЗКА слота. Исполнитель: Engineer (Muse, субагент).
Запрещено трогать: логику существующих путей (только новые public-методы поверх), сцену, main.
Сдать: коммит в ветку task/044-load-slot + ручной план + commit message. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: 043 пишет slot0.json (версия, тики, баланс, зоны, дома defId+x/y, жители buildingIndex+size, дороги x/y). Восстановление строит город заново: сначала CLEAR текущего, потом APPLY по порядку зоны→дороги→дома→жители→казна→часы. Прочитано оркестратором: TreasuryView.treasury приватно; ZoneView.Rebuild публично, а BuildGroups/RefreshView приватны; SettlementSystem.Settle публично; RoadSystem.Place публично, список дорог в контроллере; PopulationSystem.MoveIn/EvictHome публично; SimulationTime имеет ctor(long) — часы выставить молча можно.

- [ ] Шаг 1: хуки (каждый ≤10 строк нового кода):
  - TreasuryView.ResetBalance(int): заменить treasury новым Treasury(сумма) (казна пересоздаётся как после reload).
  - SimulationClock.RestoreTime(long ticks): CurrentTime = new SimulationTime(ticks), accumulated = 0, БЕЗ событий (иначе налоги/рост сработают N раз).
  - ZonePaintingController: public RepaintAll() (те же BuildGroups+RefreshView) + public ClearZones() (снять все зоны + repaint).
  - SettlementGrowthController: public RestoreGrown(defId, origin) (def из Definitions по Id, settlement.Settle + states.Add + View + RefreshOccupied, БЕЗ оплаты) + public ClearGrown() (снос всех выросших + выселение через PopulationSource + вид, порядком TryDemolish).
  - RoadPlacementController: прочитать устройство списка/вида/оплаты целиком до правок; public RestoreRoad(cell) (Place + список + вид, БЕЗ оплаты) + public ClearRoads().
- [ ] Шаг 2: `Persistence/LoadSystem.cs` (~70 строк, чистый C#): Apply(snapshot, caboodle живых доступов?) — НЕТ: доступы только через контроллеры, поэтому LoadSystem хранит чистые шаги над переданными системами (zones/roads/settlement/population) + Validate переиспользовать из SaveSlotStorage; порядок зон→дороги→дома→жители; баланс/часы выставить через хуки снаружи (в контроллере).
- [ ] Шаг 3: `Persistence/LoadController.cs` (~70 строк, тот же объект Persistence, те же 6 ссылок): Update F9 → LoadNow(); публичный bool LoadNow(): ReadSlot → Validate → ClearAll (зоны/дороги/дома через хуки, жители — EvictHome по всем Homes) → Apply → ResetBalance → RestoreTime → Debug.Log итога. Файла нет/битый → LogError + false, город НЕ трогать.
- [ ] Шаг 4: тесты EditMode: roundtrip систем (собрать ZoneSystem+SettlementSystem+PopulationSystem вживую, снапшот через SaveSystem, Clear, Apply, счётчики сошлись); Validate битого; ClearAll на пустом — без ошибок.
- [ ] Шаг 5: самопроверка (лимиты; хуки не меняют старые пути — дифф каждого показать; порядок apply; безоплатность restore; часы без событий) + отчёт + commit message. Ручной план: копия не нужна (репо=проект, файлы уже в дереве после коммита? НЕТ — файлы в ветке, в рабочем дереве они есть: инженер коммитит в ветку, дерево = ветка) — Refresh на стопе + добавить LoadController на Persistence + Play: построить → SaveNow → достроить → LoadNow → счётчики = файлу + Stop + Console + run_tests.
