namespace Citybuilder
{
    // Категории событий в ленте города (EPIC-09 шаг 1, ADR-007).
    public enum EventKind
    {
        Supply = 0,
        Congestion = 1,
        Employment = 2,
        Treasury = 3,
        Fire = 4,
        Info = 5
    }
}
