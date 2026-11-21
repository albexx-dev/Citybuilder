using System.Collections.Generic;

namespace Citybuilder
{
    // Кольцевая лента последних 50 событий города с дедупликацией в окне 10 тиков (EPIC-09 шаг 1, ADR-007/013).
    // Plain C#, без Unity: запись — ключ + аргументы, язык известен только view на момент отрисовки.
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

        // Добавляет событие, если за последние 10 тиков не было такого же (kind + key + args).
        public bool Add(CityEvent cityEvent)
        {
            if (string.IsNullOrEmpty(cityEvent.MessageKey)) return false;
            string key = (int)cityEvent.Kind + ":" + cityEvent.MessageKey + ":" + CityEvent.ArgsKey(cityEvent.Args);
            int prevTick;
            if (lastTickByKey.TryGetValue(key, out prevTick) && cityEvent.Tick - prevTick < DedupTicks)
            {
                return false;
            }
            lastTickByKey[key] = cityEvent.Tick;
            if (buffer.Count >= MaxEvents) buffer.RemoveAt(0);
            buffer.Add(cityEvent);
            return true;
        }

        // Удобный шов для продюсеров, которые не держат готовую запись.
        public bool Add(EventKind kind, string messageKey, object[] args, int tick)
        {
            return Add(new CityEvent(kind, messageKey, args, tick));
        }

        // Снимок событий в порядке от старых к новым.
        public IReadOnlyList<CityEvent> Snapshot()
        {
            return new List<CityEvent>(buffer);
        }

        // Есть ли событие вида kind без аллокации снимка (опрос каждого кадра OnGUI).
        public bool Contains(EventKind kind)
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                if (buffer[i].Kind == kind) return true;
            }
            return false;
        }

        // Сброс для «Новой игры».
        public void Clear()
        {
            buffer.Clear();
            lastTickByKey.Clear();
        }
    }
}
