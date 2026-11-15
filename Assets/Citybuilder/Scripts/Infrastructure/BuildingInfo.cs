namespace Citybuilder
{
    // Снимок данных выбранного здания для инфопанели (EPIC-12 шаг 1).
    public struct BuildingInfo
    {
        public bool Found;
        public bool IsGrown;
        public string Id;
        public string DisplayName;
        public GridPosition Origin;
        public int Cost;
        public int UpkeepPerHour;
        public int Residents;
        public int Capacity;
        public int Level; // Уровень дома 1..3 (EPIC-59); у не-жилых зданий всегда 1.
        public int JobsProvided;
        public ServiceKind ServiceKind;
        public int ServiceRadius;
        public int CoveredHomes;
        public int TotalHomes;
        public float PowerRate01;
        public float WaterRate01;
        public float WasteRate01;
        public float FireCoverage01;
        public float PoliceCoverage01;
        public float HealthCoverage01;
        public float LeisureCoverage01;
        public bool RoadConnected;
        public bool Upgradeable; // Кнопка «Улучшить» (EPIC-51): станция/служба не на максимуме.
        public int UpgradePrice; // Цена следующего уровня; 0 — кнопки нет.

        public bool HasServiceRadius
        {
            get { return Found && ServiceKind != ServiceKind.None && ServiceRadius > 0; }
        }
    }
}
