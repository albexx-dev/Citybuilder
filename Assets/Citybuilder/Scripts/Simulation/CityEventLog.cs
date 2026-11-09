using System.Collections.Generic;

namespace Citybuilder
{
    // Кольцевая лента последних 50 событий города с дедупликацией в окне 10 тиков (EPIC-09 шаг 1, ADR-007).
    // Plain C#, без Unity и без глобальной шины: контроллеры пишут на границе тика, EventLogView читает снимок.
    public class CityEventLog
    {
        public const int MaxEvents = 50;
        public const int DedupTicks = 10;

        private readonly List<CityEvent> buffer = new List<CityEvent>(MaxEvents);
        private readonly Dictionary<string, int> lastTickByKey = new Dictionary<string, int>();

        public int Count
        {
            get { return buffer.Count; }
        }

        // Добавляет событие, если за последние 10 тиков не было такого же (kind + message).
        public bool Add(EventKind kind, string message, int tick)
        {
            if (string.IsNullOrEmpty(message)) return false;
            int safeTick = tick < 0 ? 0 : tick;
            string key = (int)kind + ":" + message;
            int prevTick;
            if (lastTickByKey.TryGetValue(key, out prevTick) && safeTick - prevTick < DedupTicks)
            {
                return false;
            }
            lastTickByKey[key] = safeTick;
            if (buffer.Count >= MaxEvents) buffer.RemoveAt(0);
            buffer.Add(new CityEvent(kind, message, safeTick));
            return true;
        }

        // Снимок событий в порядке от старых к новым.
        public IReadOnlyList<CityEvent> Snapshot()
        {
            return new List<CityEvent>(buffer);
        }

        // Сброс для «Новой игры».
        public void Clear()
        {
            buffer.Clear();
            lastTickByKey.Clear();
        }
    }
}
