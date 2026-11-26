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
            // v5: уровень здания 1..3 — у выросшего дома и у вручную улучшенной станции.
            // 0 = не задано (слот v4), при чтении трактуется как 1.
            public int Level;
        }

        [Serializable]
        public class HouseholdEntry
        {
            public int BuildingIndex;
            public int Size;
            // v4: координаты дома. HasHome = false — старый слот, дом ищется по индексу.
            public bool HasHome;
            public int HomeX;
            public int HomeY;
        }

        [Serializable]
        public class RoadEntry
        {
            public int X;
            public int Y;
            // v7 (ADR-018/022): Id типа дороги (asphalt/brick/wood/stone) и уровень (земля 1, мост 2).
            // Пустое значение = старый слот v1–v6 → asphalt; Level=0 тоже трактуется как 1.
            public string Type;
            public int Level = 1;
        }

        // v6 (ADR-014): опциональные записи. В слотах v1–v5 полей нет — читается как пусто/0.
        [Serializable]
        public class GarbageQueueEntry
        {
            public int X;
            public int Y;
            public int Accumulated;
        }

        [Serializable]
        public class ComfortStreakEntry
        {
            public int X;
            public int Y;
            public int Streak;
        }

        [Serializable]
        public class ConstructionProgressEntry
        {
            public int X;
            public int Y;
            public string DefId;
            public int TicksElapsed;
        }

        // v7 (ADR-022): ставка по району — идентификатор и ступень 1..3.
        [Serializable]
        public class DistrictTaxEntry
        {
            public string DistrictId;
            public int Rate;
        }

        // v1 — без TaxLevel, v2 — с TaxLevel, v3 — плюс ручные здания, v4 — ступень цели,
        // v5 — уровни и ставки, v6 — очереди/кражи/стройка, v7 — поля ниже.
        public int Version = 7;
        public long TickCount;
        public int Balance;
        // Налоговая политика: 0 = не задано (слот v1) → обычный, 1/2/3 = низкий/обычный/высокий.
        // Поле живёт и в v5: это контракт совместимости со слотами v2..v4.
        public int TaxLevel;
        // v5: три ставки по зонам — Residential/Commercial/Industrial, каждая 1..3.
        // null или пустой массив = слот v4: все три зоны равны TaxLevel.
        public int[] TaxRates;
        // Ступень цели (v4): 0..3, где 0 — первая ступень ещё не пройдена.
        public int GoalStage;
        public List<ZoneEntry> Zones = new List<ZoneEntry>();
        public List<BuildingEntry> Buildings = new List<BuildingEntry>();
        public List<HouseholdEntry> Households = new List<HouseholdEntry>();
        public List<RoadEntry> Roads = new List<RoadEntry>();
        // Ручные здания игрока (v3): те, что поставлены мышью, а не выросли.
        public List<BuildingEntry> ManualBuildings = new List<BuildingEntry>();
        // v5 (хвост #48): цепочка квестов — id активных и выполненных задач. Поля опциональны:
        // старый слот без них читается пустыми списками — цепочка стартует заново.
        public List<string> QuestActive = new List<string>();
        public List<string> QuestCompleted = new List<string>();
        // v6 (ADR-014): все четыре поля необязательны; null/пусто = поведение слота v5.
        public int TheftCounter;
        public List<GarbageQueueEntry> GarbageQueue = new List<GarbageQueueEntry>();
        public List<ComfortStreakEntry> ComfortStreak = new List<ComfortStreakEntry>();
        public List<ConstructionProgressEntry> ConstructionProgress = new List<ConstructionProgressEntry>();
        // v7 (ADR-022): все поля необязательны; отсутствие/null/битое значение → дефолт v6.
        public float Debt;
        public float DebtInterestPercent;
        public float MarketMultiplier = 1f;
        public List<DistrictTaxEntry> DistrictTaxRates = new List<DistrictTaxEntry>();
        public int MayorRating;
        public int MilestoneCounterV2;
    }
}
