using System;

namespace Citybuilder
{
    // Чистая логика цели: победа и поражение опросом систем, своего состояния нет.
    public class GoalSystem
    {
        public const int GoalResidents = 50;
        public const int LoseDay = 3;
        // Поражение — пустой город после дня 3: 3 суток по 24 тика.
        public const long LoseTicks = (long)LoseDay * SimulationTime.TicksPerDay;

        private readonly PopulationSystem population;
        private readonly SimulationClock clock;

        public GoalSystem(PopulationSystem population, SimulationClock clock)
        {
            if (population == null) throw new ArgumentNullException("population");
            if (clock == null) throw new ArgumentNullException("clock");
            this.population = population;
            this.clock = clock;
        }

        public int Residents
        {
            get { return population.TotalResidents; }
        }

        public long Ticks
        {
            get { return clock.CurrentTime.TickCount; }
        }

        // Доля пути к цели 0..1. Переполнение сверху режется.
        public float Progress01
        {
            get
            {
                float progress = (float)Residents / GoalResidents;
                if (progress < 0f) return 0f;
                if (progress > 1f) return 1f;
                return progress;
            }
        }

        public bool Won
        {
            get { return Residents >= GoalResidents; }
        }

        public bool Lost
        {
            get { return Ticks > LoseTicks && Residents == 0; }
        }
    }
}
