# TASKS/079 — пересборка windows64 + smoke

Дата: 25.10.2026. Ветка: task/079-build. Кода не меняли, только сборка и проверка.

## Сборка
- `UnityEditor.BuildPipeline.BuildPlayer`: сцена `Assets/Citybuilder/Scenes/Prototype.unity`, target `StandaloneWindows64`, путь `Builds/CityBuilder/CityBuilder.exe`.
- Результат: **Succeeded**, ошибок 0, размер 86 МБ, время 25 с.
- Свежесть: `Builds/CityBuilder/CityBuilder_Data/globalgamemanagers` — 25.10.2026 16:55.

## Smoke
- Запуск: `CityBuilder.exe -logFile Builds/player-smoke.log -screen-width 1280 -screen-height 720`.
- Окно: заголовок «CityBuilder», процесс живой 21 с, завершён принудительно.
- Лог: версия 6000.3.25f1, D3D12, Input System инициализированы, `UnloadTime 5.3 ms`; исключений, `NullReference`, ошибок сборки — **нет**.

## Ограничения
- Smoke проверяет запуск и чистый старт, а не прохождение сценария: клики/рост/UI в плеере руками не проходили.
- Билд не коммитится (`Builds/` в .gitignore); артефакт лежит локально.
