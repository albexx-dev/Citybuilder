# TASKS/091 — пересборка windows64 + smoke

Дата: 26.10.2026. Ветка: task/091-build. Кода не меняли, только сборка и проверка.

## Что вошло
Изменения после 086: ступени цели с наградами (087), общий `SupplyFeed` для энергии/воды (088), полиция как вторая служба (089), сохранение ступени цели в снапшот v4 (090).

## Сборка
- `UnityEditor.BuildPipeline.BuildPlayer`: сцена `Assets/Citybuilder/Scenes/Prototype.unity`, `StandaloneWindows64`, путь `Builds/CityBuilder/CityBuilder.exe`.
- Результат: **Succeeded**, ошибок 0, размер 86 МБ, время 12 с.

## Smoke
- Запуск: `CityBuilder.exe -logFile Builds/player-smoke.log -screen-width 1280 -screen-height 720`.
- Окно «CityBuilder», процесс живой 20 с, завершён принудительно.
- В логе нет `Exception`, `NullReference`, ошибок компиляции; старт чистый.

## Ограничения
- Smoke проверяет запуск, а не сценарий: рост, палитра, полиция, сохранение/загрузка в плеере руками не проходились.
- Билд не коммитится (`Builds/` в .gitignore).
