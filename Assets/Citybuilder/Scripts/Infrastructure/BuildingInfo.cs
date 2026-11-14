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

        public bool HasServiceRadius
        {
            get { return Found && ServiceKind != ServiceKind.None && ServiceRadius > 0; }
        }
    }
}
