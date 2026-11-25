using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хвост класса PopulationController (issue #127): вынесен в отдельную часть,
    // чтобы исходный файл не превышал мягкий лимит 150 строк.
    public partial class PopulationController : MonoBehaviour
    {
        private bool IsHomeConnected(BuildingState home)
        {
            if (Connectivity == null && Growth != null) Connectivity = Growth.GetComponent<ConnectivityController>();
            return Connectivity != null && Connectivity.IsHomeConnected(home);
        }

        private void ResolveCityRefs()
        {
            if (Growth == null) return;
            if (Power == null) Power = Growth.GetComponent<PowerController>();
            if (Water == null) Water = Growth.GetComponent<WaterController>();
            if (Waste == null) Waste = Growth.GetComponent<WasteController>();
            if (Services == null) Services = Growth.GetComponent<ServiceCoverageController>();
        }

        private void EnsureNeeds()
        {
            if (needs != null || population == null || Growth == null || Growth.Settlement == null) return;
            ResolveCityRefs();
            needs = new NeedSystem(population, Growth.Settlement,
                Power != null ? () => Power.Rate01 : (Func<float>)null,
                Water != null ? () => Water.Rate01 : (Func<float>)null,
                Waste != null ? () => Waste.Rate01 : (Func<float>)null,
                Services != null ? () => Services.RateOf(ServiceKind.Fire) : (Func<float>)null,
                Services != null ? NeedProviders.Health(Services.System) : null,
                Services != null ? NeedProviders.Leisure(Services.System) : null);
        }

        private void ClampLimits()
        {
            HouseholdSize = Mathf.Max(1, HouseholdSize);
            MoveInIntervalSec = Mathf.Max(1f, MoveInIntervalSec);
            MinEmployment = Mathf.Clamp01(MinEmployment);
            MinSatisfaction = Mathf.Clamp01(MinSatisfaction);
        }
    }
}
