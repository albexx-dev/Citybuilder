# TASKS/019 — Build Mode Menu

## Цель

Переключение режимов стройки кнопками вместо галочек: Выкл / Дома / Дороги / Зоны / Рост. Включён всегда ровно один (или ни одного). Закрывает «меню режимов» из бэклога.

## Контекст

Сейчас режимы переключаются enabled-галочками четырёх контроллеров на объекте Grid, причём только при выключенном Play (иначе конфликты ввода). PopulationController (таймер без ввода) — вне меню, живёт сам по себе.

## Что нужно сделать

1. `BuildModeMenu` (MonoBehaviour, модуль UI):
   - Inspector: ссылки на BuildingPlacementController, RoadPlacementController, ZonePaintingController, SettlementGrowthController (все опциональны — недостающие ищутся `GetComponent` на том же объекте, лениво).
   - OnGUI-панель кнопок вверху справа: [Выкл] [Дома] [Дороги] [Зоны] [Рост]. Активный подсвечен (другой цвет кнопки). Клик включает выбранный (`enabled = true; placementActive = true`) и выключает остальные (`enabled = false`).
   - placementActive у BuildingPlacementController приватный? Проверить: если приватный — управлять только через `enabled` для него, для остальных (публичный placementActive) ставить оба флага. Зафиксировать в отчёте.
   - По умолчанию при старте — режим Выкл (все четыре выключены), чтобы ничего не конфликтовало.
2. `RoadPlacementController` (правка, +1 метод): `OnDisable()` — сброс drag (`dragging = false; previewCells.Clear();`) + `View?.HidePreview()` с null-гардами (View может быть не создан). Иначе переключение посреди drag оставит висящее превью/коммит.
3. `BuildingPlacementController` (правка, +1 метод): `OnDisable()` — спрятать ghost (`if (ghost != null) ghost.SetActive(false)`), иначе выключение оставит висящий куб-призрак.

## Что не нужно делать

- Не делать UI Toolkit/uGUI (только OnGUI, как TreasuryView/PopulationView).
- Не управлять PopulationController, камерой, казной.
- Не делать хоткеи (позже отдельным проходом при желании).
- Не трогать остальное.

## Зависимости

TASKS/007, 010, 012, 014 (Done). Ничего не блокирует.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/UI/BuildModeMenu.cs
- TASKS/019-mode-menu.md (этот файл)
- TASKS/019-breakdown.md

Изменить:
- Assets/Citybuilder/Scripts/Construction/RoadPlacementController.cs (только OnDisable)
- Assets/Citybuilder/Scripts/Construction/BuildingPlacementController.cs (только OnDisable)
- TASKS/README.md (статус 019)

## Критерии готовности

- [ ] 1 новый файл до 150 строк; правки — по одному методу.
- [ ] Клик «Дороги» → только road-контроллер включён; клик «Дома» → только building; «Выкл» → все выключены.
- [ ] Road-drag, прерванный выключением: превью скрыто, коммита нет. Ghost дома после выключения не висит.
- [ ] Console без ошибок. [Header]/Tooltip, методы до 30 строк.

## Железные уроки (не нарушать)

- Только Unlit/Color (материалов нет).
- Состояние ленивое. Refresh (scope all) + компиляция.
- Сцену трогай только при выключенном Play. Меню само соблюдает «один режим за раз».

## План проверки

1. Unity (Play ВЫКЛ): файлы скопированы, Refresh all, компиляция, Console чистый.
2. E2E через мост в Play: добавить меню на Grid transient-вызовом, клики заменить прямыми вызовами переключения, сверить enabled-флаги; road-превью очистить через выключение и проверить скрытие; ghost скрыт. Ручные клики по кнопкам — за разработчиком.
3. Сцена: добавить BuildModeMenu на Grid в сцену (как камеру в 018) и сохранить — меню должно быть у пользователя сразу.
