using System;

namespace Citybuilder
{
    // Идемпотентная привязка пересчёта снимка к тикам часов. Plain C#: тестируется без Unity.
    public class CoverageTickBinding
    {
        private SimulationClock clock;
        private Action<SimulationTime> handler;
        private bool bound;

        // Повторный Bind теми же часами не создаёт дублей (двойной OnEnable).
        public void Bind(SimulationClock target, Action<SimulationTime> onTick)
        {
            if (target == null || onTick == null) return;
            if (bound && clock == target) return;
            Unbind();
            clock = target;
            handler = onTick;
            clock.TickStarted += handler;
            bound = true;
        }

        public void Unbind()
        {
            if (!bound) return;
            clock.TickStarted -= handler;
            clock = null;
            handler = null;
            bound = false;
        }
    }
}
