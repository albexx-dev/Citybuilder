using System.Collections.Generic;

namespace Citybuilder
{
    // Троттлинг однотипных событий города: один и тот же вид события проходит
    // не чаще cooldownTicks тиков. Дедуп по строке (CityEventLog) не спасает,
    // когда в тексте меняются числа — процент дефицита растёт каждый тик.
    public class EventThrottlePolicy
    {
        private readonly Dictionary<string, int> lastTicks = new Dictionary<string, int>();
        private readonly int cooldownTicks;

        public EventThrottlePolicy(int cooldownTicks)
        {
            this.cooldownTicks = cooldownTicks < 1 ? 1 : cooldownTicks;
        }

        // Первый вызов вида пропускается, далее — не чаще cooldownTicks тиков.
        public bool Allow(string kindKey, int tick)
        {
            int last;
            if (!lastTicks.TryGetValue(kindKey, out last))
            {
                lastTicks[kindKey] = tick;
                return true;
            }
            if (tick - last < cooldownTicks)
            {
                return false;
            }
            lastTicks[kindKey] = tick;
            return true;
        }

        public void Reset()
        {
            lastTicks.Clear();
        }
    }
}
