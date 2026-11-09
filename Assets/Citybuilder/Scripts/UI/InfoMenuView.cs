using UnityEngine;

namespace Citybuilder
{
    // Информационное меню по кнопке «Статистика» или Tab (EPIC-14).
    // В закрытом состоянии ничего не рисует и не аллоцирует; строки кешируются при открытии/по интервалу.
    public class InfoMenuView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Потребности города. Пусто = ищется в сцене.")]
        public NeedsView Needs;
        [Tooltip("Цель города. Пусто = ищется в сцене.")]
        public GoalView Goal;
        [Tooltip("Связность дорог. Пусто = ищется на этом же объекте.")]
        public ConnectivityView Connectivity;
        [Tooltip("Энергия. Пусто = ищется на этом же объекте.")]
        public PowerView Power;
        [Tooltip("Вода. Пусто = ищется на этом же объекте.")]
        public WaterView Water;
        [Tooltip("Мусор. Пусто = ищется на этом же объекте.")]
        public WasteView Waste;
        [Tooltip("Пожарные и полиция. Пусто = ищется на этом же объекте.")]
        public FireCoverageView Fire;
        [Tooltip("Налоговая политика. Пусто = ищется на этом же объекте.")]
        public TaxPolicyView Tax;
        [Tooltip("Содержание зданий. Пусто = ищется на этом же объекте.")]
        public UpkeepView Upkeep;
        [Tooltip("Прогрессия разблокировок. Пусто = ищется на этом же объекте.")]
        public ProgressionView Progression;

        private const float RefreshIntervalSec = 0.5f;
        private const float ContentHeight = 420f;

        private bool isOpen;
        private Vector2 scroll;
        private float nextRefreshTime;
        private readonly string[] cachedLines = new string[8];

        public bool IsOpen
        {
            get { return isOpen; }
        }

        public void Toggle()
        {
            SetOpen(!isOpen);
        }

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
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Toggle();
            }
            if (isOpen && Time.unscaledTime >= nextRefreshTime)
            {
                RebuildCache();
            }
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
            if (Power == null) Power = GetComponent<PowerView>();
            if (Water == null) Water = GetComponent<WaterView>();
            if (Waste == null) Waste = GetComponent<WasteView>();
            if (Fire == null) Fire = GetComponent<FireCoverageView>();
            if (Tax == null) Tax = GetComponent<TaxPolicyView>();
            if (Upkeep == null) Upkeep = GetComponent<UpkeepView>();
            if (Progression == null) Progression = GetComponent<ProgressionView>();
            if (Needs == null) Needs = FindFirstObjectByType<NeedsView>();
            if (Goal == null) Goal = FindFirstObjectByType<GoalView>();
        }

        private void RebuildCache()
        {
            nextRefreshTime = Time.unscaledTime + RefreshIntervalSec;
            cachedLines[0] = Needs != null ? Needs.CurrentText : string.Empty;
            cachedLines[1] = Goal != null ? Goal.CurrentText : string.Empty;
            cachedLines[2] = Connectivity != null ? Connectivity.CurrentText : string.Empty;
            cachedLines[3] = Power != null ? Power.CurrentText : string.Empty;
            cachedLines[4] = Water != null ? Water.CurrentText : string.Empty;
            cachedLines[5] = Waste != null ? Waste.CurrentText : string.Empty;
            cachedLines[6] = Fire != null ? Fire.CurrentText : string.Empty;
            cachedLines[7] = Progression != null ? Progression.CurrentText : string.Empty;
        }

        private void DrawCachedSections(float width)
        {
            float y = 4f;
            DrawSection(ref y, "=== Город ===", cachedLines[0]);
            DrawSection(ref y, "=== Цель ===", cachedLines[1]);
            DrawHeader(ref y, "=== Сети ===");
            DrawLine(ref y, cachedLines[2]);
            DrawLine(ref y, cachedLines[3]);
            DrawLine(ref y, cachedLines[4]);
            DrawLine(ref y, cachedLines[5]);
            DrawSection(ref y, "=== Службы ===", cachedLines[6]);
            DrawBudgetSection(ref y, width);
            DrawSection(ref y, "=== Развитие ===", cachedLines[7]);
        }

        private void DrawBudgetSection(ref float y, float width)
        {
            DrawHeader(ref y, "=== Бюджет ===");
            if (Tax != null)
            {
                if (GUI.Button(new Rect(4f, y, Mathf.Min(260f, width - 8f), 26f), Tax.CurrentLabel))
                {
                    Tax.Policy.Next();
                    RebuildCache();
                }
                y += 30f;
            }
            if (Upkeep != null) DrawLine(ref y, Upkeep.CurrentText);
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
