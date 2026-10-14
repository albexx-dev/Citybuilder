# TASKS/035-breakdown

Ввод: задача 035, модуль UI (1 новый файл + подписи кнопок), хоткеи меню режимов. Исполнитель: Engineer (Muse, субагент).
Запрещено трогать: всё кроме UI/BuildModeHotkeys.cs, UI/BuildModeMenu.cs (только массив labels) и TASKS/035-breakdown.md (галочки).
Сдать: коммит в ветку task/035-menu-hotkeys + ручной план проверки + commit message. В main НЕ коммитить, НЕ пушить, Unity НЕ трогать.

Контекст: BuildModeMenu (150 строк) — 5 кнопок Выкл/Дома/Дороги/Зоны/Рост, только мышь. Хоткеи вынести в отдельный компонент, меню не раздувать.

- [ ] Шаг 1: новый `UI/BuildModeHotkeys.cs` (~45 строк):
  - поле `public BuildModeMenu Menu` (ленивый добор через GetComponent, null-гард + ошибка как у остальных);
  - `Update`: Esc → `Mode.Off`, Alpha1..Alpha4 → Houses/Roads/Zones/Growth через `menu.SetMode`;
  - подписи кнопок в BuildModeMenu: только массив labels → "Выкл [Esc]", "Дома [1]", "Дороги [2]", "Зоны [3]", "Рост [4]" (логику меню не трогать);
  - совместимость: Esc уже гасит призрак (Houses) и drag (Roads) — итог одинаковый (режим Выкл), конфликтов нет; цифры свободны (камера: WASD/QE/колесо/ПКМ); гард кликов не нужен (клавиатура, не мышь);
  - лимиты: файл ≤150 строк, методы ≤30, namespace Citybuilder, [Header]/Tooltip.
- [ ] Шаг 2: самопроверка (все 5 клавиш переключают CurrentMode; Esc из каждого режима → Off; Menu=null гард; повторный Esc в Off без ошибок; эксклюзивность режимов цела) + отчёт + commit message. Ручной план для приёмки через мост: копия ТОЛЬКО на стопе + Refresh scope all + добавить BuildModeHotkeys на Grid (только на стопе) + Play-проверка клавиш + Stop + Console чистый.
