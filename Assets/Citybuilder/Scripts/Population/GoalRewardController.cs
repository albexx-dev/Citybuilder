namespace Citybuilder
{
    // Начисляет награды за ступени цели. Чистая логика без Unity и без своего состояния.
    // Вызывается из Update, чтобы OnGUI оставался только отрисовкой.
    public sealed class GoalRewardController
    {
        public int Claim(GoalSystem goals, GoalStages stages, Treasury treasury)
        {
            if (goals == null || stages == null || treasury == null)
            {
                return 0;
            }
            int reward = stages.Claim(goals.Residents);
            if (reward > 0)
            {
                treasury.Earn(reward);
            }
            return reward;
        }
    }
}
