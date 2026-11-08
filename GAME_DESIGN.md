# Citybuilder Game Design

## 1. Game fantasy

Игрок — мэр и планировщик живого города. Город растёт как система: решения в одном месте отзываются в другом.

## 2. What the player does

Планирует город, строит дороги, размечает районы, ставит здания, управляет бюджетом, ресурсами, инфраструктурой и транспортом, следит за нуждами жителей.

## 3. City development loop

Построить → заселить → дать работу → получить налоги → улучшить сети → повысить привлекательность → привлечь новых жителей → расширить город.

Связи: дороги → доступность → время поездок → доступность работы → доход → потребление → экономика; коммуналка → качество жизни → привлекательность.

## 4. Buildings

Типы: жилые, коммерческие, промышленные, инфраструктурные, общественные. V1: только 1 дом 2x2 без заселения. Параметры типа: размер, стоимость, рабочие места, потребление/производство (после V1).

## 5. Roads

Клеточные, соединяются в сегменты, дают доступность зданий. V1: дорог нет. Этап 4.

## 6. Districts

Жилые / коммерческие / промышленные зоны. После среза (Этап 5).

## 7. Resources

Electricity, Water, Food, Materials, Fuel, Waste. Вводить по одному, не все сразу. V1: нет.

## 8. Economy

Бюджет, доходы/расходы, стоимость строительства, налоги. V1: только казна — начальный баланс, списание за стройку, запрет при нехватке, UI баланса, понятный результат транзакции.

## 9. Population

Граждане и домохозяйства: жильё, работа, доход, образование, потребности, миграция. Начать с домохозяйств, не с детальной симуляции каждого. V1: жителей нет.

## 10. Jobs

Рабочие места в зданиях, доступность зависит от транспорта. После Этапа 5.

## 11. Needs

Жильё, доход, еда, здоровье, образование, безопасность, досуг. Влияют на счастье и миграцию. После V1.

## 12. Infrastructure

Электричество, вода, канализация, мусор, пожарные, полиция, медицина, образование. По одной службе за раз. После V1.

## 13. Transport

Маршруты, поездки, нагрузка, общественный транспорт, пробки. После сетки, дорог, домов и работы (Этап 8).

## 14. Events and problems

Пробки, безработица, нехватка воды/света, пожары, жалобы. После базовой симуляции.

## 15. Progression

Открытие новых зданий и политик за рост населения и бюджет. Баланс позже.

## 16. Difficulty

Старт с запасом денег, мягкие штрафы. Жёсткая экономика после среза.

## 17. First playable prototype

1. Камера сверху.
2. Сетка 50x50.
3. Один дом 2x2 (куб-примитив).
4. Предпросмотр: зелёный — можно, красный — нельзя.
5. Проверки границ и занятости.
6. Установка занимает клетки, удаление освобождает.
7. Начальный бюджет, стоимость стройки, UI баланса, отмена по Escape.

## 18. v1.1 scope (supersedes the frozen minimal release)

Первый минимальный релиз состоялся в задаче 047; заморозка ADR-004 снята post-release amendment. Актуальные рамки — `DECISIONS/ADR-005-v1.1-scope.md` и `AGENTS.md` → Scope lock.

В v1.1 входит реализованное в задачах 001–104: сетка 50×50, зоны, рост по тикам, население, налоговая политика, энергия / вода / мусор, пожарные и полиция по радиусу, связность дорог и пробки, ступени цели 50/100/200, сохранение v4 (налог, ступень цели, ручные здания, домохозяйства по координатам), стартовая подсказка, статус роста и HUD.

Экраны победы/поражения удалены задачей 049. `GoalSystem.Won/Lost` оставлен осознанно как основа для исходов в следующей версии, но UI его не использует. Без нового ADR по-прежнему запрещены: транспорт с машинами, многоуровневость, облачные сейвы, Jobs/Burst, DI-контейнеры, `async/await`, новые сторонние пакеты.

## 19. No ending (песочница без финала)

Решение владельца от 08.11.2026: партия **не имеет конца**. Экранов победы и поражения нет и не будет; `GoalSystem.Won/Lost` признаётся мёртвым кодом и удаляется в v1.1.

Цель со ступенями 50/100/200 остаётся вехами роста с наградами: после последней ступени город развивается свободно, без «конца игры». Долгосрочная мотивация — рост, службы и баланс, а не финальный экран.

## 20. Balance (v1.1)

Фактические цифры, снятые с кода и ассетов на момент шага 1 эпика баланса (`TASKS/EPIC-06-balance.md`). Раздел описывает **то, что игра считает сейчас**, а не целевой баланс: правка значений — это шаги 2-4 эпика. Источник каждой цифры указан в строке: файл с номером строки для логики и значений по умолчанию, имя ассета для данных типа, `Prototype.unity:строка` для числа, зашитого в Inspector. Пути к скриптам и тестам — относительно `Assets/Citybuilder/` (`Scripts/<Модуль>/…`, `Tests/…`), ассеты лежат в `Assets/Citybuilder/Data/Buildings/`, сцена — `Assets/Citybuilder/Scenes/Prototype.unity`.

### Buildings

| Тип | Cost, $ | Upkeep, $/ч | Жильё | Раб. места | Радиус | Потребление P / W | Отдача P / W / утилизация | Мусор | Зона, разблокировка | Источник |
|-----|---------|-------------|-------|------------|--------|-------------------|---------------------------|-------|---------------------|----------|
| House | 100 | 0 | 4 | 0 | 0 | 1 / 1 | 0 / 0 / 0 | 1 | Residential, сразу | `House.asset` |
| Shop | 150 | 1 | 0 | 6 | 0 | 1 / 1 | 0 / 0 / 0 | 2 | Commercial, с 4 выросших зданий | `Shop.asset` |
| Factory | 200 | 2 | 0 | 8 | 0 | 1 / 1 | 0 / 0 / 0 | 3 | Industrial, с 8 выросших зданий | `Factory.asset` |
| Power Station | 300 | 8 | 0 | 2 | 0 | 0 / 0 | 12 / 0 / 0 | 0 | Industrial, сразу | `PowerStation.asset` |
| Water Tower | 250 | 6 | 0 | 1 | 0 | 0 / 0 | 0 / 12 / 0 | 0 | Industrial, сразу | `WaterTower.asset` |
| Waste Depot | 200 | 5 | 0 | 2 | 0 | 0 / 0 | 0 / 0 / 12 | 0 | Industrial, сразу | `WasteDepot.asset` |
| Fire Station | 300 | 6 | 0 | 2 | 6 | 1 / 1 | 0 / 0 / 0 | 1 | Commercial, сразу | `FireStation.asset` |
| Police Station | 250 | 6 | 0 | 2 | 5 | 1 / 1 | 0 / 0 / 0 | 1 | Commercial, сразу | `PoliceStation.asset` |
| Asphalt Road | 10 за клетку | нет поля upkeep | — | — | — | — | — | — | — | `AsphaltRoad.asset`, `Construction/RoadDefinition.cs:18` |

Станции (Power / Water / Waste) намеренно имеют нулевое потребление энергии и воды: иначе при полном дефиците их нельзя построить для лечения дефицита (`PowerStation.asset`, `WaterTower.asset`, `WasteDepot.asset`, комментарий в `Construction/BuildingDefinition.cs:49-58`).

Зоны и типы служб — числа enum: `ZoneType` None=0 / Residential=1 / Commercial=2 / Industrial=3 (`World/ZoneType.cs:6-9`), `ServiceKind` None=0 / Fire=1 / Police=2 (`World/ServiceKind.cs:6-8`). Больниц и парков в репозитории нет, поэтому у факторов «досуг» и «здоровье» в `AttractivenessPolicy` пока нет источника (см. `Known discrepancies`).

`PoliceStation.asset` записан неполно: ключи `Footprint`, `UnlockBuildings`, `PowerDemand`, `PowerOutput`, `WaterDemand`, `WaterOutput`, `WasteOutput`, `WasteCapacity`, `Prefab` в файле отсутствуют, и их значения приходят из инициализаторов полей `Construction/BuildingDefinition.cs:19,26,47,52,55,60,63,66,74` (2x2, 0, 1, 0, 1, 0, 1, 0, пусто). В таблице строка Police Station показывает именно эти действующие значения.

### Economy

| Параметр | Значение | Источник |
|----------|----------|----------|
| Стартовая казна | 1500 $ | `UI/TreasuryView.cs:10` (значение по умолчанию), `Prototype.unity:1094` (Inspector) |
| Ставка налога за работающего в час | Low 1, Normal 2, High 3; режим по умолчанию Normal | `Economy/TaxPolicy.cs:31-36`, `:16` |
| Формула сбора | занятые × ставка за час; ставка ≤ 0 — сбора нет | `Economy/TaxPolicy.cs:48-52`, `Economy/TaxSystem.cs:32-39` |
| Занятые | `min(жители, рабочие места)` | `Population/EmploymentSystem.cs:26-29` |
| Рабочие места и жильё в расчёте | рабочие места и жильё считаются только по **выросшим** зданиям (`SettlementSystem.Settle`) | `Population/SettlementSystem.cs:68-69`, `Population/SettlementGrowthController.cs:99,119`, `Persistence/LoadSystem.cs:85` |
| Содержание за час | сумма `UpkeepPerHour` по выросшим + ручным зданиям | `Core/UpkeepPolicy.cs:22,48`, `Economy/EconomyTicker.cs:139-140` |
| Списывается ли содержание при нуле | да, казна уходит в минус; `Charge` не проверяет баланс | `Economy/Treasury.cs:63-72` |
| Отрицательный баланс = дефицит | `Balance < 0` | `Economy/Treasury.cs:27-30` |
| Стройка и рост при нехватке денег | отказ (`TrySpend`), рост платит `ConstructionCost` типа | `Economy/Treasury.cs:46-59`, `Population/SettlementGrowthController.cs:101`, `Construction/BuildingPlacementController.cs:96` |
| Оплата дороги | новые клетки × `CostPerCell` | `Construction/RoadPlacementController.cs:109`, `Construction/RoadSystem.cs:53-60` |
| Награды ступеней цели | 1000 / 2000 / 4000 $ за 50 / 100 / 200 жителей | `Population/GoalStages.cs:9-10`, выдаёт `Population/GoalRewardController.cs:13-17` |
| Порог «нет денег» в статусе роста | баланс ниже самого дешёвого `ConstructionCost` | `UI/GrowthStatusView.cs:64-75`, `UI/GrowthStatusPolicy.cs:28` |

### Pace and gates

| Параметр | Значение | Источник |
|----------|----------|----------|
| Тик = игровой час | 1 тик = 1 час, сутки = 24 тика | `Core/SimulationTime.cs:5-9` |
| Реальная длительность часа | 1 с на Playing, ×4 на FastForward (0,25 с), Pause останавливает тики | `Simulation/SimulationClock.cs:9,54`, `Simulation/SimulationSpeed.cs:7-9` |
| Базовых тиков на следующее здание | 5 | `Population/SettlementGrowthController.cs:21` (значение по умолчанию), `Prototype.unity:351` (Inspector) |
| Множитель темпа | худший из (пробки, снабжение), не произведение; кламп 1..3 | `Population/GrowthPace.cs:12-18` |
| Требуемых тиков | `базовые × множитель`, потолок 1000 | `Population/GrowthPace.cs:7,21-27` |
| Дефицит казны | требуемых тиков = 1000 (стоп) до выбора множителя + отдельный гейт допустимости | `Population/GrowthPace.cs:37-41`, `Population/GrowthEligibilityPolicy.cs:42` |
| Пробки: порог / шаг / потолок | нагрузка клетки ≥ 4; `1 + (нагрузка − 4 + 1) × 0.25`; максимум 3 | `Transport/CongestionController.cs:17-21`, `Prototype.unity:563-565`, `Transport/CongestionSystem.cs:102-118` |
| Пробки → темп | `round(худший множитель)`, минимум 1 | `Transport/CongestionSystem.cs:143-151` |
| Снабжение → темп | ×1 при ставке ≥ 0.99, ×2 при ≥ 0.5, ×3 ниже | `Infrastructure/SupplyGrowthPolicy.cs:7-16` |
| Потолок выросших зданий | 30 (кламп минимум 1) | `Population/SettlementGrowthController.cs:22,148`, `Prototype.unity:352` (Inspector) |
| Гейт въезда по занятости | `MinEmployment = 0.4` против доли трудоустроенных | `Population/PopulationController.cs:27,106`, `Prototype.unity:775` (Inspector) |
| Порога удовлетворённости в коде | нет — `AverageSatisfaction` только для HUD | `Population/NeedSystem.cs:94-101`, `UI/NeedsView.cs:64` |
| Интервал въезда | 7 **реальных** секунд (таймер на `Time.deltaTime`, от скорости симуляции не зависит) | `Population/PopulationController.cs:25,96-97`, `Prototype.unity:774` (Inspector) |
| Размер домохозяйства | 2 жителя (кламп минимум 1) | `Population/PopulationController.cs:23,158`, `Prototype.unity:773` (Inspector) |
| Условие въезда | `жители дома + размер ≤ вместимость дома` | `Population/PopulationController.cs:134-138` |
| Куда разрешён въезд | только в выросшие здания (`Growth.Homes`), сначала связанные дорогами | `Population/PopulationController.cs:107-110,140-145` |
| Автовырастание в сцене | компонент `SettlementGrowthController` выключен (`m_Enabled: 0`), включается режимом Growth | `Prototype.unity:335`, `UI/BuildModeMenu.cs:61-64` |
| Типы, которые растут сами | House, Shop, Factory (3 ассета) | `Prototype.unity:347-350` (Inspector, `Definitions`) |
| Типы ручной стройки | все 8 ассетов зданий | `Prototype.unity:269-277`, `Prototype.unity:621-629` |

### Supply

| Ресурс | Бесплатная база города | Станция: отдача / стоимость / upkeep | Пороги темпа | Источник |
|--------|------------------------|--------------------------------------|--------------|----------|
| Энергия | 12 | Power Station +12 / 300 $ / 8 $/ч | `GoodRate 0.99`, `SevereRate 0.5` | `Infrastructure/PowerController.cs:19` + `Prototype.unity:519`, `PowerStation.asset`, `Infrastructure/SupplyGrowthPolicy.cs:7-8` |
| Вода | 12 | Water Tower +12 / 250 $ / 6 $/ч | те же | `Infrastructure/WaterController.cs:19` + `Prototype.unity:548`, `WaterTower.asset` |
| Мусор | 12 (утилизация) | Waste Depot 12 ёмкости / 200 $ / 5 $/ч | те же | `Infrastructure/WasteController.cs:20` + `Prototype.unity:593`, `WasteDepot.asset` |

Доля покрытия считается как `supply / demand` (при спросе 0 или полном покрытии — 1), нехватка делится поровну между всеми потребителями; «накрыт» — при доле ≥ 0.999. Источник: `Core/SupplyBalance.cs:9,27-43,85-88`. Снимок снабжения пересобирается каждые 2 реальные секунды (`Infrastructure/PowerController.cs:23`, `Prototype.unity:520`).

### Attractiveness

| Фактор | Вклад | Вход | Источник |
|--------|-------|------|----------|
| База | 1.0 | — | `Core/AttractivenessPolicy.cs:11,25` |
| Досуг (покрытие парками) | + 0.2 × coverage | доля 0..1 | `:12,26` |
| Здоровье (покрытие больницами) | + 0.1 × coverage | доля 0..1 | `:13,27` |
| Снабжение | − 0.3 × (1 − худшая из трёх ставок) | доля 0..1 | `:14,28`, `:49-57` |
| Пробки | − 0.1 × (множитель − 1), бонуса ниже 1 нет | худший множитель пробок | `:15,29`, `:60-64` |
| Дефицит казны | − 0.2 | флаг | `:16,30` |
| Диапазон | 0.5 .. 1.5 (кламп), NaN → 1.0 | — | `:8-9,67-73` |

Открытый вопрос владельцу: потолок `MaxMultiplier = 1.5` (`:9`) при текущих весах **недостижим** — с входами 0..1 максимум равен `1 + 0.2 + 0.1 = 1.3`, а в кламп 1.5 результат попадает только в тестах, которые подают покрытия больше 1 (`Tests/AttractivenessPolicyTests.cs:46-59`). Шаг 1 фиксирует расхождение и не предлагает правку: менять веса, добавлять фактор или опускать потолок — решение владельца, оно относится к шагам 2-4 эпика. Второе наблюдение по этой же политике: вызова `AttractivenessPolicy.Evaluate` из игрового кода нет — только тесты (`Tests/AttractivenessPolicyTests.cs`), то есть множитель пока не влияет ни на интервал въезда, ни на что-либо ещё: шаги 2-3 EPIC-05 не выполнены.

### Known discrepancies

- Больница и парк: `TASKS/EPIC-06-balance.md:25` задаёт $500 / $150, `TASKS/EPIC-04-upkeep.md:18` — upkeep 10 / 2. Ассетов нет (в `Assets/Citybuilder/Data/Buildings/` 8 зданий + 1 дорога), `ServiceKind` знает только None/Fire/Police (`World/ServiceKind.cs:6-8`), EPIC-01 и EPIC-02 — `not started` (`TASKS/README.md`). Строка плана неподтверждаема.
- `TASKS/EPIC-04-upkeep.md:32` (шаг 3) говорит «значения upkeep в 10 ассетах»: ассетов зданий 8, значения записаны в эти 8 ассетов, заводить upkeep с нуля больше не нужно.
- `TASKS/EPIC-06-balance.md:27` («Содержание … свести с налогами»): содержание уже лежит в ассетах (коммит `77ad5f6`), сводить остаётся налоги, награды и стоимость роста.
- `MinSatisfaction = 0.5` как гейт въезда назван в `TASKS/EPIC-03-needs-v2.md:17` и `TASKS/EPIC-05-attractiveness.md:7`, но в коде такого поля нет: `Population/PopulationController.cs:106` проверяет только занятость, `AverageSatisfaction` (`Population/NeedSystem.cs:94-101`) используется только в HUD.
- Дефицит казны не переживает save/load: `Persistence/SaveSystem.cs:41` пишет `Math.Max(0, balance)`, `Persistence/SaveSlotStorage.cs:44` отвергает слот с отрицательным балансом, конструктор `Economy/Treasury.cs:34` клампит минус в 0. Долг при загрузке гасится бесплатно.
- `Economy/EconomyTicker.cs:24` дублирует ставку Normal (`Economy/TaxPolicy.cs:36`) числом 2 и служит фолбэком, когда политика не подключена (`:43-47`); в сцене политика подключена (`Prototype.unity:155`), так что дубль в игре не используется, но он может разойтись с политикой при правке цифр.
- Цель 50 жителей задана дважды: `Population/GoalSystem.cs:9` (`GoalResidents = 50`) и `Population/GoalStages.cs:9` (`Targets[0] = 50`).
- Потолок населения ниже третьей ступени цели: `MaxSettlements = 30` (`Prototype.unity:352`) при вместимости дома 4 (`House.asset`) и нулевой вместимости магазина и фабрики — максимум 120 жителей, и меньше, если slot'ы роста занимают нежилые типы; ступень 200 (`Population/GoalStages.cs:9`) при этих числах недостижима.
- Пробки не замедляют рост в собранной игре: у `Transport/CongestionController.cs:59` (`TrackLoad`) нет вызова из игрового кода — нагрузку путей никто не копит, `WorstMultiplier` остаётся 1. Про «пробки влияют на скорость роста» пишет `AGENTS.md` (Текущее состояние v1.1).
- Ручные здания платят содержание (`Economy/EconomyTicker.cs:139-140`), но не дают рабочих мест и жилья: счётчики меняются только в `SettlementSystem.Settle` (`Population/SettlementSystem.cs:68-69`), а ручная стройка зовёт `BuildingSystem.Place` (`Construction/BuildingPlacementController.cs:99`). Построенные вручную магазин и фабрика не попадают в налоговую базу, построенный вручную дом не принимает жителей (`Population/PopulationController.cs:107`).
- `Population/PopulationController.cs:96` отсчитывает въезд по `Time.deltaTime`: FastForward ускоряет рост в 4 раза (`Simulation/SimulationClock.cs:54`), но не въезд семей, а Pause останавливает тики и не останавливает въезд.
