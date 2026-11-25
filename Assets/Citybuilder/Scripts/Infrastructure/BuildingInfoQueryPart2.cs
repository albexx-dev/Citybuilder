using System;
using System.Collections.Generic;
using UnityEngine;

namespace Citybuilder
{
    // Хелпер общего заполнения снимка (issue #127).
    public static partial class BuildingInfoQuery
    {
        private static void FillCommonFields(ref BuildingInfo info, BuildingDefinition def,
            BuildingState state, bool isGrown, PopulationSystem population,
            float powerRate01, float waterRate01, float wasteRate01,
            Func<BuildingState, bool> isRoadConnected)
        {
            info.Found = true;
            info.IsGrown = isGrown;
            info.Id = def.Id ?? string.Empty;
            info.DisplayName = string.IsNullOrEmpty(def.DisplayName) ? info.Id : def.DisplayName;
            info.DisplayNameKey = def.DisplayNameKey ?? string.Empty;
            info.Origin = state.Origin;
            info.Cost = def.ConstructionCost;
            info.UpkeepPerHour = BuildingLevelPolicy.ScaledUpkeep(
                def.UpkeepPerHour < 0 ? 0 : def.UpkeepPerHour, state.Level);
            info.Capacity = def.ResidentCapacity;
            info.Level = state.Level;
            info.JobsProvided = def.JobsProvided;
            info.Residents = isGrown && population != null ? population.ResidentsIn(state) : 0;
            info.ServiceKind = def.ServiceKind;
            info.ServiceRadius = BuildingLevelPolicy.ServiceRadius(def.ServiceRadius, state.Level);
            info.PowerRate01 = Sanitize01(powerRate01);
            info.WaterRate01 = Sanitize01(waterRate01);
            info.WasteRate01 = Sanitize01(wasteRate01);
            info.RoadConnected = isRoadConnected != null && isRoadConnected(state);
            info.UpgradePrice = UpgradeController.PriceFor(def, state.Level);
            info.Upgradeable = info.UpgradePrice > 0;
        }
    }
}
