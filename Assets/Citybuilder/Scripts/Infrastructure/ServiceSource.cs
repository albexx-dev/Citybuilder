namespace Citybuilder
{
    // Источник службы (EPIC-01): центр станции и радиус покрытия в клетках.
    // Отрицательный радиус приводится к нулю.
    public struct ServiceSource
    {
        public readonly GridPosition Center;
        public readonly int Radius;

        public ServiceSource(GridPosition center, int radius)
        {
            Center = center;
            Radius = radius > 0 ? radius : 0;
        }
    }
}
