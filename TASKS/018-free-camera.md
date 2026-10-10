# TASKS/018 — Free Camera

## Цель

Свободная камера: WASD/стрелки — панорама, колесо — зум, QE или ПКМ-drag — поворот. Ни с чем не конфликтует, по умолчанию включена.

## Контекст

Сейчас камера статична сверху (25,50,25, pitch 90°, perspective 60°). BACKLOG «свободная камера». Других скриптов камеры в проекте нет — конфликтов нет. Только старый Input.

## Что нужно сделать

1. `FreeCameraController` (MonoBehaviour, модуль Presentation):
   - Inspector: Target (Camera, опционален — иначе Camera.main), MoveSpeed (default 20, минимум 1), ZoomSpeed (default 10, минимум 1), RotateSpeed (default 90 град/сек, минимум 1), MinHeight (5), MaxHeight (80), MapWidth/MapHeight (default 50/50 под сетку V1), Margin (default 10).
   - Update: WASD/стрелки — панорама в плоскости XZ относительно yaw камеры; колесо мыши — движение вдоль взгляда (zoom), высота clamp [MinHeight, MaxHeight]; Q/E — поворот yaw; зажатая ПКМ + движение мыши — yaw + pitch (pitch clamp [20, 90]).
   - Clamp позиции: X/Z в [−Margin, Map+Margin].
   - Публичные методы для проверки без ввода: `Pan(float right, float forward)`, `Zoom(float steps)`, `AddYaw(float degrees)` — Update только транслирует ввод в них.
   - Камера по умолчанию ВКЛЮЧЕНА (контроллерам стройки не мешает — разные системы). Состояние ленивое: Target резолвится каждый кадр если пуст.

## Что не нужно делать

- Не делать следование за объектом, пресеты видов, Cinemachine, edge-scroll, тач.
- Не делать ортографию (камера perspective, не трогать projection).
- Не трогать Building*/Road*/Zone*/Population*/Economy/Simulation/GridVisualizer.

## Зависимости

Нет (камера ни от чего не зависит). Ничего не блокирует.

## Изменяемые файлы

Создать:
- Assets/Citybuilder/Scripts/Presentation/FreeCameraController.cs
- TASKS/018-free-camera.md (этот файл)
- TASKS/018-breakdown.md

Изменить:
- TASKS/README.md (статус 018)

## Критерии готовности

- [ ] 1 новый файл до 150 строк, методы до 30 строк.
- [ ] Pan сдвигает XZ, Zoom меняет высоту в клампах, AddYaw крутит вокруг Y; позиция не уходит за Margin.
- [ ] Ручная: WASD едет, колесо зумает, ПКМ крутит, Q/E крутят. Console без ошибок.
- [ ] Code rules: namespace Citybuilder, [Header]/Tooltip, без LINQ, без синглтонов.

## Железные уроки (не нарушать)

- Только Unlit/Color (в 018 материалов нет).
- Состояние в MonoBehaviour только ленивое.
- Скопировал файлы в Unity — Assets/Refresh (scope all), дождись компиляции.
- Сцену трогай только при выключенном Play.

## План проверки

1. Unity (Play ВЫКЛ): файл скопирован, Refresh all, компиляция, Console чистый.
2. E2E через мост в Play: повесить контроллер на MainCamera transient-вызовом, вызвать Pan/Zoom/AddYaw напрямую, сверить позицию/высоту/yaw и клампы. Ручной прогон клавиш — за разработчиком.
