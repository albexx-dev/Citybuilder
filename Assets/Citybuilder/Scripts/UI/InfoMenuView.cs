using UnityEngine;

namespace Citybuilder
{
    // Информационное меню по кнопке «Статистика» или Tab (EPIC-14, EPIC-05, EPIC-15, EPIC-19).
    public partial class InfoMenuView : MonoBehaviour
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
        [Tooltip("Экономика города: кражи и преступность (EPIC-47). Пусто = ищется в сцене.")] public EconomyTicker Economy;
        [Tooltip("Мусоровозы (эпик #52). Пусто = ищется в сцене.")] public GarbageTruckView Garbage;

        private const float RefreshIntervalSec = 0.5f;
        private const float ContentHeight = 860f; // + кнопка масштаба 8/30 (#113), секция «История» и вывоз мусора

        // Порядок строк налогов в бюджете (эпик #49).
        private static readonly ZoneType[] BudgetZones =
        {
            ZoneType.Residential, ZoneType.Commercial, ZoneType.Industrial
        };

        private bool isOpen;
        private Vector2 scroll;
        private float nextRefreshTime;
        private readonly string[] cachedLines = new string[13];

        public bool IsOpen { get { return isOpen; } }

        // Смена языка пересобирает кеш строк: инфоменю держит готовые тексты, а не ключи (как EventLogView).
        private void OnLanguageChanged() { if (isOpen) RebuildCache(); }
        private void OnEnable() { Localization.LanguageChanged += OnLanguageChanged; }
        private void OnDisable() { Localization.LanguageChanged -= OnLanguageChanged; }

        public void Toggle() { SetOpen(!isOpen); }

        public void SetOpen(bool open)
        {
            if (isOpen != open) AudioController.PlaySwooshGlobal();
            isOpen = open;
            if (isOpen) { ResolveRefs(); RebuildCache(); }
            else UiClickGuard.Set("info_menu", new Rect());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && BuildModeHotkeys.CanProcessPanelHotkeys())
            {
                if (!isOpen) CloseSiblingPanels();
                Toggle();
            }
            if (isOpen && Time.unscaledTime >= nextRefreshTime) RebuildCache();
        }

        // Хоткей открывает меню поверх других: лента и задачи закрываются (EPIC-14/09/48).
        private void CloseSiblingPanels()
        {
            EventLogView log = FindFirstObjectByType<EventLogView>();
            QuestLogView quests = FindFirstObjectByType<QuestLogView>();
            if (log != null) log.SetOpen(false);
            if (quests != null) quests.SetOpen(false);
            SettingsView.CloseIfOpen();
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
            if (Economy == null) Economy = FindFirstObjectByType<EconomyTicker>();
            if (Garbage == null) Garbage = FindFirstObjectByType<GarbageTruckView>();
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
            cachedLines[11] = Economy != null ? CrimeLine(Economy.Crimes) : string.Empty;
            cachedLines[12] = Garbage != null ? Garbage.ServiceText : string.Empty;
        }

        // Строка преступности (EPIC-47): процент = 1 − покрытие полиции города.
        private static string CrimeLine(CrimeSystem crimes)
        {
            return Localization.T("info_crime", Mathf.RoundToInt(crimes.CrimeLevel01 * 100f));
        }

    }
}
