using System;

namespace Citybuilder
{
    // Чистая логика вехи цели (EPIC-08: без исходов победы/поражения, доктрина песочницы GAME_DESIGN §19).
    public class GoalSystem
    {
        public const int GoalResidents = 50;

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

        // Доля пути к первой вехе 0..1. Переполнение сверху режется.
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
    }
}
