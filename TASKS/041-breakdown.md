# TASKS/041-breakdown

Ввод: задача 041, ДОКИ (без инженера, делает оркестратор). Аудит P3 + п.6: README-квикстарт, DECISIONS/, первым ADR Jobs/Burst.
Запрещено трогать: всё кроме README.md, DECISIONS/, TASKS/041-breakdown.md, TASKS/README.md.
Сдать: коммит в ветку task/041-docs-adr → мерж → пуш.

- [x] Шаг 1: README.md — квикстарт: открыть корень как Unity-проект (6000.3.25f1), сцены (Prototype — игра, CityBuilder.unity — легаси-дубликат), тесты через Test Runner (EditMode, сборка Tests), мост (порт 8080, Tools/Check-UnityMcp.ps1), структура веток/мержей (см. AGENTS.md).
- [x] Шаг 2: DECISIONS/ADR-001-jobs-burst-trigger.md — критерий перехода: замеры Profiler при TotalResidents >1000 ИЛИ тик p95 >5 мс (бюджет ARCHITECTURE §14); до этого plain C# + MonoBehaviour, запрет DOTS/Jobs/Burst (уже в ARCHITECTURE §14/§8, ADR фиксирует триггер, а не вводит новое).
- [x] Шаг 3: DECISIONS/ADR-002-persistence-deferred.md — Persistence не V1: состояние только ленивое в MonoBehaviour, запрет новых систем с состоянием только в MonoBehaviour, plain C# State отдельно от View (уже правило, ADR фиксирует срок: пересмотр при задаче сохранений).
- [x] Шаг 4: DECISIONS/ADR-003-repo-is-project.md — корень репо = Unity-проект (039): Assets/Packages/ProjectSettings+.meta в git, Screenshots исключены, ручная копия .cs отменена.
