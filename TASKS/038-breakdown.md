# TASKS/038-breakdown

Ввод: задача 038, модуль Population (1 правка), гейт въезда по потребностям. Исполнитель: Engineer (Muse, субагент). Серия: 036 ядро + 037 панель (смержены) → 038 гейт. Закрывает «потребности жителей»: потребности влияют на миграцию (GAME_DESIGN §11).
Запрещено трогать: всё кроме Population/PopulationController.cs и TASKS/038-breakdown.md (галочки).
Сдать: коммит в ветку task/038-movein-gate + ручной план проверки + commit message. В main НЕ коммитить, НЕ пушить, Unity НЕ трогать.

Контекст: PopulationController.TryMoveIn (113 строк) заселяет first-fit по таймеру без условий. NeedSystem (036): AverageSatisfaction 0..1; пустой город с домами = 1; город без работы быстро падает <0.5 (пример: 2/4 заселено, мест 0 → (0.5+0+0)/3 = 0.167).

- [x] Шаг 1: гейт в PopulationController (~+15 строк, итог ≤150):
  - поле `public float MinSatisfaction = 0.5f` ([Header Move-in], Tooltip, кламп 0..1 в ClampLimits);
  - приватный NeedSystem, ленивая сборка когда готовы population и Growth.Settlement (паттерн EnsurePopulation/Tick-пересборка);
  - в TryMoveIn перед циклом: если NeedSystem собран и AverageSatisfaction < MinSatisfaction → return (не въезжают); не собран (поселений ещё нет) → старое поведение;
  - остальное (таймер, first-fit, HouseholdSize, View, выселение) не трогать;
  - лимиты: файл ≤150, методы ≤30, русские комментарии точечно.
- [x] Шаг 2: самопроверка (пустой город с домами avg=1 → въезд открыт; без работы после 1-й семьи avg≈0.17 → закрыт; с магазином avg≈0.7 → открыт; MinSatisfaction кламп 0..1; needs=null → старое поведение; лимиты) + отчёт + commit message. Ручной план для приёмки через мост: копия ТОЛЬКО на стопе + Refresh + сцена без изменений (настройки по умолчанию) + Play: жилая зона без рабочих мест → только 1 семья; добавить коммерцию/промзону → въезд продолжается + Stop + Console чистый.
