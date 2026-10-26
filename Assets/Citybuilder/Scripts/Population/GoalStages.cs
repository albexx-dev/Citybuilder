namespace Citybuilder
{
    // Ступени цели: 50 → 100 → 200 жителей. Чистая логика без Unity,
    // состояние живёт внутри и снаружи не требуется.
    public class GoalStages
    {
        public const int StageCount = 3;

        private static readonly int[] Targets = { 50, 100, 200 };
        private static readonly int[] Rewards = { 1000, 2000, 4000 };

        private int stage;

        // Число уже взятых ступеней: 0..3, где 3 — все взяты.
        public int Stage
        {
            get { return stage; }
        }

        // Текущая цель. После последней ступени остаётся 200.
        public int Target
        {
            get
            {
                if (stage >= Targets.Length) return Targets[Targets.Length - 1];
                return Targets[stage];
            }
        }

        public bool AllDone
        {
            get { return stage >= Targets.Length; }
        }

        // Доля пути внутри текущей ступени: 0..1, сверху и снизу режется.
        public float Progress01(int residents)
        {
            if (residents <= 0) return 0f;
            int index = stage < Targets.Length ? stage : Targets.Length - 1;
            int lower = index == 0 ? 0 : Targets[index - 1];
            int upper = Targets[index];
            if (residents <= lower) return 0f;
            if (residents >= upper) return 1f;
            return (float)(residents - lower) / (upper - lower);
        }

        // Забирает награды за все перейдённые ступени. Повторно — 0,
        // отрицательное население не двигает состояние.
        public int Claim(int residents)
        {
            if (residents < 0) return 0;
            int reward = 0;
            while (stage < Targets.Length && residents >= Targets[stage])
            {
                reward += Rewards[stage];
                stage++;
            }
            return reward;
        }
    }
}
