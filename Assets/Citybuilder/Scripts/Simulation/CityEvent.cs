namespace Citybuilder
{
    // Одно событие в ленте города (EPIC-09 шаг 1, ADR-007): только данные.
    // Форматирование строки — в EventLogView (ADR-007/013), домен не знает язык.
    public struct CityEvent
    {
        public readonly EventKind Kind;
        public readonly string Message;
        public readonly int Tick;

        public CityEvent(EventKind kind, string message, int tick)
        {
            Kind = kind;
            Message = message ?? string.Empty;
            Tick = tick < 0 ? 0 : tick;
        }
    }
}
