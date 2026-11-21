# ADR-015: Ограниченное исключение — глобальный доступ к AudioController для UI-звуков

Date: 2026-11-21. Status: accepted.

## Context

Ревью уровня HIGH: `Assets/Citybuilder/Scripts/Presentation/AudioController.cs:20,54,98-102` держит `private static AudioController instance`, который выставляется в `Awake` и лениво ищется через `FindFirstObjectByType<AudioController>()` в `TryGetInstance`. Статические `PlayClickGlobal/PlayCoinGlobal/PlayPopGlobal/PlaySwooshGlobal` (`AudioController.cs:32-50`) вызываются только из IMGUI-вью: `BottomBarStyle`, `GoalView`, `EventLogView`, `InfoMenuView`, `QuestLogView`, `PauseMenuView`, `SettingsView`.

`AGENTS.md:68` («Do not create global singletons unless explicitly approved») и `ARCHITECTURE.md:13` («Никаких огромных Manager-классов и глобальных синглтонов») запрещают такой паттерн без явного одобрения; в `DECISIONS/` одобрения не было. Паттерн уже существует в коде, поэтому ADR честно фиксирует его как допустимое исключение и не приписывает одобрение задним числом владельцу репозитория.

IMGUI (`OnGUI`) не предоставляет DI: у вью нет инжектируемых зависимостей, а передача ссылки на `AudioController` в каждое из семи вью добавила бы сериализуемые поля и связи в единственную сцену `Prototype.unity` ради косметических звуков.

## Decision

- Настоящий ADR и есть явное одобрение в смысле `AGENTS.md:68` для уже существующего паттерна. Решение принято оркестратором в рамках делегированных архитектурных полномочий (роль Architect/Orchestrator в `AGENTS.md`), а не как задним числом полученное одобрение владельца.
- **Область исключения:** только `AudioController` и его статические `PlayClickGlobal/PlayCoinGlobal/PlayPopGlobal/PlaySwooshGlobal` для UI-звуков, вызываемых из IMGUI-вью. `instance` и `TryGetInstance`/`FindFirstObjectByType` остаются внутренней деталью класса: наружу не предоставляется публичный доступ к самому синглтону.
- **Причина:** IMGUI не имеет DI; прокидывание ссылки в каждое вью раздуло бы сцену `Prototype.unity` новыми полями и связями без архитектурной выгоды.
- **Запрет расширения:** каждый новый глобальный синглтон по-прежнему требует отдельного ADR до кода. Исключение не становится прецедентом для других контроллеров, менеджеров или глобальных шин событий; использование `AudioController` как сервиса домена также требует отдельного ADR.
- **Граница слоёв:** `AudioController` остаётся Unity-адаптером в `Presentation`; домен и симуляция не вызывают `Play*Global`, звук инициируется только вью.

## Consequences

- Audio-сервис доступен из любого IMGUI-вью без ссылок в сцене; клики, награды, лента и меню озвучиваются как сейчас. Код, сцена и Unity не меняются.
- Тесты не должны опираться на `static instance` или `FindFirstObjectByType`: проверяются чистые методы (`ClampVolume`) и поведение через публичные методы экземпляра, созданного в тесте. Статические `Play*Global` — тонкая glue-обвязка вью, не объект тестирования.
- Любое расширение паттерна (новый синглтон, публичный доступ к `instance`, вызов звука из домена) — отдельный ADR до кода.
- Связанные документы: `AGENTS.md:68`, `ARCHITECTURE.md:13`. Настоящий ADR — адресное исключение из обоих правил; все прочие их положения сохраняются.
