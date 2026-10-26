# TASKS/086 — пересборка windows64 + smoke

Дата: 26.10.2026. Ветка: task/086-build. Кода не меняли, только сборка и проверка.

## Что вошло
Все изменения после 055 и особенно 078–085: пожарное покрытие в потребностях, подсказка прогрессии, ручные здания в инфраструктуре, сохранение налоговой политики и ручных зданий (снапшот v3).

## Сборка
- `UnityEditor.BuildPipeline.BuildPlayer`: сцена `Assets/Citybuilder/Scenes/Prototype.unity`, `StandaloneWindows64`, путь `Builds/CityBuilder/CityBuilder.exe`.
- Результат: **Succeeded**, ошибок 0, размер 86 МБ, время 15 с.

## Smoke
- Запуск: `CityBuilder.exe -logFile Builds/player-smoke.log -screen-width 1280 -screen-height 720`.
- Окно «CityBuilder», процесс живой 20 с, завершён принудительно.
- В логе нет `Exception`, `NullReference`, ошибок компиляции; старт чистый.

## Ограничения
- Smoke проверяет запуск, а не сценарий: рост, палитра, сохранение/загрузка в плеере руками не проходились.
- Билд не коммитится (`Builds/` в .gitignore).
