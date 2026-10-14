# TASKS/035-breakdown

Ввод: задача 035, модуль UI (1 новый файл + подписи кнопок), хоткеи меню режимов. Исполнитель: Engineer (Muse, субагент).
Запрещено трогать: всё кроме UI/BuildModeHotkeys.cs, UI/BuildModeMenu.cs (только массив labels) и TASKS/035-breakdown.md (галочки).
Сдать: коммит в ветку task/035-menu-hotkeys + ручной план проверки + commit message. В main НЕ коммитить, НЕ пушить, Unity НЕ трогать.

Контекст: BuildModeMenu (150 строк) — 5 кнопок Выкл/Дома/Дороги/Зоны/Рост, только мышь. Хоткеи вынести в отдельный компонент, меню не раздувать.

- [x] Шаг 1: новый `UI/BuildModeHotkeys.cs` (~45 строк):
  - поле `public BuildModeMenu Menu` (ленивый добор через GetComponent, null-гард + ошибка как у остальных);
  - `Update`: Esc → `Mode.Off`, Alpha1..Alpha4 → Houses/Roads/Zones/Growth через `menu.SetMode`;
  - подписи кнопок в BuildModeMenu: только массив labels → "Выкл [Esc]", "Дома [1]", "Дороги [2]", "Зоны [3]", "Рост [4]" (логику меню не трогать);
  - совместимость: Esc уже гасит призрак (Houses) и drag (Roads) — итог одинаковый (режим Выкл), конфликтов нет; цифры свободны (камера: WASD/QE/колесо/ПКМ); гард кликов не нужен (клавиатура, не мышь);
  - лимиты: файл ≤150 строк, методы ≤30, namespace Citybuilder, [Header]/Tooltip.
- [x] Шаг 2: самопроверка (все 5 клавиш переключают CurrentMode; Esc из каждого режима → Off; Menu=null гард; повторный Esc в Off без ошибок; эксклюзивность режимов цела) + отчёт + commit message. Ручной план для приёмки через мост: копия ТОЛЬКО на стопе + Refresh scope all + добавить BuildModeHotkeys на Grid (только на стопе) + Play-проверка клавиш + Stop + Console чистый.

## Отчёт инженера

Готово. BuildModeHotkeys.cs — 58 строк, Update 28 строк, ResolveMenu 15 строк (лимиты соблюдены). BuildModeMenu.cs — только массив labels, 150 строк (не вырос).

Самопроверка (статическая, Unity не трогал):
- Esc → SetMode(Off); 1/2/3/4 → Houses/Roads/Zones/Growth через SetMode → CurrentMode переключается во всех 5 случаях;
- Esc безусловный, из любого режима ведёт в Off;
- Menu=null: ResolveMenu пишет LogError в стиле остальных контроллеров, гасит enabled, Update выходит — без NullReference;
- повторный Esc в Off: SetMode(Off) идемпотентен (всем enabled=false), ошибок нет;
- эксклюзивность цела: SetMode и OnGUI не тронуты, заменена только строка labels.

Commit message: `Add menu mode hotkeys Esc 1-4`

## Ручной план проверки через Unity-мост (для оркестратора)

1. Дождаться стопа редактора (копия/Refresh/добавление компонентов — ТОЛЬКО на стопе).
2. Через мост: Refresh scope all — убедиться, что BuildModeHotkeys скомпилировался, Console без ошибок.
3. Через мост (на стопе): добавить компонент BuildModeHotkeys на объект Grid (там же, где BuildModeMenu). Ссылку Menu можно не задавать — доберётся через GetComponent.
4. Play. По очереди нажать 1, 2, 3, 4 — активная кнопка подсвечивается жёлтым: Дома [1], Дороги [2], Зоны [3], Рост [4].
5. Из каждого режима нажать Esc — подсветка возвращается на «Выкл [Esc]», призрак дома / drag дорог гаснут.
6. Нажать Esc повторно в Выкл — без ошибок в Console.
7. Stop. Console чистый (кроме предсущих, если были).
