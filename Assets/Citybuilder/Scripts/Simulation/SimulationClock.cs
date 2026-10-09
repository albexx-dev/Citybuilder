using System;

namespace Citybuilder
{
    // Игровые часы. Накапливает реальные секунды и выдаёт тики.
    // V1: 1 тик = 1 игровой час. Playing — тик в секунду, FastForward — x4.
    public class SimulationClock
    {
        public const float SecondsPerTick = 1f;

        private float accumulatedSeconds;

        public SimulationTime CurrentTime { get; private set; }
        public SimulationSpeed Speed { get; private set; }

        public event Action<SimulationTime> TickStarted;
        public event Action<SimulationTime> TickCompleted;

        public SimulationClock()
        {
            CurrentTime = new SimulationTime(0);
            Speed = SimulationSpeed.Playing;
            accumulatedSeconds = 0f;
        }

        public void SetSpeed(SimulationSpeed speed)
        {
            Speed = speed;
        }

        // Вызывать каждый кадр с реальным deltaTime. Остаток сохраняется.
        public void Update(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException("deltaTime", "DeltaTime cannot be negative.");
            }
            if (Speed == SimulationSpeed.Paused)
            {
                return;
            }
            accumulatedSeconds = accumulatedSeconds + deltaTime;
            float interval = SecondsPerTick / (float)Speed;
            while (accumulatedSeconds >= interval)
            {
                accumulatedSeconds = accumulatedSeconds - interval;
                RunSingleTick();
            }
        }

        private void RunSingleTick()
        {
            SimulationTime next = CurrentTime.Advance(1);
            if (TickStarted != null)
            {
                TickStarted(next);
            }
            CurrentTime = next;
            if (TickCompleted != null)
            {
                TickCompleted(CurrentTime);
            }
        }
    }
}
