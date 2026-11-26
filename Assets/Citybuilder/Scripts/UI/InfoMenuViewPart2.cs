using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса InfoMenuView (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class InfoMenuView : MonoBehaviour
    {
        private readonly DistrictPolicy districts = new DistrictPolicy();
        private QuestLogView questLog;
        private bool questLogSearched;

        // Районы доступны адаптеру сцены: новые ссылки в инспекторе не нужны.
        public DistrictPolicy Districts { get { return districts; } }

        private void DrawCachedSections(float width)
        {
            float y = 4f;
            DrawSection(ref y, Localization.T("info_section_city"), cachedLines[0]);
            DrawSection(ref y, Localization.T("info_section_goal") + "  " + RatingLine(), cachedLines[1]);
            DrawHeader(ref y, Localization.T("info_section_networks"));
            for (int i = 2; i <= 6; i++) DrawLine(ref y, cachedLines[i]);
            DrawHeader(ref y, Localization.T("info_section_services"));
            DrawLine(ref y, cachedLines[7]);
            DrawLine(ref y, cachedLines[8]);
            DrawLine(ref y, cachedLines[11]);
            DrawLine(ref y, cachedLines[12]);
            DrawBudgetSection(ref y, width);
            DrawHeader(ref y, Localization.T("info_section_development"));
            DrawLine(ref y, cachedLines[9]);
            DrawLine(ref y, cachedLines[10]);
            InfoMenuGraphRangeButton.Draw(ref y, width); // масштаб 8/30 дней (issue #113)
            MetricsGraphView.Draw(ref y, width);
        }

        private void DrawBudgetSection(ref float y, float width)
        {
            DrawHeader(ref y, Localization.T("info_section_budget"));
            if (Tax != null) DrawTaxRows(ref y, width);
            DrawDistrictRows(ref y);
            if (Upkeep != null)
            {
                Color c = Upkeep.CurrentLook == UpkeepStatusPolicy.Look.Deficit ? UpkeepView.DeficitColor : Color.white;
                HudText.Draw(12f, y, Upkeep.CurrentText, c);
                y += 24f;
            }
        }

        // Три строки налогов (эпик #49): клик циклит уровень своей зоны, подпись — фактический сбор.
        private void DrawTaxRows(ref float y, float width)
        {
            for (int i = 0; i < BudgetZones.Length; i++)
            {
                if (GUI.Button(new Rect(4f, y, Mathf.Min(260f, width - 8f), 26f), Tax.LabelFor(BudgetZones[i])))
                {
                    AudioController.PlayClickGlobal();
                    Tax.Policy.NextZone(BudgetZones[i]);
                    RebuildCache();
                }
                y += 30f;
            }
        }

        // Рейтинг выводится из выполненных вех и казны: отдельного сохранённого значения нет.
        private string RatingLine()
        {
            if (!questLogSearched)
            {
                questLog = FindFirstObjectByType<QuestLogView>();
                questLogSearched = true;
            }
            QuestSystem quests = questLog != null ? questLog.Quests : null;
            if (quests == null) return string.Empty;
            QuestContext context = quests.LastContext;
            return MayorRating.Label(quests.CompletedCount, context.Residents, context.Balance);
        }

        // Строки районов: занятые распределяются теми же кумулятивными долями, что и по зонам.
        private void DrawDistrictRows(ref float y)
        {
            if (Tax == null) return;
            int[] ids = districts.DistrictIds();
            Dictionary<int, int> employed = districts.DistributeEmployed(EmployedTotal());
            for (int i = 0; i < ids.Length; i++)
            {
                int count;
                employed.TryGetValue(ids[i], out count);
                HudText.Draw(12f, y, districts.LabelFor(Tax.Policy, ZoneType.Residential, ids[i], count));
                y += 24f;
            }
        }

        private int EmployedTotal()
        {
            if (Economy == null || Economy.Tax == null) return 0;
            return Economy.Tax.EmployedPerZone().Total;
        }

        private static void DrawSection(ref float y, string header, string line)
        {
            DrawHeader(ref y, header);
            DrawLine(ref y, line);
        }

        private static void DrawHeader(ref float y, string header)
        {
            HudText.Draw(4f, y, header);
            y += 24f;
        }

        private static void DrawLine(ref float y, string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            HudText.Draw(12f, y, line);
            y += 24f;
        }
    }
}
