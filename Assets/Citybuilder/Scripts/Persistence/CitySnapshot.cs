using System;
using System.Collections.Generic;

namespace Citybuilder
{
    // Слот сохранения: plain-данные для JsonUtility. Только public-поля, без Dictionary.
    [Serializable]
    public class CitySnapshot
    {
        [Serializable]
        public class ZoneEntry
        {
            public int X;
            public int Y;
            public int Type;
        }

        [Serializable]
        public class BuildingEntry
        {
            public string DefId;
            public int X;
            public int Y;
        }

        [Serializable]
        public class HouseholdEntry
        {
            public int BuildingIndex;
            public int Size;
        }

        [Serializable]
        public class RoadEntry
        {
            public int X;
            public int Y;
        }

        // v1 — без TaxLevel, v2 — с TaxLevel, v3 — плюс ручные здания,
        // v4 — плюс ступень цели (GoalStage).
        public int Version = 4;
        public long TickCount;
        public int Balance;
        // Налоговая политика: 0 = не задано (слот v1) → обычный, 1/2/3 = низкий/обычный/высокий.
        public int TaxLevel;
        // Ступень цели (v4): 0..3, где 0 — первая ступень ещё не пройдена.
        public int GoalStage;
        public List<ZoneEntry> Zones = new List<ZoneEntry>();
        public List<BuildingEntry> Buildings = new List<BuildingEntry>();
        public List<HouseholdEntry> Households = new List<HouseholdEntry>();
        public List<RoadEntry> Roads = new List<RoadEntry>();
        // Ручные здания игрока (v3): те, что поставлены мышью, а не выросли.
        public List<BuildingEntry> ManualBuildings = new List<BuildingEntry>();
    }
}
