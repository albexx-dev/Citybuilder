using System;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса InfoPanelView (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class InfoPanelView : MonoBehaviour
    {
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
