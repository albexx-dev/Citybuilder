using UnityEngine;

namespace Citybuilder
{
    // Информационное меню по кнопке «Статистика» или Tab (EPIC-14, EPIC-05, EPIC-15).
    public class InfoMenuView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Потребности города. Пусто = ищется в сцене.")] public NeedsView Needs;
        [Tooltip("Цель города. Пусто = ищется в сцене.")] public GoalView Goal;
        [Tooltip("Связность дорог. Пусто = ищется на этом же объекте.")] public ConnectivityView Connectivity;
        [Tooltip("Пробки на дорогах (EPIC-15). Пусто = ищется на этом же объекте.")] public CongestionController Congestion;
        [Tooltip("Энергия. Пусто = ищется на этом же объекте.")] public PowerView Power;
        [Tooltip("Вода. Пусто = ищется на этом же объекте.")] public WaterView Water;
        [Tooltip("Мусор. Пусто = ищется на этом же объекте.")] public WasteView Waste;
        [Tooltip("Службы города (пожарные, полиция, больницы, досуг). Пусто = ищется на этом же объекте.")] public FireCoverageView Fire;
        [Tooltip("Налоговая политика. Пусто = ищется на этом же объекте.")] public TaxPolicyView Tax;
        [Tooltip("Содержание зданий. Пусто = ищется на этом же объекте.")] public UpkeepView Upkeep;
        [Tooltip("Прогрессия разблокировок. Пусто = ищется на этом же объекте.")] public ProgressionView Progression;
        [Tooltip("Население города (для привлекательности). Пусто = ищется в сцене.")] public PopulationController Population;

        private const float RefreshIntervalSec = 0.5f;
        private const float ContentHeight = 510f;

        private bool isOpen;
        private Vector2 scroll;
        private float nextRefreshTime;
        private readonly string[] cachedLines = new string[11];

        public bool IsOpen { get { return isOpen; } }

        public void Toggle() { SetOpen(!isOpen); }

        public void SetOpen(bool open)
        {
            isOpen = open;
            if (isOpen)
            {
                ResolveRefs();
                RebuildCache();
            }
            else
            {
                UiClickGuard.Set("info_menu", new Rect());
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab)) Toggle();
            if (isOpen && Time.unscaledTime >= nextRefreshTime) RebuildCache();
        }

        private void OnGUI()
        {
            if (!isOpen) return;
            Rect panel = HudLayout.InfoMenuRect(Screen.width, Screen.height);
            UiClickGuard.Set("info_menu", panel);
            BottomBarStyle.DrawBarBackground(panel);
            Rect inner = new Rect(panel.x + 10f, panel.y + 8f, panel.width - 20f, panel.height - 16f);
            Rect view = new Rect(0f, 0f, inner.width - 20f, ContentHeight);
            scroll = GUI.BeginScrollView(inner, scroll, view);
            DrawCachedSections(view.width);
            GUI.EndScrollView();
        }

        private void ResolveRefs()
        {
            if (Connectivity == null) Connectivity = GetComponent<ConnectivityView>();
            if (Congestion == null) Congestion = GetComponent<CongestionController>();
            if (Power == null) Power = GetComponent<PowerView>();
            if (Water == null) Water = GetComponent<WaterView>();
            if (Waste == null) Waste = GetComponent<WasteView>();
            if (Fire == null) Fire = GetComponent<FireCoverageView>();
            if (Tax == null) Tax = GetComponent<TaxPolicyView>();
            if (Upkeep == null) Upkeep = GetComponent<UpkeepView>();
            if (Progression == null) Progression = GetComponent<ProgressionView>();
            if (Needs == null) Needs = FindFirstObjectByType<NeedsView>();
            if (Goal == null) Goal = FindFirstObjectByType<GoalView>();
            if (Population == null) Population = FindFirstObjectByType<PopulationController>();
        }

        private void RebuildCache()
        {
            nextRefreshTime = Time.unscaledTime + RefreshIntervalSec;
            cachedLines[0] = Needs != null ? Needs.CurrentText : string.Empty;
            cachedLines[1] = Goal != null ? Goal.CurrentText : string.Empty;
            cachedLines[2] = Connectivity != null ? Connectivity.CurrentText : string.Empty;
            cachedLines[3] = Congestion != null ? Congestion.CurrentSummaryText : string.Empty;
            cachedLines[4] = Power != null ? Power.CurrentText : string.Empty;
            cachedLines[5] = Water != null ? Water.CurrentText : string.Empty;
            cachedLines[6] = Waste != null ? Waste.CurrentText : string.Empty;
            cachedLines[7] = Fire != null ? Fire.CurrentText : string.Empty;
            cachedLines[8] = Fire != null ? Fire.HealthLeisureText : string.Empty;
            cachedLines[9] = Progression != null ? Progression.CurrentText : string.Empty;
            float attr = Population != null ? Population.CurrentAttractiveness : 1f;
            cachedLines[10] = AttractivenessPolicy.FormatLine(attr);
        }

        private void DrawCachedSections(float width)
        {
            float y = 4f;
            DrawSection(ref y, "=== Город ===", cachedLines[0]);
            DrawSection(ref y, "=== Цель ===", cachedLines[1]);
            DrawHeader(ref y, "=== Сети ===");
            for (int i = 2; i <= 6; i++) DrawLine(ref y, cachedLines[i]);
            DrawHeader(ref y, "=== Службы ===");
            DrawLine(ref y, cachedLines[7]);
            DrawLine(ref y, cachedLines[8]);
            DrawBudgetSection(ref y, width);
            DrawHeader(ref y, "=== Развитие ===");
            DrawLine(ref y, cachedLines[9]);
            DrawLine(ref y, cachedLines[10]);
        }

        private void DrawBudgetSection(ref float y, float width)
        {
            DrawHeader(ref y, "=== Бюджет ===");
            if (Tax != null && GUI.Button(new Rect(4f, y, Mathf.Min(260f, width - 8f), 26f), Tax.CurrentLabel))
            {
                Tax.Policy.Next();
                RebuildCache();
            }
            if (Tax != null) y += 30f;
            if (Upkeep != null)
            {
                Color c = Upkeep.CurrentLook == UpkeepStatusPolicy.Look.Deficit ? UpkeepView.DeficitColor : Color.white;
                HudText.Draw(12f, y, Upkeep.CurrentText, c);
                y += 24f;
            }
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
