namespace Citybuilder
{
    // Многошаговый туториал v3 (EPIC-70, шаг-тексты): полные предложения с точными
    // названиями кнопок. Чистый C# без состояния: Pick решает шаг, FormatText — текст,
    // TargetBuildingId — цель стрелки. Подсветка/стрелка — TutorialArrowView.
    public static class TutorialPolicy
    {
        public enum Step
        {
            PaintZones = 1,
            EnableGrowth = 2,
            WaitForHouse = 3,
            BuildPower = 4,
            BuildWater = 5,
            BuildWaste = 6,
            BuildPark = 7,
            Done = 8
        }

        // Кнопка нижней панели для стрелки. Номера — только идентификаторы, не индексы.
        public enum TargetButton
        {
            None = 0,
            Zones = 1,
            Growth = 2,
            Houses = 3,
            Speed4 = 4
        }

        public struct State
        {
            public int ResidentialCells;
            public bool GrowthMode;
            public int GrownCount;
            public bool HasPowerStation;
            public bool HasWaterTower;
            public bool HasWasteDepot;
            public bool HasPark;
            public bool ParkAvailable;
            public bool Dismissed;
            public string SelectedBuildingId;
        }

        // Выбирает текущий шаг по снимку города; если построено сразу всё — схлопывается в Done.
        public static Step Pick(State state)
        {
            if (state.Dismissed) return Step.Done;
            if (state.GrownCount <= 0)
            {
                if (state.ResidentialCells <= 0) return Step.PaintZones;
                if (!state.GrowthMode) return Step.EnableGrowth;
                return Step.WaitForHouse;
            }
            if (!state.HasPowerStation) return Step.BuildPower;
            if (!state.HasWaterTower) return Step.BuildWater;
            if (!state.HasWasteDepot) return Step.BuildWaste;
            if (state.ParkAvailable && !state.HasPark) return Step.BuildPark;
            return Step.Done;
        }

        // Подсказка первых 3 шагов (обратная совместимость смысла StartHintPolicy).
        public static Step PickEarly(int residentialCells, int grownCount, bool growthMode)
        {
            State s = new State
            {
                ResidentialCells = residentialCells,
                GrownCount = grownCount,
                GrowthMode = growthMode,
                HasPowerStation = true,
                HasWaterTower = true,
                HasWasteDepot = true,
                ParkAvailable = false
            };
            return Pick(s);
        }

        // Текст шага: если для шага-стройки уже выбран целевой тип — подсказка про клик по карте.
        public static string FormatText(Step step, string selectedBuildingId)
        {
            if (IsTypeSelected(step, selectedBuildingId))
            {
                return Localization.T("tutorial_select");
            }
            switch (step)
            {
                case Step.PaintZones:
                    return Localization.T("tutorial_step_1");
                case Step.EnableGrowth:
                    return Localization.T("tutorial_step_2");
                case Step.WaitForHouse:
                    return Localization.T("tutorial_step_3");
                case Step.BuildPower:
                    return Localization.T("tutorial_step_4", Localization.T("building_power_station"));
                case Step.BuildWater:
                    return Localization.T("tutorial_step_5", Localization.T("building_water_tower"));
                case Step.BuildWaste:
                    return Localization.T("tutorial_step_6", Localization.T("building_waste_depot"));
                case Step.BuildPark:
                    return Localization.T("tutorial_step_7", Localization.T("building_park"));
                default:
                    return string.Empty;
            }
        }

        public static string FormatText(Step step)
        {
            return FormatText(step, null);
        }

        // Тип уже выбран, когда выбранный в палитре тип совпал с целью шага-стройки.
        public static bool IsTypeSelected(Step step, string selectedBuildingId)
        {
            string target = TargetBuildingId(step);
            return target != null && !string.IsNullOrEmpty(selectedBuildingId) && selectedBuildingId == target;
        }

        // Идентификатор целевого здания шага-стройки (как BuildingDefinition.Id), иначе null.
        public static string TargetBuildingId(Step step)
        {
            switch (step)
            {
                case Step.BuildPower: return "power_station";
                case Step.BuildWater: return "water_tower";
                case Step.BuildWaste: return "waste_depot";
                case Step.BuildPark: return "park";
                default: return null;
            }
        }

        // Кнопка нижней панели для стрелки. Для шагов-строек это «Здания [1]» — запасная цель,
        // когда палитра над панелью не открыта (точную кнопку здания решает TutorialArrowView).
        public static TargetButton HighlightTarget(Step step)
        {
            switch (step)
            {
                case Step.PaintZones: return TargetButton.Zones;
                case Step.EnableGrowth: return TargetButton.Growth;
                case Step.WaitForHouse: return TargetButton.Speed4;
                case Step.BuildPower:
                case Step.BuildWater:
                case Step.BuildWaste:
                case Step.BuildPark: return TargetButton.Houses;
                default: return TargetButton.None;
            }
        }
    }
}
