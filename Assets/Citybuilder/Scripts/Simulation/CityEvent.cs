namespace Citybuilder
{
    // Одно событие в ленте города (EPIC-09 шаг 1, ADR-007).
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

        // Форматирует строку для UI: «День 3, 14:00 — Энергия 46%: рост замедлен».
        public string FormatLine()
        {
            int day = Tick / SimulationTime.TicksPerDay;
            int hour = Tick % SimulationTime.TicksPerDay;
            return "День " + day + ", " + hour.ToString("00") + ":00 — " + Message;
        }
    }
}
