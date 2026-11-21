using System;
using UnityEngine;

namespace Citybuilder
{
    // Инфопанель выбранного здания по клику на клетку + подсветка радиуса службы (EPIC-12 шаги 2-3).
    public class InfoPanelView : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Сетка города. Пусто = ищется на этом же объекте.")] public GridVisualizer GridVisualizer;
        [Tooltip("Выросшие дома. Пусто = ищется на этом же объекте.")] public SettlementGrowthController Growth;
        [Tooltip("Ручные здания. Пусто = ищется на этом же объекте.")] public BuildingPlacementController Placement;
        [Tooltip("Меню режимов. Пусто = ищется на этом же объекте.")] public BuildModeMenu Menu;
        [Tooltip("Население города. Пусто = ищется в сцене.")] public PopulationController Population;
        [Tooltip("Службы города. Пусто = ищется на этом же объекте.")] public ServiceCoverageController Services;
        [Tooltip("Энергия. Пусто = ищется на этом же объекте.")] public PowerController Power;
        [Tooltip("Вода. Пусто = ищется на этом же объекте.")] public WaterController Water;
        [Tooltip("Мусор. Пусто = ищется на этом же объекте.")] public WasteController Waste;
        [Tooltip("Связность дорог. Пусто = ищется на этом же объекте.")] public ConnectivityController Connectivity;

        private BuildingInfo current;

        public BuildingInfo CurrentInfo { get { return current; } }
        public bool IsOpen { get { return current.Found; } }

        public void Close()
        {
            current = new BuildingInfo();
            UiClickGuard.Set("info_panel", new Rect());
            if (GridVisualizer != null) GridVisualizer.ClearRadiusOverlay();
        }

        public void SelectCell(GridPosition cell)
        {
            ResolveRefs();
            if (GridVisualizer == null) return;
            if (Services != null) Services.RefreshNow();
            BuildingInfo next = RunQuery(cell);
            if (!next.Found || (current.Found && current.Origin == next.Origin && current.IsGrown == next.IsGrown))
            {
                Close();
                return;
            }
            current = next;
            ShowOverlay();
        }

        // Снимок выбранного здания: один источник запроса для клика и перерисовки после апгрейда.
        private BuildingInfo RunQuery(GridPosition cell)
        {
            return BuildingInfoQuery.Query(
                cell,
                GridVisualizer.Grid.Size,
                Growth != null ? Growth.Homes : null,
                Placement != null ? Placement.Placed : null,
                Population != null ? Population.Population : null,
                Services != null ? Services.System : null,
                Power != null ? Power.Rate01 : 1f,
                Water != null ? Water.Rate01 : 1f,
                Waste != null ? Waste.Rate01 : 1f,
                s => Connectivity != null && Connectivity.IsHomeConnected(s));
        }

        // Оверлей радиуса службы: после апгрейда радиус вырос — перерисовать.
        private void ShowOverlay()
        {
            if (current.HasServiceRadius) GridVisualizer.ShowRadiusOverlay(current.Origin, current.ServiceRadius, current.ServiceKind);
            else GridVisualizer.ClearRadiusOverlay();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && current.Found)
            {
                Close();
                return;
            }
            if (!Input.GetMouseButtonDown(0) || UiClickGuard.IsOverMenu(Input.mousePosition)) return;
            ResolveRefs();
            if (Menu != null && (Menu.CurrentMode == BuildModeMenu.Mode.Houses
                || Menu.CurrentMode == BuildModeMenu.Mode.Roads
                || Menu.CurrentMode == BuildModeMenu.Mode.Zones))
            {
                return;
            }
            GridPosition cell;
            if (GridVisualizer != null && GridVisualizer.TryGetHoveredCell(out cell)) SelectCell(cell);
            else Close();
        }

        private void OnGUI()
        {
            if (!current.Found) return;
            Rect panel = HudLayout.InfoPanelRect(Screen.width, Screen.height);
            UiClickGuard.Set("info_panel", panel);
            BottomBarStyle.DrawBarBackground(panel);
            DrawLines(panel.x + 10f, panel.y + 8f);
            if (InfoPanelUpgrade.Draw(panel, current)) OnUpgradeClicked();
        }

        // Кнопка «Улучшить» (EPIC-51): списание и пересчёт в InfoPanelUpgrade, панель перерисовывает снимок.
        private void OnUpgradeClicked()
        {
            ResolveRefs();
            if (Growth == null || Placement == null || !current.Found) return;
            if (!InfoPanelUpgrade.TryUpgradeCurrent(current, Growth, Placement, Services).Success) return;
            BuildingInfo next = RunQuery(current.Origin);
            if (!next.Found)
            {
                Close();
                return;
            }
            current = next;
            ShowOverlay();
        }

        private void DrawLines(float x, float y)
        {
            HudText.Draw(x, y, Localization.T("info_panel_title", BuildingDisplayName.Of(current)));
            y += 24f;
            HudText.Draw(x, y, Localization.T("info_panel_cost", current.Cost, current.UpkeepPerHour));
            y += 24f;
            if (current.Capacity > 0)
            {
                HudText.Draw(x, y, Localization.T("info_panel_residents", current.Residents, current.Capacity,
                    Localization.T(current.RoadConnected ? "info_yes" : "info_no")));
                y += 24f;
                HudText.Draw(x, y, Localization.T("info_panel_level", current.Level));
                y += 24f;
                HudText.Draw(x, y, Localization.T("info_panel_supply", Pct(current.PowerRate01), Pct(current.WaterRate01), Pct(current.WasteRate01)));
                y += 24f;
                HudText.Draw(x, y, Localization.T("info_panel_safety", Pct(current.FireCoverage01), Pct(current.PoliceCoverage01)));
                y += 24f;
                HudText.Draw(x, y, Localization.T("info_panel_care", Pct(current.HealthCoverage01), Pct(current.LeisureCoverage01)));
                return;
            }
            if (current.HasServiceRadius)
            {
                HudText.Draw(x, y, Localization.T("info_panel_radius", current.ServiceRadius, current.CoveredHomes, current.TotalHomes));
                y += 24f;
            }
            HudText.Draw(x, y, Localization.T("info_panel_level", current.Level)); // уровень станции (EPIC-51)
            y += 24f;
            if (current.JobsProvided > 0) HudText.Draw(x, y, Localization.T("info_panel_jobs", current.JobsProvided));
        }

        private void ResolveRefs()
        {
            if (GridVisualizer == null) GridVisualizer = GetComponent<GridVisualizer>();
            if (Growth == null) Growth = GetComponent<SettlementGrowthController>();
            if (Placement == null) Placement = GetComponent<BuildingPlacementController>();
            if (Menu == null) Menu = GetComponent<BuildModeMenu>();
            if (Services == null) Services = GetComponent<ServiceCoverageController>();
            if (Power == null) Power = GetComponent<PowerController>();
            if (Water == null) Water = GetComponent<WaterController>();
            if (Waste == null) Waste = GetComponent<WasteController>();
            if (Connectivity == null) Connectivity = GetComponent<ConnectivityController>();
            if (Population == null) Population = FindFirstObjectByType<PopulationController>();
        }

        private static int Pct(float rate01)
        {
            return (int)Math.Round(Mathf.Clamp01(rate01) * 100f);
        }
    }
}
