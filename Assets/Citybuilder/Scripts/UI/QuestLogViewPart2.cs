using UnityEngine;

namespace Citybuilder
{
    // Хвост класса QuestLogView (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class QuestLogView : MonoBehaviour
    {
        private void DrawQuests(QuestSystem quests)
        {
            float y = 4f;
            string header = Localization.T("quest_log_title");
            if (quests != null)
            {
                QuestContext context = quests.LastContext;
                header += "  " + MayorRating.Label(quests.CompletedCount, context.Residents, context.Balance);
            }
            HudText.Draw(4f, y, header);
            y += LineHeight;
            if (quests == null || quests.Active.Count == 0)
            {
                HudText.Draw(12f, y, Localization.T("quest_log_empty"));
                return;
            }
            for (int i = 0; i < quests.Active.Count; i++)
            {
                y = DrawQuest(y, quests.Active[i], quests.LastContext);
            }
        }

        // Строка задачи: титул с наградой, описание и прогресс («Жители 12/50»).
        private static float DrawQuest(float y, QuestDefinition quest, QuestContext context)
        {
            HudText.Draw(12f, y, quest.Title + "  " + Localization.T("quest_reward", quest.Reward));
            HudText.Draw(20f, y + LineHeight, quest.Description);
            string progress = QuestSystem.ProgressLine(quest, context);
            if (progress == null) return y + LineHeight * 2f;
            HudText.Draw(20f, y + LineHeight * 2f, progress);
            return y + LineHeight * 3f;
        }
    }
}
