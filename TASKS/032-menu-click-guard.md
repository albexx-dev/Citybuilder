# TASKS/032 — Menu Click Guard

## Цель

Клики по кнопкам меню больше не действуют на карту: стройка/снос/кисть/обзор игнорируют нажатия, начавшиеся на UI.

## Контекст

Баг: OnGUI-кнопки и Update-опрос Input реагируют на один и тот же клик — нажал «Зоны», а под кнопкой покрасилась клетка. Чинится ручной проверкой прямоугольников UI.

## Что нужно сделать

1. `UiClickGuard` (новый static class, модуль UI):
   - `Set(string key, Rect rect)` — запомнить прямоугольник (ключи: "modes", "brush", "speeds"; координаты GUI: origin слева-сверху).
   - `IsOverMenu(Vector2 mousePosition)` — true если точка внутри любого: converts Y (`Screen.height - mousePosition.y`).
   - Без состояния сцены, чистый C# + UnityEngine.Rect (без MonoBehaviour).
2. `BuildModeMenu` (правка): в OnGUI регистрировать `Set("modes", rect-колонки)` — один общий Rect на все 5 кнопок (x, margin, width, 5*(height+4)) и `Set("brush", ...)` на ряд кисти (только когда рисуется).
3. `ClockView` (правка): регистрировать `Set("speeds", ...)` на ряд кнопок скорости (только когда рисуется).
4. Гарды (по одному месту на контроллер, НЕ трогать SettlementGrowthController — он в 033):
   - BuildingPlacementController: `GetMouseButtonDown(0/1)` + `&& !UiClickGuard.IsOverMenu(Input.mousePosition)`.
   - RoadPlacementController: то же для начала drag (Down 0) и сноса (Down 1). Отпуск drag над меню — коммит как обычно (началось на карте = намерение).
   - ZonePaintingController: флаг press-origin — на Down(0/1) запомнить `strokeBlocked = IsOverMenu()`; красить/стирать только если !strokeBlocked; сбросить когда ни одна кнопка не зажата.
   - FreeCameraController: DragRotate только если ПКМ-зажим начался вне меню (флаг `lookBlocked` на Down(1), сброс когда кнопка отпущена).
   - Зависимость контроллеров от UI-статики — зафиксированное исключение из правила (альтернатива — дублировать прямоугольники — хуже).

## Что не нужно делать

- Не трогать SettlementGrowthController (033), логику меню/часов, остальное.

## Зависимости

Нет (чисто ввод). Ничего не блокирует.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/UI/UiClickGuard.cs
- TASKS/032-menu-click-guard.md (этот файл)
- TASKS/032-breakdown.md

Изменить:
- Assets/Citybuilder/Scripts/UI/BuildModeMenu.cs (регистрация rects)
- Assets/Citybuilder/Scripts/UI/ClockView.cs (регистрация rects)
- Assets/Citybuilder/Scripts/Construction/BuildingPlacementController.cs (гарды)
- Assets/Citybuilder/Scripts/Construction/RoadPlacementController.cs (гарды)
- Assets/Citybuilder/Scripts/World/ZonePaintingController.cs (флаг штриха)
- Assets/Citybuilder/Scripts/Presentation/FreeCameraController.cs (флаг обзора)

## Критерии готовности

- [ ] Новый файл + правки по 1–5 строк на контроллер; лимиты соблюдены.
- [ ] Down на кнопке: стройка/снос/кисть/обзор молчат; Down на карте работает как раньше.
- [ ] Console без ошибок.

## Железные уроки (не нарушать)

- Refresh (scope all) + компиляция. Код копировать только на стопе (пользователь играет — инженеры правят РЕПО, копирование после Stop).

## План проверки

1. Unity: Refresh all, компиляция, Console чистый.
2. E2E через мост: IsOverMenu true/false на координатах кнопок/карты; симуляция Down через guard-флаги напрямую; ручные клики — за разработчиком.
