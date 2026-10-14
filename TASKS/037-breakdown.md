# TASKS/037-breakdown

Ввод: задача 037, модуль UI (1 новый файл), панель потребностей. Исполнитель: Engineer (Muse, субагент). Серия: 036 ядро (смержено) → 037 панель → 038 гейт въезда.
Запрещено трогать: всё кроме UI/NeedsView.cs и TASKS/037-breakdown.md (галочки).
Сдать: коммит в ветку task/037-needs-panel + ручной план проверки + commit message. В main НЕ коммитить, НЕ пушить, Unity НЕ трогать.

Контекст: NeedSystem (036, смержен): ctor (PopulationSystem, SettlementSystem), геттеры HousingRoomRate/JobsRoomRate/EmploymentRate/AverageSatisfaction, всё 0..1, пустой город = 1 (кроме жилья без домов = 0). Образец вида: UI/PopulationView.cs (30 строк: опциональная ссылка, добор GetComponent, null — молча, OnGUI-лейбл слева сверху).

- [x] Шаг 1: новый `UI/NeedsView.cs` (~45 строк):
  - поля `public PopulationController Population` + `public SettlementGrowthController Growth` (опциональны, добор GetComponent; нет — молча не рисовать, как PopulationView);
  - NeedSystem строить лениво, когда готовы `Population.Population` и `Growth.Settlement` (образец Tick-пересборки из EconomyTicker: пересоздавать при null, поселения могли появиться позже);
  - OnGUI: один лейбл под строкой населения (Rect 10,50): "Потребности: 67% (жильё X% • работа Y% • занятость Z%)" — проценты округлить;
  - лимиты: файл ≤150, методы ≤30, namespace Citybuilder, [Header]/Tooltip, русские комментарии точечно.
- [x] Шаг 2: самопроверка (нет ссылок — тихо; есть контроллеры, но системы не готовы — нули без исключений; проценты = округлённые ставки ядра; лимиты) + отчёт + commit message. Ручной план для приёмки через мост: копия ТОЛЬКО на стопе + Refresh + добавить NeedsView на объект Population (только на стопе) + Play (лейбл виден, пустой город ~100%) + скриншот + Stop + Console чистый.
