# TASKS/084-breakdown

Ввод: задача 084, модуль Core (долг из 072). `NeedProviders.Clamp01` пропускает NaN: оба сравнения ложны, и среднее потребностей становится NaN. Добавляем защиту и тесты. Исполнитель: Engineer (субагент). После — Reviewer (только чтение).
Запрещено: любые другие системы, сцена, ProjectSettings, старые тесты.
Сдать: коммит в ветку task/084-nan-guard + отчёт. НЕ пушить, НЕ мержить, Unity НЕ трогать.

Контекст: `NeedProviders` (119) — три-четыре делегата-провайдера, `Rate01` → null ? 1 : Clamp01(provider()); `Clamp01` — два сравнения без проверки NaN. Решение: NaN считаем «нет ограничения» → 1 (как отсутствующий провайдер), бесконечности обрабатываются существующими сравнениями (+∞ → 1, −∞ → 0).

- [ ] Шаг 1: прочитать ЦЕЛИКОМ: Core/NeedProviders.cs, Population/NeedSystem.cs, Tests/Infrastructure/NeedProvidersTests.cs, Tests/Infrastructure/FireNeedTests.cs.
- [ ] Шаг 2: `Clamp01`: первой строкой `if (float.IsNaN(value)) return 1f;` (с комментарием, почему 1). Больше ничего не менять.
- [ ] Шаг 3: тест `Tests/Infrastructure/NaNNeedTests.cs` (~45): провайдер возвращает `float.NaN` → `Rate == 1` и `Has==true`; среднее `NeedSystem` с NaN-провайдером конечно и равно ожидаемому (например, все ставки 0.5, одна NaN → среднее как с 1.0); `+∞ → 1`, `−∞ → 0`; NaN не ломает `Sum`/`Count` (учитывается как 1/1). Старые тестовые файлы не править.
- [ ] Шаг 4: `.meta` новому файлу (GUID уникальный). Локально прогнать NaNNeedTests + NeedProvidersTests + FireNeedTests через dotnet/csc, приложить результат. Отчёт, commit message.

ЛИМИТЫ: файл ≤150, метод ≤30, 1 файл = 1 класс; без LINQ>2/generics/reflection/async. Коммит ≤5 файлов/≤250 строк.
ЧУЖОЕ: Tools/Check-UnityMcp.ps1 и UNITY_MCP_SETUP.md — не трогать.

Приёмка: Refresh → тесты 209 + новые (210+) → Play-smoke 10 с без ошибок → мерж, пуш.
