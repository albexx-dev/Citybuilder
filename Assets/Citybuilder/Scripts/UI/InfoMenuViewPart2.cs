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

        // Один набор районов с налогом: клик по строке меняет сбор в казне.
        public DistrictPolicy Districts { get { return Economy != null ? Economy.Districts : districts; } }

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

        // Строки районов: клик циклит ставку района, сбор идёт по тому же приоритету, что и в казне.
        private void DrawDistrictRows(ref float y)
        {
            if (Tax == null) return;
            DistrictPolicy policy = Districts;
            for (int i = 0; i < BudgetZones.Length; i++)
            {
                ZoneType zone = BudgetZones[i];
                int district = policy.DistrictOfZone(zone);
                if (district <= 0) continue;
                string label = policy.LabelFor(Tax.Policy, zone, district, ZoneEmployed(zone));
                if (GUI.Button(new Rect(4f, y, 260f, 26f), label))
                {
                    AudioController.PlayClickGlobal();
                    policy.SetRate(district, NextLevel(policy.LevelFor(district)));
                    RebuildCache();
                }
                y += 30f;
            }
        }

        private int ZoneEmployed(ZoneType zone)
        {
            if (Economy == null || Economy.Tax == null) return 0;
            return Economy.Tax.EmployedPerZone().ByZone(zone);
        }

        private static TaxPolicy.Level NextLevel(TaxPolicy.Level level)
        {
            if (level == TaxPolicy.Level.Low) return TaxPolicy.Level.Normal;
            if (level == TaxPolicy.Level.Normal) return TaxPolicy.Level.High;
            return TaxPolicy.Level.Low;
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
