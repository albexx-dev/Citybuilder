using System.Collections.Generic;

namespace Citybuilder
{
    // Цепочка квестов в слоте v5 (хвост #48, EPIC-64): запись из QuestSystem в CitySnapshot
    // и восстановление обратно. Чистый C#, без Unity: врайтер зовёт SaveController
    // при сборке слота, аплаер — LoadController после сброса задач в ClearAll.
    public static class QuestSnapshot
    {
        // Запись: id активных задач в порядке панели и выполненные id, без наград и ленты.
        // Системы нет — пустые списки, слот остаётся валидным.
        public static void Write(QuestSystem quests, CitySnapshot snapshot)
        {
            if (snapshot == null) return;
            snapshot.QuestActive = quests == null ? new List<string>() : quests.ActiveIds;
            snapshot.QuestCompleted = quests == null
                ? new List<string>()
                : new List<string>(quests.CompletedIds);
        }

        // Чтение: слот — источник истины, состояние цепочки заменяется целиком (сброс в начале).
        // Выполненные id идут в completed-set без наград, активные заменяются найденными в
        // каталоге задачами (неизвестные тихо пропускаются). Пустые списки — цепочка
        // стартует заново: недостающие задачи доберёт Fill на следующем тике.
        public static void Apply(CitySnapshot snapshot, QuestSystem quests)
        {
            if (snapshot == null || quests == null) return;
            quests.Reset();
            quests.RestoreCompleted(snapshot.QuestCompleted);
            quests.RestoreActive(snapshot.QuestActive);
        }
    }
}
