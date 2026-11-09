namespace Citybuilder
{
    // Многошаговый туториал v2 (EPIC-18 шаг 1): заменяет StartHintPolicy. Чистый C# без состояния.
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

        public enum TargetButton
        {
            None = 0,
            Zones = 1,
            Growth = 2,
            Houses = 3
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

        public static string FormatText(Step step)
        {
            switch (step)
            {
                case Step.PaintZones:
                    return "Шаг 1/7: [3] Зоны → «Жилая» → закрась 4×4 клетки";
                case Step.EnableGrowth:
                    return "Шаг 2/7: Включи [4] Рост — дома построятся сами";
                case Step.WaitForHouse:
                    return "Шаг 3/7: Дождись первого дома (x4 ускоряет)";
                case Step.BuildPower:
                    return "Шаг 4/7: [1] Дома → построй Электростанцию (Power Station)";
                case Step.BuildWater:
                    return "Шаг 5/7: [1] Дома → построй Водонапорную башню (Water Tower)";
                case Step.BuildWaste:
                    return "Шаг 6/7: [1] Дома → построй Мусорный полигон (Waste Depot)";
                case Step.BuildPark:
                    return "Шаг 7/7: [1] Дома → построй Парк (Park) — людям приятнее жить";
                default:
                    return string.Empty;
            }
        }

        public static TargetButton HighlightTarget(Step step)
        {
            switch (step)
            {
                case Step.PaintZones: return TargetButton.Zones;
                case Step.EnableGrowth: return TargetButton.Growth;
                case Step.BuildPower:
                case Step.BuildWater:
                case Step.BuildWaste:
                case Step.BuildPark: return TargetButton.Houses;
                default: return TargetButton.None;
            }
        }
    }
}
